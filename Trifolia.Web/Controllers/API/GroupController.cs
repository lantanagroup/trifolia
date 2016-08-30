using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Logging;
using Trifolia.Web.Models.Group;
using Trifolia.Web.Models.User;

namespace Trifolia.Web.Controllers.API
{
    public class GroupController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public GroupController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public GroupController()
            : this(DBContext.Create())
        {

        }

        #endregion

        #region My Groups

        /// <summary>
        /// Gets disclaimers from groups (that have disclaimers) that the current user is a member of
        /// </summary>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.Group.GroupDisclaimer&gt;</returns>
        [HttpGet, Route("api/Group/My/Disclaimer"), AllowAnonymous]
        public IEnumerable<GroupDisclaimer> GetMyGroupDisclaimers()
        {
            if (!CheckPoint.Instance.IsAuthenticated)
                return new List<GroupDisclaimer>();

            var currentUser = CheckPoint.Instance.GetUser(this.tdb);

            if (currentUser == null)
                return new List<GroupDisclaimer>();

            var groupsWithDisclaimers = currentUser.Groups
                .Where(y => !string.IsNullOrEmpty(y.Group.Disclaimer))
                .Select(y => y.Group);
            var groupDisclaimers = groupsWithDisclaimers.Select(y => new GroupDisclaimer(y));

            return groupDisclaimers;
        }

        /// <summary>
        /// Gets the specified group, including managers and members
        /// </summary>
        /// <param name="groupId">The id of the group to return</param>
        /// <returns>Triflia.Web.Models.Group.GroupModel</returns>
        [HttpGet, Route("api/Group/My/{groupId}"), SecurableAction()]
        public GroupModel GetMyGroup(int groupId)
        {
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            var group = this.tdb.Groups.SingleOrDefault(y => y.Id == groupId);

            if (group.Managers.Count(y => y.User == currentUser) == 0)
                throw new AuthorizationException("You are not a manager of this group");

            return new GroupModel(group);
        }

        /// <summary>
        /// Gets all of the groups that the user manages or is a member of
        /// </summary>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.Group.GroupModel&gt;</returns>
        [HttpGet, Route("api/Group/My"), SecurableAction()]
        public IEnumerable<GroupModel> GetMyGroups()
        {
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);

            var managingGroups = (from g in this.tdb.Groups
                                  join gm in this.tdb.GroupManagers on g.Id equals gm.GroupId
                                  where gm.UserId == currentUser.Id
                                  select g).ToList();
            var memberGroups = (from g in this.tdb.Groups
                                join ug in this.tdb.UserGroups on g.Id equals ug.GroupId
                                where ug.UserId == currentUser.Id
                                select g).ToList();

            IEnumerable<Group> groupList = managingGroups.Union(memberGroups).ToList();

