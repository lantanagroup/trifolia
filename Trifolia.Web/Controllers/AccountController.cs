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

namespace Trifolia.Web.Controllers
{
    public class AccountController : Controller
    {
        private IObjectRepository tdb;

        #region Constructors 

        public AccountController()
        {
            this.tdb = new TemplateDatabaseDataSource();
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
            using (DB.TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
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
            using (DB.TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                DB.User lUser = CheckPoint.Instance.GetUser(tdb);

                UserProfile lProfile = new UserProfile()
                {
                    userName = lUser.UserName,
                    accountOrganization = lUser.Organization.Name,

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
        public ActionResult NewProfile(string RedirectUrl)
        {
            NewProfileModel model = new NewProfileModel()
            {
                RedirectUrl = RedirectUrl
            };

            return View("NewProfile", model);
        }

        [HttpPost]
        [Securable()]           // Must be logged in, but no specific securables
        public ActionResult CompleteProfile(NewProfileModel model)
        {
            string userName = CheckPoint.Instance.UserName;
            string organizationName = CheckPoint.Instance.OrganizationName;

            CheckPoint.Instance.CreateUserWithDefaultPermissions(userName, organizationName, model.FirstName, model.LastName, model.Email, model.Phone);

            Dictionary<string, string> authData = CheckPoint.Instance.GetAuthenticatedData();

            if (organizationName == "HL7" && authData.ContainsKey(CheckPoint.AUTH_DATA_ROLES))
                CheckPoint.Instance.CheckHL7Roles(userName, authData[CheckPoint.AUTH_DATA_ROLES]);

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

        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl = null)
        {
            if (CheckPoint.Instance.IsAuthenticated)
                return RedirectToAction("LoggedInIndex", "Home");

            LoginModel model = GetLoginModel(returnUrl: ReturnUrl);

            return View("Login", model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult DoLogin(LoginModel model)
        {
            Organization org = this.tdb.Organizations.Single(y => y.Id == model.OrganizationId);

            // Run the re-captcha checks unless we allow re-captcha to be bypassed or the client has not specified debug mode
            if (!Properties.Settings.Default.RecaptchaAllowBypass || !this.Request.Params.ToString().Split('&').Contains("debug"))
            {
                // Check that a captcha was entered
                if (string.IsNullOrEmpty(this.Request.Form[Properties.Settings.Default.RecaptchaFormFieldName]))
                {
                    LoginModel newModel = GetLoginModel(model, App_GlobalResources.TrifoliaLang.RecaptchaNotSpecified);
                    AuditEntryExtension.SaveAuditEntry("Login", "Failed - No re-captcha response was specified", model.Username, org.Name);
                    return View("Login", newModel);
                }

                // Check that the response of the captcha was valid with google
                using (WebClient client = new WebClient())
                {
                    System.Collections.Specialized.NameValueCollection verifyParms = new System.Collections.Specialized.NameValueCollection();
                    verifyParms.Add("secret", Properties.Settings.Default.RecaptchaSecret);
                    verifyParms.Add("response", this.Request.Form[Properties.Settings.Default.RecaptchaFormFieldName]);

                    byte[] responsebytes = client.UploadValues(Properties.Settings.Default.RecaptchaVerifyUrl, Properties.Settings.Default.RecaptchaVerifyMethod, verifyParms);
                    string responsebody = System.Text.Encoding.UTF8.GetString(responsebytes);

                    dynamic verifyResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responsebody);

                    if (verifyResponse.success != true)
                    {
                        LoginModel newModel = GetLoginModel(model, App_GlobalResources.TrifoliaLang.RecaptchaInvalid);
                        AuditEntryExtension.SaveAuditEntry("Login", "Failed - The re-captcha response specified is not valid: " + responsebody, model.Username, org.Name);
                        return View("Login", newModel);
                    }
                }
            }

            if (CheckPoint.Instance.ValidateUser(model.Username, org.Name, model.Password))
            {
                Response.Cookies.Clear();

                string userData = string.Format("{0}={1}", CheckPoint.AUTH_DATA_ORGANIZATION, org.Name);
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                    2, 
                    model.Username, 
                    DateTime.Now, 
                    DateTime.Now.AddDays(20), 
                    model.RememberMe, 
                    userData);
                string encAuthTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encAuthTicket);

                if (model.RememberMe)
                    faCookie.Expires = DateTime.Now.AddDays(20);

                Response.Cookies.Set(faCookie);

                // Audit the login
                AuditEntryExtension.SaveAuditEntry("Login", "Success", model.Username, org.Name);

                if (!string.IsNullOrEmpty(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("LoggedInIndex", "Home");
            }
            else
            {
                LoginModel newModel = GetLoginModel(
                    model.ReturnUrl,
                    model.Username,
                    model.OrganizationId,
                    App_GlobalResources.TrifoliaLang.AuthenticationInvalid,
                    model.RememberMe);

                // Audit the failed login
                AuditEntryExtension.SaveAuditEntry("Login", "Failed", model.Username, org.Name);

                return View("Login", newModel);
            }
        }

        [AllowAnonymous]
        public ActionResult HL7LoginRedirect()
        {
            string redirectUrl = string.Format("{0}://{1}/Account/DoHL7Login",
                Request.Url.Scheme,
                Request.Url.Authority);

            string url = string.Format(Properties.Settings.Default.HL7LoginUrlFormat,
                HL7AuthHelper.API_KEY,
                redirectUrl);

            return Redirect(url);
        }

        [AllowAnonymous]
        public ActionResult DoHL7Login(HL7LoginModel model)
        {
            string validateRequestHashFormat = string.Format("{0}|{1}|{2}", model.userid, model.timestampUTCEpoch, HL7AuthHelper.API_KEY);
            string validateRequestHash = HL7AuthHelper.GetEncrypted(validateRequestHashFormat, HL7AuthHelper.SHARED_KEY);

            // The hash does not match what we expect, this is an invalid request
            if (validateRequestHash != model.requestHash)
            {
                Log.For(this).Error("Invalid attempt to login as HL7 user with user ID {0} and request hash '{1}'", model.userid, model.requestHash);
                return Redirect("/?Message=" + App_GlobalResources.TrifoliaLang.HL7AttemptInvalid);
            }

            try
            {
                // Verify that the request sent from HL7 took less than 5 minutes
                if (!HL7AuthHelper.ValidateTimestamp(model.timestampUTCEpoch))
                {
                    Log.For(this).Warn("Request to login took longer than 5 minutes to reach the server.");
                    return Redirect("/?Message=" + App_GlobalResources.TrifoliaLang.HL7AuthTimeout);
                }
            }
            catch
            {
                Log.For(this).Error("Timestamp passed in request to HL7 login is not a valid timestamp: {0}", model.timestampUTCEpoch);
                return Redirect("/?Message=An error occurred while logging in.");
            }

            string userData = string.Format("{0}=HL7;{1}={2};{3}={4}", CheckPoint.AUTH_DATA_ORGANIZATION, CheckPoint.AUTH_DATA_USERID, model.userid, CheckPoint.AUTH_DATA_ROLES, model.roles);
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, model.userid, DateTime.Now, DateTime.Now.AddDays(20), true, userData);
            string encAuthTicket = FormsAuthentication.Encrypt(authTicket);

            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encAuthTicket);
            faCookie.Expires = DateTime.Now.AddDays(20);

            if (Response.Cookies[FormsAuthentication.FormsCookieName] != null)
                Response.Cookies.Set(faCookie);
            else
                Response.Cookies.Add(faCookie);

            CheckPoint.Instance.CheckHL7Roles(model.userid, model.roles);

            // Audit the login
            AuditEntryExtension.SaveAuditEntry("Login", "Success", model.userid, "HL7");

            // Either return the user to the specified url, or to the default homepage if none is specified
            return Redirect(!string.IsNullOrEmpty(model.ReturnUrl) ? model.ReturnUrl : "/");
        }

        private LoginModel GetLoginModel(LoginModel defaults, string message)
        {
            LoginModel newModel = GetLoginModel(
                defaults.ReturnUrl,
                defaults.Username,
                defaults.OrganizationId,
                message,
                defaults.RememberMe);

            return newModel;
        }

        private LoginModel GetLoginModel(string returnUrl = null, string username = null, int? organizationId = null, string message = null, bool rememberMe = false)
        {
            LoginModel model = new LoginModel()
            {
                ReturnUrl = returnUrl,
                Username = username,
                OrganizationId = organizationId,
                Message = message,
                RememberMe = rememberMe,
                RecaptchaAllowBypass = Properties.Settings.Default.RecaptchaAllowBypass
            };

            if (returnUrl == null)
            {
                returnUrl = string.Format("{0}://{1}/Account/DoHL7Login",
                    Request.Url.Scheme,
                    Request.Url.Authority);
            }
            else if (!returnUrl.Contains("://"))
            {
                if (!returnUrl.StartsWith("/"))
                    returnUrl = "/" + returnUrl;

                returnUrl = string.Format("{0}://{1}{2}",
                    Request.Url.Scheme,
                    Request.Url.Authority,
                    returnUrl);
            }

            // Determine the HL7 login link
            model.HL7LoginLink = string.Format(Properties.Settings.Default.HL7LoginUrlFormat,
                HL7AuthHelper.API_KEY,
                returnUrl);

            // Bypass the HL7 organization, since the app has a separate page just for logging in as HL7
            foreach (var cOrganization in this.tdb.Organizations.Where(y => y.Name != "HL7").OrderBy(y => y.Name))
            {
                model.Organizations.Add(cOrganization.Id, cOrganization.Name);
            }

            if (organizationId == null && model.Organizations.Count > 0)
                model.OrganizationId = model.Organizations.Keys.First();

            return model;
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
