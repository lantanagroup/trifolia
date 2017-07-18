using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Import.Models
{
    public class ImportStatusModel
    {
        private IObjectRepository tdb;
        private Dictionary<Template, EntityState> importedTemplates { get; set; }
        private Dictionary<ImplementationGuide, EntityState> importedImplementationGuides { get; set; }
        private Dictionary<TemplateConstraint, EntityState> importedConstraints { get; set; }
        private Dictionary<TemplateConstraintSample, EntityState> importedConstraintSamples { get; set; }
        private Dictionary<TemplateSample, EntityState> importedTemplateSamples { get; set; }
        private List<ValueSet> importedValueSets { get; set; }
        private List<CodeSystem> importedCodeSystems { get; set; }

        public List<string> Messages { get; set; }
        public bool Success { get; set; }

        public List<ImportedTemplate> Templates
        {
            get
            {
                List<ImportedTemplate> templateStatuses = new List<ImportedTemplate>();

                foreach (var importedTemplate in this.importedTemplates)
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
                        var constraintState = this.importedConstraints[constraint];
                        var newConstraintStatus = new ImportedConstraint()
                        {
                            Number = constraint.GetFormattedNumber(),
                            Status = constraintState.ToString()
                        };

                        foreach (var constraintSample in constraint.Samples)
                        {
                            var sampleState = this.importedConstraintSamples[constraintSample];
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
                        var sampleStatus = this.importedTemplateSamples[sample];
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

                foreach (var importedImplementationGuide in this.importedImplementationGuides)
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

        public List<ImportedValueSet> ValueSets
        {
            get
            {
                List<ImportedValueSet> importedValueSets = (from vs in this.importedValueSets
                                                            select new ImportedValueSet()
                                                            {
                                                                Name = vs.Name,
                                                                Identifier = vs.GetIdentifier()
                                                            }).ToList();
                return importedValueSets;
            }
        }

        public List<ImportedCodeSystem> CodeSystems
        {
            get
            {
                List<ImportedCodeSystem> importedCodeSystems = (from cs in this.importedCodeSystems
                                                                select new ImportedCodeSystem()
                                                                {
                                                                    Name = cs.Name,
                                                                    Identifier = cs.Oid
                                                                }).ToList();
                return importedCodeSystems;
            }
        }

        public ImportStatusModel(IObjectRepository tdb)
        {
            this.Messages = new List<string>();
            this.importedTemplates = new Dictionary<Template, EntityState>();
            this.importedImplementationGuides = new Dictionary<ImplementationGuide, EntityState>();
            this.importedConstraints = new Dictionary<TemplateConstraint, EntityState>();
            this.importedConstraintSamples = new Dictionary<TemplateConstraintSample, EntityState>();
            this.importedTemplateSamples = new Dictionary<TemplateSample, EntityState>();
            this.importedValueSets = new List<ValueSet>();
            this.importedCodeSystems = new List<CodeSystem>();
            this.tdb = tdb;
        }

        public void AddValueSet(ValueSet valueSet)
        {
            this.importedValueSets.Add(valueSet);
        }

        public void AddCodeSystem(CodeSystem codeSystem)
        {
            this.importedCodeSystems.Add(codeSystem);
        }

        public void AddImportedImplementationGuide(ImplementationGuide implementationGuide)
        {
            if (implementationGuide == null || this.importedImplementationGuides.ContainsKey(implementationGuide))
                return;

            var dataSource = this.tdb as TrifoliaDatabase;

            if (dataSource != null)
                this.importedImplementationGuides.Add(implementationGuide, dataSource.Entry(implementationGuide).State);
            else
                this.importedImplementationGuides.Add(implementationGuide, EntityState.Detached);
        }

        public void AddImportedTemplate(Template template)
        {
            if (template == null)
                return;

            var dataSource = this.tdb as TrifoliaDatabase;

            if (dataSource != null)
            {
                var state = dataSource.Entry(template).State;

                foreach (var constraint in template.ChildConstraints)
                {
                    var constraintState = dataSource.Entry(constraint);
                    this.importedConstraints.Add(constraint, constraintState.State);

                    foreach (var constraintSample in constraint.Samples)
                    {
                        var constraintSampleState = dataSource.Entry(constraintState);
                        this.importedConstraintSamples.Add(constraintSample, constraintSampleState.State);
                    }

                }

                foreach (var sample in template.TemplateSamples)
                {
                    var sampleState = dataSource.Entry(sample);
                    this.importedTemplateSamples.Add(sample, sampleState.State);
                }

                this.importedTemplates.Add(template, state);
            }
            else
            {
                this.importedTemplates.Add(template, EntityState.Detached);
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

        public class ImportedValueSet
        {
            public string Name { get; set; }
            public string Identifier { get; set; }
        }

        public class ImportedCodeSystem
        {
            public string Name { get; set; }
            public string Identifier { get; set; }
        }
    }
}