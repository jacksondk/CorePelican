using RazorEngineCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class MainPageTransformer
    {
        private RazorEngine _engine;
        private IRazorEngineCompiledTemplate _compiledTemplate;
        private IRazorEngineCompiledTemplate _compiledLayoutTemplate;

        static public MainPageTransformer SetupTemplate(Configuration configuration)
        {
            var transformer = new MainPageTransformer();
            transformer._engine = new RazorEngineCore.RazorEngine();
            var layoutTemplate = File.ReadAllText(Path.Combine(configuration.TemplatePath, "layout.cshtml"));
            var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "articles.cshtml"));
            transformer._compiledTemplate = transformer._engine.Compile(template);
            transformer._compiledLayoutTemplate = transformer._engine.Compile(layoutTemplate);
            return transformer;
        }

        public string GenerateArticleContent(IEnumerable<Article> articles)
        {
            string content = _compiledTemplate.Run(
                new
                {
                    Articles = articles,
                }
                );
            return _compiledLayoutTemplate.Run(
                new
                {
                    Title = "Articles",
                    Content = content
                });
        }
    }
}
