using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Web.Models;
using Trifolia.Authentication;
using Trifolia.Config;
using Trifolia.Authorization;
using Trifolia.Shared;

namespace Trifolia.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult LoggedInIndex(string message = null)
        {
            if (User.Identity.IsAuthenticated && CheckPoint.Instance.User == null)
                return RedirectToAction("NewProfile", "Account");

            return Home(message);
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("LoggedInIndex");

            return Home();
        }

        private ActionResult Home(string message = null)
        {
            HomeModel model = new HomeModel();

            model.DisplayInternalTechSupportPanel = CheckPoint.Instance.IsDataAdmin;
            model.Message = message;

            return View("Index", model);
        }

        public ActionResult Error(string message = null)
        {
            Exception ex = null;

            try
            {
                ex = (Exception)HttpContext.Application[Request.UserHostAddress.ToString()];
                ViewData["Message"] = ex.Message;
            }
            catch
            {
                ViewData["Message"] = "An error occurred.";
            }

            return View();
        }
    }
}
