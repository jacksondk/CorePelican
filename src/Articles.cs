using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class Articles : IEnumerable<Article>
    {
        private List<Article> _articles;
        private readonly Configuration _configuration;

        public Articles(Configuration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<Article> LoadArticles()
        {
            var files = Directory.GetFiles(_configuration.ArticlePath, "*.md");
            _articles = files.Select(f =>
            {
                return ArticleParser.ParseContent(f);
            }).ToList();
            return _articles;
        }

        public void CreateArticles(GlobalLayoutTransformer pageTransformer)
        {
            Article.AllArticles = _articles;
            var articleCreator = ArticleTransformer.SetupTemplate(_configuration);
            foreach (var article in _articles)
            {
                var totalPath = Path.Combine(_configuration.OutputPath, article.DatePath);
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
                    Title = article.Title
                };
                File.WriteAllText(articleOutputFileName, pageTransformer.GeneratePage(pageModel));
            }
        }

        public IEnumerator<Article> GetEnumerator()
        {
            return ((IEnumerable<Article>)_articles).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_articles).GetEnumerator();
        }
    }
}
