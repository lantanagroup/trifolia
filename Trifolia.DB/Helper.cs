using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Trifolia.DB
{
    internal static class Helper
    {

        public const string SpecialCharactersRegexPattern = @"[^\w\s]";

        public static string GetCleanName(string name, int? maxCharacters = null)
        {
            string newName = name
                .Replace("\t", "_")
                .Replace(" ", "_");

            // Replace special characters in the bookmark with nothing
            Regex specialCharactersRegex = new Regex(SpecialCharactersRegexPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            newName = specialCharactersRegex.Replace(newName, "");

            if (maxCharacters != null && newName.Length > maxCharacters.Value)
                newName = newName.Substring(0, maxCharacters.Value);

            return newName;
        }
    }
}
