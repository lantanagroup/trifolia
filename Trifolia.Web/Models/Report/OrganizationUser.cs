using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Report
{
    public class OrganizationUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool OkayToContact { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ExternalOrganizationName { get; set; }
        public string ExternalOrganizationType { get; set; }
    }
}