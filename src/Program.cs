﻿using System;
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

        // Serve with python
        // python -m http.server 8000
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

            Dictionary<string, List<Article>> tagStatistics = SortAfterTags(articles);

            
            string tagCloudHtml = CreateTagCloudHtml(tagStatistics);

            CreateTagPages(config, pageTransformer, tagStatistics, tagCloudHtml);

            CreatePages(config, articles, pageTransformer, tagCloudHtml);

            
            foreach (var directory in config.StaticContentDirectories)
            {
                var source = directory.Source;
                var destination = Path.Combine(config.OutputPath, directory.Destination);
                DirectoryCopy(source, destination, true);
            }

            CreateMainPage(config, articles, pageTransformer, tagCloudHtml);


            // Main tags page
            // Page for each tag
        }

        private static void CreateMainPage(Configuration config, IEnumerable<Article> articles, GlobalLayoutTransformer pageTransformer, string tagCloudHtml)
        {
            var mainCreator = MainPageTransformer.SetupTemplate(config);
            var mainpage = mainCreator.GenerateArticleContent(new MainPageModel
            {
                Articles = articles
            });
            var outputFileName = Path.Combine(config.OutputPath, "index.html");
            File.WriteAllText(outputFileName, pageTransformer.GeneratePage(new GlobalModel
            {
                Content = mainpage,
                TagCloudHtml = tagCloudHtml,
                Title = "Main page"
            }));
        }

        private static void CreatePages(Configuration config, IEnumerable<Article> articles, GlobalLayoutTransformer pageTransformer, string tagCloudHtml)
        {
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
                    TagCloudHtml = tagCloudHtml,
                    Title = article.Title
                };
                File.WriteAllText(articleOutputFileName, pageTransformer.GeneratePage(pageModel));
            }
        }

        private static void CreateTagPages(Configuration config, GlobalLayoutTransformer pageTransformer, Dictionary<string, List<Article>> tagStatistics, string tagCloudHtml)
        {
            // Make tag index pages
            var tagPath = Path.Combine(config.OutputPath, "tag");
            if (Directory.Exists(tagPath) is false)
                Directory.CreateDirectory(tagPath);

            var tagIndexCreator = TagPageTransformer.SetupTemplate(config);
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
                    TagCloudHtml = tagCloudHtml,
                    Title = tag.Key,
                };

                File.WriteAllText(htmlFileName, pageTransformer.GeneratePage(pageModel));
            }
        }

        private static string CreateTagCloudHtml(Dictionary<string, List<Article>> tagStatistics)
        {
            StringBuilder tagCloudBuilder = new StringBuilder();
            foreach (var tag in tagStatistics)
            {
                tagCloudBuilder.Append($"<span><a href=\"/tag/{tag.Key}.html\">{tag.Key} ({tag.Value.Count})</a></span>");
            }
            var tagCloud = tagCloudBuilder.ToString();
            return tagCloud;
        }

        private static Dictionary<string, List<Article>> SortAfterTags(IEnumerable<Article> articles)
        {
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

            return tagStatistics;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                if (File.Exists(tempPath) == false)
                    file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
