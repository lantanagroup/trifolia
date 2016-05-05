using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Report
{
    public class TrifoliaOrganizationDetail
    {
        public TrifoliaOrganizationDetail()
        {
            this.ExternalOrganizations = new List<ExternalOrganizationDetail>();
            this.Users = new List<OrganizationUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalAuthoredTemplates { get; set; }
        public int TotalEditingUsers { get; set; }
        public int TotalNonEditingUsers { get; set; }
        public int TotalUsers { get; set; }

        public List<ExternalOrganizationDetail> ExternalOrganizations { get; set; }
        public List<OrganizationUser> Users { get; set; }
    }
}