using RazorEngineCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class GlobalLayoutTransformer
    {
        private RazorEngine _engine;
        private IRazorEngineCompiledTemplate<GlobalModel> _compiledLayoutTemplate;

        static public GlobalLayoutTransformer SetupTemplate(Configuration configuration)
        {
            var transformer = new GlobalLayoutTransformer();
            transformer._engine = new RazorEngineCore.RazorEngine();
            var layoutTemplate = File.ReadAllText(Path.Combine(configuration.TemplatePath, "layout.cshtml"));
            transformer._compiledLayoutTemplate = transformer._engine.Compile<GlobalModel>(layoutTemplate);
            return transformer;
        }

        public string GeneratePage(GlobalModel model)
        {
            return _compiledLayoutTemplate.Run(instance => instance.Model = model);
        }
    }
}
