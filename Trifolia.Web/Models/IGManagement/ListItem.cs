using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class ListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPublished { get; set; }
    }
}