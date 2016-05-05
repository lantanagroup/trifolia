using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trifolia.Shared;
using Trifolia.Authorization;
using Trifolia.Web.Models.OrganizationManagement;

namespace Trifolia.Web.Controllers
{
    [Securable(SecurableNames.ORGANIZATION_DETAILS)]
    public class OrganizationController : Controller
    {
        DB.IObjectRepository tdb = null;

        #region Constructors

        public OrganizationController()
            : this(new DB.TemplateDatabaseDataSource())
        {

        }

        public OrganizationController(DB.IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        #region Action Methods

        [Securable(SecurableNames.ORGANIZATION_DETAILS)]
        public ActionResult Details(int? id)
        {
            return View("Details", id);
        }

        [Securable(SecurableNames.ORGANIZATION_LIST)]
        public ActionResult List()
        {
            return View("List");
        }

        [Securable(SecurableNames.ORGANIZATION_DETAILS)]
        public ActionResult My()
        {
            int? lOrganizationId = null;
            string lUserName = this.User.Identity.Name;
            DB.User lCurrentUser = CheckPoint.Instance.GetUser(tdb);

            if (lCurrentUser == null)
            {
                throw new KeyNotFoundException(string.Format("{0} was not found in the Trifolia database", lUserName));
            }

            lOrganizationId = lCurrentUser.OrganizationId;

            return RedirectToAction("Details", new { id = lOrganizationId.Value });
        }

        #endregion
    }
}