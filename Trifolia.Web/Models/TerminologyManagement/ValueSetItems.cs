using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class ValueSetItems
    {
        public ValueSetItems()
        {
            this.Items = new List<ValueSetItem>();
        }

        public int TotalItems { get; set; }
        public IEnumerable<ValueSetItem> Items { get; set; }
    }
}