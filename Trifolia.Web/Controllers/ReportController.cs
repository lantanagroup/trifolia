using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Authorization;

namespace Trifolia.Web.Controllers
{
    public class ReportController : Controller
    {
        [Securable(SecurableNames.ADMIN)]
        public ActionResult Organizations()
        {
            return View("Organizations");
        }

        [Securable(SecurableNames.REPORT_TEMPLATE_COMPLIANCE)]
        public ActionResult TemplateValidation()
        {
            return View("TemplateValidation");
        }

        [Securable(SecurableNames.REPORT_TEMPLATE_REVIEW)]
        public ActionResult TemplateReview()
        {
            return View("TemplateReview");
        }
    }
}
