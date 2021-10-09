using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CorePelican
{
    public class LinkChecker
    {
        private record LinkPair(Uri page, Uri linkOnPage);

        private readonly Stack<LinkPair> _baseUrl = new Stack<LinkPair>();
        private readonly HtmlWeb htmlWeb = new HtmlWeb();
        private readonly string host;
        private readonly Dictionary<Uri, long> timings = new Dictionary<Uri, long>();

        public LinkChecker(Uri baseUrl)
        {
            this._baseUrl.Push(new LinkPair(baseUrl, baseUrl));
            host = baseUrl.Host;
        }

        public enum LinkErrorType
        {
            DOES_NOT_EXIST,
            NOT_HTTPS
        }
        public record LinkError(string page, string link, LinkErrorType type);
        
        public Dictionary<Uri,long> Statistics { get { return timings; } }
        public List<LinkError> FindErrors()
        {
            var errors = new List<LinkError>();
            var errorsFound = new Dictionary<Uri, Uri>();
            var alreadyChecked = new List<Uri>();
            var timer = new Stopwatch();
            while (_baseUrl.Any())
            {
                var lookInto = _baseUrl.Pop();
                var newPage = lookInto.linkOnPage;
                if (alreadyChecked.Contains(newPage))
                    continue;

                alreadyChecked.Add(lookInto.linkOnPage);
                try
                {
                    if (newPage.Scheme == "http" || newPage.Scheme == "https")
                    {
                        timer.Reset(); 
                        timer.Start();
                        var document = htmlWeb.Load(newPage);
                        timer.Stop();
                        timings.Add(newPage, timer.ElapsedMilliseconds);
                        
                        
                        if (newPage.Host == host)
                        {
                            FindPairs(newPage, document, "a", "href");
                            FindPairs(newPage, document, "link", "href");
                            FindPairs(newPage, document, "img", "src");
                            FindPairs(newPage, document, "script", "src");
                            FindPairs(newPage, document, "style", "src");                            
                        }
                        else
                        {
                            if (newPage.Scheme != "https")
                            {
                                errors.Add(new LinkError(lookInto.page.ToString(), lookInto.linkOnPage.ToString(), LinkErrorType.NOT_HTTPS));
                            }
                        }
                    }
                }
                catch (WebException)
                {                    
                    errors.Add(new LinkError(lookInto.page.ToString(), 
                        lookInto.linkOnPage.ToString(), 
                        LinkErrorType.DOES_NOT_EXIST));
                }
            }

            return errors;
        }



        void FindPairs(Uri newPage, HtmlDocument document, string tag, string attribute)
        {
            var nodes = document.DocumentNode.SelectNodes($"//{tag}");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {

                    var nodeHref = node.Attributes[attribute]?.Value;
                    if (nodeHref != null)
                    {
                        var evalUri = new Uri(newPage, nodeHref);
                        _baseUrl.Push(new LinkPair(newPage, evalUri));
                    }
                    else
                    {
                        Console.WriteLine("Found a without a href");
                    }
                }
            }
        }
      
    }

    
}
