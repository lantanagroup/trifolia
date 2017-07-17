using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Import.Models;
using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;

namespace Trifolia.Import.Native
{
    public class TrifoliaImporter
    {
        private IObjectRepository tdb;

        #region Constructors

        public TrifoliaImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public TrifoliaImporter()
            : this(DBContext.Create())
        {

        }

        #endregion

        public ImportStatusModel Import(ImportModel model)
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

            TerminologyImporter termImporter = new TerminologyImporter(this.tdb);
            termImporter.ImportCodeSystems(model.CodeSystem);
            termImporter.ImportValueSets(model.ValueSet);

            foreach (var addedValueSet in termImporter.AddedValueSets)
            {
                importStatus.AddValueSet(addedValueSet);
            }

            foreach (var addedCodeSystem in termImporter.AddedCodeSystems)
            {
                importStatus.AddCodeSystem(addedCodeSystem);
            }

            TemplateImporter templateImporter = new TemplateImporter(this.tdb, termImporter.AddedValueSets, shouldUpdate: true);
            List<Template> importedTemplates = templateImporter.Import(model.Template);
            importStatus.AddImportedTemplates(importedTemplates);
            importStatus.Messages.AddRange(templateImporter.Errors);

            if (importStatus.Messages.Count == 0)
            {
                try
                {
                    this.tdb.SaveChanges();
                    importStatus.Success = true;
                }
                catch (Exception ex)
                {
                    importStatus.Messages.Add("Error saving changes from import: " + ex.Message);
                }
            }

            return importStatus;
        }
    }
}
