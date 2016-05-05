using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class CopyResults
    {
        public CopyResults()
        {
            this.Errors = new List<string>();
            this.NewTemplates = new List<TemplateEntry>();
        }

        public List<string> Errors { get; set; }
        public List<TemplateEntry> NewTemplates { get; set; }

        public class TemplateEntry
        {
            public string Url { get; set; }
            public string Name { get; set; }
            public string Oid { get; set; }
        }
    }
}