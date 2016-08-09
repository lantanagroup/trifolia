using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using Trifolia.Shared.ImportExport;
using System.Data.Entity.Core.Objects;
using Trifolia.Import.Models;
using Trifolia.Import.Native;

namespace Trifolia.Web.Controllers.API
{
    public class ImportController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public ImportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public ImportController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        [HttpPost, Route("api/Import/Trifolia"), SecurableAction(SecurableNames.IMPORT)]
        public ImportStatusModel ImportTrifoliaModel(ImportModel model)
        {
            TrifoliaImporter importer = new TrifoliaImporter(this.tdb);
            return importer.Import(model);
        }
    }
}
