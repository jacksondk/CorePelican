using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class GlobalModel : RazorEngineCore.RazorEngineTemplateBase
    {
        public string TagCloudHtml { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }
    }
}
