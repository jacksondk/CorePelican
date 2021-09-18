using RazorEngineCore;
using System.IO;

namespace CorePelican
{
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

}
