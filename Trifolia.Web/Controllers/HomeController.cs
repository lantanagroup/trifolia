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

namespace Trifolia.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult LoggedInIndex()
        {
            if (User.Identity.IsAuthenticated && CheckPoint.Instance.User == null)
                return RedirectToAction("NewProfile", "Account");

            HomeModel model = new HomeModel();
            
            model.DisplayInternalTechSupportPanel = CheckPoint.Instance.IsDataAdmin;

            // Determine the did you know tip
            var didYouKnowTips = Properties.Settings.Default.DidYouKnowItems;
            int randIndex = new Random().Next(0, didYouKnowTips.Count);
            model.DidYouKnowTip = didYouKnowTips[randIndex <= didYouKnowTips.Count - 1 ? randIndex : didYouKnowTips.Count - 1];

            StringCollection lMessages = Properties.Settings.Default.WhatsNewItems;

            foreach (string lCurrentMessage in lMessages)
            {
                model.WhatsNewMessages.Add(lCurrentMessage);
            }

            return View(model);
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("LoggedInIndex");

            LogInViewModel model = new LogInViewModel();
            string redirectUrl = string.Format("{0}://{1}/Account/DoHL7Login",
                Request.Url.Scheme,
                Request.Url.Authority);

            model.DisplayInternalTechSupportPanel = CheckPoint.Instance.IsDataAdmin;

            // Determine the HL7 login link
            model.HL7LoginLink = string.Format(Properties.Settings.Default.HL7LoginUrlFormat,
                HL7AuthHelper.API_KEY,
                redirectUrl);

            // Determine the did you know tip
            var didYouKnowTips = Properties.Settings.Default.DidYouKnowItems;
            int randIndex = new Random().Next(0, didYouKnowTips.Count);
            model.DidYouKnowTip = didYouKnowTips[randIndex <= didYouKnowTips.Count - 1 ? randIndex : didYouKnowTips.Count - 1];

            return View(model);
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
