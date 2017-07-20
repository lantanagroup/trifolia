using System.Linq;
using System.Web.Mvc;
using Trifolia.Authorization;
using Trifolia.DB;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;

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

        [Securable(SecurableNames.EXPORT_SCHEMATRON, SecurableNames.EXPORT_VOCAB, SecurableNames.EXPORT_WORD, SecurableNames.EXPORT_XML)]
        public ActionResult Index()
        {
            return View("Export");
        }

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
