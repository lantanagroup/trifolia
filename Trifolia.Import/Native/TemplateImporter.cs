using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using Trifolia.DB;
using Trifolia.Authorization;
using TDBTemplate = Trifolia.DB.Template;
using TDBTemplateConstraint = Trifolia.DB.TemplateConstraint;
using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using ImportTemplate = Trifolia.Shared.ImportExport.Model.TrifoliaTemplate;
using ImportConstraint = Trifolia.Shared.ImportExport.Model.ConstraintType;
using ImportSingleValueCode = Trifolia.Shared.ImportExport.Model.ConstraintTypeSingleValueCode;
using ImportValueSet = Trifolia.Shared.ImportExport.Model.ConstraintTypeValueSet;
using ImportCodeSystem = Trifolia.Shared.ImportExport.Model.ConstraintTypeCodeSystem;
using ImportConstraintSample = Trifolia.Shared.ImportExport.Model.ConstraintTypeSample;
using Trifolia.Logging;

namespace Trifolia.Import.Native
{
    public class TemplateImporter
    {
        private IObjectRepository tdb = null;
        private List<Trifolia.Shared.ImportExport.Model.TrifoliaTemplate> importTemplates = null;
        private List<string> errors = null;
        private List<TDBTemplate> importedTemplates = null;
        private bool shouldUpdate = false;
        private IEnumerable<ValueSet> valueSets;

        public ImplementationGuide DefaultImplementationGuide { get; set; }
        public User DefaultAuthorUser { get; set; }

        public List<string> Errors
        {
            get { return errors; }
            set { errors = value; }
        }

        public TemplateImporter(IObjectRepository tdb, List<ValueSet> addedValueSets = null, bool shouldUpdate = false)
        {
            this.tdb = tdb;
            this.shouldUpdate = shouldUpdate;

            if (addedValueSets != null)
                this.valueSets = tdb.ValueSets.Include("Identifiers").ToList().Concat(addedValueSets);
            else
                this.valueSets = tdb.ValueSets.Include("Identifiers");
        }

        public List<TDBTemplate> Import(List<ImportTemplate> importTemplates)
        {
            this.errors = new List<string>();
            this.importedTemplates = new List<TDBTemplate>();
            this.importTemplates = importTemplates;

            this.importTemplates.ForEach(y =>
            {
                AddImportTemplate(y);
            });

            return this.importedTemplates;
        }

