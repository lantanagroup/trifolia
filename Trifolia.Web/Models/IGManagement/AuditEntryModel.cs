using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class AuditEntryModel
    {
        public DateTime AuditDate { get; set; }
        public string Username { get; set; }
        public string IP { get; set; }
        public int? ImplementationGuideId { get; set; }
        public string TemplateName { get; set; }
        public int? ConformanceNumber { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
    }
}