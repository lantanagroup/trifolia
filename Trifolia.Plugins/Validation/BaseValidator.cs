using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Plugins;
using Trifolia.Shared.Validation;

namespace Trifolia.Plugins.Validation
{
    public abstract class BaseValidator : IValidator
    {
        protected IObjectRepository tdb;

        public BaseValidator(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public ValidationResults ValidateImplementationGuide(int implementationGuideId)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            var igSchema = implementationGuide.ImplementationGuideType.GetSimpleSchema();

            return this.ValidateImplementationGuide(implementationGuide, igSchema);
        }

        protected virtual ValidationResults ValidateImplementationGuide(ImplementationGuide implementationGuide, SimpleSchema igSchema)
        {
            ValidationResults results = new ValidationResults();

            var plugin = implementationGuide.ImplementationGuideType.GetPlugin();
            var childTemplateIds = implementationGuide.ChildTemplates.Select(y => y.Id);
            var containedTemplateIds = (from vtr in this.tdb.ViewTemplateRelationships.AsNoTracking()
                                      join pt in childTemplateIds on vtr.ParentTemplateId equals pt
                                      join ct in this.tdb.Templates.AsNoTracking() on vtr.ChildTemplateId equals ct.Id
                                      select ct.Id).Distinct();
            var containedTemplates = (from cti in containedTemplateIds
                                      join t in this.tdb.Templates.AsNoTracking() on cti equals t.Id
                                      select t).ToList();
            var valueSets = (from tc in this.tdb.TemplateConstraints
                             join t in this.tdb.Templates on tc.TemplateId equals t.Id
                             join vs in this.tdb.ValueSets on tc.ValueSetId equals vs.Id
                             join cti in childTemplateIds on t.Id equals cti
                             select vs);
            var valueSetCodesWithWhitespace = (from vsm in this.tdb.ViewValueSetMemberWhiteSpaces
                                               join vs in valueSets on vsm.ValueSetId equals vs.Id
                                               select new
                                               {
                                                   ValueSet = vs,
                                                   ValueSetMember = vsm
                                               });

            foreach (var template in implementationGuide.ChildTemplates)
            {
                var result = new TemplateValidationResult()
                {
                    Id = template.Id,
                    Name = template.Name,
                    Oid = template.Oid,
                    Items = this.ValidateTemplate(template, igSchema, containedTemplates)
                };

                if (result.Items.Count > 0)
                    results.TemplateResults.Add(result);
            }

            foreach (var vscww in valueSetCodesWithWhitespace)
            {
                var identifier = vscww.ValueSet.GetIdentifier(plugin);

                if (vscww.ValueSetMember.Code.Trim() != vscww.ValueSetMember.Code)
                    results.Messages.Add("Value set \"" + vscww.ValueSet.Name + "\" (" + identifier + ") has a code \"" + vscww.ValueSetMember.Code + "\" with leading or trailing white spaces.");

                if (vscww.ValueSetMember.DisplayName.Trim() != vscww.ValueSetMember.DisplayName)
                    results.Messages.Add("Value set \"" + vscww.ValueSet.Name + "\" (" + identifier + ") has a code \"" + vscww.ValueSetMember.Code + "\" whose display name \"" + vscww.ValueSetMember.DisplayName + "\" has leading or trailing white spaces.");
            }

            if (implementationGuide.HasImportedValueSets(this.tdb, ValueSetImportSources.VSAC))
            {
                User currentUser = CheckPoint.Instance.GetUser(this.tdb);

                try
                {
                    if (!currentUser.HasValidUmlsLicense())
                    {
                        results.RestrictDownload = true;
                        results.Messages.Add("This implementation guide contains VSAC content that you do not currently have a license to. <a href=\"/Account/MyProfile\">Update your profile</a> with your UMLS/VSAC credentials to export this implementation guide.");
                    }
                }
                catch (Exception ex)
                {
                    results.RestrictDownload = true;
                    results.Messages.Add("This implementation guide contains VSAC content. Your UMLS license could not be verified due to an error. Please submit a support request related to this message.");
                    Logging.Log.For(this).Error("Error checking if the user {0} has a valid UMLS license", ex, currentUser.UserName);
                    throw ex;
                }
            }

            return results;
        }

