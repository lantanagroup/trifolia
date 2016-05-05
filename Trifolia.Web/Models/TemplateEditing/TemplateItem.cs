using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.DB;

namespace Trifolia.Web.Models.TemplateEditing
{
    public class TemplateItem
    {
        public TemplateItem()
        {

        }

        public TemplateItem(Template template)
        {
            this.Id = template.Id;
            this.Name = template.Name;
            this.Oid = template.Oid;
            this.PrimaryContextType = template.PrimaryContextType;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string PrimaryContextType { get; set; }
    }
}