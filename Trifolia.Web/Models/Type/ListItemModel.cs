using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Type
{
    public class ListItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }
        public int TemplateCount { get; set; }
    }
}