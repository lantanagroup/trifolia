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
        private Dictionary<Template, EntityState> ImportedTemplates { get; set; }
        private Dictionary<ImplementationGuide, EntityState> ImportedImplementationGuides { get; set; }
        private Dictionary<TemplateConstraint, EntityState> ImportedConstraints { get; set; }
        private Dictionary<TemplateConstraintSample, EntityState> ImportedConstraintSamples { get; set; }
        private Dictionary<TemplateSample, EntityState> ImportedTemplateSamples { get; set; }

        public List<string> Messages { get; set; }

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

                    foreach (var constraint in importedTemplate.Key.ChildConstraints)
                    {
                        var constraintState = this.ImportedConstraints[constraint];
                        var newConstraintStatus = new ImportedConstraint()
                        {
                            Number = constraint.GetFormattedNumber(),
                            Status = constraintState.ToString()
                        };

                        foreach (var constraintSample in constraint.Samples)
                        {
                            var sampleState = this.ImportedConstraintSamples[constraintSample];
                            newConstraintStatus.Samples.Add(new ImportedConstraintSample()
                            {
                                Name = constraintSample.Name,
                                Status = sampleState.ToString()
                            });
                        }

                        templateStatus.Constraints.Add(newConstraintStatus);
                    }

                    foreach (var sample in importedTemplate.Key.TemplateSamples)
                    {
                        var sampleStatus = this.ImportedTemplateSamples[sample];
                        templateStatus.Samples.Add(new ImportedTemplateSample()
                        {
                            Name = sample.Name,
                            Status = sampleStatus.ToString()
                        });
                    }

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
            this.ImportedConstraints = new Dictionary<TemplateConstraint, EntityState>();
            this.ImportedConstraintSamples = new Dictionary<TemplateConstraintSample, EntityState>();
            this.ImportedTemplateSamples = new Dictionary<TemplateSample, EntityState>();
            this.tdb = tdb;
        }

        public void AddImportedImplementationGuide(ImplementationGuide implementationGuide)
        {
            if (implementationGuide == null || this.ImportedImplementationGuides.ContainsKey(implementationGuide))
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

                foreach (var constraint in template.ChildConstraints)
                {
                    var constraintState = dataSource.ObjectStateManager.GetObjectStateEntry(constraint);
                    this.ImportedConstraints.Add(constraint, constraintState.State);

                    foreach (var constraintSample in constraint.Samples)
                    {
                        var constraintSampleState = dataSource.ObjectStateManager.GetObjectStateEntry(constraintState);
                        this.ImportedConstraintSamples.Add(constraintSample, constraintSampleState.State);
                    }

                }

                foreach (var sample in template.TemplateSamples)
                {
                    var sampleState = dataSource.ObjectStateManager.GetObjectStateEntry(sample);
                    this.ImportedTemplateSamples.Add(sample, sampleState.State);
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
            public ImportedTemplate()
            {
                this.Samples = new List<ImportedTemplateSample>();
                this.Constraints = new List<ImportedConstraint>();
            }

            public int InternalId { get; set; }
            public string Name { get; set; }
            public string Identifier { get; set; }
            public string Status { get; set; }
            
            public List<ImportedTemplateSample> Samples { get; set; }

            public List<ImportedConstraint> Constraints { get; set; }
        }

        public class ImportedTemplateSample
        {
            public string Name { get; set; }
            public string Status { get; set; }
        }

        public class ImportedConstraint
        {
            public ImportedConstraint()
            {
                this.Samples = new List<ImportedConstraintSample>();
            }

            public string Number { get; set; }
            public string Status { get; set; }

            public List<ImportedConstraintSample> Samples { get; set; }
        }

        public class ImportedConstraintSample
        {
            public string Name { get; set; }
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