using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class MoveModel
    {
        public bool IsPublished { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public int ImplementationGuideId { get; set; }
        public int TemplateTypeId { get; set; }
        public string PrimaryContext { get; set; }
        public string PrimaryContextType { get; set; }
    }
}