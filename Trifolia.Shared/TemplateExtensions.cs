using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using Trifolia.Shared;
using Trifolia.Shared.ImportExport.Model;
using Trifolia.Authentication;

namespace Trifolia.DB
{
    public static class TemplateExtensions
    {

        #region Schema

        public static SimpleSchema GetSchema(this Template template, SimpleSchema igSchema = null)
        {
            if (igSchema == null)
                igSchema = template.OwningImplementationGuide.ImplementationGuideType.GetSimpleSchema();

            if (!string.IsNullOrEmpty(template.PrimaryContextType))
                return igSchema.GetSchemaFromContext(template.PrimaryContextType);
            else if (!string.IsNullOrEmpty(template.TemplateType.RootContextType))
                return igSchema.GetSchemaFromContext(template.TemplateType.RootContextType);

            return igSchema;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates a template against the schema to determine if any constraints within the template cause don't align with the schema.
        /// </summary>
        /// <param name="errors">The errors list that should be populated when issues occur matching the template/constraints to the schema.</param>
        /// <param name="templateSchema">The schema that the template should be validated against.</param>
        public static List<TemplateValidationResult> ValidateTemplate(this Template template, SimpleSchema templateSchema = null, SimpleSchema igSchema = null)
        {
            if (templateSchema == null)
                templateSchema = template.GetSchema(igSchema);

            List<TemplateValidationResult> errors = new List<TemplateValidationResult>();

            XmlDocument tempDoc = new XmlDocument();
            XPathNavigator xpathNavigator = tempDoc.CreateNavigator();

            if (templateSchema == null)
            {
                errors.Add(
                    TemplateValidationResult.CreateResult(ValidationLevels.Error, "Template context is not found within the schema"));
            }
            else
            {
                List<TemplateConstraint> rootConstraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null).ToList();
                rootConstraints.ForEach(y => 
                {
                    // The first child in the schema is the complex type itself
                    template.ValidateTemplateConstraint(xpathNavigator, errors, templateSchema, templateSchema.Children, y);
                });
            }

            return errors;
        }