        public List<ValidationResult> ValidateTemplate(int templateId)
        {
            Template template = this.tdb.Templates.Single(y => y.Id == templateId);
            SimpleSchema igSchema = template.OwningImplementationGuide.ImplementationGuideType.GetSimpleSchema();

            return ValidateTemplate(template, igSchema);
        }

        public virtual List<ValidationResult> ValidateTemplate(Template template, SimpleSchema igSchema, IEnumerable<Template> allContainedTemplates = null)
        {
            SimpleSchema templateSchema = template.GetSchema(igSchema);

            List<ValidationResult> results = new List<ValidationResult>();

            var igType = template.ImplementationGuideType;

            if (igType.SchemaURI == ImplementationGuideType.FHIR_NS)
            {
                if (!template.IsIdentifierURL())
                    results.Add(ValidationResult.CreateResult(ValidationLevels.Error, "FHIR profiles should use HTTP(S) identifiers."));

                if (template.Oid.IndexOf("_") >= 0)
                    results.Add(ValidationResult.CreateResult(ValidationLevels.Warning, "FHIR profile's long identifier should use - (dash) instead of _ (underscore)"));

                if (template.Bookmark.IndexOf("_") >= 0)
                    results.Add(ValidationResult.CreateResult(ValidationLevels.Warning, "FHIR profile's short identifier should use - (dash) instead of _ (underscore)"));

                if (template.IsIdentifierURL() && !template.Oid.EndsWith(template.Bookmark))
                    results.Add(ValidationResult.CreateResult(ValidationLevels.Error, "FHIR profile's long identifier should end with the short identifier"));
            }

            XmlDocument tempDoc = new XmlDocument();
            XPathNavigator xpathNavigator = tempDoc.CreateNavigator();

            if (templateSchema == null)
            {
                results.Add(
                    ValidationResult.CreateResult(ValidationLevels.Error, "Template context is not found within the schema"));
            }
            else
            {
                List<TemplateConstraint> rootConstraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null).ToList();
                rootConstraints.ForEach(y =>
                {
                    // The first child in the schema is the complex type itself
                    ValidateTemplateConstraint(template, xpathNavigator, results, templateSchema, templateSchema.Children, y, allContainedTemplates);
                });
            }

            return results;
        }

