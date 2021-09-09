using System;
using System.Collections.Generic;

namespace CorePelican
{

    public class Article
    {
        public DateTime TimeStamp { get; set; }

        public String Html { get; set; }

        public Dictionary<string, string> Tags { get; set; }
    }

}