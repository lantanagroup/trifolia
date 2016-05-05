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
            : this(new TemplateDatabaseDataSource())
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
        [HttpGet, Route("api/Organization"), SecurableAction(SecurableNames.ORGANIZATION_LIST)]
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

            model.Groups = (from g in organization.Groups
                            select new OrganizationGroup()
                            {
                                Id = g.Id,
                                Name = g.Name
                            }).ToList();

            foreach (var user in organization.Users)
            {
                var newUser = CreateOrganizationUser(user);
                model.Users.Add(newUser);
            }

            return model;
        }

        [HttpPost, Route("api/Organization/{organizationId}/User"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public OrganizationUser SaveUser(int organizationId, OrganizationUser model)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            User user = organization.Users.SingleOrDefault(y => y.Id == model.Id);

            // Check if the user's username is already in use by another user
            var foundDuplicateUserNames = this.tdb.Users.Where(y => y.OrganizationId == organizationId && (model.Id == null || y.Id != model.Id) && y.UserName == model.UserName);

            if (foundDuplicateUserNames.Count() > 0)
                throw new Exception("This username is already in use!");

            // If no user was found in the DB by the user's id, then create a new one
            if (user == null)
            {
                user = new User();
                user.Organization = organization;
                this.tdb.Users.AddObject(user);

                // Assign the new user the default role
                Role defaultRole = this.tdb.Roles.SingleOrDefault(y => y.IsDefault);

                if (defaultRole != null)
                {
                    UserRole userRole = new UserRole()
                    {
                        User = user,
                        Role = defaultRole
                    };
                    this.tdb.UserRoles.AddObject(userRole);
                }
            }

            user.UserName = model.UserName;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            this.tdb.SaveChanges();

            return CreateOrganizationUser(user);
        }

        [HttpPost, Route("api/Organization"), SecurableAction(SecurableNames.ORGANIZATION_LIST)]
        public int SaveOrganization(OrganizationModel model)
        {
            Organization organization = this.tdb.Organizations.SingleOrDefault(y => y.Id == model.Id);

            if (organization == null)
            {
                organization = new Organization();
                this.tdb.Organizations.AddObject(organization);
            }

            organization.Name = model.Name;

            this.tdb.SaveChanges();

            return organization.Id;
        }

        [HttpDelete, Route("api/Organization/{organizationId}/User/{userId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public void DeleteUser(int organizationId, int userId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            var user = organization.Users.Single(y => y.Id == userId);

            user.Roles.ToList().ForEach(y => { this.tdb.UserRoles.DeleteObject(y); });
            user.Groups.ToList().ForEach(y => { this.tdb.UserGroups.DeleteObject(y); });

            this.tdb.Users.DeleteObject(user);

            this.tdb.SaveChanges();
        }

        [HttpPost, Route("api/Organization/{organizationId}/Group"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public int SaveGroup(int organizationId, OrganizationGroup model)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            Group group = organization.Groups.SingleOrDefault(y => y.Id == model.Id);

            if (group == null)
            {
                group = new Group();
                group.Organization = organization;
                this.tdb.Groups.AddObject(group);
            }

            group.Name = model.Name;

            this.tdb.SaveChanges();

            return group.Id;
        }

        [HttpPost, Route("api/Organization/{organizationId}/User/{userId}/Group/{groupId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public void AssignUserToGroup(int organizationId, int userId, int groupId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            var user = organization.Users.Single(y => y.Id == userId);
            var group = organization.Groups.Single(y => y.Id == groupId);

            if (user.Groups.Count(y => y.Group == group) > 0)
                return;

            UserGroup userGroup = new UserGroup();
            userGroup.User = user;
            userGroup.Group = group;
            this.tdb.UserGroups.AddObject(userGroup);

            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/Organization/{organizationId}/User/{userId}/Group/{groupId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public void UnassignUserFromGroup(int organizationId, int userId, int groupId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            var userGroup = this.tdb.UserGroups.SingleOrDefault(y => y.UserId == userId && y.GroupId == groupId);

            if (userGroup != null)
            {
                this.tdb.UserGroups.DeleteObject(userGroup);

                this.tdb.SaveChanges();
            }
        }

        [HttpPost, Route("api/Organization/{organizationId}/User/{userId}/Role/{roleId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public void AssignUserToRole(int organizationId, int userId, int roleId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            var user = organization.Users.Single(y => y.Id == userId);
            var role = this.tdb.Roles.Single(y => y.Id == roleId);

            if (user.Roles.Count(y => y.Role == role) > 0)
                return;

            UserRole userRole = new UserRole();
            userRole.User = user;
            userRole.Role = role;
            this.tdb.UserRoles.AddObject(userRole);

            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/Organization/{organizationId}/User/{userId}/Role/{roleId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public void UnassignUserFromRole(int organizationId, int userId, int roleId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            var userRole = this.tdb.UserRoles.SingleOrDefault(y => y.UserId == userId && y.RoleId == roleId);

            if (userRole != null)
            {
                this.tdb.UserRoles.DeleteObject(userRole);

                this.tdb.SaveChanges();
            }
        }

        [HttpDelete, Route("api/Organization/{organizationId}/Group/{groupId}"), SecurableAction(SecurableNames.ORGANIZATION_DETAILS)]
        public void DeleteGroup(int organizationId, int groupId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);
            var group = organization.Groups.Single(y => y.Id == groupId);

            group.Users.ToList().ForEach(y =>
            {
                this.tdb.UserGroups.DeleteObject(y);
            });

            group.ImplementationGuidePermissions.ToList().ForEach(y =>
            {
                this.tdb.ImplementationGuidePermissions.DeleteObject(y);
            });

            group.DefaultPermissions.ToList().ForEach(y =>
            {
                this.tdb.OrganizationDefaultPermissions.DeleteObject(y);
            });

            this.tdb.Groups.DeleteObject(group);

            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/Organization/{organizationId}"), SecurableAction(SecurableNames.ADMIN)]
        public void DeleteOrganization(int organizationId)
        {
            var organization = this.tdb.Organizations.Single(y => y.Id == organizationId);

            // Delete groups
            foreach (var cGroup in organization.Groups)
            {
                cGroup.Users.ToList().ForEach(y => this.tdb.UserGroups.DeleteObject(y));
            }

            organization.Groups.ToList().ForEach(y => this.tdb.Groups.DeleteObject(y));

            // Delete users
            organization.Users.ToList().ForEach(y => this.tdb.Users.DeleteObject(y));

            // Delete role restrictions associated with this org
            organization.RoleRestrictions.ToList().ForEach(y => this.tdb.RoleRestrictions.DeleteObject(y));

            this.tdb.Organizations.DeleteObject(organization);

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

        private OrganizationUser CreateOrganizationUser(User user)
        {
            var newUser = new OrganizationUser()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                UserName = user.UserName
            };

            newUser.Roles = user.Roles.Select(y => y.RoleId);
            newUser.Groups = user.Groups.Select(y => y.GroupId);

            return newUser;
        }
    }
}
