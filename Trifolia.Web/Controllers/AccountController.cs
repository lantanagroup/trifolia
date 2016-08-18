using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net;

using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;

using Trifolia.Authentication;
using Trifolia.Shared;
using Trifolia.Logging;
using Trifolia.Authorization;
using Trifolia.Web.Filters;
using Trifolia.Web.Models.Account;
using Trifolia.DB;
using Trifolia.Config;

using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.Messaging;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Trifolia.Authentication.Models;

namespace Trifolia.Web.Controllers
{
    public class AccountController : Controller
    {
        private IObjectRepository tdb;
        private WebServerClient authClient;

        #region Constructors 

        public AccountController()
            : this(DBContext.Create())
        {
            this.authClient = CreateClient();
        }

        public AccountController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        #region Profile

        [Securable()]
        [HttpPost]
        public ActionResult SaveProfile(UserProfile aProfile)
        {
            using (DB.IObjectRepository tdb = DBContext.Create())
            {
                DB.User lUser = CheckPoint.Instance.GetUser(tdb);

                lUser.Use(u =>
                {
                    u.FirstName = aProfile.firstName;
                    u.LastName = aProfile.lastName;
                    u.Phone = aProfile.phone;
                    u.Email = aProfile.email;
                    u.OkayToContact = aProfile.okayToContact;
                    u.ExternalOrganizationName = aProfile.organization;
                    u.ExternalOrganizationType = aProfile.organizationType;
                    u.ApiKey = aProfile.apiKey;
                });

                tdb.SaveChanges();

                return ProfileData();
            }
        }

        [Securable()]
        public ActionResult MyProfile()
        {
            return View();
        }

        [Securable()]
        public JsonResult ProfileData()
        {
            using (DB.IObjectRepository tdb = DBContext.Create())
            {
                DB.User lUser = CheckPoint.Instance.GetUser(tdb);

                UserProfile lProfile = new UserProfile()
                {
                    userName = lUser.UserName,

                    firstName = lUser.FirstName,
                    lastName = lUser.LastName,
                    phone = lUser.Phone,
                    email = lUser.Email,
                    okayToContact = lUser.OkayToContact.HasValue ? lUser.OkayToContact.Value : false,
                    organization = lUser.ExternalOrganizationName,
                    organizationType = lUser.ExternalOrganizationType,
                    apiKey = lUser.ApiKey
                };

                return Json(lProfile, JsonRequestBehavior.AllowGet);
            }
        }

        [Securable()]           // Must be logged in, but no specific securables
        public ActionResult NewProfile(string RedirectUrl, string firstName = null, string lastName = null, string email = null, string phone = null)
        {
            NewProfileModel model = new NewProfileModel()
            {
                RedirectUrl = RedirectUrl,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone
            };

            return View("NewProfile", model);
        }

        [HttpPost]
        [Securable()]           // Must be logged in, but no specific securables
        public ActionResult CompleteProfile(NewProfileModel model)
        {
            string userName = CheckPoint.Instance.UserName;

            CheckPoint.Instance.CreateUserWithDefaultPermissions(userName, model.FirstName, model.LastName, model.Email, model.Phone);

            Dictionary<string, string> authData = CheckPoint.Instance.GetAuthenticatedData();

            // Update the other fields in the user's profile
            User user = CheckPoint.Instance.GetUser(this.tdb);

            user.OkayToContact = model.OkayToContact;
            user.ExternalOrganizationName = model.Organization;
            user.ExternalOrganizationType = model.OrganizationType;

            this.tdb.SaveChanges();

            if (!string.IsNullOrEmpty(model.RedirectUrl))
                return Json(model.RedirectUrl);
            else
                return Json(Url.Action("LoggedInIndex", "Home"));
        }

        #endregion

        public static WebServerClient CreateClient()
        {
            if (string.IsNullOrEmpty(AppSettings.OAuth2ClientIdentifier))
            {
                Log.For(typeof(AccountController)).Error("OAuth2ClientIdentifier is not specified in configuration!");
                throw new Exception("Authentication is incorrectly configured");
            }

            if (string.IsNullOrEmpty(AppSettings.OAuth2ClientSecret))
            {
                Log.For(typeof(AccountController)).Error("OAuth2ClientSecret is not specified in configuration!");
                throw new Exception("Authentication is incorrectly configured");
            }

            var desc = GetAuthServerDescription();
            var client = new WebServerClient(desc, clientIdentifier: AppSettings.OAuth2ClientIdentifier);
            client.ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(AppSettings.OAuth2ClientSecret);
            return client;
        }