        public List<TDBTemplate> Import(string importXml)
        {
            ImportModel import = null;

            try
            {
                using (StringReader sr = new StringReader(importXml))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ImportModel));
                    import = (ImportModel)serializer.Deserialize(sr);
                }
            }
            catch
            {
                return this.importedTemplates;
            }

            return Import(import.Template);
        }

        private bool ImportConstraintExists(ImportTemplate importTemplate, int number)
        {
            foreach (var importConstraint in importTemplate.Constraint)
            {
                if (ImportConstraintExists(importConstraint, number))
                    return true;
            }

            return false;
        }

        private bool ImportConstraintExists(ImportConstraint importConstraint, int number)
        {
            if (importConstraint.number == number)
                return true;

            foreach (var childConstraint in importConstraint.Constraint)
            {
                if (ImportConstraintExists(childConstraint, number))
                    return true;
            }

            return false;
        }

        private void UpdateTemplateProperties(Template template, ImportTemplate importTemplate)
        {
            if (template.Name != importTemplate.title)
                template.Name = importTemplate.title;

            if (template.Bookmark != importTemplate.bookmark)
                template.Bookmark = importTemplate.bookmark;

            if (template.IsOpen != importTemplate.isOpen)
                template.IsOpen = importTemplate.isOpen;

            if (AreStringsDifferent(template.Notes, importTemplate.Notes))
                template.Notes = importTemplate.Notes;

            if (AreStringsDifferent(template.Description, importTemplate.Description))
                template.Description = importTemplate.Description;

            if (template.PrimaryContext != importTemplate.context)
                template.PrimaryContext = importTemplate.context;

            if (template.PrimaryContextType != importTemplate.contextType)
                template.PrimaryContextType = importTemplate.contextType;
        }

        private void UpdateTemplateSamples(Template template, ImportTemplate importTemplate)
        {
            var foundSamples = new List<TemplateSample>();

            if (importTemplate.Sample != null && importTemplate.Sample.Count > 0)
            {
                foreach (var importSample in importTemplate.Sample)
                {
                    if (string.IsNullOrEmpty(importSample.name))
                    {
                        errors.Add("Template sample does not have a name");
                        continue;
                    }

                    // Look for a match sample by both name and sample text. Maybe they changed the name, or maybe the change the sample text.
                    // If they only changed one, then we should update the existing entry.
                    // If they updated both, we won't find a match, and should add a new one. All non-matched samples will be deleted
                    var foundSample = template.TemplateSamples.SingleOrDefault(y => y.Name.ToLower() == importSample.name.ToLower());

                    if (foundSample == null && importSample.Value != null)
                        foundSample = template.TemplateSamples.SingleOrDefault(y => y.XmlSample != null && y.XmlSample.ToLower().Replace("\r", "").Replace("\n", "") == importSample.Value.ToLower().Replace("\r", "").Replace("\n", ""));

                    if (foundSample == null)
                    {
                        foundSample = new TemplateSample()
                        {
                            Template = template,
                            Name = importSample.name,
                            XmlSample = importSample.Value
                        };
                        this.tdb.TemplateSamples.Add(foundSample);
                    }
                    else
                    {
                        if (foundSample.Name != importSample.name)
                            foundSample.Name = importSample.name;

                        if (AreStringsDifferent(foundSample.XmlSample, importSample.Value))
                            foundSample.XmlSample = importSample.Value;
                    }

                    foundSamples.Add(foundSample);
                }
            }

            var deleteSamples = (from ts in template.TemplateSamples
                                 where !foundSamples.Contains(ts)
                                 select ts).ToList();

            foreach (var deleteSample in deleteSamples)
                this.tdb.TemplateSamples.Remove(deleteSample);
        }

        private TDBTemplate AddImportTemplate(Trifolia.Shared.ImportExport.Model.TrifoliaTemplate importTemplate)
        {
            var alreadyBuildTemplate = this.importedTemplates.SingleOrDefault(y => y.Oid == importTemplate.identifier);

            if (alreadyBuildTemplate != null)
                return alreadyBuildTemplate;

            TDBTemplate foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == importTemplate.identifier);

            if (string.IsNullOrEmpty(importTemplate.identifier) || importTemplate.identifier.Length > 255)
            {
                errors.Add("Template OID is not specified.");
                return null;
            }

            if (string.IsNullOrEmpty(importTemplate.title) || importTemplate.title.Length > 255)
            {
                errors.Add("Template name is not specified.");
                return null;
            }

            var template = this.tdb.Templates.SingleOrDefaultInclAdded(y => y.Oid.ToLower() == importTemplate.identifier.ToLower());

            if (!shouldUpdate && template != null)
            {
                errors.Add("Template already exists and should not be updating");
                return template;
            }

            if (template == null)
            {
                template = new TDBTemplate();
                template.Oid = importTemplate.identifier;

                if (!template.IsIdentifierII() && !template.IsIdentifierOID() && !template.IsIdentifierURL())
                {
                    errors.Add("Template identifier " + template.Oid + " is not properly formatted. Must be urn:hl7ii:XXX:YYY or urn:oid:XXX or http(s)://XXX");
                    return null;
                }
                
                if (this.DefaultAuthorUser != null)
                    template.Author = this.DefaultAuthorUser;
                else 
                    template.Author = CheckPoint.Instance.GetUser(this.tdb);

                this.tdb.Templates.Add(template);
            }

            // Find implementation guide type
            ImplementationGuideType igType = this.tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name.ToLower() == importTemplate.implementationGuideType.ToLower());

            if (igType == null)
            {
                this.errors.Add(string.Format(
                    "Could not find implementation guide type \"{0}\" for template with identifier \"{1}\"",
                    importTemplate.implementationGuideType,
                    importTemplate.identifier));
                return null;
            }

            template.ImplementationGuideType = igType;

            if (importTemplate.ImplementationGuide != null && !string.IsNullOrEmpty(importTemplate.ImplementationGuide.name))
            {
                ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefaultInclAdded(y => 
                    y.Name.ToLower() == importTemplate.ImplementationGuide.name.ToLower() &&
                    (
                        (y.Version == null && importTemplate.ImplementationGuide.version == 1) || 
                        (y.Version != null && y.Version.Value == importTemplate.ImplementationGuide.version)
                    ));

                if (ig == null)
                {
                    this.errors.Add(string.Format(
                        "Could not find implementation guide \"{0}\" for template with identifier \"{1}\"",
                        importTemplate.ImplementationGuide.name,
                        importTemplate.identifier));
                    return null;
                }
                else if (ig.ImplementationGuideType != igType)
                {
                    this.errors.Add(string.Format(
                        "The implementation guide type for the implementation guide \"{0}\" is not the same as the import.",
                        importTemplate.ImplementationGuide.name));
                    return null;
                }

                if (template.OwningImplementationGuide != ig)
                    template.OwningImplementationGuide = ig;

                if (!ig.ChildTemplates.Contains(template))
                    ig.ChildTemplates.Add(template);
            }
            else if (this.DefaultImplementationGuide != null)
            {
                template.OwningImplementationGuide = this.DefaultImplementationGuide;
            }
            else
            {
                errors.Add("No implementation guide specified for template/profile: " + importTemplate.identifier);
                return null;
            }

            // Find the template type
            TemplateType templateType = igType.TemplateTypes.SingleOrDefault(y => y.Name.ToLower() == importTemplate.templateType.ToLower());

            if (templateType == null)
            {
                this.errors.Add(string.Format(
                    "Could not find template type \"{0}\" for template with identifier \"{1}\"",
                    importTemplate.templateType,
                    importTemplate.identifier));
                return null;
            }
            else if (template.TemplateType != templateType)
            {
                template.TemplateType = templateType;
            }

            // Find or build the implied template
            if (!string.IsNullOrEmpty(importTemplate.impliedTemplateOid))
            {
                if (importTemplate.impliedTemplateOid.ToLower() == importTemplate.identifier.ToLower())
                {
                    this.errors.Add(string.Format(
                        "Template with identifier \"{0}\" implies itself. Cannot import template.",
                        importTemplate.identifier));
                    return null;
                }

                TDBTemplate impliedTemplate = FindOrBuildTemplate(importTemplate.impliedTemplateOid);

                // If we didn't find the template in the database or in the import, create an error for the template and don't add it
                if (impliedTemplate == null)
                {
                    this.errors.Add(string.Format(
                        "Couldn't find implied template \"{0}\" in either the system or the import, for template with identifier \"{1}\"",
                        importTemplate.impliedTemplateOid,
                        importTemplate.identifier));
                    return null;
                }
                else
                {
                    if (template.ImpliedTemplate != impliedTemplate)
                        template.ImpliedTemplate = impliedTemplate;

                    if (!impliedTemplate.ImplyingTemplates.Contains(template))
                        impliedTemplate.ImplyingTemplates.Add(template);
                }
            }

            this.UpdateTemplateProperties(template, importTemplate);

            this.UpdateTemplateSamples(template, importTemplate);

            // Extensions
            foreach (var existingExtension in template.Extensions.ToList())
            {
                if (importTemplate.Extension.Count(y => y.identifier == existingExtension.Identifier) == 0)
                    this.tdb.TemplateExtensions.Remove(existingExtension);
            }

            foreach (var importExtension in importTemplate.Extension)
            {
                var foundExtension = template.Extensions.SingleOrDefault(y => y.Identifier == importExtension.identifier);

                if (foundExtension == null)
                {
                    foundExtension = new TemplateExtension();
                    template.Extensions.Add(foundExtension);
                }

                foundExtension.Type = importExtension.type;
                foundExtension.Value = importExtension.value;
            }

            try
            {
                // Add each of the template's constraints
                if (importTemplate.Constraint != null)
                    importTemplate.Constraint.ToList().ForEach(y => AddImportConstraint(template, null, y));
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Error importing constraints for template.", ex);
                return null;
            }

            // If the object is changed, make sure the user has permissions to the implementation guide
            if (this.tdb is TrifoliaDatabase)
            {
                var dataSource = this.tdb as TrifoliaDatabase;
                var templateState = dataSource.Entry(template).State;

                if (templateState == System.Data.Entity.EntityState.Unchanged)
                {
                    var constraintStates = (from c in template.ChildConstraints
                                            where dataSource.Entry(c).State != System.Data.Entity.EntityState.Unchanged
                                            select c);
                    var constraintSampleStates = (from c in template.ChildConstraints
                                                  join cs in this.tdb.TemplateConstraintSamples on c equals cs.Constraint
                                                  where dataSource.Entry(cs).State != System.Data.Entity.EntityState.Unchanged
                                                  select cs);
                    var templateSampleStates = (from s in template.TemplateSamples
                                                where dataSource.Entry(s).State != System.Data.Entity.EntityState.Unchanged
                                                select s);

                    if (constraintStates.Count() > 0 || constraintSampleStates.Count() > 0 || templateSampleStates.Count() > 0)
                        templateState = System.Data.Entity.EntityState.Modified;
                }

                if (templateState != System.Data.Entity.EntityState.Unchanged && !CheckPoint.Instance.GrantEditImplementationGuide(template.OwningImplementationGuide.Id) && !CheckPoint.Instance.IsDataAdmin)
                {
                    this.Errors.Add("You do not have permission to modify template \"" + template.Name + "\" with identifier " + template.Oid);
                    return null;
                }
            }
            
            this.importedTemplates.Add(template);

            return template;
        }

        private bool AreStringsDifferent(string value1, string value2)
        {
            if (string.IsNullOrEmpty(value1) && string.IsNullOrEmpty(value2))
                return false;

            var compareValue1 = value1 != null ?
                value1.Replace("\n", "").Replace("\r", "") :
                null;
            var compareValue2 = value2 != null ?
                value2.Replace("\n", "").Replace("\r", "") :
                null;

            return compareValue1 != compareValue2; 
        }

        private bool AreBooleansDifferent(bool? value1, bool value2, bool value2Specified)
        {
            if (value1 == null && !value2Specified)
                return false;

            return value1 != value2;
        }

        private string GetImportConformance(Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance importConformance)
        {
            switch (importConformance)
            {
                case Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance.SHALL:
                case Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance.SHOULD:
                case Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance.MAY:
                    return importConformance.ToString();
                case Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance.SHALLNOT:
                    return "SHALL NOT";
                case Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance.SHOULDNOT:
                    return "SHOULD NOT";
                case Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance.MAYNOT:
                    return "MAY NOT";
                default:
                    return string.Empty;
            }
        }

        private void UpdateConstraintProperties(TemplateConstraint constraint, ImportConstraint importConstraint)
        {
            var importConformance = GetImportConformance(importConstraint.conformance);

            if (constraint.Context != importConstraint.context)
                constraint.Context = importConstraint.context;

            if (AreStringsDifferent(constraint.Conformance, importConformance))
                constraint.Conformance = importConformance;

            if (constraint.Cardinality != importConstraint.cardinality)
                constraint.Cardinality = importConstraint.cardinality;

            if (constraint.DataType != importConstraint.dataType)
                constraint.DataType = importConstraint.dataType;

            if (constraint.IsBranch != importConstraint.isBranch)
                constraint.IsBranch = importConstraint.isBranch;

            if (constraint.IsBranchIdentifier != importConstraint.isBranchIdentifier)
                constraint.IsBranchIdentifier = importConstraint.isBranchIdentifier;

            if (constraint.IsSchRooted != importConstraint.isSchRooted)
                constraint.IsSchRooted = importConstraint.isSchRooted;

            if (AreBooleansDifferent(constraint.IsStatic, importConstraint.isStatic, importConstraint.isStaticSpecified))
                constraint.IsStatic = importConstraint.isStaticSpecified ? (bool?)importConstraint.isStatic : null;

            if (constraint.IsPrimitive != importConstraint.isPrimitive)
                constraint.IsPrimitive = importConstraint.isPrimitive;

            if (constraint.IsPrimitive && AreStringsDifferent(constraint.PrimitiveText, importConstraint.NarrativeText))
                constraint.PrimitiveText = importConstraint.NarrativeText;

            if (AreStringsDifferent(constraint.Schematron, importConstraint.SchematronTest))
                constraint.Schematron = importConstraint.SchematronTest;

            if (constraint.Number != importConstraint.number)
                constraint.Number = importConstraint.number;

            if (constraint.DisplayNumber != importConstraint.displayNumber)
                constraint.DisplayNumber = importConstraint.displayNumber;

            if (AreStringsDifferent(constraint.Notes, importConstraint.Notes))
                constraint.Notes = importConstraint.Notes;

            if (AreStringsDifferent(constraint.Label, importConstraint.Label))
                constraint.Label = importConstraint.Label;

            if (constraint.IsHeading != importConstraint.isHeading)
                constraint.IsHeading = importConstraint.isHeading;

            if (AreStringsDifferent(constraint.HeadingDescription, importConstraint.HeadingDescription))
                constraint.HeadingDescription = importConstraint.HeadingDescription;
        }

        private void UpdateConstraintSamples(TemplateConstraint constraint, ImportConstraint importConstraint)
        {
            List<TemplateConstraintSample> foundSamples = new List<TemplateConstraintSample>();

            if (importConstraint.Sample != null && importConstraint.Sample.Count > 0)
            {
                foreach (var importSample in importConstraint.Sample)
                {
                    if (string.IsNullOrEmpty(importSample.name))
                    {
                        errors.Add("Constraint sample does not have a name");
                        continue;
                    }

                    // Look for a match sample by both name and sample text. Maybe they changed the name, or maybe the change the sample text.
                    // If they only changed one, then we should update the existing entry.
                    // If they updated both, we won't find a match, and should add a new one. All non-matched samples will be deleted
                    var foundSample = constraint.Samples.SingleOrDefault(y => y.Name.ToLower() == importSample.name.ToLower());

                    if (foundSample == null && !string.IsNullOrEmpty(importSample.Value))
                        foundSample = constraint.Samples.SingleOrDefault(y => y.SampleText != null && y.SampleText.ToLower().Replace("\r", "").Replace("\n", "") == importSample.Value.ToLower().Replace("\r", "").Replace("\n", ""));

                    if (foundSample == null)
                    {
                        var newSample = new TemplateConstraintSample()
                        {
                            Constraint = constraint,
                            Name = importSample.name,
                            SampleText = importSample.Value
                        };
                        this.tdb.TemplateConstraintSamples.Add(newSample);
                    }
                    else
                    {
                        if (foundSample.Name != importSample.name)
                            foundSample.Name = importSample.name;

                        if (AreStringsDifferent(foundSample.SampleText, importSample.Value))
                            foundSample.SampleText = importSample.Value;
                    }

                    foundSamples.Add(foundSample);
                }
            }

            var deleteSamples = (from cs in constraint.Samples
                                 where !foundSamples.Contains(cs)
                                 select cs).ToList();

            foreach (var deleteSample in deleteSamples)
                this.tdb.TemplateConstraintSamples.Remove(deleteSample);
        }

        private void UpdateConstraintContainedTemplate(TemplateConstraint constraint, string containedTemplateOid)
        {
            // Find the contained template if one is specified
            if (!string.IsNullOrEmpty(containedTemplateOid))
            {
                if (containedTemplateOid.ToLower() == constraint.Template.Oid.ToLower())
                {
                    this.errors.Add(string.Format(
                        "Template with oid \"{0}\" has a constraint which contains the same template. Constraint cannot be imported.",
                        constraint.Template.Oid));
                    throw new Exception("Constraint has an error.");
                }

                TDBTemplate containedTemplate = FindOrBuildTemplate(containedTemplateOid);

                if (containedTemplate == null)
                {
                    this.errors.Add(string.Format(
                        "Could not find contained template \"{0}\" for constraint with number \"{1}\" in template with oid \"{2}\"",
                        containedTemplateOid,
                        constraint.Number,
                        constraint.Template.Oid));
                    // TODO: Eventually add add a relationship that is not bound to a template stored in the database
                }

                if (constraint.ContainedTemplate != containedTemplate)
                    constraint.ContainedTemplate = containedTemplate;
            }
        }

        private void UpdateConstraintBinding(TemplateConstraint constraint, ImportConstraint importConstraint)
        {
            if (importConstraint.Item is ImportSingleValueCode)
            {
                ImportSingleValueCode importSvc = importConstraint.Item as ImportSingleValueCode;

                if (constraint.Value != importSvc.code)
                    constraint.Value = importSvc.code;

                if (constraint.DisplayName != importSvc.displayName)
                    constraint.DisplayName = importSvc.displayName;
            }
            else if (importConstraint.Item is ImportValueSet)
            {
                ImportValueSet importVs = importConstraint.Item as ImportValueSet;
                bool vsHasError = false;

                if (AreBooleansDifferent(constraint.IsStatic, importVs.isStatic, importVs.isStaticSpecified))
                    constraint.IsStatic = importVs.isStatic;

                // Old bug in Trifolia allowing the same value set identifier to be used more than once
                var foundValueSets = (from vsi in this.valueSets.SelectMany(y => y.Identifiers)
                                      where vsi.Identifier.ToLower().Trim() == importVs.identifier.ToLower().Trim()
                                      select vsi.ValueSet).Distinct().ToList();

                if (foundValueSets.Count() == 0)
                {
                    var newValueSet = new ValueSet()
                    {
                        Name = string.IsNullOrEmpty(importVs.name) ? importVs.identifier + " incomplete" : importVs.name,
                        IsIncomplete = true,
                        Description = "Automatically generated by template import",
                        LastUpdate = DateTime.Now
                    };
                    
                    var newValueSetIdentifier = new ValueSetIdentifier();
                    newValueSetIdentifier.Identifier = importVs.identifier;

                    if (importVs.identifier.StartsWith("http://") || importVs.identifier.StartsWith("https://"))
                    {
                        newValueSetIdentifier.Type = ValueSetIdentifierTypes.HTTP;
                    }
                    else if (importVs.identifier.StartsWith("urn:oid:"))
                    {
                        newValueSetIdentifier.Type = ValueSetIdentifierTypes.Oid;
                    }
                    else if (importVs.identifier.StartsWith("urn:hl7ii:"))
                    {
                        newValueSetIdentifier.Type = ValueSetIdentifierTypes.HL7II;
                    }
                    else
                    {
                        this.Errors.Add("Value set referenced by constraint has incorrect identifier (" + importVs.identifier + ")");
                        vsHasError = true;
                    }

                    if (!vsHasError)
                    {
                        newValueSet.Identifiers.Add(newValueSetIdentifier);
                        this.tdb.ValueSets.Add(newValueSet);
                        foundValueSets.Add(newValueSet);
                    }
                }

                if (vsHasError)
                    return;

                if (constraint.ValueSet != null && foundValueSets.Contains(constraint.ValueSet))
                    return;

                if (constraint.ValueSet != foundValueSets.First())
                    constraint.ValueSet = foundValueSets.First();
            }
        }

        private void UpdateConstraintCodeSystem(TemplateConstraint constraint, ImportConstraint importConstraint)
        {
            if (importConstraint.CodeSystem != null && !string.IsNullOrEmpty(importConstraint.CodeSystem.identifier))
            {
                ImportCodeSystem importCs = importConstraint.CodeSystem;
                CodeSystem foundCodeSystem = this.tdb.CodeSystems.FirstOrDefault(y => y.Oid.ToLower() == importCs.identifier.ToLower());

                if (foundCodeSystem == null)
                {
                    if (string.IsNullOrEmpty(importCs.name))
                    {
                        this.errors.Add(string.Format(
                            "Code System with oid \"{0}\" could not be found for constraint with number \"{1}\" in template with identifier \"{2}\"",
                            importCs.identifier,
                            importConstraint.number,
                            constraint.Template.Oid));
                        throw new Exception("Constraint has an error.");
                    }

                    foundCodeSystem = new CodeSystem()
                    {
                        Name = importCs.name,
                        Oid = importCs.identifier,
                        Description = "Automatically generated by template import"
                    };
                    this.tdb.CodeSystems.Add(foundCodeSystem);
                }

                constraint.CodeSystem = foundCodeSystem;
            }
        }

        private void UpdateConstraintCategories(TemplateConstraint constraint, ImportConstraint importConstraint)
        {
            if (importConstraint.Category == null || importConstraint.Category.Count == 0)
            {
                if (!string.IsNullOrEmpty(constraint.Category))
                    constraint.Category = string.Empty;
                return;
            }

            var categories = importConstraint.Category.Select(y => y.name.Replace(',', '-'));
            var categoriesString = String.Join(",", categories);

            if (AreStringsDifferent(constraint.Category, categoriesString))
                constraint.Category = categoriesString;
        }

        private void AddImportConstraint(TDBTemplate template, TDBTemplateConstraint parentConstraint, ImportConstraint importConstraint)
        {
            if (importConstraint.isVerbose)
                return;

            TDBTemplateConstraint constraint = null;

            if (importConstraint.numberSpecified)
                constraint = template.ChildConstraints.SingleOrDefault(y => y.ParentConstraint == parentConstraint && y.Number == importConstraint.number);
            else if (!string.IsNullOrEmpty(importConstraint.displayNumber))
                constraint = template.ChildConstraints.SingleOrDefault(y => y.ParentConstraint == parentConstraint && y.DisplayNumber == importConstraint.displayNumber);

            if (constraint == null)
            {
                constraint = new TDBTemplateConstraint();
                constraint.Number = importConstraint.number;
                constraint.DisplayNumber = importConstraint.displayNumber;
                constraint.Template = template;
                constraint.ParentConstraint = parentConstraint;
                this.tdb.TemplateConstraints.Add(constraint);

                // TODO: Order the constraint? Or let the template re-order the constraints when it is edited?
            }

            this.UpdateConstraintProperties(constraint, importConstraint);

            this.UpdateConstraintContainedTemplate(constraint, importConstraint.containedTemplateOid);

            this.UpdateConstraintBinding(constraint, importConstraint);

            this.UpdateConstraintCodeSystem(constraint, importConstraint);

            this.UpdateConstraintCategories(constraint, importConstraint);

            this.UpdateConstraintSamples(constraint, importConstraint);

            // Add each of the constraint's child constraints
            if (importConstraint.Constraint != null)
                importConstraint.Constraint.ToList().ForEach(y => AddImportConstraint(template, constraint, y));
        }

        private TDBTemplate FindOrBuildTemplate(string identifier)
        {
            TDBTemplate template = this.tdb.Templates.SingleOrDefaultInclAdded(y => y.Oid.ToLower() == identifier.ToLower());

            // If we didn't find the implied template in the database, check our import
            if (template == null)
            {
                // Try to find if the template has already been built in the import
                template = this.importedTemplates.SingleOrDefault(y => y.Oid.ToLower() == identifier.ToLower());

                // If it hasn't been built yet, then find the template in the import and build it
                if (template == null)
                {
                    ImportTemplate importImpliedTemplate = this.importTemplates.SingleOrDefault(y => y.identifier.ToLower() == identifier.ToLower());

                    if (importImpliedTemplate != null)
                        template = AddImportTemplate(importImpliedTemplate);
                }
            }

            return template;
        }
    }
}
