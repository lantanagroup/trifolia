using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Shared;
using Trifolia.Generation.IG;
using Trifolia.Export.Schematron;
using Trifolia.Generation.Green;
using Trifolia.Authentication;
using Trifolia.Shared.ImportExport;

using Trifolia.Web.Models.Export;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Terminology;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;

using Ionic.Zip;

namespace Trifolia.Web.Controllers
{
    public class ExportController : Controller
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public ExportController()
            : this(DBContext.Create())
        {

        }

        public ExportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region MS Word

        [Securable(SecurableNames.EXPORT_WORD)]
        public ActionResult MSWord(int implementationGuideId, bool dy = false)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to this implementation guide.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            return View("MSWord", implementationGuideId);
        }

        #endregion

        #region XML

        [Securable(SecurableNames.EXPORT_XML)]
        public ActionResult XML(int implementationGuideId, bool dy = false)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            return View("Xml", implementationGuideId);
        }

        #endregion

        #region Vocabulary

        [Securable(SecurableNames.EXPORT_VOCAB)]
        public ActionResult Vocabulary(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to this implementation guide.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            return View("Vocabulary", implementationGuideId);
        }

        #endregion

        #region Schematron

        [Securable(SecurableNames.EXPORT_SCHEMATRON)]
        public ActionResult Schematron(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to this implementation guide.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            return View("Schematron", implementationGuideId);
        }

        #endregion

        #region Green

        [Securable(SecurableNames.EXPORT_GREEN)]
        public ActionResult Green(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to this implementation guide.");

            return View("Green", implementationGuideId);
        }

        #endregion

        #region Other

        private string GetCancelUrl()
        {
            if (Request.UrlReferrer == null)
                return "/";

            return Request.UrlReferrer.PathAndQuery;
        }

        #endregion
    }
}
