using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net;

using Trifolia.Authentication;
using Trifolia.Shared;
using Trifolia.Logging;
using Trifolia.Authorization;
using Trifolia.Web.Filters;
using Trifolia.Web.Models.Account;
using Trifolia.DB;
using Trifolia.Config;

namespace Trifolia.Web.Controllers
{
    public class ImportController : Controller
    {
        private IObjectRepository tdb;

        #region Constructors 

        public ImportController()
        {
            this.tdb = new TemplateDatabaseDataSource();
        }

        public ImportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        [Securable(SecurableNames.IMPORT)]
        public ActionResult Index()
        {
            return View();
        }
    }
}
