﻿using System;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CorePelican
{
    class Program
    {

        static void ParseContent(string filename)
        {
            var content = System.IO.File.ReadAllLines(filename);

            int lineIndex = 0;
            while (string.IsNullOrWhiteSpace(content[lineIndex]) is false)
            {
                var line = content[lineIndex];
                var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
                var key = parts[0];
                var value = string.Join(':', parts.AsSpan().Slice(1).ToArray());

                Console.WriteLine($"Got {key} with value {value}");
                lineIndex++;
            }

            var articleLines = content.AsSpan().Slice(lineIndex + 1).ToArray();


            var document = Markdig.Markdown.Parse(string.Join("\n", articleLines));
            foreach (var link in document.Descendants<LinkInline>())
                Console.WriteLine(link.Url);

            

            var template = 
@"<html><head>
<title>Hello</title>
</head>
<body>
@Model.Content
</body>
</html>
";
            RazorEngineCore.RazorEngine engine = new RazorEngineCore.RazorEngine();
            RazorEngineCore.IRazorEngineCompiledTemplate razorEngineCompiledTemplate = engine.Compile(template);

            string result = razorEngineCompiledTemplate.Run(
                new
                {
                    Content = document.ToHtml(),
                }
                );
            Console.WriteLine(result);

        }

        private static void TraverseDocument(Markdig.Syntax.ContainerBlock container)
        {
            Console.WriteLine("There are {0} blocks", container.Count);
            foreach (Block block in container)
            {
                Console.WriteLine(block.GetType().ToString());
                if (block is ContainerBlock)
                {
                    Console.WriteLine("Going down");
                    TraverseDocument((ContainerBlock)block);
                }

                if (block is LinkReferenceDefinition)
                {
                    var link = (LinkReferenceDefinition)block;
                    Console.WriteLine($"Found link Label: {link.Label} Title: {link.Title} Url: {link.Url}");
                }
            }
        }

        static void Main(string[] args)
        {
            ParseContent(args[0]);
        }
    }
}