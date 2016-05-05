using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Authentication;
using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Authorization;

namespace Trifolia.Shared
{
    public class LookupOrganization
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string contactName;

        public string ContactName
        {
            get { return contactName; }
            set { contactName = value; }
        }

        public static List<LookupOrganization> GetOrganizations(IObjectRepository tdb)
        {
            string organizationName = CheckPoint.Instance.OrganizationName;

            List<Organization> organizations = tdb.Organizations.ToList();

            if (!CheckPoint.Instance.IsDataAdmin)
                organizations = organizations.Where(y => y.Name.ToLower() == organizationName.ToLower()).ToList();

            return (from o in organizations
                    select new LookupOrganization()
                    {
                        Id = o.Id,
                        Name = o.Name,
                        ContactName = o.ContactName
                    }).ToList();
        }

        public static List<LookupOrganization> GetOrganizations()
        {
            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                return GetOrganizations(tdb);
            }
        }
    }
}
