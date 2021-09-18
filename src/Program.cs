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
        static void Main(string[] args)
        {
            var configFile = File.ReadAllText(args[0]);
            var config = System.Text.Json.JsonSerializer.Deserialize<Configuration>(configFile);

            var files = Directory.GetFiles(config.ArticlePath, "*.md");
            var articles = files.Select(f =>
            {
                return ArticleParser.ParseContent(f);
            });

            if (Directory.Exists(config.OutputPath) is false)
            {
                Directory.CreateDirectory(config.OutputPath);
            }

            // Make tag statistics
            var tagStatistics = new Dictionary<string, List<Article>>();
            foreach (var article in articles)
            {
                foreach (var tag in article.Tags)
                {
                    if (tagStatistics.ContainsKey(tag) == false)
                        tagStatistics[tag] = new List<Article>();
                    tagStatistics[tag].Add(article);
                }
            }
            // Make tag index pages
            var tagPath = Path.Combine(config.OutputPath, "tag");
            if (Directory.Exists(tagPath) is false)
                Directory.CreateDirectory(tagPath);
            var tagIndexCreator = TagPageTransformer.SetupTemplate(config);
            foreach (var tag in tagStatistics)
            {
                var htmlFileName = Path.Combine(tagPath, $"{tag.Key}.html");
                File.WriteAllText(htmlFileName,
                    tagIndexCreator.GenerateArticleContent(tag.Key, tag.Value));
            }

            // Pages
            var articleCreator = ArticleTransformer.SetupTemplate(config);
            foreach (var article in articles)
            {
                var totalPath = Path.Combine(config.OutputPath, article.DatePath);
                if (Directory.Exists(totalPath) is false)
                    Directory.CreateDirectory(totalPath);
                var articleOutputFileName = Path.Combine(totalPath, article.HtmlFileName);

                File.WriteAllText(articleOutputFileName, articleCreator.GenerateArticleContent(article));
            }

            var mainCreator = MainPageTransformer.SetupTemplate(config);
            var mainpage = mainCreator.GenerateArticleContent(articles);
            var outputFileName = Path.Combine(config.OutputPath, "index.html");
            File.WriteAllText(outputFileName, mainpage);

            // Tag cloud
            // Main tags page
            // Page for each tag
        }
    }
}
