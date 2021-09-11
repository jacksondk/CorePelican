using System;
using System.Collections.Generic;
using System.IO;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CorePelican
{
    class Program
    {

        static Article ParseContent(string filename)
        {
            var content = File.ReadAllLines(filename);

            var dict = new Dictionary<string, string>();
            int lineIndex = 0;
            while (string.IsNullOrWhiteSpace(content[lineIndex]) is false)
            {
                var line = content[lineIndex];
                var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                var key = parts[0];
                var value = string.Join(':', parts.AsSpan().Slice(1).ToArray());

                dict.Add(key, value);

                lineIndex++;
            }

            var articleLines = content.AsSpan().Slice(lineIndex + 1).ToArray();

            var document = Markdig.Markdown.Parse(string.Join("\n", articleLines));
            
            // Finding links
            //foreach (var link in document.Descendants<LinkInline>())
            //    Console.WriteLine(link.Url);

            return new Article
            {
                Title = dict["Title"],
                HtmlContent = document.ToHtml()
            };
        }

        static void WriteArticle(Article article)
        {

            RazorEngineCore.RazorEngine engine = new RazorEngineCore.RazorEngine();
            var template = File.ReadAllText("template/article.html");
            RazorEngineCore.IRazorEngineCompiledTemplate razorEngineCompiledTemplate = engine.Compile(template);

            string result = razorEngineCompiledTemplate.Run(
                new
                {
                    Title = article.Title,
                    Content = article.HtmlContent
                }
                );
            Console.WriteLine(result);

        }


        static void Main(string[] args)
        {
            var article = ParseContent(args[0]);
            WriteArticle(article);
        }
    }
}
