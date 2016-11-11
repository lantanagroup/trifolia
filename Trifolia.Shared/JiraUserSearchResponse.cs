using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared
{
    public class JiraUserSearchResponse
    {
        public string self { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string emailAddress { get; set; }
    }
}
