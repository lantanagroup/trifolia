using System.Web.Mvc;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Web.Controllers
{
    public class IGTypeManagementController : Controller
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public IGTypeManagementController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public IGTypeManagementController()
            : this(DBContext.Create())
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Types

        [HttpGet, Securable(SecurableNames.ADMIN)]
        public ActionResult List()
        {
            return View("Types");
        }

        [HttpGet, Securable(SecurableNames.ADMIN)]
        public ActionResult Edit(int? implementationGuideTypeId)
        {
            return View("EditType", implementationGuideTypeId);
        }

        [HttpGet, Securable(SecurableNames.ADMIN)]
        public ActionResult EditSchemaChoices(int implementationGuideTypeId)
        {
            return View("SchemaChoices", implementationGuideTypeId);
        }

        #endregion
    }
}
