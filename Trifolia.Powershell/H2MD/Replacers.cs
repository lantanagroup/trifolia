using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Trifolia.Powershell.H2MD
{
    public static class Replacers
    {

        private static string GetCodeContent(string code)
        {
            var match = Regex.Match(code, @"<code[^>]*?>([^<]*?)</code>");
            var groups = match.Groups;
            return groups[1].Value;
        }

        private static string IndentLines(string content)
        {
            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            return lines.Aggregate("", (current, line) => current + IndentLine(line));
        }
        private static string TabsToSpaces(string tag)
        {
            return tag.Replace("\t", "    ");
        }

        private static string IndentLine(string line)
        {
            if (line.Trim().Equals(string.Empty))
            {
                return "";
            }
            return line + Environment.NewLine + "    ";
        }

        public static string ReplaceCode(string html)
        {
            var finalHtml = html;
            var singleLineCodeBlocks = new Regex(@"<code>([^\n]*?)</code>").Matches(finalHtml);
            singleLineCodeBlocks.Cast<Match>().ToList().ForEach(block =>
            {
                var code = block.Value;
                var content = GetCodeContent(code);
                finalHtml = finalHtml.Replace(code, string.Format("`{0}`", content));
            });

            var multiLineCodeBlocks = new Regex(@"<code>([^>]*?)</code>").Matches(finalHtml);
            multiLineCodeBlocks.Cast<Match>().ToList().ForEach(block =>
            {
                var code = block.Value;
                var content = GetCodeContent(code);
                finalHtml = finalHtml.Replace(code, string.Format("```{0}```", content));
            });

            return finalHtml;
        }
    }
}
