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
    public class RoleManagementController : Controller
    {
        private IObjectRepository tdb;

        #region Constructors

        public RoleManagementController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public RoleManagementController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        /// <summary>
        /// Prepares the model and returns the main Roles view
        /// </summary>
        public ActionResult Roles()
        {
            return View("Roles");
        }
    }
}
