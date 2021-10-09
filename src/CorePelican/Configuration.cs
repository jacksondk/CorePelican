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

        public string TemplatePath { get; set; }
        public string OutputPath { get; set; }
        public List<SourceDestination> StaticContentDirectories { get; set; }
    }

    public class SourceDestination
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }
}
