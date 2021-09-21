using Markdig;
using System;
using System.Collections.Generic;

namespace CorePelican
{
    public class Article
    {

        public string Title { get; set; }

        public string HtmlFileName
        {
            get
            {
                return Title
                    .Replace(' ', '-')
                    .Replace("?", "")
                    .Replace("#", "")
                    .ToLower() + ".html";
            }
        }

        public string DatePath
        {
            get
            {
                return TimeStamp.ToString("yyyy/MM/dd");
            }
        }

        public string IsoDate
        {
            get { return TimeStamp.ToString("yyyy-MM-dd"); }
        }


        public String TotalPath
        {
            get
            {
                return "/" + DatePath + "/" + HtmlFileName;
            }
        }

        public DateTime TimeStamp { get; set; }

        private string _markdownContent;
        public String MarkDownContent
        {
            get
            {
                return _markdownContent;
            }
            set
            {
                _markdownContent = value;
                HtmlContent = Markdown.Parse(value).ToHtml();
            }
        }
        private string _markdownTopContent;
        public String MarkDownTopContent
        {
            get
            {
                return _markdownTopContent;
            }
            set
            {
                _markdownTopContent = value;
                TopHtmlContent = Markdown.Parse(value).ToHtml();
            }
        }

        public ISet<string> Tags { get; set; }

        public String HtmlContent { get; private set; }

        public String TopHtmlContent { get; private set; }
    }

}