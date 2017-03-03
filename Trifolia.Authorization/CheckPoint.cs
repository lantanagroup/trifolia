using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

using Trifolia.Logging;
using Trifolia.Authentication;
using Trifolia.DB;
using System.Security.Principal;

namespace Trifolia.Authorization
{
    /// <summary>
    /// Provides security authorization for securable items
    /// </summary>
    public class CheckPoint
    {
        public const string AUTH_DATA_OAUTH2_TOKEN = "OAuth2Token";
        public const string AUTH_DATA_SUPPORT_METHOD = "SupportMethod";

        #region Configuration

        private static string[] TrustedServers
        {
            get
            {
                var value = ConfigurationManager.AppSettings["TrustedServers"];

                if (string.IsNullOrEmpty(value))
                    return new string[] { };

                return value.Split(',');
            }
        }

        #endregion

        #region Singleton

        private static CheckPoint _instance;
        public static CheckPoint Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CheckPoint();

                return _instance;
            }
            internal set
            {
                _instance = value;
            }
        }

        #endregion

        private IPrincipal GetPrincipal()
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null)
                return HttpContext.Current.User;
            else if (System.Threading.Thread.CurrentPrincipal != null)
                return System.Threading.Thread.CurrentPrincipal;
            
            return null;
        }

        public User User
        {
            get
            {
                return GetUser();
            }
        }

        public bool IsDataAdmin
        {
            get
            {
                if (GetPrincipal() == null || !GetPrincipal().Identity.IsAuthenticated)
                    return false;

                using (IObjectRepository tdb = DBContext.Create())
                {
                    User user = GetUser(tdb);
                    return user.Roles.Count(y => y.Role.IsAdmin == true) > 0;
                }
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                if (GetPrincipal() == null)
                    return false;

                return GetPrincipal().Identity.IsAuthenticated;
            }
        }

        public string UserName
        {
            get
            {
                if (!IsAuthenticated)
                    return string.Empty;

                return GetPrincipal().Identity.Name;
            }
        }

        public string HostAddress
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    return HttpContext.Current.Request.UserHostAddress;

                return string.Empty;
            }
        }

        public string UserFullName
        {
            get
            {
                if (!IsAuthenticated)
                    return string.Empty;

                User user = this.User;

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.FirstName))
                        return string.Format("{0} {1}", user.FirstName, user.LastName);

                    return user.UserName;
                }

                return HttpContext.Current.User.Identity.Name;
            }
        }

        private bool IsTrustedServer
        {
            get
            {
                if (HttpContext.Current == null)
                    return false;

                var remoteAddress = HttpContext.Current.Request.Params["REMOTE_ADDR"];

                if (!string.IsNullOrEmpty(remoteAddress) && TrustedServers.Contains(remoteAddress))
                    return true;

                return false;
            }
        }

        #region Ctor

        private CheckPoint()
        {
            
        }

        ~CheckPoint()
        {

        }

        #endregion

        #region Public Template and IG Permissions

        public bool GrantViewTemplate(int templateId)
        {
            if (!IsAuthenticated || this.User == null)
            {
                if (IsTrustedServer)
                    return true;

                return false;
            }

            if (IsDataAdmin)
                return true;

            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ViewTemplatePermissions.Count(y => y.UserId == this.User.Id && y.TemplateId == templateId && y.Permission == "View") > 0;
            }
        }

        public bool GrantEditTemplate(int templateId)
        {
            if (!IsAuthenticated || this.User == null)
            {
                if (IsTrustedServer)
                    return true;

                return false;
            }

            if (IsDataAdmin)
                return true;

            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ViewTemplatePermissions.Count(y => y.UserId == this.User.Id && y.TemplateId == templateId && y.Permission == "Edit") > 0;
            }
        }

        public bool GrantViewImplementationGuide(int implementationGuideId)
        {
            if (!IsAuthenticated || this.User == null)
            {
                if (IsTrustedServer)
                    return true;

                return false;
            }

            if (IsDataAdmin)
                return true;

            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ViewImplementationGuidePermissions.Count(y => y.UserId == this.User.Id && y.ImplementationGuideId == implementationGuideId && y.Permission == "View") > 0;
            }
        }

        public bool GrantEditImplementationGuide(int implementationGuideId)
        {
            if (!IsAuthenticated || this.User == null)
            {
                if (IsTrustedServer)
                    return true;

                return false;
            }

            if (IsDataAdmin)
                return true;

            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ViewImplementationGuidePermissions.Count(y => y.UserId == this.User.Id && y.ImplementationGuideId == implementationGuideId && y.Permission == "Edit") > 0;
            }
        }

        #endregion

        #region Public Methods

        public bool HasSecurables(params string[] aSecurableNames)
        {
            if (!IsAuthenticated)
                return false;

            return HasSecurable(this.User, aSecurableNames);
        }

        public User GetUser()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetUser(tdb);
            }
        }

        /// <summary>
        /// Overloaded so that the returned User object can be from the same context that a reference is being saved to
        /// </summary>
        public User GetUser(IObjectRepository repo)
        {
            if (!IsAuthenticated)
                return null;

            string userName = GetPrincipal().Identity.Name.ToLower();
            return repo.Users.SingleOrDefault(y => y.UserName.ToLower() == userName);
        }

        public AuthorizationTypes Authorize(string aUserName, params string[] aSecurableNames)
        {
            if (!IsAuthenticated || this.User == null)
                return AuthorizationTypes.UserNonExistant;

            if (!HasSecurable(this.User, aSecurableNames))
                return AuthorizationTypes.UserNotAuthorized;

            return AuthorizationTypes.AuthorizationSuccessful;
        }

        public void CreateUserWithDefaultPermissions(string aUserName, string aFirstName, string aLastName, string aEmail, string aPhone)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                if (tdb.Users.Count(y => y.UserName.ToLower() == aUserName.ToLower()) > 0)
                    throw new Exception("A user with that username already exists in the organization.");

                User newUser = new User()
                {
                    UserName = aUserName,
                    FirstName = aFirstName,
                    LastName = aLastName,
                    Email = aEmail,
                    Phone = aPhone
                };

                // Find the default role for new users.
                Role defaultRole = tdb.Roles.SingleOrDefault(y => y.IsDefault == true);

                if (defaultRole != null)
                {
                    UserRole newUserRole = new UserRole()
                    {
                        User = newUser,
                        Role = defaultRole
                    };

                    newUser.Roles.Add(newUserRole);
                }

                tdb.Users.Add(newUser);

                tdb.SaveChanges();
            }
        }

        public Dictionary<string, string> GetAuthenticatedData()
        {
            TrifoliaApiIdentity apiIdentity = GetPrincipal().Identity as TrifoliaApiIdentity;
            HttpCookie authCookie = HttpContext.Current != null ? HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName] : null;
            Dictionary<string, string> userData = new Dictionary<string, string>();

            if (apiIdentity != null)
            {
                // TODO?
            }
            else if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                string[] userDataSplit = authTicket.UserData.Split(';');

                foreach (string cUserData in userDataSplit)
                {
                    string[] nameValuePair = cUserData.Split('=');

                    if (nameValuePair.Length != 2)
                        continue;

                    userData.Add(nameValuePair[0], nameValuePair[1]);
                }
            }

            return userData;
        }

        #endregion

        private bool HasSecurable(User user, params string[] aSecurableNames)
        {
            if (user == null)
                return false;

            if (aSecurableNames == null || aSecurableNames.Length == 0)
                return true;

            using (IObjectRepository tdb = DBContext.Create())
            {
                var securables = (from v in tdb.ViewUserSecurables
                                  join s in aSecurableNames on v.SecurableName.ToLower() equals s.ToLower()
                                  where v.UserId == user.Id
                                  select v);

                if (securables.Count() == 0)
                    return false;

                return true;
            }
        }
    }
}