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

namespace Trifolia.Shared.ImportExport
{
    public class TemplateImporter
    {
        private IObjectRepository tdb = null;
        private List<Trifolia.Shared.ImportExport.Model.TrifoliaTemplate> importTemplates = null;
        private List<string> errors = null;
        private List<TDBTemplate> importedTemplates = null;
        private bool shouldUpdate = false;

        public ImplementationGuide DefaultImplementationGuide { get; set; }
        public User DefaultAuthorUser { get; set; }

        public List<string> Errors
        {
            get { return errors; }
            set { errors = value; }
        }

        public TemplateImporter(IObjectRepository tdb, bool shouldUpdate = false)
        {
            this.tdb = tdb;
            this.shouldUpdate = shouldUpdate;
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

            var template = this.tdb.Templates.SingleOrDefault(y => y.Oid.ToLower() == importTemplate.identifier.ToLower());

            if (!shouldUpdate && template != null)
            {
                errors.Add("Template already exists and should not be updating");
                return template;
            }

            if (template == null)
            {
                template = new TDBTemplate();
                template.Author = this.DefaultAuthorUser;
                template.Oid = importTemplate.identifier;
                
                if (this.DefaultAuthorUser != null)
                    template.Author = this.DefaultAuthorUser;
                else 
                    template.Author = CheckPoint.Instance.GetUser(this.tdb);

                this.tdb.Templates.AddObject(template);
            }

            this.UpdateTemplateProperties(template, importTemplate);

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
                ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => 
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

            // Extensions
            foreach (var existingExtension in template.Extensions.ToList())
            {
                if (importTemplate.Extension.Count(y => y.identifier == existingExtension.Identifier) == 0)
                    template.Extensions.Remove(existingExtension);
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
            catch
            {
                return null;
            }
            
            this.importedTemplates.Add(template);

            return template;
        }

        private bool AreStringsDifferent(string value1, string value2)
        {
            if (string.IsNullOrEmpty(value1) && string.IsNullOrEmpty(value2))
                return false;

            return value1 != value2;
        }

        private bool AreBooleansDifferent(bool? value1, bool value2, bool value2Specified)
        {
            if (value1 == null && !value2Specified)
                return false;

            return value1 != value2;
        }

        private void UpdateConstraintProperties(TemplateConstraint constraint, ImportConstraint importConstraint)
        {
            if (constraint.Context != importConstraint.context)
                constraint.Context = importConstraint.context;

            if (constraint.Conformance != importConstraint.conformance.ToString())
                constraint.Conformance = importConstraint.conformance.ToString();

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
                    throw new Exception("Constraint has an error.");
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

                if (AreBooleansDifferent(constraint.IsStatic, importVs.isStatic, importVs.isStaticSpecified))
                    constraint.IsStatic = importVs.isStatic;

                ValueSet foundValueSet = this.tdb.ValueSets.SingleOrDefault(y => y.Oid.ToLower() == importVs.oid.ToLower());

                if (foundValueSet == null)
                {
                    foundValueSet = new ValueSet()
                    {
                        Oid = importVs.oid,
                        Name = importVs.oid + " incomplete",
                        IsIncomplete = true,
                        Description = "Automatically generated by template import",
                        LastUpdate = DateTime.Now
                    };
                    this.tdb.ValueSets.AddObject(foundValueSet);
                }

                if (constraint.ValueSet != foundValueSet)
                    constraint.ValueSet = foundValueSet;
            }
        }

        private void UpdateConstraintCodeSystem(TemplateConstraint constraint, ImportConstraint importConstraint)
        {
            if (importConstraint.CodeSystem != null && !string.IsNullOrEmpty(importConstraint.CodeSystem.oid))
            {
                ImportCodeSystem importCs = importConstraint.CodeSystem;
                CodeSystem foundCodeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Oid.ToLower() == importCs.oid.ToLower());

                if (foundCodeSystem == null)
                {
                    this.errors.Add(string.Format(
                        "Code System with oid \"{0}\" could not be found for constraint with number \"{1}\" in template with oid \"{2}\"",
                        importCs.oid,
                        importConstraint.number,
                        constraint.Template.Oid));
                    throw new Exception("Constraint has an error.");
                }

                constraint.CodeSystem = foundCodeSystem;
            }
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
                this.tdb.TemplateConstraints.AddObject(constraint);

                // TODO: Order the constraint? Or let the template re-order the constraints when it is edited?
            }

            this.UpdateConstraintProperties(constraint, importConstraint);

            this.UpdateConstraintContainedTemplate(constraint, importConstraint.containedTemplateOid);

            this.UpdateConstraintBinding(constraint, importConstraint);

            this.UpdateConstraintCodeSystem(constraint, importConstraint);

            // Add each of the constraint's child constraints
            if (importConstraint.Constraint != null)
                importConstraint.Constraint.ToList().ForEach(y => AddImportConstraint(template, constraint, y));
        }

        private TDBTemplate FindOrBuildTemplate(string identifier)
        {
            TDBTemplate template = this.tdb.Templates.SingleOrDefault(y => y.Oid.ToLower() == identifier.ToLower());

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
