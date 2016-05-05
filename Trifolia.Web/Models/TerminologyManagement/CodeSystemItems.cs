using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class CodeSystemItems
    {
        public int total { get; set; }
        public CodeSystemItem[] rows { get; set; }
    }
}