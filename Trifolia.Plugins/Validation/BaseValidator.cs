using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.Validation;

namespace Trifolia.Plugins.Validation
{
    public abstract class BaseValidator : IValidator
    {
        private IObjectRepository tdb;

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

            foreach (var template in implementationGuide.ChildTemplates)
            {
                var result = new TemplateValidationResult()
                {
                    Id = template.Id,
                    Name = template.Name,
                    Oid = template.Oid,
                    Items = this.ValidateTemplate(template, igSchema)
                };

                if (result.Items.Count > 0)
                    results.TemplateResults.Add(result);
            }

            return results;
        }

        public List<ValidationResult> ValidateTemplate(int templateId)
        {
            Template template = this.tdb.Templates.Single(y => y.Id == templateId);
            SimpleSchema igSchema = template.OwningImplementationGuide.ImplementationGuideType.GetSimpleSchema();

            return ValidateTemplate(template, igSchema);
        }

        public virtual List<ValidationResult> ValidateTemplate(Template template, SimpleSchema igSchema)
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
                    ValidateTemplateConstraint(template, xpathNavigator, results, templateSchema, templateSchema.Children, y);
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
        public static void ValidateTemplateConstraint(Template template, XPathNavigator xpathNavigator, List<ValidationResult> results, SimpleSchema schema, List<SimpleSchema.SchemaObject> schemaObjects, TemplateConstraint currentConstraint)
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
                if (currentConstraint.ContainedTemplate != null
                    && currentConstraint.ContainedTemplate.PrimaryContextType != null
                    && foundSchemaObject != null)
                {
                    string containedTemplateDataType = currentConstraint.ContainedTemplate.PrimaryContextType;
                    string constraintDataType = !string.IsNullOrEmpty(currentConstraint.DataType) ? currentConstraint.DataType : foundSchemaObject.DataType;

                    bool isFhirResourceReference = schema.Schema.TargetNamespace == "http://hl7.org/fhir" && (constraintDataType == "ResourceReference" || constraintDataType == "Reference");

                    if (!isFhirResourceReference && containedTemplateDataType.ToLower() != constraintDataType.ToLower())
                    {
                        results.Add(ValidationResult.CreateResult(
                            template.Id,
                            template.Name,
                            currentConstraint.Number.Value,
                            ValidationLevels.Error,
                            "Contained template \"{0}\" has a type of \"{1}\" which does not match the containing element \"{2}\"",
                            currentConstraint.ContainedTemplate.Oid,
                            containedTemplateDataType,
                            constraintDataType));
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
                    // Warn when a constraint that is associated with the schema that has multiple cardinality but is not branched
                    if (template.IsOpen &&
                        !foundSchemaObject.Cardinality.EndsWith("..1") &&
                        !foundSchemaObject.Cardinality.EndsWith("..0") &&
                        !currentConstraint.IsBranch)
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

                childConstraints.ForEach(y => ValidateTemplateConstraint(template, xpathNavigator, results, schema, childSchemaObjects, y));
            }
        }
    }
}
