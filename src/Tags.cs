using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class Tags
    {
        private Dictionary<string, List<Article>> _tagStatistics;
        private readonly Configuration _configuration;

        public Tags(Configuration configuration)
        {
            _configuration = configuration;
        }

        public void MakeStatistics(IEnumerable<Article> articles)
        {
            _tagStatistics = SortAfterTags(articles);
        }

        public string CreateTagCloudHtml()
        {
            return CreateTagCloudHtml(_tagStatistics);
        }

        public void CreateTagPages(GlobalLayoutTransformer pageTransformer)
        {
            // Make tag index pages
            var tagPath = Path.Combine(_configuration.OutputPath, "tag");
            if (Directory.Exists(tagPath) is false)
                Directory.CreateDirectory(tagPath);

            var tagIndexCreator = TagPageTransformer.SetupTemplate(_configuration);
            foreach (var tag in _tagStatistics)
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
                    Title = tag.Key,
                };

                File.WriteAllText(htmlFileName, pageTransformer.GeneratePage(pageModel));
            }
        }

        private static Dictionary<string, List<Article>> SortAfterTags(IEnumerable<Article> articles)
        {            
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
    }
}
