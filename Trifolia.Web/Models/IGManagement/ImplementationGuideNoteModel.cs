using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class ImplementationGuideNoteModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }        
        public string Type { get; set; }
        public string Note { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int OwningImplementationGuideId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int? ConstraintNumber { get; set; }

        public string Number
        {
            get
            {
                return string.Format("{0}-{1}", OwningImplementationGuideId, ConstraintNumber);
            }
        }
    }
}