        /// <summary>
        /// Perform validations on a single constraint.
        /// </summary>
        /// <param name="results">The list of errors that should be added to when a constraint has an error matching the schema</param>
        /// <param name="schemaObjects">The list of sibling-level schema objects that the constraint should be compared to</param>
        /// <param name="currentConstraint">The current constraint to match against the schema</param>
        public void ValidateTemplateConstraint(
            Template template, 
            XPathNavigator xpathNavigator, 
            List<ValidationResult> results, 
            SimpleSchema schema, 
            List<SimpleSchema.SchemaObject> schemaObjects, 
            TemplateConstraint currentConstraint,
            IEnumerable<Template> allContainedTemplates)
        {
            List<TemplateConstraint> childConstraints = currentConstraint.ChildConstraints.ToList();

            if (!string.IsNullOrEmpty(currentConstraint.Schematron))
            {
                try
                {
                    XPathExpression expr = xpathNavigator.Compile(currentConstraint.Schematron);
                }
                catch (XPathException ex)
                {
                    results.Add(ValidationResult.CreateResult(template.Id, template.Name, currentConstraint.Number.Value, ValidationLevels.Error, "Custom schematron is not valid: " + ex.Message));
                }
            }

            if (currentConstraint.IsPrimitive && string.IsNullOrEmpty(currentConstraint.PrimitiveText))
            {
                results.Add(ValidationResult.CreateResult(template.Id, template.Name, currentConstraint.Number.Value, ValidationLevels.Error, "Primitive does not have any narrative text."));
                return;
            }
            else if (!string.IsNullOrEmpty(currentConstraint.Context))
            {
                string context = currentConstraint.Context;
                bool isAttribute = context.StartsWith("@");

                // If it is an attribute, then we need to remove the @ from the context
                if (isAttribute)
                    context = context.Substring(1);

                SimpleSchema.SchemaObject foundSchemaObject = schemaObjects.SingleOrDefault(y => y.Name.ToLower() == context.ToLower() && y.IsAttribute == isAttribute);

                // Verify that the template (if specified) matches the datatype of the constraints
                if (foundSchemaObject != null)
                {
                    var templates = allContainedTemplates != null ? allContainedTemplates : this.tdb.Templates.AsEnumerable();
                    var containedTemplates = (from tcr in currentConstraint.References
                                              join t in templates on tcr.ReferenceIdentifier equals t.Oid
                                              where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                              select t);
                    string constraintDataType = !string.IsNullOrEmpty(currentConstraint.DataType) ? currentConstraint.DataType : foundSchemaObject.DataType;

                    foreach (var containedTemplate in containedTemplates)
                    {
                        string containedTemplateDataType = containedTemplate.PrimaryContextType;

                        bool isFhirResourceReference = schema.Schema.TargetNamespace == "http://hl7.org/fhir" && (constraintDataType == "ResourceReference" || constraintDataType == "Reference");

                        if (!isFhirResourceReference && containedTemplateDataType.ToLower() != constraintDataType.ToLower())
                        {
                            results.Add(ValidationResult.CreateResult(
                                template.Id,
                                template.Name,
                                currentConstraint.Number.Value,
                                ValidationLevels.Error,
                                "Contained template \"{0}\" has a type of \"{1}\" which does not match the containing element \"{2}\"",
                                containedTemplate.Oid,
                                containedTemplateDataType,
                                constraintDataType));
                        }
                    }
                }

                // Verify that branched elements have at least one identifier
                if (currentConstraint.IsBranch && childConstraints.Count(y => y.IsBranchIdentifier) == 0)
                    results.Add(ValidationResult.CreateResult(template.Id, template.Name, currentConstraint.Number.Value, ValidationLevels.Warning, "Branched constraint \"{0}\" does not have any identifiers associated with it.", currentConstraint.GetXpath()));

                // Verify that a schema-object can be matched to this constraint
                if (foundSchemaObject == null)
                {
                    results.Add(ValidationResult.CreateResult(template.Id, template.Name, currentConstraint.Number.Value, ValidationLevels.Error, "Constraint has context of \"{0}\" which is not found in the schema.", currentConstraint.GetXpath()));
                    return; // Do not process child constraints when the parent is not matched to the schema
                }
                else if (foundSchemaObject.Cardinality != null && currentConstraint.Cardinality != null)
                {
                    var siblings = currentConstraint.ParentConstraint != null ? currentConstraint.ParentConstraint.ChildConstraints : template.ChildConstraints.Where(y => y.ParentConstraint == null);
                    var isDuplicate = siblings.Count(y => y.Context == currentConstraint.Context) > 1;

                    // Warn when a constraint that is associated with the schema that has multiple cardinality but is not branched
                    if (template.IsOpen &&
                        !foundSchemaObject.Cardinality.EndsWith("..1") &&
                        !foundSchemaObject.Cardinality.EndsWith("..0") &&
                        !currentConstraint.IsBranch &&
                        isDuplicate)
                    {
                        var error = ValidationResult.CreateResult(template.Id, template.Name, currentConstraint.Number.Value, ValidationLevels.Warning, "Schema allows multiple for \"{0}\" but the constraint is not branched. Consider branching this constraint.", currentConstraint.GetXpath());
                        results.Add(error);
                    }

                    if (foundSchemaObject.Cardinality.EndsWith("..1") && !(currentConstraint.Cardinality.EndsWith("..0") || currentConstraint.Cardinality.EndsWith("..1")))
                    {
                        var error = ValidationResult.CreateResult(template.Id, template.Name, currentConstraint.Number.Value, ValidationLevels.Error, "The cardinality for the element is loosened in the constraint from the underlying schema.");
                        results.Add(error);
                    }
                }

                List<SimpleSchema.SchemaObject> childSchemaObjects = foundSchemaObject.Children;

                // If a data-type is specified, then we should find the data-type within the schema, since it is likely different than what
                // is specified by default.
                if (!string.IsNullOrEmpty(currentConstraint.DataType))
                {
                    SimpleSchema.SchemaObject foundSchemaTypeObject = schema.FindFromType(currentConstraint.DataType);

                    if (foundSchemaTypeObject != null)
                        childSchemaObjects = foundSchemaTypeObject.Children;
                }

                childConstraints.ForEach(y => ValidateTemplateConstraint(template, xpathNavigator, results, schema, childSchemaObjects, y, allContainedTemplates));
            }
        }
    }
}
