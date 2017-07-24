using System.Linq;
using System.Web.Mvc;
using Trifolia.Authorization;
using Trifolia.DB;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Web.Controllers
{
    public class ExportController : Controller
    {
        [Securable(SecurableNames.EXPORT_SCHEMATRON, SecurableNames.EXPORT_VOCAB, SecurableNames.EXPORT_WORD, SecurableNames.EXPORT_XML)]
        public ActionResult Index()
        {
            return View("Export");
        }
    }
}
