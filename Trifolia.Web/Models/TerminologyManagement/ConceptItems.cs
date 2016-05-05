using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class ConceptItems
    {
        public int total { get; set; }
        public ConceptItem[] rows { get; set; }
    }
}