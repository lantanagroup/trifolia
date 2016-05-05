using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class SmallListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }

        public string DisplayName
        {
            get
            {
                return string.Format("{0} - {1}", Name, Oid);
            }
        }
    }
}