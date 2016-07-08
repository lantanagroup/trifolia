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
using Trifolia.Web.Models.Import;

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

        [HttpPost, Route("api/Import/Trifolia")]
        public ImportStatusModel ImportTrifoliaModel(ImportModel model)
        {
            ImportStatusModel importStatus = new ImportStatusModel(this.tdb);
            List<ImplementationGuide> importedImplementationGuides = new List<ImplementationGuide>();

            foreach (var importImplementationGuide in model.ImplementationGuide)
            {
                ImplementationGuideImporter importer = new ImplementationGuideImporter(this.tdb);
                var importedImplementationGuide = importer.Import(importImplementationGuide);
                importStatus.AddImportedImplementationGuide(importedImplementationGuide);
                importStatus.Messages.AddRange(importer.Errors);
            }

            TemplateImporter templateImporter = new TemplateImporter(this.tdb, shouldUpdate: true);
            List<Template> importedTemplates = templateImporter.Import(model.Template);
            importStatus.AddImportedTemplates(importedTemplates);
            importStatus.Messages.AddRange(templateImporter.Errors);

            //this.tdb.SaveChanges();

            return importStatus;
        }
    }
}
