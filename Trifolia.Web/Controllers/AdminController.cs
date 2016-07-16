using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Web.Models.RoleManagement;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Web.Controllers
{
    [Securable(SecurableNames.ADMIN)]
    public class AdminController : Controller
    {
        #region Constructors

        public AdminController()
        {
        }

        #endregion

        /// <summary>
        /// Prepares the model and returns the main Roles view
        /// </summary>
        public ActionResult Role()
        {
            return View("Roles");
        }

        /// <summary>
        /// Prepares the model and returns the main Roles view
        /// </summary>
        public ActionResult Group()
        {
            return View("Groups");
        }

        /// <summary>
        /// Prepares the model and returns the main Roles view
        /// </summary>
        public ActionResult User()
        {
            return View("Users");
        }

        /// <summary>
        /// Prepares the model and returns the main Roles view
        /// </summary>
        public ActionResult Organization()
        {
            return View("Organizations");
        }
    }
}
