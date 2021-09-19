using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var globalModel = new GlobalModel();
            var files = Directory.GetFiles(config.ArticlePath, "*.md");
            var articles = files.Select(f =>
            {
                return ArticleParser.ParseContent(f);
            });

            if (Directory.Exists(config.OutputPath) is false)
            {
                Directory.CreateDirectory(config.OutputPath);
            }

            var pageTransformer = GlobalLayoutTransformer.SetupTemplate(config);

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
            StringBuilder tagCloudBuilder = new StringBuilder();
            foreach (var tag in tagStatistics)
            {
                tagCloudBuilder.Append($"<span><a href=\"{tagPath}/{tag.Key}.html\">{tag.Key} ({tag.Value.Count})</a></span>");
            }
            var tagCloud = tagCloudBuilder.ToString();

            foreach (var tag in tagStatistics)
            {

                var htmlFileName = Path.Combine(tagPath, $"{tag.Key}.html");
                var tagPageContent = tagIndexCreator.GenerateArticleContent(
                    new TagPageModel
                    {
                        TagName = tag.Key,
                        Articles = tag.Value
                    });
                GlobalModel pageModel = new GlobalModel
                {
                    Content = tagPageContent,
                    TagCloudHtml = tagCloud,
                    Title = tag.Key,
                };

                File.WriteAllText(htmlFileName, pageTransformer.GeneratePage(pageModel));
            }            


            // Pages
            var articleCreator = ArticleTransformer.SetupTemplate(config);
            foreach (var article in articles)
            {
                var totalPath = Path.Combine(config.OutputPath, article.DatePath);
                if (Directory.Exists(totalPath) is false)
                    Directory.CreateDirectory(totalPath);
                var articleOutputFileName = Path.Combine(totalPath, article.HtmlFileName);

                GlobalModel pageModel = new GlobalModel
                {
                    Content = articleCreator.GenerateArticleContent(
                        new ArticleModel
                        {
                            Date = article.TimeStamp.ToString("yyyy-MM-dd"),
                            HtmlContent = article.HtmlContent,
                            Title = article.Title,
                        }),
                    TagCloudHtml = tagCloud,
                    Title = article.Title
                };
                File.WriteAllText(articleOutputFileName, pageTransformer.GeneratePage(pageModel));
            }

            var mainCreator = MainPageTransformer.SetupTemplate(config);
            var mainpage = mainCreator.GenerateArticleContent(new MainPageModel
            {
                Articles = articles
            });
            var outputFileName = Path.Combine(config.OutputPath, "index.html");
            File.WriteAllText(outputFileName, pageTransformer.GeneratePage(new GlobalModel
            {
                Content = mainpage,
                TagCloudHtml = tagCloud,
                Title = "Main page"
            }));

            // Main tags page
            // Page for each tag
        }
    }
}