            return (from g in groupList
                    select new GroupModel(g, currentUser));
        }

        /// <summary>
        /// Creates a new group. The current user is automatically added as a manager of the group.
        /// Members and managers of the model are not created as part of this operation.
        /// </summary>
        /// <param name="model">Information about the group to create</param>
        /// <returns>Trifolia.Web.Models.Group.GroupModel</returns>
        [HttpPost, Route("api/Group/My"), SecurableAction()]
        public GroupModel AddMyGroup([FromBody] GroupModel model)
        {
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            Group newGroup = new Group()
            {
                Name = model.Name,
                Description = model.Description,
                Disclaimer = model.Disclaimer,
                IsOpen = model.IsOpen
            };
            this.tdb.Groups.AddObject(newGroup);

            GroupManager newGroupManager = new GroupManager()
            {
                User = CheckPoint.Instance.GetUser(this.tdb),
                Group = newGroup
            };
            this.tdb.GroupManagers.AddObject(newGroupManager);

            this.tdb.SaveChanges();

            return new GroupModel(newGroup);
        }

        /// <summary>
        /// Updates the specified group based on the information passed in the body/model.
        /// </summary>
        /// <param name="groupId">The id of the group to update</param>
        /// <param name="model">Information about the group to update</param>
        /// <returns>Trifolia.Web.Models.Group.GroupModel</returns>
        [HttpPut, Route("api/Group/My/{groupId}"), SecurableAction()]
        public GroupModel UpdateMyGroup(int groupId, [FromBody] GroupModel model)
        {
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            var group = this.tdb.Groups.SingleOrDefault(y => y.Id == groupId);

            if (group.Managers.Count(y => y.User == currentUser) == 0)
                throw new AuthorizationException("You are not a manager of this group");

            if (group.Name != model.Name)
                group.Name = model.Name;

            if (group.Description != model.Description)
                group.Description = model.Description;

            if (group.Disclaimer != model.Disclaimer)
                group.Disclaimer = model.Disclaimer;

            if (group.IsOpen != model.IsOpen)
                group.IsOpen = model.IsOpen;

            this.tdb.SaveChanges();

            return new GroupModel(group);
        }

        /// <summary>
        /// Removes the current user from the specified group
        /// </summary>
        /// <param name="groupId">The id of the group to be removed from</param>
        [HttpDelete, Route("api/Group/{groupId}/User"), SecurableAction()]
        public void LeaveGroup(int groupId)
        {
            var group = this.tdb.Groups.SingleOrDefault(y => y.Id == groupId);
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);

            if (group == null)
                throw new ArgumentException("Invalid groupId");

            var userGroup = this.tdb.UserGroups.SingleOrDefault(y => y.UserId == currentUser.Id && y.GroupId == group.Id);

            if (userGroup == null)
                return;

            this.tdb.UserGroups.DeleteObject(userGroup);
            this.tdb.SaveChanges();
        }

        /// <summary>
        /// Attempts to join the current user to the specified group.
        /// If the user is already a member of the group, nothing is done.
        /// If the group is also a manager of the group, the user is added as a member.
        /// If the group requires approval to join, an email is sent to the managers of the group.
        /// If the group is open for anyone to join, the user is added as a member of the group.
        /// </summary>
        /// <param name="groupId">The id of the group for the current user to join</param>
        [HttpPost, Route("api/Group/{groupId}/User"), SecurableAction()]
        public bool JoinGroup(int groupId)
        {
            var group = this.tdb.Groups.SingleOrDefault(y => y.Id == groupId);
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);

            if (group == null)
                throw new ArgumentException("Invalid groupId");

            if (group.Users.Count(y => y.UserId == currentUser.Id) > 0)
                return true;

            var newUserGroup = new UserGroup()
            {
                Group = group,
                User = currentUser
            };

            if (group.Managers.Count(y => y.UserId == currentUser.Id) > 0 || group.IsOpen)
            {
                this.tdb.UserGroups.AddObject(newUserGroup);
                this.tdb.SaveChanges();
                return true;
            }
            else
            {
                try
                {
                    var client = new SmtpClient(AppSettings.MailHost, AppSettings.MailPort)
                    {
                        Credentials = new NetworkCredential(AppSettings.MailUser, AppSettings.MailPassword),
                        Port = AppSettings.MailPort,
                        EnableSsl = AppSettings.MailEnableSSL
                    };

                    var editGroupUrl = string.Format("{0}://{1}/Account/Group/{2}", Request.RequestUri.Scheme, Request.RequestUri.Authority, group.Id);
                    var htmlBody = string.Format(Properties.Settings.Default.JoinGroupEmailBodyHtml,
                        currentUser.FirstName,
                        currentUser.LastName,
                        currentUser.Email,
                        group.Name,
                        editGroupUrl);
                    var textBody = string.Format(Properties.Settings.Default.JoinGroupEmailBodyText,
                        currentUser.FirstName,
                        currentUser.LastName,
                        currentUser.Email,
                        group.Name,
                        editGroupUrl);

                    MailMessage message = new MailMessage();
                    message.IsBodyHtml = true;
                    message.Body = htmlBody;
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.Subject = Properties.Settings.Default.JoinGroupEmailSubject;
                    message.From = new MailAddress(AppSettings.MailFromAddress);

                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, new ContentType("text/html")));
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, new ContentType("text/plain")));

                    foreach (var groupManager in group.Managers)
                        message.To.Add(new MailAddress(groupManager.User.Email));

                    client.Send(message);

                    return false;
                }
                catch (Exception ex)
                {
                    Log.For(this).Error("Error sending email to managers for user to join group", ex);
                    throw ex;
                }
            }
        }

        #endregion

        #region Members and Managers

        /// <summary>
        /// Adds a user to the specified group as a manager
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <param name="userId">The id of the user</param>
        /// <returns>Trifolia.Web.Models.User.SearchUserModel</returns>
        [HttpPost, Route("api/Group/{groupId}/Manager/{userId}"), SecurableAction()]
        public SearchUserModel AddManagerToGroup(int groupId, int userId)
        {
            var group = this.tdb.Groups.Single(y => y.Id == groupId);
            var user = this.tdb.Users.Single(y => y.Id == userId);

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.ADMIN))
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);

                if (group.Managers.Count(y => y.UserId == currentUser.Id) == 0)
                    throw new AuthorizationException("You are not an admin, and you are not a manager of this group");
            }

            if (this.tdb.GroupManagers.Count(y => y.GroupId == groupId && y.UserId == userId) == 0)
            {
                var newGroupManager = new GroupManager()
                {
                    Group = group,
                    User = user
                };
                this.tdb.GroupManagers.AddObject(newGroupManager);
                this.tdb.SaveChanges();
            }

            return new SearchUserModel(user);
        }

        /// <summary>
        /// Adds a user to the specified group as a member
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <param name="userId">The id of the user</param>
        /// <returns>Trifolia.Web.Models.User.SearchUserModel</returns>
        [HttpPost, Route("api/Group/{groupId}/Member/{userId}"), SecurableAction()]
        public SearchUserModel AddMemberToGroup(int groupId, int userId)
        {
            var group = this.tdb.Groups.SingleOrDefault(y => y.Id == groupId);
            var user = this.tdb.Users.Single(y => y.Id == userId);

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.ADMIN))
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);

                if (group.Managers.Count(y => y.UserId == currentUser.Id) == 0)
                    throw new AuthorizationException("You are not an admin, and you are not a manager of this group");
            }

            if (this.tdb.UserGroups.Count(y => y.GroupId == groupId && y.UserId == userId) == 0)
            {
                var newUserGroup = new UserGroup()
                {
                    Group = group,
                    User = user
                };
                this.tdb.UserGroups.AddObject(newUserGroup);
                this.tdb.SaveChanges();
            }

            return new SearchUserModel(user);
        }

        /// <summary>
        /// Removes a manager (user) from the specified group
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <param name="userId">The id of the user</param>
        [HttpDelete, Route("api/Group/{groupId}/Manager/{userId}"), SecurableAction()]
        public void RemoveManagerFromGroup(int groupId, int userId)
        {
            var group = this.tdb.Groups.SingleOrDefault(y => y.Id == groupId);
            var groupManager = this.tdb.GroupManagers.SingleOrDefault(y => y.GroupId == groupId && y.UserId == userId);

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.ADMIN))
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);

                if (group.Managers.Count(y => y.UserId == currentUser.Id) == 0)
                    throw new AuthorizationException("You are not an admin, and you are not a manager of this group");
            }

            if (groupManager != null)
            {
                this.tdb.GroupManagers.DeleteObject(groupManager);
                this.tdb.SaveChanges();
            }
        }

        /// <summary>
        /// Removes a member (user) from the specified group
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <param name="userId">The id of the user</param>
        [HttpDelete, Route("api/Group/{groupId}/Member/{userId}"), SecurableAction()]
        public void RemoveMemberFromGroup(int groupId, int userId)
        {
            var group = this.tdb.Groups.SingleOrDefault(y => y.Id == groupId);
            var userGroup = this.tdb.UserGroups.SingleOrDefault(y => y.GroupId == groupId && y.UserId == userId);

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.ADMIN))
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);

                if (group.Managers.Count(y => y.UserId == currentUser.Id) == 0)
                    throw new AuthorizationException("You are not an admin, and you are not a manager of this group");
            }

            if (userGroup != null)
            {
                this.tdb.UserGroups.DeleteObject(userGroup);
                this.tdb.SaveChanges();
            }
        }

        /// <summary>
        /// Gets all managers of the specified group
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.User.SearchUserModel&gt;</returns>
        [HttpGet, Route("api/Group/{groupId}/Manager"), SecurableAction()]
        public IEnumerable<SearchUserModel> GetGroupManagers(int groupId)
        {
            Group group = this.tdb.Groups.Single(y => y.Id == groupId);

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.ADMIN))
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);

                if (group.Managers.Count(y => y.UserId == currentUser.Id) == 0)
                    throw new AuthorizationException("You are not an admin, and you are not a manager of this group");
            }

            return group.Managers.Select(y => new SearchUserModel(y.User));
        }

        /// <summary>
        /// Gets all members of the specified group
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.User.SearchUserModel&gt;</returns>
        [HttpGet, Route("api/Group/{groupId}/Member"), SecurableAction()]
        public IEnumerable<SearchUserModel> GetGroupMembers(int groupId)
        {
            Group group = this.tdb.Groups.Single(y => y.Id == groupId);

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.ADMIN))
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);

                if (group.Managers.Count(y => y.UserId == currentUser.Id) == 0)
                    throw new AuthorizationException("You are not an admin, and you are not a manager of this group");
            }

            return group.Users.Select(y => new SearchUserModel(y.User));
        }

        #endregion

        #region Administration

        /// <summary>
        /// Gets all groups
        /// </summary>
        /// <param name="onlyNotMember">Indicates that only groups the current user is not a member of should be returned</param>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.Group.GroupModel&gt;</returns>
        [HttpGet, Route("api/Group"), SecurableAction()]
        public IEnumerable<GroupModel> GetGroups(bool onlyNotMember = false)
        {
            var groups = this.tdb.Groups.AsQueryable();
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);

            if (onlyNotMember)
                groups = groups.Where(y => y.Users.Count(x => x.UserId == currentUser.Id) == 0);

            var groupModels = (from g in groups.ToList()
                               select new GroupModel(g, currentUser));

            return groupModels;
        }

        [HttpGet, Route("api/Group/{groupId}"), SecurableAction(SecurableNames.ADMIN)]
        public GroupModel GetGroup(int groupId)
        {
            var group = this.tdb.Groups.Single(y => y.Id == groupId);
            return new GroupModel(group);
        }

        /// <summary>
        /// Creates or updates a group based on the body/model specified
        /// Updates require an Id to be specified in the model
        /// Responds with the id of the group that was created/updated
        /// </summary>
        /// <param name="model">Information about the group to create/update</param>
        /// <returns>int</returns>
        [HttpPost, Route("api/Group"), SecurableAction(SecurableNames.ADMIN)]
        public int SaveGroup(GroupModel model)
        {
            Group group = this.tdb.Groups.SingleOrDefault(y => y.Id == model.Id);

            if (group == null)
            {
                group = new Group();
                this.tdb.Groups.AddObject(group);
            }

            group.Name = model.Name;
            group.Description = model.Description;
            group.Disclaimer = model.Disclaimer;
            group.IsOpen = model.IsOpen;

            this.tdb.SaveChanges();

            return group.Id;
        }

        /// <summary>
        /// Deletes the specified group
        /// </summary>
        /// <param name="groupId">The id of the group to delete</param>
        [HttpDelete, Route("api/Group/{groupId}"), SecurableAction()]
        public void DeleteGroup(int groupId)
        {
            var group = this.tdb.Groups.Single(y => y.Id == groupId);

            if (!CheckPoint.Instance.HasSecurables(SecurableNames.ADMIN))
            {
                var currentUser = CheckPoint.Instance.GetUser(this.tdb);

                if (group.Managers.Count(y => y.UserId == currentUser.Id) == 0)
                    throw new AuthorizationException("You are not an admin, and you are not a manager of this group");
            }

            group.Users.ToList().ForEach(y =>
            {
                this.tdb.UserGroups.DeleteObject(y);
            });

            group.ImplementationGuidePermissions.ToList().ForEach(y =>
            {
                this.tdb.ImplementationGuidePermissions.DeleteObject(y);
            });

            group.Managers.ToList().ForEach(y =>
            {
                this.tdb.GroupManagers.DeleteObject(y);
            });

            this.tdb.Groups.DeleteObject(group);

            this.tdb.SaveChanges();
        }

        #endregion
    }
}