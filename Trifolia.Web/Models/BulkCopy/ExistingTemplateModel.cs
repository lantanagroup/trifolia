using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class ExistingTemplateModel
    {
        public string Name { get; set; }
        public string Oid { get; set; }
        public bool IsSameImplementationGuide { get; set; }
    }
}