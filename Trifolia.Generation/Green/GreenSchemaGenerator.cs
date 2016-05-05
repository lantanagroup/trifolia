using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Generation.Green
{
    public class GreenSchemaGenerator : SchemaGenerationBase
    {
        private const string DATATYPE_NS = "gdt";

        private IObjectRepository tdb = null;
        private Template rootTemplate;
        private XmlSchema baseSchema;
        private XmlSchema schema;
        private XmlSchema dataTypesSchema;
        private SchemaCopier schemaCopier;
        private List<string> dataTypes = new List<string>();
        private bool separateDataTypes = false;
        private string packageName = string.Empty;

        private GreenSchemaGenerator(IObjectRepository tdb, Template rootTemplate, bool separateDataTypes = false)
        {
            this.schema = new XmlSchema();
            this.tdb = tdb;
            this.rootTemplate = rootTemplate;
            this.baseSchema = Helper.GetIGSchema(this.rootTemplate.ImplementationGuideType);
            this.separateDataTypes = separateDataTypes;

            this.packageName = string.Format("{0}_{1}",
                Helper.GetCleanName(rootTemplate.OwningImplementationGuide.NameWithVersion),
                Helper.GetCleanName(rootTemplate.Name));

            if (separateDataTypes)
            {
                this.dataTypesSchema = new XmlSchema();
                this.dataTypesSchema.TargetNamespace = this.baseSchema.TargetNamespace;
                this.schemaCopier = new SchemaCopier(this.baseSchema, this.dataTypesSchema, this.baseSchema.TargetNamespace);
                this.schema.Namespaces.Add(DATATYPE_NS, this.baseSchema.TargetNamespace);
            }
            else
            {
                this.schemaCopier = new SchemaCopier(this.baseSchema, this.schema, string.Empty);
            }

            if (this.rootTemplate.GreenTemplates.Count() == 0)
                throw new ArgumentException("Root template specified does not have a green template definition");
        }

        public static GreenSchemaPackage Generate(IObjectRepository tdb, Template rootTemplate, bool separateDataTypes = false)
        {
            GreenSchemaGenerator generator = new GreenSchemaGenerator(tdb, rootTemplate, separateDataTypes);
            generator.GenerateSchema();
            return generator.GetPackage();
        }

        /// <summary>
        /// Begins the process for building/generating the XmlSchema.
        /// </summary>
        /// <remarks>
        /// Executes BuildTemplate for the root template (which recursively builds each referenced template).
        /// Creates a root element within the schema for the root template.
        /// </remarks>
        private void GenerateSchema()
        {
            var rootGreenTemplate = this.rootTemplate.GreenTemplates.First();

            // Add the root element as the first global node in the schema
            XmlSchemaElement rootElement = new XmlSchemaElement()
            {
                Name = rootGreenTemplate.Name,
                SchemaTypeName = new XmlQualifiedName(rootGreenTemplate.Name)
            };

            if (!string.IsNullOrEmpty(this.rootTemplate.PrimaryContext))
                rootElement.Annotation = this.CreateAnnotation("XPATH = /" + this.rootTemplate.PrimaryContext);

            // Add all of the complex typs (BuildTemplateComplexType is recursive, so we start with the root green template)
            BuildTemplate(rootGreenTemplate);

            this.schema.Items.Add(rootElement);
        }

        /// <summary>
        /// Returns the green schema package with the datatypes content (if datatypes are separated)
        /// and the main green schema content.
        /// </summary>
        private GreenSchemaPackage GetPackage()
        {
            GreenSchemaPackage package = new GreenSchemaPackage();
            package.GreenSchemaFileName = string.Format("{0}.xsd", this.packageName);

            using (StringWriter sw = new StringWriter())
            {
                this.schema.Write(sw);
                package.GreenSchemaContent = sw.ToString();
            }

            if (this.dataTypesSchema != null)
            {
                package.DataTypesFileName = string.Format("{0}_datatypes.xsd", this.packageName);

                // When separating the data types out into a different schema, we need to import the data
                // types schema into the main schema. This cannot be done with XmlSchemaImport (which looks nicer)
                // because the <xs:import> is automatically removed when this.schema.Write() is called.
                int firstLineBreakIndex = package.GreenSchemaContent.IndexOf("\n");
                int secondLineBreakIndex = package.GreenSchemaContent.IndexOf("\n", firstLineBreakIndex + 1);
                string importXml = string.Format("  <xs:import namespace=\"{0}\" schemaLocation=\"{1}\" />\n",
                    this.baseSchema.TargetNamespace,
                    package.DataTypesFileName);
                package.GreenSchemaContent = package.GreenSchemaContent.Insert(secondLineBreakIndex + 1, importXml);

                using (StringWriter sw = new StringWriter())
                {
                    this.dataTypesSchema.Write(sw);
                    package.DataTypesContent = sw.ToString();
                }
            }

            return package;
        }

        /// <summary>
        /// Finds a complexType with the specified name within the generated schema.
        /// </summary>
        /// <remarks>
        /// Used to determine if the template has already been built (to avoid endlessly calling BuildTemplate 
        ///   for recursive template references).
        /// </remarks>
        /// <returns>
        /// The XmlSchemaComplexType found with the specified name in the generated schema, or null if none is found.
        /// </returns>
        private XmlSchemaComplexType FindTemplateComplexType(string name)
        {
            foreach (XmlSchemaObject currentSchemaObject in this.schema.Items)
            {
                XmlSchemaComplexType currentSchemaComplexType = currentSchemaObject as XmlSchemaComplexType;

                if (currentSchemaComplexType == null)
                    continue;

                if (currentSchemaComplexType.Name == name)
                    return currentSchemaComplexType;
            }

            return null;
        }

        /// <summary>
        /// Creates a schema complexType out of the green template and inserts it at the top of the schemas items
        /// </summary>
        /// <remarks>
        /// Adds an annotation that indicates what canonical template it represents
        /// If the template implies another template, the implied template is build if it has 
        ///   a green template definition, and this template's complexType extends its parent
        /// Calls BuildConstraint for each root constraint within the green template (root 
        ///   constraints don't have a ParentGreenConstraint)
        /// </remarks>
        private void BuildTemplate(GreenTemplate greenTemplate)
        {
            if (FindTemplateComplexType(greenTemplate.Name) != null)
                return;
            
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            XmlSchemaComplexType newComplexType = new XmlSchemaComplexType()
            {
                Name = greenTemplate.Name
            };

            // Create an annotation telling the implementers what canonical template this complex type represents
            string annotationText = string.Format("Template \"{0}\" ({1}) from implementation guide \"{2}\"",
                greenTemplate.Template.Name,
                greenTemplate.Template.Oid,
                greenTemplate.Template.OwningImplementationGuide.Name);
            newComplexType.Annotation = CreateAnnotation(annotationText);

            // If this template inherits from another, build the parent and have this complex type extend its base
            if (greenTemplate.Template.ImpliedTemplate != null)
            {
                var impliedTemplate = greenTemplate.Template.ImpliedTemplate;

                if (impliedTemplate.GreenTemplates.Count > 0)
                {
                    var impliedGreenTemplate = greenTemplate.Template.ImpliedTemplate.GreenTemplates.First();
                    BuildTemplate(impliedGreenTemplate);

                    XmlSchemaComplexContent newComplexContent = new XmlSchemaComplexContent();
                    newComplexType.ContentModel = newComplexContent;

                    XmlSchemaComplexContentExtension newExtension = new XmlSchemaComplexContentExtension();
                    newExtension.BaseTypeName = new XmlQualifiedName(impliedGreenTemplate.Name);
                }
            }

            if (newComplexType.ContentModel == null && newComplexType.Particle == null)
                newComplexType.Particle = sequence;

            // Green Constraints
            var rootGreenConstraints = greenTemplate.ChildGreenConstraints.Where(y => y.ParentGreenConstraint == null);

            if (rootGreenConstraints.Count() > 0)
            {
                foreach (var currentRootGreenConstraint in rootGreenConstraints)
                {
                    XmlSchemaObject newConstraintElement = BuildConstraint(currentRootGreenConstraint);

                    if (newConstraintElement != null)
                        sequence.Items.Add(newConstraintElement);
                }
            }

            this.schema.Items.Insert(0, newComplexType);
        }

        /// <summary>
        /// Builds a schema object from the green constraint specified, and returns the new object.
        /// </summary>
        /// <returns>
        /// An XmlSchemaChoice when the constraint represents a contained template that has multiple derived children.
        /// An XmlElement when the constraint is either an editable constraint, or represents a single contained template.
        /// </returns>
        /// <remarks>
        /// When the constraint represents a simple editable constraint (not one or more contained templates),
        ///   the datatype of the constraint is checked and copied from the base schema
        /// Recursively iterates through all child constraints and adds the return of BuildConstraint to a 
        ///   complexContent/sequence within the constraint's element
        /// </remarks>
        private XmlSchemaObject BuildConstraint(GreenConstraint greenConstraint)
        {
            string lowerBound;
            string upperBound;

            GetConstraintBounds(greenConstraint, out lowerBound, out upperBound);

            // Check if there is a contained template on this constraint
            if (greenConstraint.TemplateConstraint.ContainedTemplate != null)
            {
                var containedTemplate = greenConstraint.TemplateConstraint.ContainedTemplate;
                var implyingGreenTemplates = containedTemplate.ImplyingTemplates
                    .Where(y => y.GreenTemplates.Count > 0)
                    .Select(y => y.GreenTemplates.First());

                // If the contained template has a green template
                if (containedTemplate.GreenTemplates.Count > 0)
                {
                    XmlSchemaElement newConstraintElement = new XmlSchemaElement()
                    {
                        Name = greenConstraint.Name,
                        MinOccursString = lowerBound,
                        MaxOccursString = upperBound
                    };

                    if (!string.IsNullOrEmpty(greenConstraint.RootXpath))
                        newConstraintElement.Annotation = this.CreateAnnotation("XPATH = " + greenConstraint.RootXpath);

                    var containedGreenTemplate = containedTemplate.GreenTemplates.FirstOrDefault();

                    if (containedGreenTemplate != null)
                    {
                        BuildTemplate(containedGreenTemplate);
                        newConstraintElement.SchemaTypeName = new XmlQualifiedName(containedGreenTemplate.Name);
                    }

                    return newConstraintElement;
                }
                // Or if the contained template has derived templates which have green templates
                else if (implyingGreenTemplates.Count() > 0)
                {
                    XmlSchemaChoice newChoice = new XmlSchemaChoice();

                    foreach (var currentGreenTemplate in implyingGreenTemplates)
                    {
                        BuildTemplate(currentGreenTemplate);

                        XmlSchemaElement newConstraintElement = new XmlSchemaElement()
                        {
                            Name = currentGreenTemplate.Name,
                            SchemaTypeName = new XmlQualifiedName(currentGreenTemplate.Name),
                            MinOccursString = lowerBound,
                            MaxOccursString = upperBound
                        };

                        if (!string.IsNullOrEmpty(greenConstraint.RootXpath))
                            newConstraintElement.Annotation = this.CreateAnnotation("XPATH = " + greenConstraint.RootXpath);

                        newChoice.Items.Add(newConstraintElement);
                    }

                    return newChoice;
                }
            }
            else
            {
                XmlSchemaElement newConstraintElement = new XmlSchemaElement()
                {
                    Name = greenConstraint.Name,
                    MinOccursString = lowerBound,
                    MaxOccursString = upperBound
                };

                if (!string.IsNullOrEmpty(greenConstraint.RootXpath))
                    newConstraintElement.Annotation = this.CreateAnnotation("XPATH = " + greenConstraint.RootXpath);

                // If we aren't a branch, then check the datatype of the green constraint and copy it
                // from the base schema. Otherwise, create complex content for its children constraints
                if (greenConstraint.ChildGreenConstraints.Count == 0)
                {
                    if (greenConstraint.ImplementationGuideTypeDataType != null)
                    {
                        string dataType = greenConstraint.ImplementationGuideTypeDataType.DataTypeName;

                        this.schemaCopier.CopyDataType(dataType);

                        if (this.separateDataTypes)
                            newConstraintElement.SchemaTypeName = new XmlQualifiedName(dataType, this.baseSchema.TargetNamespace);
                        else
                            newConstraintElement.SchemaTypeName = new XmlQualifiedName(dataType);
                    }
                    else
                    {
                        newConstraintElement.SchemaTypeName = new XmlQualifiedName("xs:string");
                    }
                }
                else
                {
                    XmlSchemaComplexType childComplexType = new XmlSchemaComplexType();
                    newConstraintElement.SchemaType = childComplexType;

                    XmlSchemaSequence sequence = new XmlSchemaSequence();
                    childComplexType.Particle = sequence;

                    foreach (var currentChildGreenConstraint in greenConstraint.ChildGreenConstraints)
                    {
                        XmlSchemaObject newChildConstraintElement = BuildConstraint(currentChildGreenConstraint);

                        if (newChildConstraintElement != null)
                            sequence.Items.Add(newChildConstraintElement);
                    }
                }

                return newConstraintElement;
            }

            return null;
        }

        /// <summary>
        /// Gets the lower and upper bounds of a constraint to be used by XSD element's MinOccurs and MaxOccurs values.
        /// If the green constraint is not collapsing canonical constraints, it simply returns the cardinality defined
        /// by the canonical constraint. If it *is* collapsing one or more canonical constraints, it finds
        /// the maximum possible bounds of the constraints that it is collapsing.
        /// </summary>
        /// <param name="greenConstraint">The green constraint whose bounds should be determined.</param>
        /// <param name="lowerBound">Out parameter that is set to the maximum available lower bound of the constraint</param>
        /// <param name="upperBound">Out parameter that is set to the maximum available upper bound of the constraint</param>
        private void GetConstraintBounds(GreenConstraint greenConstraint, out string lowerBound, out string upperBound)
        {
            int maxLowerBound = greenConstraint.TemplateConstraint.CardinalityType.Left;
            int maxUpperBound = greenConstraint.TemplateConstraint.CardinalityType.Right;

            int? templateParentConstrantId = greenConstraint.TemplateConstraint.ParentConstraintId;
            int? greenParentConstraintId = greenConstraint.ParentGreenConstraint != null ? (int?)greenConstraint.ParentGreenConstraint.TemplateConstraintId : null;

            // If we are collapsing some elements...
            if (templateParentConstrantId != greenParentConstraintId)
            {
                TemplateConstraint current = greenConstraint.TemplateConstraint.ParentConstraint;

                while (current != null && current.Id != greenParentConstraintId)
                {
                    if (current.CardinalityType.Left > maxLowerBound)
                        maxLowerBound = current.CardinalityType.Left;

                    if (current.CardinalityType.Right > maxUpperBound)
                        maxUpperBound = current.CardinalityType.Right;

                    current = current.ParentConstraint;
                }
            }

            if (maxLowerBound == int.MaxValue)
                lowerBound = "unbounded";
            else
                lowerBound = maxLowerBound.ToString();

            if (maxUpperBound == int.MaxValue)
                upperBound = "unbounded";
            else
                upperBound = maxUpperBound.ToString();
        }
    }
}
