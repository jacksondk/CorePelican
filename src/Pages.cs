using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class Pages : IEnumerable<Article>
    {
        private List<Article> _articles;
        private Configuration _configuration;

        public Pages(Configuration configuration)
        {
            _configuration = configuration;
        }

        public void CreatePages(GlobalLayoutTransformer pageTransformer)
        {
            Article.AllArticles = _articles.ToList();
            var articleCreator = ArticleTransformer.SetupTemplate(_configuration);
            var totalPath = Path.Combine(_configuration.OutputPath, "pages");
            if (Directory.Exists(totalPath) is false)
                Directory.CreateDirectory(totalPath);
            foreach (var article in _articles)
            {
                var pageFileName = Path.Combine(totalPath, article.HtmlFileName);

                GlobalModel pageModel = new GlobalModel
                {
                    Content = articleCreator.GenerateArticleContent(
                        new ArticleModel
                        {
                            Date = article.TimeStamp.ToString("yyyy-MM-dd"),
                            HtmlContent = article.HtmlContent,
                            Title = article.Title,
                        }),
                    Title = article.Title
                };
                File.WriteAllText(pageFileName, pageTransformer.GeneratePage(pageModel));
            }
        }

        public IEnumerable<Article> LoadPages(Configuration config)
        {
            _articles = Directory.GetFiles(config.PagePath, "*.md")
                            .Select(f =>
                            {
                                return ArticleParser.ParseContent(f);
                            }).ToList();
            return this;
        }

        public IEnumerator<Article> GetEnumerator()
        {
            return _articles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _articles.GetEnumerator();
        }
    }
}
