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
        public const string AUTH_DATA_ROLES = "Roles";
        public const string AUTH_DATA_USERID = "UserId";
        public const string AUTH_DATA_ORGANIZATION = "Organization";
        public const string HL7_ROLE_IS_MEMBER = "ismember";
        public const string HL7_ROLE_IS_COCHAIR = "iscochair";
        public const string HL7_ROLE_IS_STAFF = "isstaff";
        public const string HL7_ROLE_IS_BOARD = "isboardmember";

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

        public string OrganizationName
        {
            get
            {
                if (!IsAuthenticated)
                    return string.Empty;

                Dictionary<string, string> authData = GetAuthenticatedData();

                if (authData != null && authData.ContainsKey(AUTH_DATA_ORGANIZATION))
                    return authData[AUTH_DATA_ORGANIZATION];

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

                if (!string.IsNullOrEmpty(remoteAddress) && Properties.Settings.Default.TrustedServers.Contains(remoteAddress))
                    return true;

                return false;
            }
        }

        public bool IsTrustedSharedSecret
        {
            get
            {
                if (HttpContext.Current == null)
                    return false;

                var authorizationHeader = HttpContext.Current.Request.Headers["Authorization"];

                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
                {
                    var authorizationDataBytes = System.Convert.FromBase64String(authorizationHeader.Substring(6));
                    var authorizationData = System.Text.Encoding.UTF8.GetString(authorizationDataBytes);
                    var authorizationSplit = authorizationData.Split('|');

                    if (authorizationSplit.Length == 3)
                    {
                        var timestampString = authorizationSplit[0];
                        long timestamp;
                        var salt = authorizationSplit[1];
                        var requestHashBytes = System.Convert.FromBase64String(authorizationSplit[2]);
                        var requestHash = System.Text.Encoding.UTF8.GetString(requestHashBytes);

                        long.TryParse(timestampString, out timestamp);

                        var timestampDate = new DateTime(1970, 1, 1).AddMilliseconds(timestamp);

                        if (timestampDate > DateTime.UtcNow.AddMinutes(5) || timestampDate < DateTime.UtcNow.AddMinutes(-5))
                            return false;

                        var cryptoProvider = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                        var actualHashData = timestamp + "|" + salt + "|" + Properties.Settings.Default.SharedSecret;
                        var actualHashDataBytes = System.Text.Encoding.UTF8.GetBytes(actualHashData);
                        var actualHashBytes = cryptoProvider.ComputeHash(actualHashDataBytes);
                        var actualHash = System.Text.Encoding.UTF8.GetString(actualHashBytes);

                        if (actualHash == requestHash)
                            return true;
                    }
                }

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
                if (IsTrustedServer || IsTrustedSharedSecret)
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
                if (IsTrustedServer || IsTrustedSharedSecret)
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
                if (IsTrustedServer || IsTrustedSharedSecret)
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
                if (IsTrustedServer || IsTrustedSharedSecret)
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

        public bool ValidateUser(string username, string organization, string password)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                Organization org = tdb.Organizations.SingleOrDefault(y => y.Name.ToLower() == organization.ToLower());

                if (org == null)
                    return false;

                if (string.IsNullOrEmpty(org.AuthProvider))
                    return Membership.ValidateUser(username, password);

                return Membership.Providers[org.AuthProvider].ValidateUser(username, password);
            }
        }

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
            string orgName = OrganizationName.ToLower();

            return repo.Users.SingleOrDefault(y => y.UserName.ToLower() == userName && y.Organization.Name.ToLower() == orgName);
        }

        public AuthorizationTypes Authorize(string aUserName, string aOrganizationName, params string[] aSecurableNames)
        {
            if (!IsAuthenticated || this.User == null)
                return AuthorizationTypes.UserNonExistant;

            // When a user is an HL7 user, check that they have the appropriate roles associated
            if (aOrganizationName == Properties.Settings.Default.HL7OrganizationName)
            {
                Dictionary<string, string> authData = GetAuthenticatedData();

                if (authData.ContainsKey(AUTH_DATA_ROLES))
                    this.CheckHL7Roles(aUserName, authData[AUTH_DATA_ROLES]);
            }

            if (!HasSecurable(this.User, aSecurableNames))
                return AuthorizationTypes.UserNotAuthorized;

            return AuthorizationTypes.AuthorizationSuccessful;
        }

        public void CreateUserWithDefaultPermissions(string aUserName, string aOrganizationName, string aFirstName, string aLastName, string aEmail, string aPhone)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                Organization org = tdb.Organizations.Single(y => y.Name == aOrganizationName);

                if (tdb.Users.Count(y => y.UserName.ToLower() == aUserName.ToLower() && y.OrganizationId == org.Id) > 0)
                    throw new Exception("A user with that username already exists in the organization.");

                User newUser = new User()
                {
                    UserName = aUserName,
                    Organization = org,
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

                tdb.Users.AddObject(newUser);

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
                userData.Add("Organization", apiIdentity.OrganizationName);
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

        #region Private Methods

        public void CheckHL7Roles(string userName, string hl7Roles)
        {
            Log.For(this).Debug("Entering CheckHL7Roles");

            using (IObjectRepository tdb = DBContext.Create())
            {
                User user = tdb.Users.SingleOrDefault(y => y.UserName == userName && y.Organization.Name == Properties.Settings.Default.HL7OrganizationName);

                if (user == null || string.IsNullOrEmpty(hl7Roles))
                {
                    Log.For(this).Info("User not found with username \"{0}\" and organization \"{1}\"", userName, Properties.Settings.Default.HL7OrganizationName);
                    return;
                }

                string[] hl7RolesSplit = hl7Roles.Split(',');

                Log.For(this).Debug("Matching Trifolia roles to HL7 roles ({0})", hl7Roles);

                foreach (string cHl7Role in hl7RolesSplit)
                {
                    switch (cHl7Role)
                    {
                        case HL7_ROLE_IS_MEMBER:
                            this.EnsureUserHasRoles(tdb, user, Properties.Settings.Default.HL7MemberRole);
                            break;
                        case HL7_ROLE_IS_BOARD:
                            this.EnsureUserHasRoles(tdb, user, Properties.Settings.Default.HL7BoardRole);
                            break;
                        case HL7_ROLE_IS_COCHAIR:
                            this.EnsureUserHasRoles(tdb, user, Properties.Settings.Default.HL7CoChairRole);
                            break;
                        case HL7_ROLE_IS_STAFF:
                            this.EnsureUserHasRoles(tdb, user, Properties.Settings.Default.HL7StaffRole);
                            break;
                        default:
                            Log.For(this).Debug("Did not find matching role for flag sent from HL7: {0}", cHl7Role);
                            break;
                    }
                }
            }
        }

        private void EnsureUserHasRoles(IObjectRepository tdb, User user, string roleName)
        {
            Log.For(this).Debug("Entering EnsureUserHasRole");

            if (string.IsNullOrEmpty(roleName))
            {
                Log.For(this).Info("Empty roleName specified");
                return;
            }

            Role foundRole = tdb.Roles.SingleOrDefault(y => y.Name.ToLower() == roleName.ToLower());

            if (foundRole == null)
            {
                Log.For(this).Warn("No role found with name \"{0}\"", roleName);
                return;
            }

            bool userHasRole = user.Roles.Count(y => y.Role == foundRole) > 0;
            bool restrictedRole = foundRole.Restrictions.Count(y => y.OrganizationId == user.Organization.Id) > 0;

            Log.For(this).Debug("User has role: {0}\nRole is restricted: {1}", userHasRole, restrictedRole);

            if (!restrictedRole && !userHasRole)
            {
                Log.For(this).Debug("Assigning role {0} to user {1} ({2})", foundRole.Name, user.UserName, user.Organization.Name);

                tdb.UserRoles.AddObject(new UserRole()
                {
                    User = user,
                    Role = foundRole
                });
                tdb.SaveChanges();
            }
        }

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

        #endregion
    }
}