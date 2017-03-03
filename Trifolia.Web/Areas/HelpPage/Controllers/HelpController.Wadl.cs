using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Trifolia.Web.Areas.HelpPage.ModelDescriptions;
using Trifolia.Web.Areas.HelpPage.Models;

namespace Trifolia.Web.Areas.HelpPage.Controllers
{
	public partial class HelpController
	{
        public ActionResult Wadl(string controllerDescriptor)
        {
            try
            {
                var apiDescriptions = Configuration.Services.GetApiExplorer().ApiDescriptions;
                var apisWithHelp = apiDescriptions.Select(api => Configuration.GetHelpPageApiModel(api.GetFriendlyId()));

                return View(apisWithHelp);
            }
            catch (Exception)
            {
                return Redirect("/api/Help");
            }
        }

	}
}