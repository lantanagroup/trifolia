using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Authentication;
using Trifolia.Authorization;
using Trifolia.Web.Controllers;
using Trifolia.DB;

namespace Trifolia.Web.Filters
{
    public class SecurableAuthorizationFilter : IAuthorizationFilter, IFilterProvider
    {
        private const string LOGIN_URL_FORMAT = "/Account/Login?ReturnUrl={0}";
        private const string UNKNOWN_USERNAME = "UNKNOWN";

        #region Public Methods

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // If the user is authenticated but does not yet exist in the system, redirect them to the "NewProfile" page
            if (filterContext.HttpContext.User.Identity.IsAuthenticated &&
                CheckPoint.Instance.User == null && 
                filterContext.ActionDescriptor.ActionName != "NewProfile" &&
                filterContext.ActionDescriptor.ActionName != "CompleteProfile")
            {
                filterContext.Result = new RedirectResult("/Account/NewProfile");
                return;
            }

            // Stop if the action/controller allows anonymous access
            if (this.AllowAnonymous(filterContext))
                return;

            SecurableAttribute lSecurable = this.GetSecurable(filterContext);

            try
            {
                if (lSecurable != null && lSecurable.SecurableNames != null && lSecurable.SecurableNames.Length > 0)
                {
                    if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                        throw new AuthorizationException(UNKNOWN_USERNAME, lSecurable.SecurableNames);

                    string lUserName = filterContext.HttpContext.User.Identity.Name;

                    this.AssertAuthorized(lUserName, lSecurable.SecurableNames);
                }
            }
            catch (AuthorizationException)
            {
                string url = string.Format(LOGIN_URL_FORMAT, filterContext.HttpContext.Request.Url.PathAndQuery);
                filterContext.Result = new RedirectResult(url);
            }

            this.AuditLogin();
        }

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            List<Filter> lFilters = new List<Filter>();

            if (controllerContext.Controller.GetType().GetCustomAttributes(
                typeof(AllowAnonymousAttribute), true).Count() > 0)
            {
                return lFilters;
            }

            if (actionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Count() > 0)
            {
                return lFilters;
            }

            lFilters.Add(new Filter(this, FilterScope.Controller, null));

            return lFilters;
        }

        #endregion

        #region Private Methods

        private bool AllowAnonymous(AuthorizationContext filterContext)
        {
            object lSecurableMethodAttribute = (from a in filterContext.ActionDescriptor.GetCustomAttributes(false)
                                                where a is AllowAnonymousAttribute
                                                select a).FirstOrDefault();

            if (lSecurableMethodAttribute != null)
                return true;

            ControllerBase lController = filterContext.Controller;
            object lSecurableObject = (from a in lController.GetType().GetCustomAttributes(false)
                                       where a is AllowAnonymousAttribute
                                       select a).FirstOrDefault();

            if (lSecurableObject != null)
                return true;

            return false;
        }

        private SecurableAttribute GetSecurable(AuthorizationContext filterContext)
        {
            //Authorize by action if method applies securable attribute
            object lSecurableMethodAttribute = (from a in filterContext.ActionDescriptor.GetCustomAttributes(false)
                                                where a is SecurableAttribute
                                                select a).FirstOrDefault();

            SecurableAttribute lSecurableMethod = lSecurableMethodAttribute as SecurableAttribute;

            if (lSecurableMethod != null)
            {
                return lSecurableMethod;
            }

            ControllerBase lController = filterContext.Controller;
            object lSecurableObject = (from a in lController.GetType().GetCustomAttributes(false)
                                       where a is SecurableAttribute
                                       select a).FirstOrDefault();
            SecurableAttribute lSecurable = lSecurableObject as SecurableAttribute;

            if (lSecurable == null)
            {
                string lExceptionMessage = string.Format("{0} is missing required attribute {1} which should be applied to the controller or the action being invoked",
                    lController.GetType().Name, typeof(SecurableAttribute).Name);

                throw new NotImplementedException(lExceptionMessage);
            }

            return lSecurable;
        }

        private void AssertAuthorized(string aUserName, string[] aSecurableNames)
        {
            AuthorizationTypes lAuthType = CheckPoint.Instance.Authorize(aUserName, aSecurableNames);

            if (lAuthType == AuthorizationTypes.AuthorizationSuccessful)
            {
                return;
            }

            if (lAuthType == AuthorizationTypes.UserNotAuthorized)
            {
                throw new AuthorizationException(aUserName, aSecurableNames);
            }

            if (lAuthType == AuthorizationTypes.UserNonExistant)
            {
                // TODO: Capture the first, last, phone and email of new user
                throw new Exception("User does not exist");
            }
        }

        /// <summary>
        /// Checks if the authenticated user has a "Login" audit within the last 24 hours. If not, creates a
        /// Login audit.
        /// </summary>
        /// <remarks>
        /// When user is logged in via "remember me", Login does not have to occur.
        /// </remarks>
        private void AuditLogin()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                var user = CheckPoint.Instance.GetUser(tdb);
                DateTime minDate = DateTime.Now.AddHours(-24);

                // Determine if a login audit has been recorded in the last 24 hours
                if (tdb.AuditEntries.Count(y => y.AuditDate > minDate && y.Username == user.UserName && y.Type == "Login") == 0)
                    AuditEntryExtension.SaveAuditEntry("Login", "Success", user.UserName);
            }
        }

        #endregion
    }
}