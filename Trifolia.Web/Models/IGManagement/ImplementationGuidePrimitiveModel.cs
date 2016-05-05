using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class ImplementationGuidePrimitiveModel
    {
        public int ConstraintNumber { get; set; }
        public int OwningImplementationGuideId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string PrimitiveText { get; set; }
        public string Schematron { get; set; }

        public string Number
        {
            get
            {
                return string.Format("{0}-{1}", OwningImplementationGuideId, ConstraintNumber);
            }
        }
    }
}