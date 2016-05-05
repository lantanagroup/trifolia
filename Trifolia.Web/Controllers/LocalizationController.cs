using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trifolia.Authorization;

namespace Trifolia.Web.Controllers
{
    public class LocalizationController : Controller
    {
        [Securable]
        public JsonResult LocalizationScript()
        {
            foreach (string lCurrentLanguage in Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"].Split(';'))
            {
                string[] lCurrentLanguageCultureCombo = lCurrentLanguage.Split(',');
                string lClientLanguageWithCulture = lCurrentLanguageCultureCombo[0];
                string lClientLanguage = lCurrentLanguageCultureCombo.Length > 1 ? lCurrentLanguageCultureCombo[1] : string.Empty;

                string lClientLanguageWithCultureFileName = string.Format("/Scripts/Localization/localizedClient.{0}.js", lClientLanguageWithCulture);
                string lClientLanguageFileName = string.Format("/Scripts/Localization/localizedClient.{0}.js", lClientLanguage);

                if (System.IO.File.Exists(Request.MapPath(lClientLanguageWithCultureFileName)))
                {
                    return Json(new { fileName = lClientLanguageWithCultureFileName }, JsonRequestBehavior.AllowGet);
                }
                else if (System.IO.File.Exists(Request.MapPath(lClientLanguageFileName)))
                {
                    return Json(new { fileName = lClientLanguageFileName }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    continue;
                }
            }
            
            string lDefaultLanguageFileName = "/Scripts/Localization/localizedClient.en.js";
            return Json(new { fileName = lDefaultLanguageFileName }, JsonRequestBehavior.AllowGet);
        }
    }
}