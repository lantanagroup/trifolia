using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Web.Models.User;
using System.Text;
using System.Security.Cryptography;
using Trifolia.Config;
using Trifolia.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Dynamic;
using Trifolia.Web.Models.Account;
using Trifolia.Shared;
using Trifolia.Import.VSAC;

namespace Trifolia.Web.Controllers.API
{
    public class UserController : ApiController
    {
        private const string MailChimpReleaseAnnouncementStatusUrlFormat = "{0}/lists/{1}/members/{2}";
        private const string MailChimpSubscribeReleaseAnnouncementUrlFormat = "{0}/lists/{1}/members/";
        private IObjectRepository tdb;

        #region Construct/Dispose

        public UserController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public UserController()
            : this(DBContext.Create())
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        private string GetEmailMD5(string email)
        {
            byte[] emailBytes = Encoding.ASCII.GetBytes(email);
            MD5 md5 = MD5.Create();
            byte[] emailMD5 = md5.ComputeHash(emailBytes);

            StringBuilder emailMD5Text = new StringBuilder();
            for (int i = 0; i < emailMD5.Length; i++)
                emailMD5Text.Append(emailMD5[i].ToString("X2"));

            return emailMD5Text.ToString();
        }

        [HttpGet, Route("api/Config/ReleaseAnnouncement"), AllowAnonymous]
        public bool IsReleaseAnnouncementsSupported()
        {
            return !string.IsNullOrEmpty(AppSettings.MailChimpApiKey) &&
                !string.IsNullOrEmpty(AppSettings.MailChimpBaseUrl) &&
                !string.IsNullOrEmpty(AppSettings.MailChimpReleaseAnnouncementList);
        }

        /// <summary>
        /// Checks the status of the currently logged-in user on the configured mail chimp release announcements list
        /// </summary>
        /// <returns>true if the user is susbcribed, false if they WERE subscribed, but are now unsubscribed, null if they have not subscribed before</returns>
        [HttpGet, Route("api/User/Me/ReleaseAnnouncement"), SecurableAction]
        public bool? IsSubscribedReleaseAnnouncements()
        {
            if (string.IsNullOrEmpty(AppSettings.MailChimpApiKey))
            {
                Log.For(this).Error("MailChimpAuthentication appSetting key is not configured. This API method should not be getting called.");
                throw new NotSupportedException("Release announcements has not been configured for this installation of Trifolia");
            }

            if (string.IsNullOrEmpty(AppSettings.MailChimpBaseUrl))
            {
                Log.For(this).Error("MailChimpBaseUrl appSetting key is not configured. This API method should not be getting called.");
                throw new NotSupportedException("Release announcements has not been configured for this installation of Trifolia");
            }

            if (string.IsNullOrEmpty(AppSettings.MailChimpReleaseAnnouncementList))
            {
                Log.For(this).Error("MailChimpReleaseAnnouncementList appSetting key is not configured. This API method should not be getting called.");
                throw new NotSupportedException("Release announcements has not been configured for this installation of Trifolia");
            }

            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            string emailMD5Text = this.GetEmailMD5(currentUser.Email);
            string url = string.Format(MailChimpReleaseAnnouncementStatusUrlFormat, AppSettings.MailChimpBaseUrl, AppSettings.MailChimpReleaseAnnouncementList, emailMD5Text);

            WebRequest request = HttpWebRequest.Create(url);
            request.Method = "GET";
            request.Headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("trifolia:" + AppSettings.MailChimpApiKey));

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        dynamic responseObj = JsonConvert.DeserializeObject(sr.ReadToEnd());

                        if (responseObj.status == "subscribed" || responseObj.status == "pending")
                            return true;
                        else
                            return false;
                    }
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse exceptionResponse = (HttpWebResponse)wex.Response;

