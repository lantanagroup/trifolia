using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class EditBookmarksModel
    {
        public EditBookmarksModel()
        {
            this.TemplateItems = new List<TemplateItem>();
        }

        public int ImplementationGuideId { get; set; }
        public string Name { get; set; }
        public List<TemplateItem> TemplateItems { get; set; }

        public class TemplateItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Bookmark { get; set; }
        }
    }
}