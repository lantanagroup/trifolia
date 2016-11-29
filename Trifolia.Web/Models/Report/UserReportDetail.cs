using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Report
{
    public class UserReportDetail
    {
        public UserReportDetail()
        {
            this.ExternalOrganizations = new List<ExternalOrganizationDetail>();
            this.Users = new List<UserInfo>();
        }

        public int TotalAuthoredTemplates { get; set; }
        public int TotalEditingUsers { get; set; }
        public int TotalNonEditingUsers { get; set; }
        public int TotalUsers { get; set; }

        public List<ExternalOrganizationDetail> ExternalOrganizations { get; set; }
        public List<UserInfo> Users { get; set; }

        public class UserInfo
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
}