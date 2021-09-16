using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using RazorEngineCore;

namespace CorePelican
{
    class Program
    {

        static Article ParseContent(string filename)
        {
            var content = File.ReadAllLines(filename);

            var dict = new Dictionary<string, string>();
            int lineIndex = 0;
            while (string.IsNullOrWhiteSpace(content[lineIndex]) is false)
            {
                var line = content[lineIndex];
                var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                var key = parts[0];
                var value = string.Join(':', parts.AsSpan().Slice(1).ToArray());

                dict.Add(key, value);

                lineIndex++;
            }
            while (string.IsNullOrWhiteSpace(content[lineIndex]) is true)
            {
                lineIndex++;
            }
            var firstLineInDocument = lineIndex;
            while (string.IsNullOrWhiteSpace(content[lineIndex]) is false)
            {
                lineIndex++;
            }
            var lastLineOfFirstParagraph = lineIndex;

            var articleLines = content.AsSpan().Slice(firstLineInDocument).ToArray();
            var firstParagraph = content.AsSpan().Slice(firstLineInDocument, lastLineOfFirstParagraph - firstLineInDocument).ToArray();
                       
            var document = Markdown.Parse(string.Join("\n", articleLines));
            var topDoc = Markdown.Parse(string.Join("\n", firstParagraph));

            // Finding links
            //foreach (var link in document.Descendants<LinkInline>())
            //    Console.WriteLine(link.Url);

            DateTime.TryParseExact(dict["Date"], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate);
            return new Article
            {
                Title = dict["Title"],
                TimeStamp = parsedDate,
                HtmlContent = document.ToHtml(),
                TopHtmlContent = topDoc.ToHtml(),                
            };
        }

        public class ArticleTransformer
        {
            private RazorEngine _engine;
            private IRazorEngineCompiledTemplate _compiledTemplate;
            private IRazorEngineCompiledTemplate _compiledLayoutTemplate;

            private ArticleTransformer()
            {
                
            }

            static public ArticleTransformer SetupTemplate(Configuration configuration)
            {
                var transformer = new ArticleTransformer();
                transformer._engine = new RazorEngineCore.RazorEngine();
                var layoutTemplate = File.ReadAllText(Path.Combine(configuration.TemplatePath, "layout.cshtml"));
                var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "article.cshtml"));
                transformer._compiledTemplate = transformer._engine.Compile(template);
                transformer._compiledLayoutTemplate = transformer._engine.Compile(layoutTemplate);
                return transformer;
            }

            public string GenerateArticleContent(Article article)
            {
                string content = _compiledTemplate.Run(
                    new
                    {
                        Title = article.Title,
                        HtmlContent = article.HtmlContent,
                        Date = article.TimeStamp.ToString("yyyy-MM-dd")
                    }
                    );
                return _compiledLayoutTemplate.Run(
                    new
                    {
                        Title = article.Title,
                        Content = content
                    });
            }
        }

        public class MainPageTransformer
        {
            private RazorEngine _engine;
            private IRazorEngineCompiledTemplate _compiledTemplate;
            private IRazorEngineCompiledTemplate _compiledLayoutTemplate;

            static public MainPageTransformer SetupTemplate(Configuration configuration)
            {
                var transformer = new MainPageTransformer();
                transformer._engine = new RazorEngineCore.RazorEngine();
                var layoutTemplate = File.ReadAllText(Path.Combine(configuration.TemplatePath, "layout.cshtml"));
                var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "articles.cshtml"));
                transformer._compiledTemplate = transformer._engine.Compile(template);
                transformer._compiledLayoutTemplate = transformer._engine.Compile(layoutTemplate);
                return transformer;
            }
        
            public string GenerateArticleContent(IEnumerable<Article> articles)
            {
                string content = _compiledTemplate.Run(
                    new
                    {
                        Articles = articles,
                    }
                    );
                return _compiledLayoutTemplate.Run(
                    new
                    {
                        Title = "Articles",
                        Content = content
                    });
            }
        }


        //public class PageTransformer
        //{
        //    private RazorEngine _engine;
        //    private IRazorEngineCompiledTemplate _layoutTemplate;

        //    public PageTransformer(Configuration configuration)
        //    {
        //        _engine = new RazorEngine();
        //        var layoutTemplate = File.ReadAllText(Path.Combine(configuration.TemplatePath, "layout.cshtml"));
        //        _layoutTemplate = _engine.Compile(layoutTemplate);
        //    }

        //    public string DoLayout(string title, string content)
        //    {
        //        return _layoutTemplate.Run(new
        //        {
        //            Content = content,
        //            Title = title
        //        });
        //    }
        //}
        public class Configuration
        {
            public string ArticlePath { get; set; }
            public string StaticFilesPath { get; set; }
            public string TemplatePath { get; set; }
            public string OutputPath { get; set; }
        }

        static void Main(string[] args)
        {
            var configFile = File.ReadAllText(args[0]);
            var config = System.Text.Json.JsonSerializer.Deserialize<Configuration>(configFile);

            var files = Directory.GetFiles(config.ArticlePath, "*.md");
            var articles = files.Select(f =>
            {
                return ParseContent(f);
            });

            if (Directory.Exists(config.OutputPath) is false)
            {
                Directory.CreateDirectory(config.OutputPath);
            }

            //var htmlCreator = new PageTransformer(config);
            var articleCreator = ArticleTransformer.SetupTemplate(config);
            foreach(var article in articles)
            {
                var htmlFileName = article.Title
                    .Replace(' ', '-')
                    .Replace("?","")
                    .ToLower() + ".html";
                
                var ymdPath = article.TimeStamp.ToString("yyyy/MM/dd");
                var totalPath = Path.Combine(config.OutputPath, ymdPath);
                if (Directory.Exists(totalPath) is false)
                    Directory.CreateDirectory(totalPath);
                var articleOutputFileName = Path.Combine(totalPath, htmlFileName);

                File.WriteAllText(articleOutputFileName, articleCreator.GenerateArticleContent(article));
            }

            var mainCreator = MainPageTransformer.SetupTemplate(config);
            var mainpage = mainCreator.GenerateArticleContent(articles);
            var outputFileName = Path.Combine(config.OutputPath, "index.html");
            File.WriteAllText(outputFileName, mainpage);
        }
    }
}
