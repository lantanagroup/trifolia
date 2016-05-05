using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Shared;
using Trifolia.Web.Models.Shared;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Web.Controllers
{
    public class IGTypeManagementController : Controller
    {
        private IObjectRepository tdb;

        #region Constructors

        public IGTypeManagementController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public IGTypeManagementController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        #region Types

        [HttpGet, Securable(SecurableNames.ADMIN)]
        public ActionResult List()
        {
            return View("Types");
        }

        [HttpGet, Securable(SecurableNames.ADMIN)]
        public ActionResult Edit(int? implementationGuideTypeId)
        {
            return View("EditType", implementationGuideTypeId);
        }

        #endregion
    }
}
