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
using ImportModel = Trifolia.Shared.ImportExport.Model.TemplateExport;
using ImportTemplate = Trifolia.Shared.ImportExport.Model.TemplateExportTemplate;
using ImportConstraint = Trifolia.Shared.ImportExport.Model.ConstraintType;
using ImportSingleValueCode = Trifolia.Shared.ImportExport.Model.ConstraintTypeSingleValueCode;
using ImportValueSet = Trifolia.Shared.ImportExport.Model.ConstraintTypeValueSet;
using ImportCodeSystem = Trifolia.Shared.ImportExport.Model.ConstraintTypeCodeSystem;

namespace Trifolia.Shared.ImportExport
{
    public class TemplateImporter
    {
        private IObjectRepository tdb = null;
        private List<ImportTemplate> importTemplates = null;
        private List<string> errors = null;
        private List<TDBTemplate> newTemplates = null;
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

        public List<TDBTemplate> Import(string importXml)
        {
            ImportModel import = null;

            this.errors = new List<string>();
            this.newTemplates = new List<TDBTemplate>();

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
                return this.newTemplates;
            }

            this.importTemplates = new List<ImportTemplate>(import.Template);

            this.importTemplates.ForEach(y =>
            {
                AddImportTemplate(y);
            });

            return this.newTemplates;
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

        private TDBTemplate AddImportTemplate(ImportTemplate importTemplate)
        {
            var alreadyBuildTemplate = this.newTemplates.SingleOrDefault(y => y.Oid == importTemplate.identifier);

            if (alreadyBuildTemplate != null)
                return alreadyBuildTemplate;

            TDBTemplate foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == importTemplate.identifier);

            if (foundTemplate != null)
            {
                // Only update templates when the user says to
                if (!shouldUpdate)
                    throw new Exception("Cannot create template that already exists: " + importTemplate.identifier);

                if (!CheckPoint.Instance.IsDataAdmin && CheckPoint.Instance.GrantEditTemplate(foundTemplate.Id))
                    throw new AuthorizationException("You do not have permission to edit template " + foundTemplate.Oid);

                this.tdb.Templates.DeleteObject(foundTemplate);

                var constraints = this.tdb.TemplateConstraints.Where(y => y.Template == foundTemplate).ToList();
                foreach (var constraint in constraints)
                {
                    this.tdb.TemplateConstraints.DeleteObject(constraint);
                }
            }

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

            TDBTemplate newTemplate = new TDBTemplate()
            {
                Oid = importTemplate.identifier,
                Name = importTemplate.title,
                Bookmark = importTemplate.bookmark,
                IsOpen = importTemplate.isOpen,
                Notes = importTemplate.Notes,
                Description = importTemplate.Description,
                PrimaryContext = importTemplate.context,
                PrimaryContextType = importTemplate.contextType,
                Author = this.DefaultAuthorUser
            };

            this.newTemplates.Add(newTemplate);
            this.tdb.Templates.AddObject(newTemplate);

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

            newTemplate.ImplementationGuideType = igType;
            newTemplate.ImplementationGuideTypeId = igType.Id;

            if (!string.IsNullOrEmpty(importTemplate.owningImplementationGuideName))
            {
                ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Name.ToLower() == importTemplate.owningImplementationGuideName.ToLower());

                if (ig == null)
                {
                    this.errors.Add(string.Format(
                        "Could not find implementation guide \"{0}\" for template with identifier \"{1}\"",
                        importTemplate.owningImplementationGuideName,
                        importTemplate.identifier));
                    return null;
                }
                else if (ig.ImplementationGuideType != igType)
                {
                    this.errors.Add(string.Format(
                        "The implementation guide type for the implementation guide \"{0}\" is not the same as the import.",
                        importTemplate.owningImplementationGuideName));
                    return null;
                }

                newTemplate.OwningImplementationGuide = ig;
                newTemplate.OwningImplementationGuideId = ig.Id;

                if (!ig.ChildTemplates.Contains(newTemplate))
                    ig.ChildTemplates.Add(newTemplate);
            }
            else
            {
                newTemplate.OwningImplementationGuide = this.DefaultImplementationGuide;
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
            else
            {
                newTemplate.TemplateType = templateType;
                newTemplate.TemplateTypeId = templateType.Id;
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
                    newTemplate.ImpliedTemplate = impliedTemplate;
                    impliedTemplate.ImplyingTemplates.Add(newTemplate);

                    if (newTemplate.EntityState == System.Data.Entity.EntityState.Modified || newTemplate.EntityState == System.Data.Entity.EntityState.Unchanged)
                        newTemplate.ImpliedTemplateId = impliedTemplate.Id;
                }
            }

            // Extensions
            foreach (var existingExtension in newTemplate.Extensions.ToList())
            {
                if (importTemplate.Extension.Count(y => y.identifier == existingExtension.Identifier) == 0)
                    newTemplate.Extensions.Remove(existingExtension);
            }

