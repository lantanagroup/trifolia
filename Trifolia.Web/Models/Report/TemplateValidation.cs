using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Report
{
    public class TemplateValidation
    {
        public TemplateValidation()
        {
            this.Items = new List<TemplateValidationResult>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }

        public List<TemplateValidationResult> Items { get; set; }
    }
}