using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using Trifolia.Authentication;
using Trifolia.Shared;
using Trifolia.Authorization;

namespace Trifolia.Web
{
    public class SecurableForm : Page
    {
        protected override void OnPreLoad(EventArgs e)
        {
            SecurableAttribute securableAttribute = this.GetType().GetCustomAttributes(true).SingleOrDefault(y => y.GetType() == typeof(SecurableAttribute)) as SecurableAttribute;

            if (securableAttribute != null && securableAttribute.SecurableNames != null && !UserHasSecurable(securableAttribute.SecurableNames))
            {
                RedirectToLogin();
                return;
            }

            base.OnPreLoad(e);
        }

        /// <summary>
        /// Overloaded version of UserHasSecurable(string[]) for checking a single securable
        /// </summary>
        public bool UserHasSecurable(string securable)
        {
            if (string.IsNullOrEmpty(securable))
                return true;

            return UserHasSecurable(new string[] { securable });
        }

        /// <summary>
        /// Determines if the authenticated user has a role associated with the named securable.
        /// </summary>
        /// <param name="securables">The name(s) of the securable to check.</param>
        /// <returns>Returns false if a user is not logged in. Return true if the authenticated user has a role associated with the requested securable, otherwise returns false.</returns>
        public bool UserHasSecurable(string[] securables)
        {
            if (securables.Count(y => string.IsNullOrEmpty(y)) > 0)
                return true;

            if (!Page.User.Identity.IsAuthenticated)
                return false;

            string userName = Page.User.Identity.Name;
            string organizationName = CheckPoint.Instance.OrganizationName;

            if (CheckPoint.Instance.Authorize(userName, organizationName, securables) == AuthorizationTypes.AuthorizationSuccessful)
                return true;

            return false;
        }

        private void RedirectToLogin()
        {
            string url = string.Format("/Account/Login?ReturnUrl={0}",
                Request.Url.AbsolutePath);

            Response.Redirect(url);
        }
    }
}