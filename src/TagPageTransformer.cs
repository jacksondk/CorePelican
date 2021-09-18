using RazorEngineCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class TagPageTransformer
    {
        private RazorEngine _engine;
        private IRazorEngineCompiledTemplate _compiledTemplate;
        private IRazorEngineCompiledTemplate _compiledLayoutTemplate;

        static public TagPageTransformer SetupTemplate(Configuration configuration)
        {
            var transformer = new TagPageTransformer();
            transformer._engine = new RazorEngineCore.RazorEngine();
            var layoutTemplate = File.ReadAllText(Path.Combine(configuration.TemplatePath, "layout.cshtml"));
            var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "tagindex.cshtml"));
            transformer._compiledTemplate = transformer._engine.Compile(template);
            transformer._compiledLayoutTemplate = transformer._engine.Compile(layoutTemplate);
            return transformer;
        }

        public string GenerateArticleContent(string tagname, IEnumerable<Article> articles)
        {
            string content = _compiledTemplate.Run(
                new
                {
                    TagName = tagname,
                    Articles = articles,
                }
                );
            return _compiledLayoutTemplate.Run(
                new
                {
                    Title = $"Articles tagged {tagname}",
                    Content = content
                });
        }
    }
}
