using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using Trifolia.DB;
using Trifolia.Export.MSWord;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Export.Versioning;
using Trifolia.Logging;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Export.HTML
{
    public class HtmlExporter
    {
        public class ExportConstraintReference : ConstraintReference
        {
            public string ImplementationGuide { get; set; }
            public DateTime? PublishDate { get; set; }
        }

        private IObjectRepository tdb;
        private bool offline = false;
        private List<ExportConstraintReference> constraintReferences;

        public HtmlExporter(IObjectRepository tdb, bool offline = false)
        {
            this.tdb = tdb;
            this.offline = offline;
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
                Log.For(this).Trace("Generating HTML export for " + implementationGuideId);

                IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
                var igTypePlugin = ig.ImplementationGuideType.GetPlugin();
                var firstTemplateType = ig.ImplementationGuideType.TemplateTypes.OrderBy(y => y.OutputOrder).FirstOrDefault();
                var valueSets = ig.GetValueSets(this.tdb);
                var parentTemplateIds = templateIds != null ? templateIds.ToList() : null;
                var templates =
                    ig.GetQueryableRecursiveTemplates(this.tdb, parentTemplateIds, inferred)
                    .IncludeDetails(true, true, true)
                    .ToList();
                var constraints = templates.SelectMany(y => y.ChildConstraints);
                var previousVersionIds = templates.Where(y => y.PreviousVersionTemplateId != null).Select(y => y.PreviousVersionTemplateId.Value).ToList();
                var previousVersions = this.tdb.Templates.Where(y => previousVersionIds.Contains(y.Id))
                                        .IncludeDetails(true, true, true)
                                        .ToList();
                var containedTemplates = (from st in templates
                                          join vtr in this.tdb.ViewTemplateRelationships.AsNoTracking() on st.Id equals vtr.ParentTemplateId
                                          join t in this.tdb.Templates.AsNoTracking() on vtr.ChildTemplateId equals t.Id
                                          select new
                                          {
                                              Relationship = vtr,
                                              Template = t
                                          }).ToList();
                var containedByTemplates = (from st in templates
                                            join vtr in this.tdb.ViewTemplateRelationships.AsNoTracking() on st.Id equals vtr.ChildTemplateId
                                            join t in this.tdb.Templates.AsNoTracking() on vtr.ParentTemplateId equals t.Id
                                            select new
                                            {
                                                Relationship = vtr,
                                                Template = t
                                            }).ToList();
                this.constraintReferences = (from c in constraints
                                             join tcr in this.tdb.TemplateConstraintReferences.AsNoTracking() on c.Id equals tcr.TemplateConstraintId
                                             join t in this.tdb.Templates.AsNoTracking() on tcr.ReferenceIdentifier equals t.Oid
                                             join ig1 in this.tdb.ImplementationGuides.AsNoTracking() on t.OwningImplementationGuideId equals ig1.Id
                                             where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                             select new ExportConstraintReference()
                                             {
                                                 Bookmark = t.Bookmark,
                                                 Identifier = t.Oid,
                                                 Name = t.Name,
                                                 TemplateConstraintId = tcr.TemplateConstraintId,
                                                 IncludedInIG = templates.Contains(t),
                                                 ImplementationGuide = 
                                                    !string.IsNullOrEmpty(ig1.DisplayName) ? 
                                                    ig1.DisplayName :
                                                    ig1.Name + " V" + ig1.Version.ToString(),
                                                 PublishDate = ig1.PublishDate
                                             }).ToList();

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

                Log.For(this).Trace("Including Volume 1 sections");

                // Create the section models
                var sections = ig.Sections.OrderBy(y => y.Order).ToList();
                model.Volume1Sections = (from igs in sections
                                         select new ViewDataModel.Section()
                                         {
                                             Heading = igs.Heading,
                                             Content = igs.Content.MarkdownToHtml(),
                                             Level = igs.Level
                                         }).ToList();

                Log.For(this).Trace("Including FHIR resources attached to IG");

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

                Log.For(this).Trace("Including value sets");

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
                                                join cs in this.tdb.CodeSystems on vsm.CodeSystemId equals cs.Id
                                                select new ViewDataModel.ValueSetMember()
                                                {
                                                    Code = vsm.Code,
                                                    DisplayName = vsm.DisplayName,
                                                    CodeSystemIdentifier = cs.Oid,
                                                    CodeSystemName = cs.Name
                                                }).ToList();

                    model.ValueSets.Add(newValueSetModel);

                    // Add code systems used by this value set to the IG
                    var codeSystems = (from vsm in members
                                       join cs in this.tdb.CodeSystems on vsm.CodeSystemId equals cs.Id
                                       select new ViewDataModel.CodeSystem()
                                       {
                                           Identifier = cs.Oid,
                                           Name = cs.Name
                                       });
                    model.CodeSystems.AddRange(codeSystems);
                }

                Log.For(this).Trace("Including templates");

                foreach (var template in templates)
                {
                    var templateId = template.Id;
                    var templateSchema = schema.GetSchemaFromContext(template.PrimaryContextType);
                    var previousVersion = previousVersions.SingleOrDefault(y => y.Id == template.PreviousVersionTemplateId);
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
                    if (previousVersion != null)
                    {
                        var comparer = VersionComparer.CreateComparer(this.tdb, igTypePlugin, igSettings);
                        var result = comparer.Compare(previousVersion, template);

                        newTemplateModel.Changes = new DifferenceModel(result)
                        {
                            Id = template.Id,
                            TemplateName = template.Name,
                            PreviousTemplateName = string.Format("{0} ({1})", previousVersion.Name, previousVersion.Oid),
                            PreviousTemplateId = previousVersion.Id
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
                    var thisContainedTemplates = containedTemplates
                        .Where(y => y.Relationship.ParentTemplateId == templateId)
                        .GroupBy(y => y.Template)
                        .Select(y => y.First());
                    newTemplateModel.ContainedTemplates = thisContainedTemplates.Select(y => new ViewDataModel.TemplateReference(y.Template)).ToList();

                    // Contained by templates                    
                    var thisContainedByTemplates = containedByTemplates
                        .Where(y => y.Relationship.ChildTemplateId == templateId)
                        .GroupBy(y => y.Template)
                        .Select(y => y.First());
                    newTemplateModel.ContainedByTemplates = thisContainedByTemplates.Select(y => new ViewDataModel.TemplateReference(y.Template)).ToList();

                    // Implying templates
                    var implyingTemplates = (from t in templates
                                             where t.ImpliedTemplateId == template.Id
                                             select t).Distinct().ToList();
                    newTemplateModel.ImplyingTemplates = implyingTemplates.Select(y => new ViewDataModel.TemplateReference(y)).ToList();

                    model.Templates.Add(newTemplateModel);

                    // Create the constraint models (hierarchically)
                    var parentConstraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null);
                    CreateConstraints(igSettings, igTypePlugin, parentConstraints, newTemplateModel.Constraints, templateSchema);
                }

                Log.For(this).Trace("Including template types");

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

                Log.For(this).Trace("Including code systems");
                model.CodeSystems = model.CodeSystems.Distinct().ToList();
            }
            
            this.FixImagePaths(model);

            Log.For(this).Trace("Done generating HTML export for " + implementationGuideId);

            return model;
        }

        private string FixImagePaths(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            Regex regex = new Regex(@"src=""\/api\/ImplementationGuide\/(\d+)\/Image\/(.+?)""", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                string replacement = string.Format(@"src=""images/{0}""", match.Groups[2].Value);
                text = text.Replace(match.Groups[0].Value, replacement);
            }

            return text;
        }

        private void FixImagePaths(ViewDataModel model)
        {
            if (!this.offline)
                return;

            Log.For(this).Trace("Fixing paths to images for offline copy.");

            foreach (var template in model.Templates)
            {
                template.Description = this.FixImagePaths(template.Description);
                
                foreach (var constraint in template.Constraints)
                {
                    constraint.Narrative = this.FixImagePaths(constraint.Narrative);
                }
            }

            foreach (var section in model.Volume1Sections)
            {
                section.Content = this.FixImagePaths(section.Content);
            }
        }

        private void CreateConstraints(IGSettingsManager igManager, IIGTypePlugin igTypePlugin, IEnumerable<TemplateConstraint> constraints, List<ViewDataModel.Constraint> parentList, SimpleSchema templateSchema, SimpleSchema.SchemaObject schemaObject = null)
        {
            foreach (var constraint in constraints.OrderBy(y => y.Order))
            {
                var theConstraint = constraint;

                // TODO: Possible bug? Should schemaObject always be re-set? Remove schemaObject == null from if()
                if (templateSchema != null && schemaObject == null)
                    schemaObject = templateSchema.Children.SingleOrDefault(y => y.Name == constraint.Context);

                IFormattedConstraint fc = new FormattedConstraint(this.tdb, igManager, igTypePlugin, theConstraint, this.constraintReferences.Cast<ConstraintReference>().ToList(), "#/volume2/", "#/valuesets/#", true, true, true, false);

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

                newConstraintModel.ContainedTemplates = (from cr in this.constraintReferences
                                                         where cr.TemplateConstraintId == theConstraint.Id
                                                         select new ViewDataModel.TemplateReference()
                                                         {
                                                             Identifier = cr.Identifier,
                                                             Bookmark = cr.Bookmark,
                                                             ImplementationGuide = cr.ImplementationGuide,
                                                             PublishDate = cr.PublishDate,
                                                             Name = cr.Name
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
                CreateConstraints(igManager, igTypePlugin, theConstraint.ChildConstraints, newConstraintModel.Constraints, null, nextSchemaObject);
            }
        }
    }
}