        public static AuthorizationServerDescription GetAuthServerDescription()
        {
            if (string.IsNullOrEmpty(AppSettings.OAuth2AuthorizationEndpoint))
            {
                Log.For(typeof(AccountController)).Error("OAuth2AuthorizationEndpoint is not specified in configuration!");
                throw new Exception("Authentication is incorrectly configured");
            }

            if (string.IsNullOrEmpty(AppSettings.OAuth2TokenEndpoint))
            {
                Log.For(typeof(AccountController)).Error("OAuth2TokenEndpoint is not specified in configuration!");
                throw new Exception("Authentication is incorrectly configured");
            }

            var authServerDescription = new AuthorizationServerDescription();
            authServerDescription.AuthorizationEndpoint = new Uri(AppSettings.OAuth2AuthorizationEndpoint);
            authServerDescription.TokenEndpoint = new Uri(AppSettings.OAuth2TokenEndpoint);
            authServerDescription.ProtocolVersion = ProtocolVersion.V20;
            return authServerDescription;
        }

        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl = null)
        {
            if (string.IsNullOrEmpty(Request.QueryString["code"]))
            {
                return InitAuth();
            }
            else
            {
                return OAuthCallback();
            }
        }

        private ActionResult InitAuth()
        {
            var state = new AuthorizationState();
            var uri = Request.Url.AbsoluteUri;
            uri = RemoveQueryStringFromUri(uri);
            state.Callback = new Uri(uri);
            state.Scope.Add("openid");

            var r = this.authClient.PrepareRequestUserAuthorization(state);
            return r.AsActionResult();
        }

        private static string RemoveQueryStringFromUri(string uri)
        {
            int index = uri.IndexOf('?');
            if (index > -1)
            {
                uri = uri.Substring(0, index);
            }
            return uri;
        }

        private void RemoveQueryStringParam(string paramName)
        {
            // reflect to readonly property
            PropertyInfo isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            // make collection editable
            isreadonly.SetValue(this.Request.QueryString, false, null);

            // remove
            this.Request.QueryString.Remove(paramName);

            // make collection readonly again
            isreadonly.SetValue(this.Request.QueryString, true, null);
        }

        private ActionResult OAuthCallback()
        {
            if (!string.IsNullOrEmpty(this.Request.QueryString["session_state"]))
            {
                var uri = new Uri(this.Request.Url.ToString());

                // this gets all the query string key value pairs as a collection
                var newQueryString = HttpUtility.ParseQueryString(uri.Query);

                // this removes the key if exists
                newQueryString.Remove("session_state");

                // this gets the page path from root without QueryString
                string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

                var newUrl = newQueryString.Count > 0
                    ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                    : pagePathWithoutQueryString;

                return Redirect(newUrl);
            }

            var auth = this.authClient.ProcessUserAuthorization(this.Request);

            WebClient userInfoClient = new WebClient();
            userInfoClient.Headers.Add("Authorization", "Bearer " + auth.AccessToken);
            var userInfoStream = userInfoClient.OpenRead(AppSettings.OAuth2UserInfoEndpoint);

            using (StreamReader sr = new StreamReader(userInfoStream))
            {
                var userInfoString = sr.ReadToEnd();
                var userInfo = JsonConvert.DeserializeObject<OAuth2UserInfo>(userInfoString);

                if (string.IsNullOrEmpty(userInfo.user_id))
                    throw new Exception("UserInfo returned no user_id information");

                var foundUser = this.tdb.Users.SingleOrDefault(y => y.UserName == userInfo.user_id);

                // If the user has migration information (the account that they are moving from) in their
                // profile, update trifolia so that it has the same userName/user_id.
                if (foundUser == null && userInfo.app_metadata != null && userInfo.app_metadata.migrated_account != null)
                {
                    var migratingUser = this.tdb.Users.SingleOrDefault(y =>
                        y.Id == userInfo.app_metadata.migrated_account.internalId &&
                        y.UserName == userInfo.app_metadata.migrated_account.userName);

                    if (migratingUser != null)
                    {
                        migratingUser.UserName = userInfo.user_id;
                        foundUser = migratingUser;
                    }

                    this.tdb.SaveChanges();
                }

                string userData = string.Format("{0}={1}", CheckPoint.AUTH_DATA_OAUTH2_TOKEN, auth.AccessToken);
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                    2,
                    userInfo.user_id,
                    DateTime.Now,
                    DateTime.Now.AddDays(20),
                    true,
                    userData);
                string encAuthTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encAuthTicket);

                if (auth.AccessTokenExpirationUtc != null)
                    faCookie.Expires = auth.AccessTokenExpirationUtc.Value;

                Response.Cookies.Set(faCookie);

                if (foundUser == null)
                    return NewProfile("/", userInfo.given_name, userInfo.family_name, userInfo.email, userInfo.phone);
            }

            return Redirect("/");
        }

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