            foreach (var importExtension in importTemplate.Extension)
            {
                var foundExtension = newTemplate.Extensions.SingleOrDefault(y => y.Identifier == importExtension.identifier);

                if (foundExtension == null)
                {
                    foundExtension = new TemplateExtension();
                    newTemplate.Extensions.Add(foundExtension);
                }

                foundExtension.Type = importExtension.type;
                foundExtension.Value = importExtension.value;
            }

            // Add each of the template's constraints
            if (importTemplate.Constraint != null)
                importTemplate.Constraint.ToList().ForEach(y => AddImportConstraint(newTemplate, null, y));

            return newTemplate;
        }

        private void AddImportConstraint(TDBTemplate template, TDBTemplateConstraint parentConstraint, ImportConstraint importConstraint)
        {
            if (importConstraint.isVerbose)
                return;

            TDBTemplateConstraint newConstraint = new TDBTemplateConstraint()
            {
                // Ignoring ID because this is a new constraint
                Template = template,
                ParentConstraint = parentConstraint,
                Context = importConstraint.context,
                Conformance = importConstraint.conformance.ToString(),
                Cardinality = importConstraint.cardinality,
                DataType = importConstraint.dataType,
                IsBranch = importConstraint.isBranch,
                IsBranchIdentifier = importConstraint.isBranchIdentifier,
                IsSchRooted = importConstraint.isSchRooted,
                IsStatic = importConstraint.isStatic,
                IsPrimitive = importConstraint.isPrimitive,
                PrimitiveText = importConstraint.NarrativeText,
                Schematron = importConstraint.SchematronTest,
                Number = importConstraint.number
            };

            // Find the contained template if one is specified
            if (!string.IsNullOrEmpty(importConstraint.containedTemplateOid))
            {
                if (importConstraint.containedTemplateOid.ToLower() == template.Oid.ToLower())
                {
                    this.errors.Add(string.Format(
                        "Template with oid \"{0}\" has a constraint which contains the same template. Constraint cannot be imported.",
                        template.Oid));
                    throw new Exception("Constraint has an error.");
                }

                TDBTemplate containedTemplate = FindOrBuildTemplate(importConstraint.containedTemplateOid);

                if (containedTemplate == null)
                {
                    this.errors.Add(string.Format(
                        "Could not find contained template \"{0}\" for constraint with id \"{1}\" in template with oid \"{2}\"",
                        importConstraint.containedTemplateOid,
                        importConstraint.number,
                        template.Oid));
                    throw new Exception("Constraint has an error.");
                }

                if (containedTemplate.EntityState == System.Data.Entity.EntityState.Unchanged || containedTemplate.EntityState == System.Data.Entity.EntityState.Modified)
                    newConstraint.ContainedTemplateId = containedTemplate.Id;
                
                newConstraint.ContainedTemplate = containedTemplate;
            }

            if (importConstraint.Item is ImportSingleValueCode)
            {
                ImportSingleValueCode importSvc = importConstraint.Item as ImportSingleValueCode;

                newConstraint.Value = importSvc.code;
                newConstraint.DisplayName = importSvc.displayName;
            }
            else if (importConstraint.Item is ImportValueSet)
            {
                ImportValueSet importVs = importConstraint.Item as ImportValueSet;

                newConstraint.IsStatic = importVs.isStatic;

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

                newConstraint.ValueSet = foundValueSet;
            }
            
            if (importConstraint.CodeSystem != null && !string.IsNullOrEmpty(importConstraint.CodeSystem.oid))
            {
                ImportCodeSystem importCs = importConstraint.CodeSystem;
                CodeSystem foundCodeSystem = this.tdb.CodeSystems.SingleOrDefault(y => y.Oid.ToLower() == importCs.oid.ToLower());

                if (foundCodeSystem == null)
                {
                    this.errors.Add(string.Format(
                        "Code System with oid \"{0}\" could not be found for constraint with id \"{1}\" in template with oid \"{2}\"",
                        importCs.oid,
                        importConstraint.number,
                        template.Oid));
                    throw new Exception("Constraint has an error.");
                }

                newConstraint.CodeSystem = foundCodeSystem;
                newConstraint.CodeSystemId = foundCodeSystem.Id;
            }

            // Add the constraint to the template
            this.tdb.TemplateConstraints.AddObject(newConstraint);
            template.ChildConstraints.Add(newConstraint);

            // Add each of the constraint's child constraints
            if (importConstraint.Constraint != null)
                importConstraint.Constraint.ToList().ForEach(y => AddImportConstraint(template, newConstraint, y));
        }

        private TDBTemplate FindOrBuildTemplate(string identifier)
        {
            TDBTemplate template = this.tdb.Templates.SingleOrDefault(y => y.Oid.ToLower() == identifier.ToLower());

            // If we didn't find the implied template in the database, check our import
            if (template == null)
            {
                // Try to find if the template has already been built in the import
                template = this.newTemplates.SingleOrDefault(y => y.Oid.ToLower() == identifier.ToLower());

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
