using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class IGListItem : ListItem
    {
        public string Namespace { get; set; }

        public string Identifier { get; set; }

        public int TypeId { get; set; }
    }
}