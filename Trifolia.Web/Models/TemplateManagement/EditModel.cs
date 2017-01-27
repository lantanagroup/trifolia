using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class EditModel
    {
        public int TemplateId { get; set; }
        public int? DefaultImplementationGuideId { get; set; }

        public string TemplateIdString
        {
            get
            {
                return this.TemplateId != 0 ? this.TemplateId.ToString() : "null";
            }
        }

        public string DefaultImplementationGuideIdString
        {
            get
            {
                return this.DefaultImplementationGuideId != null ? this.DefaultImplementationGuideId.ToString() : "null";
            }
        }
    }
}