                if (exceptionResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;
                else
                {
                    Log.For(this).Error("Unexpected error while communicating with Mail Chimp", wex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Unexpected error while communicating with Mail Chimp", ex);
                throw;
            }

            Log.For(this).Error("Unexpected response from MailChimp received (expected either 404 NotFound or 200 OK), actual is: " + response.StatusCode);
            throw new Exception("Unexpected response from configured release announcement list");
        }

        /// <summary>
        /// Unsubscribes the current user from the configured release announcement list
        /// </summary>
        /// <remarks>Only unsubscribes the user if they are already subscribed</remarks>
        [HttpDelete, Route("api/User/Me/ReleaseAnnouncement"), SecurableAction]
        public void UnsubscribeReleaseAnnouncement()
        {
            bool? isSubscribed = this.IsSubscribedReleaseAnnouncements();

            if (isSubscribed == null || isSubscribed == false)
                return;

            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            string emailMD5Text = this.GetEmailMD5(currentUser.Email);
            string url = string.Format(MailChimpReleaseAnnouncementStatusUrlFormat, AppSettings.MailChimpBaseUrl, AppSettings.MailChimpReleaseAnnouncementList, emailMD5Text);

            dynamic model = new ExpandoObject();
            model.status = "unsubscribed";

            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(url);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "PATCH";
            request.ContentType = "application/json";
            request.Headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("trifolia:" + AppSettings.MailChimpApiKey));

            string modelJson = JsonConvert.SerializeObject(model);
            byte[] modelBytes = Encoding.ASCII.GetBytes(modelJson);
            request.ContentLength = modelBytes.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(modelBytes, 0, modelBytes.Length);
            requestStream.Close();

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Unexpected error occurred while communicating with Mail Chimp", ex);
                throw;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.For(this).Error("Unexpected status code when susbcribing the current user to the configured release announcements list: " + response.StatusCode);
                throw new Exception("Unexpected response from release announcement list");
            }
        }

        /// <summary>
        /// Subscribes the current user to the configured release announcements list
        /// </summary>
        [HttpPost, Route("api/User/Me/ReleaseAnnouncement"), SecurableAction]
        public void SubscribeToReleaseAnnouncements()
        {
            bool? isSubscribed = this.IsSubscribedReleaseAnnouncements();
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            HttpWebRequest request = null;
            dynamic model = new ExpandoObject();

            if (isSubscribed == true)
                return;

            if (isSubscribed == null)
            {
                string url = string.Format(MailChimpSubscribeReleaseAnnouncementUrlFormat, AppSettings.MailChimpBaseUrl, AppSettings.MailChimpReleaseAnnouncementList);
                request = (HttpWebRequest) HttpWebRequest.Create(url);
                request.Method = "POST";

                model.email_address = currentUser.Email;
                model.status = "subscribed";
                model.merge_fields = new ExpandoObject();
                model.merge_fields.FNAME = currentUser.FirstName;
                model.merge_fields.LNAME = currentUser.LastName;
            }
            else if (isSubscribed == false)
            {
                string emailMD5Text = this.GetEmailMD5(currentUser.Email);
                string url = string.Format(MailChimpReleaseAnnouncementStatusUrlFormat, AppSettings.MailChimpBaseUrl, AppSettings.MailChimpReleaseAnnouncementList, emailMD5Text);
                request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "PATCH";

                model.status = "subscribed";
            }

            string modelJson = JsonConvert.SerializeObject(model);
            byte[] modelBytes = Encoding.ASCII.GetBytes(modelJson);

            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ContentLength = modelBytes.Length;
            request.ContentType = "application/json";
            request.Headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("trifolia:" + AppSettings.MailChimpApiKey));

            Stream requestStream = request.GetRequestStream();

            requestStream.Write(modelBytes, 0, modelBytes.Length);
            requestStream.Close();

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Unexpected error occurred while communicating with Mail Chimp", ex);
                throw;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.For(this).Error("Unexpected status code when susbcribing the current user to the configured release announcements list: " + response.StatusCode);
                throw new Exception("Unexpected response from release announcement list");
            }
        }

        /// <summary>
        /// Searches for users
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        /// <returns></returns>
        [HttpGet, Route("api/User/Search"), SecurableAction]
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

        /// <summary>
        /// Gets a list of all users in the database. Can only be executed by admins.
        /// </summary>
        /// <returns>A list of UserModel, populated with user meta-data, groups and roles.</returns>
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

        /// <summary>
        /// Gets the profile model for the currently logged-in user.
        /// </summary>
        [HttpGet, Route("api/User/Me"), SecurableAction]
        public UserProfile GetMyUser()
        {
            User currentUser = CheckPoint.Instance.GetUser(this.tdb);

            UserProfile model = new UserProfile()
            {
                UserName = currentUser.UserName,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Phone = currentUser.Phone,
                Email = currentUser.Email,
                OkayToContact = currentUser.OkayToContact.HasValue ? currentUser.OkayToContact.Value : false,
                Organization = currentUser.ExternalOrganizationName,
                OrganizationType = currentUser.ExternalOrganizationType,
                UmlsUsername = !string.IsNullOrEmpty(currentUser.UMLSUsername) ? "******" : string.Empty,
                UmlsPassword = !string.IsNullOrEmpty(currentUser.UMLSPassword) ? "******" : string.Empty
            };

            if (!string.IsNullOrEmpty(AppSettings.OpenIdConfigUrl))
                model.OpenIdConfigUrl = AppSettings.OpenIdConfigUrl;

            var authData = CheckPoint.Instance.GetAuthenticatedData();

            if (authData.ContainsKey(CheckPoint.AUTH_DATA_OAUTH2_TOKEN))
                model.AuthToken = authData[CheckPoint.AUTH_DATA_OAUTH2_TOKEN];

            return model;
        }


        /// <summary>
        /// This method is a public route for <see cref="ValidateUmlsCredentials(IObjectRepository, ValidateUmlsCredentialsRequestModel)"></see>
        /// </summary>
        [HttpPost, Route("api/User/ValidateUmlsCredentials"), SecurableAction]
        public ValidateUmlsCredentialsResponseModel ValidateUmlsCredentials(ValidateUmlsCredentialsRequestModel model)
        {
            User current = CheckPoint.Instance.GetUser(this.tdb);
            ValidateUmlsCredentialsResponseModel response = new ValidateUmlsCredentialsResponseModel();

            // If they have a valid umls license, they have valid credentials
            if (current.HasValidUmlsLicense(model.Username, model.Password))
            {
                response.CredentialsValid = true;
                response.LicenseValid = true;
            }
            else
            {
                // If they don't have a valid umls license,
                // check if they can authenticate... Their credentials may be valid.
                if (current.HasValidUmlsCredentials(model.Username, model.Password))
                {
                    response.CredentialsValid = true;
                }
            }

            return response;
        }

        /// <summary>
        /// Persists/updates the information in the model to the current user's profile.
        /// </summary>
        [HttpPost, Route("api/User/Me"), SecurableAction]
        public void SaveMyUser(UserProfile model)
        {
            // CheckPoint.Instance.GetUser should always return a user model. If not, this method was called incorrectly.
            User currentUser = CheckPoint.Instance.GetUser(this.tdb);

            if (currentUser.FirstName != model.FirstName)
                currentUser.FirstName = model.FirstName;

            if (currentUser.LastName != model.LastName)
                currentUser.LastName = model.LastName;

            if (currentUser.Phone != model.Phone)
                currentUser.Phone = model.Phone;

            if (currentUser.Email != model.Email)
                currentUser.Email = model.Email;

            if (currentUser.OkayToContact != model.OkayToContact)
                currentUser.OkayToContact = model.OkayToContact;

            if (currentUser.ExternalOrganizationName != model.Organization)
                currentUser.ExternalOrganizationName = model.Organization;

            if (currentUser.ExternalOrganizationType != model.OrganizationType)
                currentUser.ExternalOrganizationType = model.OrganizationType;

            if (currentUser.UMLSUsername != model.UmlsUsername && model.UmlsUsername != "******")
                currentUser.UMLSUsername = model.UmlsUsername.EncryptStringAES();

            if (currentUser.UMLSPassword != model.UmlsPassword && model.UmlsPassword != "******")
                currentUser.UMLSPassword = model.UmlsPassword.EncryptStringAES();

            tdb.SaveChanges();
        }

        /// <summary>
        /// Updates the currently logged-in user's profiles with the data in the specified model
        /// </summary>
        /// <param name="model">The object model containing the properties of the user to update.</param>
        /// <remarks>The Username property is not updated. Username cannot be changed by the user.</remarks>
        [HttpPost, Route("api/User"), SecurableAction(SecurableNames.ADMIN)]
        public void SaveUser(UserModel model)
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
                this.tdb.Users.Add(user);

                // Assign the new user the default role
                Role defaultRole = this.tdb.Roles.SingleOrDefault(y => y.IsDefault);

                if (defaultRole != null)
                {
                    UserRole userRole = new UserRole()
                    {
                        User = user,
                        Role = defaultRole
                    };
                    this.tdb.UserRoles.Add(userRole);
                }
            }

            user.UserName = model.UserName;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/User/{userId}"), SecurableAction(SecurableNames.ADMIN)]
        public void DeleteUser(int organizationId, int userId)
        {
            var user = this.tdb.Users.Single(y => y.Id == userId);

            user.Roles.ToList().ForEach(y => { this.tdb.UserRoles.Remove(y); });
            user.Groups.ToList().ForEach(y => { this.tdb.UserGroups.Remove(y); });

            this.tdb.Users.Remove(user);
            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/User/{userId}/Role/{roleId}"), SecurableAction(SecurableNames.ADMIN)]
        public void UnassignUserFromRole(int userId, int roleId)
        {
            var userRole = this.tdb.UserRoles.SingleOrDefault(y => y.UserId == userId && y.RoleId == roleId);

            if (userRole != null)
            {
                this.tdb.UserRoles.Remove(userRole);
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
            this.tdb.UserRoles.Add(userRole);

            this.tdb.SaveChanges();
        }

        [HttpDelete, Route("api/User/{userId}/Group/{groupId}"), SecurableAction(SecurableNames.ADMIN)]
        public void UnassignUserFromGroup(int userId, int groupId)
        {
            var userGroup = this.tdb.UserGroups.SingleOrDefault(y => y.UserId == userId && y.GroupId == groupId);

            if (userGroup != null)
            {
                this.tdb.UserGroups.Remove(userGroup);
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
            this.tdb.UserGroups.Add(userGroup);

            this.tdb.SaveChanges();
        }
    }
}