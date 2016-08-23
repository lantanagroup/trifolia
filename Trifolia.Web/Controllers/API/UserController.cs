using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Web.Models.User;

namespace Trifolia.Web.Controllers.API
{
    public class UserController : ApiController
    {
        private IObjectRepository tdb;

        public UserController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public UserController()
            : this(DBContext.Create())
        {

        }

        /// <summary>
        /// Searches for users
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        /// <returns></returns>
        [HttpGet, Route("api/User/Search"), SecurableAction()]
        public IEnumerable<SearchUserModel> SearchUsers(string searchText)
        {
            searchText = searchText.ToLower();

            List<SearchUserModel> matches = new List<SearchUserModel>();

            if (!string.IsNullOrEmpty(searchText))
            {
                matches.AddRange(
                    from u in this.tdb.Users
                    where
                        (u.FirstName + " " + u.LastName).ToLower().Contains(searchText) ||
                        u.Email.Contains(searchText) ||
                        u.UserName.ToLower().Contains(searchText)
                    select new SearchUserModel()
                    {
                        Id = u.Id,
                        Name = u.FirstName + " " + u.LastName
                    });
            }

            return matches.OrderBy(y => y.Name);
        }

        [HttpGet, Route("api/User"), SecurableAction(SecurableNames.ADMIN)]
        public IEnumerable<UserModel> GetUsers()
        {
            IEnumerable<UserModel> userModels = (from u in this.tdb.Users
                                          select new UserModel()
                                          {
                                              Id = u.Id,
                                              UserName = u.UserName,
                                              FirstName = u.FirstName,
                                              LastName = u.LastName,
                                              Email = u.Email,
                                              Phone = u.Phone,
                                              Groups = (from g in u.Groups
                                                        select g.GroupId).ToList(),
                                              Roles = (from r in u.Roles
                                                       select r.RoleId).ToList()
                                          });

            return userModels;
        }

        [HttpPost, Route("api/User"), SecurableAction(SecurableNames.ADMIN)]
        public User SaveUser(int organizationId, UserModel model)
        {
            User user = this.tdb.Users.SingleOrDefault(y => y.Id == model.Id);

            // Check if the user's username is already in use by another user
            var foundDuplicateUserNames = this.tdb.Users.Where(y => (model.Id == null || y.Id != model.Id) && y.UserName == model.UserName);

            if (foundDuplicateUserNames.Count() > 0)
                throw new Exception("This username is already in use!");

            // If no user was found in the DB by the user's id, then create a new one
            if (user == null)
            {
                user = new User();
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

            return user;
        }

        [HttpDelete, Route("api/User/{userId}"), SecurableAction(SecurableNames.ADMIN)]
        public void DeleteUser(int organizationId, int userId)
        {
            var user = this.tdb.Users.Single(y => y.Id == userId);

            user.Roles.ToList().ForEach(y => { this.tdb.UserRoles.DeleteObject(y); });
            user.Groups.ToList().ForEach(y => { this.tdb.UserGroups.DeleteObject(y); });

            this.tdb.Users.DeleteObject(user);
            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/User/{userId}/Role/{roleId}"), SecurableAction(SecurableNames.ADMIN)]
        public void UnassignUserFromRole(int userId, int roleId)
        {
            var userRole = this.tdb.UserRoles.SingleOrDefault(y => y.UserId == userId && y.RoleId == roleId);

            if (userRole != null)
            {
                this.tdb.UserRoles.DeleteObject(userRole);
                this.tdb.SaveChanges();
            }
        }

        [HttpPost, Route("api/User/{userId}/Role/{roleId}"), SecurableAction(SecurableNames.ADMIN)]
        public void AssignUserToRole(int userId, int roleId)
        {
            var user = this.tdb.Users.Single(y => y.Id == userId);
            var role = this.tdb.Roles.Single(y => y.Id == roleId);

            if (user.Roles.Count(y => y.Role == role) > 0)
                return;

            UserRole userRole = new UserRole();
            userRole.User = user;
            userRole.Role = role;
            this.tdb.UserRoles.AddObject(userRole);

            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/User/{userId}/Group/{groupId}"), SecurableAction(SecurableNames.ADMIN)]
        public void UnassignUserFromGroup(int userId, int groupId)
        {
            var userGroup = this.tdb.UserGroups.SingleOrDefault(y => y.UserId == userId && y.GroupId == groupId);

            if (userGroup != null)
            {
                this.tdb.UserGroups.DeleteObject(userGroup);
                this.tdb.SaveChanges();
            }
        }

        [HttpPost, Route("api/User/{userId}/Group/{groupId}"), SecurableAction(SecurableNames.ADMIN)]
        public void AssignUserToGroup(int userId, int groupId)
        {
            var user = this.tdb.Users.Single(y => y.Id == userId);
            var group = this.tdb.Groups.Single(y => y.Id == groupId);

            if (user.Groups.Count(y => y.Group == group) > 0)
                return;

            UserGroup userGroup = new UserGroup();
            userGroup.User = user;
            userGroup.Group = group;
            this.tdb.UserGroups.AddObject(userGroup);

            this.tdb.SaveChanges();
        }
    }
}