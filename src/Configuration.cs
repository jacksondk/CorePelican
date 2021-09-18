using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class Configuration
    {
        public string ArticlePath { get; set; }
        public string PagePath { get; set; }
        public string StaticFilesPath { get; set; }
        public string TemplatePath { get; set; }
        public string OutputPath { get; set; }
    }
}
