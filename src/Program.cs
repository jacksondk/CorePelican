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

            var articleLines = content.AsSpan().Slice(lineIndex + 1).ToArray();

            var document = Markdig.Markdown.Parse(string.Join("\n", articleLines));

            // Finding links
            //foreach (var link in document.Descendants<LinkInline>())
            //    Console.WriteLine(link.Url);

            DateTime.TryParseExact(dict["Date"], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate);
            return new Article
            {
                Title = dict["Title"],
                TimeStamp = parsedDate,
                HtmlContent = document.ToHtml()
            };
        }

        public class ArticleTransformer
        {
            private RazorEngine _engine;
            private IRazorEngineCompiledTemplate _compiledTemplate;

            public ArticleTransformer()
            {
                
            }

            static public ArticleTransformer SetupTemplate(Configuration configuration)
            {
                var transformer = new ArticleTransformer();
                transformer._engine = new RazorEngineCore.RazorEngine();
                var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "article.html"));
                transformer._compiledTemplate = transformer._engine.Compile(template);
                return transformer;
            }

            public void WriteArticle(string outputFileName, Article article, Configuration conf)
            {
                string result = _compiledTemplate.Run(
                    new
                    {
                        Title = article.Title,
                        Content = article.HtmlContent
                    }
                    );
                File.WriteAllText(outputFileName, result);
            }
        }

        static void WriteArticle(string outputFileName, Article article, Configuration conf)
        {
            RazorEngineCore.RazorEngine engine = new RazorEngineCore.RazorEngine();
            var template = File.ReadAllText("template/article.html");
            RazorEngineCore.IRazorEngineCompiledTemplate razorEngineCompiledTemplate = engine.Compile(template);

            string result = razorEngineCompiledTemplate.Run(
                new
                {
                    Title = article.Title,
                    Content = article.HtmlContent
                }
                );
            Console.WriteLine(result);

        }

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
                var outputFileName = Path.Combine(totalPath, htmlFileName);
                articleCreator.WriteArticle(outputFileName, article, config);
            }
                        
        }
    }
}
