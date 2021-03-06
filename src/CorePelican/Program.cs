using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CorePelican
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var configFile = File.ReadAllText(args[0]);
            var config = System.Text.Json.JsonSerializer.Deserialize<Configuration>(configFile);
            var pages = new Pages(config);
            var tags = new Tags(config);
            var articles = new Articles(config);

            articles.LoadArticles();
            tags.MakeStatistics(articles);
            pages.LoadPages(config);

            PrepareOutputDirectory(config);
            
            string tagCloudHtml = tags.CreateTagCloudHtml();
            string pageSectionHtml = pages.CreatePageSectionHtml();

            var pageTransformer = GlobalLayoutTransformer.SetupTemplate(config);
            pageTransformer.PagesHtml = pageSectionHtml;
            pageTransformer.TagCloudHtml = tagCloudHtml;

            pages.CreatePages(pageTransformer);
            tags.CreateTagPages(pageTransformer);
            articles.CreateArticles(pageTransformer);

            CreateStaticContent(config);

            CreateMainPage(config, articles, pageTransformer, tagCloudHtml);

            StartWebServer(args, config);

            var test = new LinkChecker(new System.Uri("http://localhost:5000"));
            var errors = test.FindErrors();            

            System.Console.WriteLine("Links not found");
            foreach(var error in errors.Where(e => e.type == LinkChecker.LinkErrorType.DOES_NOT_EXIST))
            {
                System.Console.WriteLine(error);
            }
            System.Console.WriteLine("\nLinks not using httpS");
            foreach (var error in errors.Where(e => e.type == LinkChecker.LinkErrorType.NOT_HTTPS))
            {
                System.Console.WriteLine(error);
            }

            System.Console.WriteLine("\n10 slowest links");
            foreach (var timing in test.Statistics.OrderByDescending(s => s.Value).Take(10))
            {
                System.Console.WriteLine($"Page {timing.Key} took {timing.Value}ms to fetch (just the html)");
            }
            System.Console.WriteLine("\nPress enter to finish");
            System.Console.ReadLine();
            System.Threading.Tasks.Task stopWebHostTask = _webHost.StopAsync();
            System.Console.WriteLine("Stopping web server");
            stopWebHostTask.Wait();
        }

        private static void CreateStaticContent(Configuration config)
        {
            foreach (var directory in config.StaticContentDirectories)
            {
                var source = directory.Source;
                var destination = Path.Combine(config.OutputPath, directory.Destination);
                DirectoryCopy(source, destination, true);
            }
        }

        private static void PrepareOutputDirectory(Configuration config)
        {
            if (Directory.Exists(config.OutputPath) is false)
            {
                Directory.CreateDirectory(config.OutputPath);
            }
        }
        
        static IWebHost _webHost;
        private static void StartWebServer(string[] args, Configuration config)
        {
            _webHost = WebHost.CreateDefaultBuilder(args)
                            .Configure(config =>
                            {
                                var options = new DefaultFilesOptions();
                                options.DefaultFileNames.Add("index.html");
                                config.UseDefaultFiles(options);
                                config.UseStaticFiles();

                            })
                            .UseWebRoot(config.OutputPath).Build();
            
            _webHost.RunAsync();
            
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
