using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class TemplateUsageModel
    {
        public TemplateUsageModel()
        {
            this.Templates = new List<Template>();
        }

        public int ImplementationGuideId { get; set; }
        public string ImplementationGuideName { get; set; }
        public bool IsSameImplementationGuide { get; set; }
        public IEnumerable<Template> Templates { get; set; }

        public class Template
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Oid { get; set; }
            public string Type { get; set; }

            public string FullName
            {
                get
                {
                    return string.Format("{0} ({1})", Name, Oid);
                }
            }
        }
    }
}