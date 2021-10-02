using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CorePelican
{
    public class Article
    {
        public string FileName { get; set; }
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
        private MarkdownDocument _parsed;
        public static List<Article> AllArticles { get; set; }
        public String MarkDownContent
        {
            get
            {
                return _markdownContent;
            }
            set
            {
                _markdownContent = value;
                _parsed = Markdown.Parse(value);


            }
        }

        private string _markdownTopContent;
        private MarkdownDocument _parsedTopContent;
        public String MarkDownTopContent
        {
            get
            {
                return _markdownTopContent;
            }
            set
            {
                _markdownTopContent = value;
                _parsedTopContent = Markdown.Parse(value);
            }
        }

        public ISet<string> Tags { get; set; }

        public String HtmlContent
        {
            get
            {
                var linkList = _parsed.Descendants<LinkInline>().ToList();
                Console.WriteLine($"Found {linkList.Count} links");
                foreach (var link in linkList)
                {
                    if (link.Url.StartsWith("http")) continue;
                    if (link.Url.StartsWith("/")) continue;
                    if (link.Url.StartsWith("|filename|"))
                    {
                        var filename = link.Url.Substring("|filename|".Length);
                        var article = AllArticles.Find(a => a.FileName == filename);
                        Console.WriteLine($"Replacing {link.Url} with {article.TotalPath}");
                        link.Url = article.TotalPath;

                        continue;
                    }

                    var art = AllArticles.Find(a => a.Title == link.Url);
                    if (art is null)
                    {
                        Console.WriteLine($"Unknow link {link.Url}");
                    }
                    else
                    {
                        Console.WriteLine($"Replacing {link.Url} with {art.TotalPath}");
                        link.Url = art.TotalPath;
                    }
                }
                return _parsed.ToHtml();
            }
        }

        public String TopHtmlContent
        {
            get
            {
                var linkList = _parsedTopContent.Descendants<LinkInline>().ToList();
                Console.WriteLine($"Found {linkList.Count} links");
                foreach (var link in linkList)
                {
                    if (link.Url.StartsWith("http")) continue;
                    if (link.Url.StartsWith("/")) continue;

                    var art = AllArticles.Find(a => a.Title == link.Url);
                    if (art is null)
                    {
                        Console.WriteLine($"Unknow link {link.Url}");
                    }
                    else
                    {
                        link.Url = art.TotalPath;
                    }
                }
                return _parsedTopContent.ToHtml();
            }
        }
    }

}