        /// <summary>
        /// Perform validations on a single constraint.
        /// </summary>
        /// <param name="errors">The list of errors that should be added to when a constraint has an error matching the schema</param>
        /// <param name="schemaObjects">The list of sibling-level schema objects that the constraint should be compared to</param>
        /// <param name="currentConstraint">The current constraint to match against the schema</param>
        public static void ValidateTemplateConstraint(this Template template, XPathNavigator xpathNavigator, List<TemplateValidationResult> errors, SimpleSchema schema, List<SimpleSchema.SchemaObject> schemaObjects, TemplateConstraint currentConstraint)
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
                    errors.Add(TemplateValidationResult.CreateResult(currentConstraint.Number, ValidationLevels.Error, "Custom schematron is not valid: " + ex.Message));
                }
            }

            if (currentConstraint.IsPrimitive && string.IsNullOrEmpty(currentConstraint.PrimitiveText))
            {
                errors.Add(TemplateValidationResult.CreateResult(currentConstraint.Number, ValidationLevels.Error, "Primitive does not have any narrative text."));
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
                        errors.Add(TemplateValidationResult.CreateResult(
                            currentConstraint.Number,
                            ValidationLevels.Error,
                            "Contained template \"{0}\" has a type of \"{1}\" which does not match the containing element \"{2}\"",
                            currentConstraint.ContainedTemplate.Oid,
                            containedTemplateDataType,
                            constraintDataType));
                    }
                }

                // Verify that branched elements have at least one identifier
                if (currentConstraint.IsBranch && childConstraints.Count(y => y.IsBranchIdentifier) == 0)
                    errors.Add(TemplateValidationResult.CreateResult(currentConstraint.Number, ValidationLevels.Warning, "Branched constraint \"{0}\" does not have any identifiers associated with it.", currentConstraint.GetXpath()));

                // Verify that a schema-object can be matched to this constraint
                if (foundSchemaObject == null)
                {
                    errors.Add(TemplateValidationResult.CreateResult(currentConstraint.Number, ValidationLevels.Error, "Constraint has context of \"{0}\" which is not found in the schema.", currentConstraint.GetXpath()));
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
                        var error = TemplateValidationResult.CreateResult(currentConstraint.Number, ValidationLevels.Warning, "Schema allows multiple for \"{0}\" but the constraint is not branched. Consider branching this constraint.", currentConstraint.GetXpath());
                        errors.Add(error);
                    }

                    if (foundSchemaObject.Cardinality.EndsWith("..1") && !(currentConstraint.Cardinality.EndsWith("..0") || currentConstraint.Cardinality.EndsWith("..1")))
                    {
                        var error = TemplateValidationResult.CreateResult(currentConstraint.Number, ValidationLevels.Error, "The cardinality for the element is loosened in the constraint from the underlying schema.");
                        errors.Add(error);
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

                childConstraints.ForEach(y => template.ValidateTemplateConstraint(xpathNavigator, errors, schema, childSchemaObjects, y));
            }
        }

        #endregion

        /// <summary>
        /// Determines if the identifier for the template is an "urn:oid:" identifier
        /// </summary>
        public static bool IsIdentifierOID(this Template template)
        {
            return IdentifierHelper.IsIdentifierOID(template.Oid);
        }

        public static bool GetIdentifierOID(this Template template, out string oid)
        {
            return IdentifierHelper.GetIdentifierOID(template.Oid, out oid);
        }

        /// <summary>
        /// Determines if the identifier for the template is an "urn:hl7ii:" (instance identifier) identifier
        /// </summary>
        public static bool IsIdentifierII(this Template template)
        {
            return IdentifierHelper.IsIdentifierII(template.Oid);
        }

        public static bool GetIdentifierII(this Template template, out string root, out string extension)
        {
            return IdentifierHelper.GetIdentifierII(template.Oid, out root, out extension);
        }

        /// <summary>
        /// Determines if the identifier for the template is a "urn:" identifier
        /// </summary>
        public static bool IsIdentifierURL(this Template template)
        {
            return IdentifierHelper.IsIdentifierURL(template.Oid);
        }

        public static bool GetIdentifierURL(this Template template, out string uri)
        {
            return IdentifierHelper.GetIdentifierURL(template.Oid, out uri);
        }

        public static string GetViewUrl(this Template template, string linkBase = null)
        {
            string oid;
            string root;
            string extension;
            string uri;

            if (template.GetIdentifierOID(out oid))
            {
                return string.Format("{0}/TemplateManagement/View/OID/{1}", linkBase, oid);
            }
            else if (template.GetIdentifierII(out root, out extension))
            {
                return string.Format("{0}/TemplateManagement/View/II/{1}/{2}", linkBase, root, extension);
            }
            else if (template.GetIdentifierURL(out uri))
            {
                if (uri.IndexOf(':') < 0 && uri.IndexOf('/') < 0 && uri.IndexOf('.') != uri.Length - 1)
                    return string.Format("{0}/TemplateManagement/View/URI/{1}", linkBase, uri);
            }

            return string.Format("{0}/TemplateManagement/View/Id/{1}", linkBase, template.Id);
        }

        public static string GetEditUrl(this Template template)
        {
            string oid;
            string root;
            string extension;
            string uri;

            if (template.GetIdentifierOID(out oid))
            {
                return string.Format("/TemplateManagement/Edit/OID/{0}", oid);
            }
            else if (template.GetIdentifierII(out root, out extension))
            {
                return string.Format("/TemplateManagement/Edit/II/{0}/{1}", root, extension);
            }
            else if (template.GetIdentifierURL(out uri))
            {
                if (uri.IndexOf(':') < 0 && uri.IndexOf('/') < 0 && uri.IndexOf('.') != uri.Length - 1)
                    return string.Format("/TemplateManagement/Edit/URI/{0}", uri);
            }

            return string.Format("/TemplateManagement/Edit/Id/{0}", template.Id);
        }

        public static string GetMoveUrl(this Template template)
        {
            string oid;
            string root;
            string extension;
            string uri;

            if (template.GetIdentifierOID(out oid))
            {
                return string.Format("/TemplateManagement/Move/OID/{0}", oid);
            }
            else if (template.GetIdentifierII(out root, out extension))
            {
                return string.Format("/TemplateManagement/Move/II/{0}/{1}", root, extension);
            }
            else if (template.GetIdentifierURL(out uri))
            {
                if (uri.IndexOf(':') < 0 && uri.IndexOf('/') < 0 && uri.IndexOf('.') != uri.Length - 1)
                    return string.Format("/TemplateManagement/Move/URI/{0}", uri);
            }

            return string.Format("/TemplateManagement/Move/Id/{0}", template.Id);
        }
    }
}
