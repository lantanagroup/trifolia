using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Shared
{
    public class OrganizationComboModel
    {
        private int? selectedOrganizationId;

        public int? SelectedOrganizationId
        {
            get { return selectedOrganizationId; }
            set { selectedOrganizationId = value; }
        }
        private List<LookupOrganization> organizations;

        public List<LookupOrganization> Organizations
        {
            get { return organizations; }
            set { organizations = value; }
        }
    }
}