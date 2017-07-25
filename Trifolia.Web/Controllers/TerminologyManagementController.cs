using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Shared;
using Trifolia.Web.Models.Shared;
using Trifolia.Web.Models.TerminologyManagement;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Web.Controllers
{
    public class TerminologyManagementController : Controller
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public TerminologyManagementController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public TerminologyManagementController()
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

        #region Terminology Browser

        [Securable(SecurableNames.VALUESET_LIST, SecurableNames.CODESYSTEM_LIST)]
        public ActionResult Browse()
        {
            return View("Browse");
        }

        #endregion

        #region View/Edit ValueSet

        [Securable(SecurableNames.VALUESET_EDIT)]
        public ActionResult EditValueSetConcepts(int valueSetId)
        {
            return View("EditValueSetConcepts", valueSetId);
        }

        [Securable(SecurableNames.VALUESET_LIST)]
        public ActionResult ViewValueSet(int valueSetId)
        {
            return View("View", valueSetId);
        }

        #endregion

        #region Import from Excel

        [Securable(SecurableNames.ADMIN)]
        public ActionResult ImportExcel()
        {
            return View("ImportExcel");
        }

        #endregion
    }
}