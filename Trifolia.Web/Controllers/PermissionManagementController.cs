using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Authentication;
using Trifolia.Shared;
using Trifolia.Web.Models.PermissionManagement;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Web.Controllers
{
    public class PermissionManagementController : Controller
    {
        private IObjectRepository tdb;

        #region Constructors

        public PermissionManagementController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public PermissionManagementController()
            : this(new TemplateDatabaseDataSource())
        { }

        #endregion

        #region Javascript Permissions

        [HttpPost]
        [AllowAnonymous]
        public ActionResult HasTemplateEditPermission(int templateId)
        {
            if (!CheckPoint.Instance.IsAuthenticated)
                return Json(false);

            if (CheckPoint.Instance.IsDataAdmin)
                return Json(true);

            bool canEditTemplate = CheckPoint.Instance.GrantEditTemplate(templateId);

            return Json(canEditTemplate);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult HasTemplateViewPermission(int templateId)
        {
            if (!CheckPoint.Instance.IsAuthenticated)
                return Json(false);

            if (CheckPoint.Instance.IsDataAdmin)
                return Json(true);

            bool canViewTemplate = CheckPoint.Instance.GrantViewTemplate(templateId);

            return Json(canViewTemplate);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult HasImplementationGuideEditPermission(int implementationGuideId)
        {
            if (!CheckPoint.Instance.IsAuthenticated)
                return Json(false);

            if (CheckPoint.Instance.IsDataAdmin)
                return Json(true);

            bool canEditImplementationGuide = CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId);

            return Json(canEditImplementationGuide);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult HasImplementationGuideViewPermission(int implementationGuideId)
        {
            if (!CheckPoint.Instance.IsAuthenticated)
                return Json(false);

            if (CheckPoint.Instance.IsDataAdmin)
                return Json(true);

            bool canViewImplementationGuide = CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId);

            return Json(canViewImplementationGuide);
        }

        #endregion
    }
}
