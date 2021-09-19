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
        private IRazorEngineCompiledTemplate<MainPageModel> _compiledTemplate;        

        static public MainPageTransformer SetupTemplate(Configuration configuration)
        {
            var transformer = new MainPageTransformer();
            transformer._engine = new RazorEngineCore.RazorEngine();            
            var template = File.ReadAllText(Path.Combine(configuration.TemplatePath, "articles.cshtml"));
            transformer._compiledTemplate = transformer._engine.Compile<MainPageModel>(template);
            return transformer;

        }

        public string GenerateArticleContent(MainPageModel model)
        {
            return _compiledTemplate.Run(instance => instance.Model = model);
        }
    }

    public class MainPageModel : RazorEngineTemplateBase
    {
        public IEnumerable<Article> Articles;
    }
}
