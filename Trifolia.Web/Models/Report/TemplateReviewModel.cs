using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Report
{
    public class TemplateReviewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateOid { get; set; }
        public int ImplementationGuideId { get; set; }
        public string ImplementationGuideName { get; set; }
        public int? ImpliedTemplateId { get; set; }
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