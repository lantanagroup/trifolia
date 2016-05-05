using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Report
{
    public class ExternalOrganizationDetail
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool CanUserEdit { get; set; }
        public bool CanContact { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
    }
}