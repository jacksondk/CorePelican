using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CorePelican
{
    public class ArticleParser
    {
        static public Article ParseContent(string filename)
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
            while (string.IsNullOrWhiteSpace(content[lineIndex]) is true)
            {
                lineIndex++;
            }
            var firstLineInDocument = lineIndex;
            while (string.IsNullOrWhiteSpace(content[lineIndex]) is false)
            {
                lineIndex++;
            }
            var lastLineOfFirstParagraph = lineIndex;

            var articleLines = content.AsSpan().Slice(firstLineInDocument).ToArray();
            var firstParagraph = content.AsSpan().Slice(firstLineInDocument, lastLineOfFirstParagraph - firstLineInDocument).ToArray();
            
            DateTime.TryParseExact(dict["Date"], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate);
            return new Article
            {
                Title = dict["Title"],
                TimeStamp = parsedDate,
                MarkDownContent = string.Join("\n", articleLines),
                MarkDownTopContent = string.Join("\n", firstParagraph),
                Tags = dict["Tags"].Split(",").Select(s => s.Trim()).ToHashSet()
            };
        }        
    }
}
