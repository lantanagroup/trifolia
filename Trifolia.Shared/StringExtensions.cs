using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Trifolia.Shared
{
    public static class StringExtensions
    {
        private static Regex invalidUtf8Characters = new Regex("[^\x00-\x7F]+");

        public static string RemoveInvalidUtf8Characters(this string theString, string replacement = "")
        {
            return invalidUtf8Characters.Replace(theString, replacement);
        }
    }
}
