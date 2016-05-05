using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class Relationship
    {
        public Relationship()
        {
            this.Bindings = new List<Binding>();
        }

        public string ImplementationGuideName { get; set; }
        public int ImplementationGuideId { get; set; }
        public bool IsImplementationGuidePublished { get; set; }
        public string TemplateName { get; set; }
        public string TemplateUrl { get; set; }
        public string TemplateOid { get; set; }
        public List<Binding> Bindings { get; set; }
    }

    public class Binding
    {
        public string ConstraintNumber { get; set; }
        public DateTime? Date { get; set; }
        public BindingStrengths Strength { get; set; }
    }

    public enum BindingStrengths
    {
        Static,
        Dynamic
    }
}