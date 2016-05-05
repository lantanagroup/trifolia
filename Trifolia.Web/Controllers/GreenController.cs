using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Trifolia.Authorization;
using Trifolia.Web.Models.GreenManagement;
using Trifolia.Web.Models.Validation;

namespace Trifolia.Web.Controllers
{
    [Securable(SecurableNames.GREEN_MODEL)]
    public class GreenController : Controller
    {
        //
        // GET: /Green/

        public ActionResult Index(int id)
        {
            return View("Index", id);
        }
    }
}