using RazorEngineCore;
using System.IO;

namespace CorePelican
{
    public class ArticleTransformer
    {
        private RazorEngine _engine;
        private IRazorEngineCompiledTemplate<ArticleModel> _compiledTemplate;
        private IRazorEngineCompiledTemplate _compiledLayoutTemplate;

        private ArticleTransformer()
        {

        }

        static public ArticleTransformer SetupTemplate(Configuration configuration)
        {
            var transformer = new ArticleTransformer();
            transformer._engine = new RazorEngine();
            var layoutTemplate = File.ReadAllText(Path.Combine(configuration.TemplatePath, "layout.cshtml"));
            var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "article.cshtml"));
            transformer._compiledTemplate = transformer._engine.Compile<ArticleModel>(template);
            transformer._compiledLayoutTemplate = transformer._engine.Compile(layoutTemplate);
            return transformer;
        }

        public string GenerateArticleContent(ArticleModel model)
        {
            return _compiledTemplate.Run(instance => instance.Model = model);            
        }
    }

    public class ArticleModel : RazorEngineTemplateBase
    {
        public string Title;
        public string HtmlContent;
        public string Date;
        public GlobalModel Global;
    }
}
