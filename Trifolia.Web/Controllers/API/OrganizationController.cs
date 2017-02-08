using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Web.Models.OrganizationManagement;
using Trifolia.Web.Models.RoleManagement;
using OrganizationModel = Trifolia.Web.Models.OrganizationManagement.OrganizationModel;

namespace Trifolia.Web.Controllers.API
{
    public class OrganizationController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructor

        public OrganizationController()
            : this(DBContext.Create())
        {
        }

        public OrganizationController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        /// <summary>
        /// Gets all organizations in Trifolia
        /// </summary>
        [HttpGet, Route("api/Organization"), SecurableAction]
        public IEnumerable<OrganizationModel> GetOrganizations()
        {
            return (from o in this.tdb.Organizations
                    select new OrganizationModel()
                    {
                        Id = o.Id,
                        Name = o.Name
                    })
                    .OrderBy(y => y.Name);
        }

        [HttpGet, Route("api/Organization/{organizationId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public OrganizationModel GetOrganization(int organizationId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            var model = new OrganizationModel()
            {
                Id = organization.Id,
                Name = organization.Name
            };

            return model;
        }

        [HttpPost, Route("api/Organization"), SecurableAction(SecurableNames.ORGANIZATION_LIST)]
        public int SaveOrganization(OrganizationModel model)
        {
            Organization organization = this.tdb.Organizations.SingleOrDefault(y => y.Id == model.Id);

            if (organization == null)
            {
                organization = new Organization();
                this.tdb.Organizations.Add(organization);
            }

            organization.Name = model.Name;

            this.tdb.SaveChanges();

            return organization.Id;
        }

        [HttpDelete, Route("api/Organization/{organizationId}"), SecurableAction(SecurableNames.ADMIN)]
        public void DeleteOrganization(int organizationId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);

            // Delete role restrictions associated with this org
            organization.RoleRestrictions.ToList().ForEach(y => this.tdb.RoleRestrictions.Remove(y));

            this.tdb.Organizations.Remove(organization);

            this.tdb.SaveChanges();
        }

        [HttpGet, Route("api/Organization/{organizationId}/Role"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public IEnumerable<OrganizationRole> GetRoles(int organizationId)
        {
            IEnumerable<OrganizationRole> roles = (from r in this.tdb.Roles
                                                   where r.Restrictions.Count(y => y.OrganizationId == organizationId) == 0
                                                   select new OrganizationRole()
                                                   {
                                                       Id = r.Id,
                                                       Name = r.Name
                                                   });
            return roles;
        }
    }
}
