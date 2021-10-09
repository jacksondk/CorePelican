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
        private IRazorEngineCompiledTemplate<TagPageModel> _compiledTemplate;        

        static public TagPageTransformer SetupTemplate(Configuration configuration)
        {
            var transformer = new TagPageTransformer();
            transformer._engine = new RazorEngineCore.RazorEngine();
            var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "tagindex.cshtml"));
            transformer._compiledTemplate = transformer._engine.Compile<TagPageModel>(template);
            return transformer;
        }

        public string GenerateArticleContent(TagPageModel model)
        {
            return _compiledTemplate.Run(instance => instance.Model = model);                
        }
    }

    public class TagPageModel : RazorEngineTemplateBase
    {
        public string TagName;
        public IEnumerable<Article> Articles;
    }
}
