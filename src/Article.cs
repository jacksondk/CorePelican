using System;
using System.Collections.Generic;

namespace CorePelican
{

    public class Article
    {
        public string Title { get; set; }

        public DateTime TimeStamp { get; set; }

        public String HtmlContent { get; set; }

        public Dictionary<string, string> Tags { get; set; }
    }

}