using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Web.Models.Import
{
    public class ImportStatusModel
    {
        private IObjectRepository tdb;

        public List<string> Messages { get; set; }

        [JsonIgnore]
        public Dictionary<Template, EntityState> ImportedTemplates { get; set; }

        [JsonIgnore]
        public Dictionary<ImplementationGuide, EntityState> ImportedImplementationGuides { get; set; }

        public List<ImportedTemplate> Templates
        {
            get
            {
                List<ImportedTemplate> templateStatuses = new List<ImportedTemplate>();

                foreach (var importedTemplate in this.ImportedTemplates)
                {
                    var templateStatus = new ImportedTemplate()
                    {
                        InternalId = importedTemplate.Key.Id,
                        Identifier = importedTemplate.Key.Oid,
                        Name = importedTemplate.Key.Name,
                        Status = importedTemplate.Value.ToString()
                    };

                    templateStatuses.Add(templateStatus);
                }

                return templateStatuses;
            }
        }

        public List<ImportedImplementationGuide> ImplementationGuides
        {
            get
            {
                List<ImportedImplementationGuide> implementationGuideStatuses = new List<ImportedImplementationGuide>();

                foreach (var importedImplementationGuide in this.ImportedImplementationGuides)
                {
                    var igStatus = new ImportedImplementationGuide()
                    {
                        InternalId = importedImplementationGuide.Key.Id,
                        Name = importedImplementationGuide.Key.Name,
                        Version = importedImplementationGuide.Key.Version == null ? 1 : importedImplementationGuide.Key.Version.Value,
                        Status = importedImplementationGuide.Value.ToString()
                    };
                    implementationGuideStatuses.Add(igStatus);
                }

                return implementationGuideStatuses;
            }
        }

        public ImportStatusModel(IObjectRepository tdb)
        {
            this.Messages = new List<string>();
            this.ImportedTemplates = new Dictionary<Template, EntityState>();
            this.ImportedImplementationGuides = new Dictionary<ImplementationGuide, EntityState>();
            this.tdb = tdb;
        }

        public void AddImportedImplementationGuide(ImplementationGuide implementationGuide)
        {
            if (implementationGuide == null)
                return;

            var dataSource = this.tdb as TemplateDatabaseDataSource;

            if (dataSource != null)
                this.ImportedImplementationGuides.Add(implementationGuide, dataSource.ObjectStateManager.GetObjectStateEntry(implementationGuide).State);
            else
                this.ImportedImplementationGuides.Add(implementationGuide, EntityState.Detached);
        }

        public void AddImportedTemplate(Template template)
        {
            if (template == null)
                return;

            var dataSource = this.tdb as TemplateDatabaseDataSource;

            if (dataSource != null)
            {
                var state = dataSource.ObjectStateManager.GetObjectStateEntry(template).State;

                if (state == EntityState.Unchanged)
                {
                    // Check the constraints of the template for changes if the template itself hasn't changed
                    foreach (var constraint in template.ChildConstraints)
                    {
                        var constraintState = dataSource.ObjectStateManager.GetObjectStateEntry(constraint);

                        if (constraintState.State != EntityState.Unchanged)
                            state = EntityState.Modified;
                    }
                }

                this.ImportedTemplates.Add(template, state);
            }
            else
            {
                this.ImportedTemplates.Add(template, EntityState.Detached);
            }
        }

        public void AddImportedTemplates(IEnumerable<Template> templates)
        {
            foreach (var template in templates)
            {
                this.AddImportedTemplate(template);
            }
        }

        public class ImportedTemplate
        {
            public int InternalId { get; set; }
            public string Name { get; set; }
            public string Identifier { get; set; }
            public string Status { get; set; }
        }

        public class ImportedImplementationGuide
        {
            public int InternalId { get; set; }
            public string Name { get; set; }
            public int Version { get; set; }
            public string Status { get; set; }
        }
    }
}