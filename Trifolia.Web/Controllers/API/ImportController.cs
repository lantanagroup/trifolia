using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using NativeModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using Trifolia.Shared.ImportExport;
using System.Data.Entity.Core.Objects;
using Trifolia.Import.Models;
using Trifolia.Import.Native;
using Trifolia.Web.Models;
using Trifolia.Import.VSAC;

namespace Trifolia.Web.Controllers.API
{
    public class ImportController : ApiController
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public ImportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public ImportController()
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

        /// <summary>
        /// Imports data from the native Trifolia format. See https://github.com/lantanagroup/trifolia/blob/master/Trifolia.Shared/ImportExport/Model/TemplateExport.xsd for the schema that is used by the import.
        /// </summary>
        /// <param name="model">The data to import (including implementation guides and templates)</param>
        /// <returns></returns>
        [HttpPost, Route("api/Import/Trifolia"), SecurableAction(SecurableNames.IMPORT)]
        public ImportStatusModel ImportTrifoliaModel(NativeModel model)
        {
            TrifoliaImporter importer = new TrifoliaImporter(this.tdb);
            return importer.Import(model);
        }
        
        [HttpPost, Route("api/Import/ValueSet"), SecurableAction(SecurableNames.IMPORT)]
        public ImportValueSetResponseModel ImportValueSet(ImportValueSetModel model)
        {
            ImportValueSetResponseModel responseModel = new ImportValueSetResponseModel();

            switch (model.Source)
            {
                case ValueSetImportSources.VSAC:
                    VSACImporter importer = new VSACImporter(this.tdb);

                    if (!importer.Authenticate(model.Username, model.Password))
                    {
                        responseModel.Message = "Invalid username/password";
                        break;
                    }

                    importer.ImportValueSet(model.Id);
                    responseModel.Success = true;

                    break;
                default:
                    responseModel.Message = "Unknown or unsupported source";
                    break;
            }

            return responseModel;
        }
    }
}
