using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Report
{
    public class TemplateReviewFilter
    {
        public int PageCount { get; set; }
        public int Count { get; set; }

        public string TemplateName { get; set; }
        public string TemplateOid { get; set; }
        public string ImplementationGuideName { get; set; }
        public string AppliesTo { get; set; }
        public string ImpliedTemplateName { get; set; }
        public string ImpliedTemplateOid { get; set; }
        public string ConstraintNumber { get; set; }
        public string IsPrimitive { get; set; }
        public string HasSchematron { get; set; }
        public string ValueSetName { get; set; }
        public string CodeSystemName { get; set; }
    }
}