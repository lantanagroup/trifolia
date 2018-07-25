using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Powershell.H2MD
{
    public class CustomReplacer : Html2Markdown.Replacement.IReplacer
    {
        public string Replace(string html)
        {
            return CustomAction.Invoke(html);
        }

        public Func<string, string> CustomAction { get; set; }
    }
}
