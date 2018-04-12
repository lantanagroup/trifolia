using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Generation.IG;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Generation.Versioning;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Export.HTML
{
    public class HtmlExporter
    {
        private IObjectRepository tdb;

        public HtmlExporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public ViewDataModel GetExportData(int implementationGuideId, int? fileId, int[] templateIds, bool inferred)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            ViewDataModel model = null;
            SimpleSchema schema = ig.ImplementationGuideType.GetSimpleSchema();

            if (fileId != null)
            {
                var file = ig.Files.Single(y => y.Id == fileId);
                var fileData = file.GetLatestData();

                if (file.ContentType != "DataSnapshot")
                    throw new Exception("File specified is not a data snapshot!");
                
                string fileDataContent = System.Text.Encoding.UTF8.GetString(fileData.Data);
                model = JsonConvert.DeserializeObject<ViewDataModel>(fileDataContent);
                model.ImplementationGuideFileId = fileId;
            }
            else
            {
                IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
                var igTypePlugin = IGTypePluginFactory.GetPlugin(ig.ImplementationGuideType);
                var firstTemplateType = ig.ImplementationGuideType.TemplateTypes.OrderBy(y => y.OutputOrder).FirstOrDefault();

                model = new ViewDataModel()
                {
                    ImplementationGuideId = implementationGuideId,
                    ImplementationGuideName = !string.IsNullOrEmpty(ig.DisplayName) ? ig.DisplayName : ig.NameWithVersion,
                    ImplementationGuideFileId = fileId,
                    Status = ig.PublishStatus.Status,
                    PublishDate = ig.PublishDate != null ? ig.PublishDate.Value.ToShortDateString() : null,
                    Volume1Html = igSettings.GetSetting(IGSettingsManager.SettingProperty.Volume1Html)
                };

                model.ImplementationGuideDescription = ig.WebDescription;
                model.ImplementationGuideDisplayName = !string.IsNullOrEmpty(ig.WebDisplayName) ? ig.WebDisplayName : model.ImplementationGuideName;

                // Create the section models
                model.Volume1Sections = (from igs in ig.Sections.OrderBy(y => y.Order)
                                         select new ViewDataModel.Section()
                                         {
                                             Heading = igs.Heading,
                                             Content = igs.Content,
                                             Level = igs.Level
                                         }).ToList();

                // Include any FHIR Resource Instance attachments with the IG
                foreach (var fhirResourceInstanceFile in ig.Files.Where(y => y.ContentType == "FHIRResourceInstance"))
                {
                    var fileData = fhirResourceInstanceFile.GetLatestData();
                    string fileContent = System.Text.Encoding.UTF8.GetString(fileData.Data);

                    ViewDataModel.FHIRResource newFhirResource = new ViewDataModel.FHIRResource()
                    {
                        Name = fhirResourceInstanceFile.FileName,
                        JsonContent = igTypePlugin.GetFHIRResourceInstanceJson(fileContent),
                        XmlContent = igTypePlugin.GetFHIRResourceInstanceXml(fileContent)
                    };

                    if (!string.IsNullOrEmpty(newFhirResource.JsonContent) || !string.IsNullOrEmpty(newFhirResource.XmlContent))
                        model.FHIRResources.Add(newFhirResource);
                }

                // Create the value set models
                var valueSets = ig.GetValueSets(this.tdb);

                foreach (var valueSet in valueSets)
                {
                    var newValueSetModel = new ViewDataModel.ValueSet()
                    {
                        Identifier = valueSet.ValueSet.GetIdentifier(igTypePlugin),
                        Name = valueSet.ValueSet.Name,
                        Source = valueSet.ValueSet.Source,
                        Description = valueSet.ValueSet.Description,
                        BindingDate = valueSet.BindingDate != null ? valueSet.BindingDate.Value.ToShortDateString() : null
                    };

                    var members = valueSet.ValueSet.GetActiveMembers(valueSet.BindingDate);
                    newValueSetModel.Members = (from vsm in members
                                                select new ViewDataModel.ValueSetMember()
                                                {
                                                    Code = vsm.Code,
                                                    DisplayName = vsm.DisplayName,
                                                    CodeSystemIdentifier = vsm.CodeSystem.Oid,
                                                    CodeSystemName = vsm.CodeSystem.Name
                                                }).ToList();

                    model.ValueSets.Add(newValueSetModel);

                    // Add code systems used by this value set to the IG
                    var codeSystems = (from vsm in members
                                       select new ViewDataModel.CodeSystem()
                                       {
                                           Identifier = vsm.CodeSystem.Oid,
                                           Name = vsm.CodeSystem.Name
                                       });
                    model.CodeSystems.AddRange(codeSystems);
                }

                // Create the template models
                var templates = ig.GetRecursiveTemplates(this.tdb, templateIds != null ? templateIds.ToList() : null, inferred);
                var constraints = (from t in templates
                                   join tc in this.tdb.TemplateConstraints.AsNoTracking() on t.Id equals tc.TemplateId
                                   select tc).AsEnumerable();
                var constraintReferences = (from c in constraints
                                            join tcr in this.tdb.TemplateConstraintReferences.AsNoTracking() on c.Id equals tcr.TemplateConstraintId
                                            join t in this.tdb.Templates.AsNoTracking() on tcr.ReferenceIdentifier equals t.Oid
                                            where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                            select new ConstraintReference()
                                            {
                                                Bookmark = t.Bookmark,
                                                Identifier = t.Oid,
                                                Name = t.Name,
                                                TemplateConstraintId = tcr.TemplateConstraintId,
                                                IncludedInIG = templates.Contains(t)
                                            }).ToList();

                foreach (var template in templates)
                {
                    var templateId = template.Id;
                    var templateSchema = schema.GetSchemaFromContext(template.PrimaryContextType);
                    var newTemplateModel = new ViewDataModel.Template()
                    {
                        Identifier = template.Oid,
                        Bookmark = template.Bookmark,
                        ContextType = template.PrimaryContextType,
                        Context = template.PrimaryContext,
                        Name = template.Name,
                        ImpliedTemplate = template.ImpliedTemplate != null ? new ViewDataModel.TemplateReference(template.ImpliedTemplate) : null,
                        Description = template.Description.MarkdownToHtml(),
                        Extensibility = template.IsOpen ? "Open" : "Closed",
                        TemplateTypeId = template.TemplateTypeId
                    };

                    // Load Template Changes
                    var lPreviousVersion = template.PreviousVersion;

                    if (lPreviousVersion != null)
                    {
                        var comparer = VersionComparer.CreateComparer(this.tdb, igTypePlugin, igSettings);
                        var result = comparer.Compare(lPreviousVersion, template);

                        newTemplateModel.Changes = new DifferenceModel(result)
                        {
                            Id = template.Id,
                            TemplateName = template.Name,
                            PreviousTemplateName = string.Format("{0} ({1})", lPreviousVersion.Name, lPreviousVersion.Oid),
                            PreviousTemplateId = lPreviousVersion.Id
                        };
                    }

                    // Code systems used in this template to the IG
                    var codeSystems = (from tc in template.ChildConstraints
                                       where tc.CodeSystem != null
                                       select new ViewDataModel.CodeSystem()
                                       {
                                           Identifier = tc.CodeSystem.Oid,
                                           Name = tc.CodeSystem.Name
                                       });
                    model.CodeSystems.AddRange(codeSystems);

                    // Samples
                    newTemplateModel.Samples = (from ts in template.TemplateSamples
                                                select new ViewDataModel.Sample()
                                                {
                                                    Id = ts.Id,
                                                    Name = ts.Name,
                                                    SampleText = ts.XmlSample
                                                }).ToList();

                    // Contained templates
                    var containedTemplates = (from tcr in this.tdb.ViewTemplateRelationships.AsNoTracking()
                                              join t in this.tdb.Templates.AsNoTracking() on tcr.ChildTemplateId equals t.Id
                                              where tcr.ParentTemplateId == templateId
                                              select t).Distinct().ToList();
                    newTemplateModel.ContainedTemplates = containedTemplates.Select(y => new ViewDataModel.TemplateReference(y)).ToList();

                    // Implying templates
                    var implyingTemplates = (from t in templates
                                             where t.ImpliedTemplateId == template.Id
                                             select t).Distinct().ToList();
                    newTemplateModel.ImplyingTemplates = implyingTemplates.Select(y => new ViewDataModel.TemplateReference(y)).ToList();

                    // Contained by templates
                    var containedByTemplates = (from tcr in this.tdb.ViewTemplateRelationships.AsNoTracking()
                                                join t in this.tdb.Templates.AsNoTracking() on tcr.ParentTemplateId equals t.Id
                                                where tcr.ChildTemplateId == templateId
                                                select t).Distinct().ToList();
                    newTemplateModel.ContainedByTemplates = containedByTemplates.Select(y => new ViewDataModel.TemplateReference(y)).ToList();

                    model.Templates.Add(newTemplateModel);



                    // Create the constraint models (hierarchically)
                    var parentConstraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null);
                    CreateConstraints(igSettings, igTypePlugin, constraintReferences, parentConstraints, newTemplateModel.Constraints, templateSchema);
                }

                // Create models for template types in the IG
                model.TemplateTypes = (from igt in igSettings.TemplateTypes
                                       join tt in this.tdb.TemplateTypes.AsNoTracking() on igt.TemplateTypeId equals tt.Id
                                       where model.Templates.Exists(y => y.TemplateTypeId == tt.Id)
                                       select new ViewDataModel.TemplateType()
                                       {
                                           TemplateTypeId = tt.Id,
                                           Name = igt.Name,
                                           ContextType = tt.RootContextType,
                                           Description = igt.DetailsText
                                       }).ToList();

                model.CodeSystems = model.CodeSystems.Distinct().ToList();
            }

            return model;
        }

        private void CreateConstraints(IGSettingsManager igManager, IIGTypePlugin igTypePlugin, List<ConstraintReference> constraintReferences, IEnumerable<TemplateConstraint> constraints, List<ViewDataModel.Constraint> parentList, SimpleSchema templateSchema, SimpleSchema.SchemaObject schemaObject = null)
        {
            foreach (var constraint in constraints.OrderBy(y => y.Order))
            {
                var theConstraint = constraint;

                // TODO: Possible bug? Should schemaObject always be re-set? Remove schemaObject == null from if()
                if (templateSchema != null && schemaObject == null)
                    schemaObject = templateSchema.Children.SingleOrDefault(y => y.Name == constraint.Context);

                IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igManager, igTypePlugin, theConstraint, constraintReferences, "#/volume2/", "#/valuesets/#", true, true, true, false);

                var newConstraintModel = new ViewDataModel.Constraint()
                {
                    Number = string.Format("{0}-{1}", theConstraint.Template.OwningImplementationGuideId, theConstraint.Number),
                    Narrative = fc.GetHtml(string.Empty, 1, true),
                    Conformance = theConstraint.Conformance,
                    Cardinality = theConstraint.Cardinality,
                    Context = theConstraint.Context,
                    DataType = theConstraint.DataType,
                    Value = theConstraint.Value,
                    ValueSetIdentifier = theConstraint.ValueSet != null ? theConstraint.ValueSet.GetIdentifier(igTypePlugin) : null,
                    ValueSetDate = theConstraint.ValueSetDate != null ? theConstraint.ValueSetDate.Value.ToShortDateString() : null,
                    IsChoice = theConstraint.IsChoice
                };

                newConstraintModel.ContainedTemplates = (from tcr in theConstraint.References
                                                         join t in this.tdb.Templates.AsNoTracking() on tcr.ReferenceIdentifier equals t.Oid
                                                         where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                                         select new ViewDataModel.TemplateReference()
                                                         {
                                                             Identifier = t.Oid,
                                                             Bookmark = t.Bookmark,
                                                             ImplementationGuide = t.OwningImplementationGuide.GetDisplayName(),
                                                             PublishDate = t.OwningImplementationGuide.PublishDate,
                                                             Name = t.Name
                                                         }).ToList();

                var isFhir = constraint.Template.ImplementationGuideType.SchemaURI == ImplementationGuideType.FHIR_NS;

                // Check if we're dealing with a FHIR constraint
                if (isFhir && schemaObject != null)
                    newConstraintModel.DataType = schemaObject.DataType;

                parentList.Add(newConstraintModel);

                var nextSchemaObject = schemaObject != null ?
                    schemaObject.Children.SingleOrDefault(y => y.Name == constraint.Context) :
                    null;

                // Recursively add child constraints
                CreateConstraints(igManager, igTypePlugin, constraintReferences, theConstraint.ChildConstraints, newConstraintModel.Constraints, null, nextSchemaObject);
            }
        }
    }
}
