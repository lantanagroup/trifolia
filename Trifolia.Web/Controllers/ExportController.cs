using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Trifolia.Shared;
using Trifolia.Generation.IG;
using Trifolia.Generation.Schematron;
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

        #region CTOR

        public ExportController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        public ExportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        #region MS Word

        [Securable(SecurableNames.EXPORT_WORD)]
        public ActionResult MSWord(int implementationGuideId, bool dy = false)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to this implementation guide.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            // Check if we need to rediret the user first
            ActionResult redirect = this.BeforeExport(ig, "MSWord");

            if (redirect != null)
                return redirect;

            return View("MSWord", implementationGuideId);
        }

        #endregion

        #region XML

        [Securable(SecurableNames.EXPORT_XML)]
        public ActionResult XML(int implementationGuideId, bool dy = false)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            ActionResult redirect = this.BeforeExport(implementationGuide, "Xml");

            if (redirect != null)
                return redirect;

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

            // Check if we need to rediret the user first
            ActionResult redirect = this.BeforeExport(ig, "Vocabulary");

            if (redirect != null)
                return redirect;

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

            // Check if we need to rediret the user first
            ActionResult redirect = this.BeforeExport(ig, "Schematron");

            if (redirect != null)
                return redirect;

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

        /// <summary>
        /// Checks if the authenticated user is from the HL7 organization.
        /// If yes, a redirect ActionResult is returned that forwards the user to the HL7 disclaimer page.
        /// The action result includes a redirectUrl parameter that tells HL7 to redirect the user back to trifolia.
        /// The redirectUrl parameter of the returned ActionResult includes the retAction passed to this method
        /// and an implementationGuideId param for the ig. 
        /// </summary>
        private ActionResult BeforeExport(ImplementationGuide ig, string retAction)
        {
            bool dy = false;
            bool.TryParse(Request.Params["dy"], out dy);

            // If the user is HL7 authenticated, they must sign a disclaimer before generating the IG
            if (CheckPoint.Instance.OrganizationName == "HL7" && !dy)
            {
                Dictionary<string, string> authData = CheckPoint.Instance.GetAuthenticatedData();

                string userId = authData["UserId"];

                string url = HL7AuthHelper.GetComplianceUrl(
                    Url.Action(retAction, "Export", new { implementationGuideId = ig.Id, dy = true }, Request.Url.Scheme),
                    userId,
                    ig.Name);

                return Redirect(url);
            }

            return null;
        }

        #endregion
    }
}
