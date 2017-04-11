using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Generation.Green.Transform
{
    public class GreenTransformGenerator
    {
        private IObjectRepository tdb;
        private Template rootTemplate;
        private XmlDocument transformDoc;
        private string schemaNamespace;
        private List<GreenTemplate> allGreenTemplates = null;
        private Dictionary<int, int> greenTemplates = null;
        private Dictionary<string, XmlElement> dataTypeTemplates = new Dictionary<string, XmlElement>();
        private SimpleSchema simpleSchema = null;

        private GreenTransformGenerator(IObjectRepository tdb)
        {
            this.tdb = tdb;
            this.transformDoc = new XmlDocument();
        }

        public GreenTransformGenerator(IObjectRepository tdb, Dictionary<int, int> greenTemplates, long templateId)
            : this(tdb)
        {
            this.rootTemplate = tdb.Templates.Single(y => y.Id == templateId);
            this.greenTemplates = greenTemplates;
            this.schemaNamespace = this.rootTemplate.ImplementationGuideType.SchemaURI;
            this.simpleSchema =
                SimpleSchema.CreateSimpleSchema(
                    Helper.GetIGSimplifiedSchemaLocation(this.rootTemplate.ImplementationGuideType));

            TransformHelper.InitializeTransform(this.transformDoc, this.schemaNamespace);
        }

        /// <summary>
        /// Creates the transform for the green template and all data types found/used within the green template
        /// and the contained templates
        /// </summary>
        public void BuildTransform()
        {
            if (!this.greenTemplates.ContainsKey(this.rootTemplate.Id))
                return;

            long rootGreenTemplateId = this.greenTemplates[this.rootTemplate.Id];
            GreenTemplate rootGreenTemplate = this.rootTemplate.GreenTemplates.SingleOrDefault(y => y.Id == rootGreenTemplateId);

            if (rootGreenTemplate == null)
                return;

            // Create the transform for the root template
            CreateTemplate(rootGreenTemplate);

            // Add the templates to the transform for the data types
            foreach (XmlElement cDataTypeTemplate in this.dataTypeTemplates.Values)
                this.transformDoc.DocumentElement.AppendChild(cDataTypeTemplate);
        }

        /// <summary>
        /// Gets the string XML for the transform that was generated. Requires that the BuildTransform() 
        /// method be called before GetTransform()
        /// </summary>
        public string GetTransform()
        {
            if (this.transformDoc == null)
                return string.Empty;

            using (MemoryStream ms = new MemoryStream())
            {
                this.transformDoc.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                StreamReader reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        private void CreateTemplate(GreenTemplate greenTemplate)
        {
            // Create the xsl:template for the green template and add it to the document
            string name = Helper.NormalizeName(greenTemplate.Name);
            XmlElement templateElement = TransformHelper.CreateXslTemplate(this.transformDoc, name, Helper.NormalizeName(greenTemplate.Name));
            this.transformDoc.DocumentElement.AppendChild(templateElement);

            // Identify all the root-level constraints within the template
            List<TemplateConstraint> rootTemplateConstraints = (from tc in tdb.TemplateConstraints
                                                                where tc.TemplateId == greenTemplate.TemplateId
                                                                  && tc.ParentConstraintId == null
                                                                select tc).ToList();

            if (!string.IsNullOrEmpty(greenTemplate.Template.TemplateType.RootContext))
            {
                // Create top-level context element
                XmlElement contextElement = this.transformDoc.CreateElement(TransformHelper.IgNamespacePrefix, greenTemplate.Template.TemplateType.RootContext, this.schemaNamespace);
                templateElement.AppendChild(contextElement);

                // TODO: This should be replaced with more generic code for IG types in the future
                // TODO: This should be put in the right location in the output. If there is an ID constraint, the order of elements won't match CDA.xsd
                TemplateConstraint foundTemplateId = (from tc in rootTemplateConstraints
                                                      join tcc in tdb.TemplateConstraints on tc.Id equals tcc.ParentConstraintId
                                                      where tc.Context == "templateId"
                                                        && tcc.Context == "@root"
                                                        && tcc.Value == greenTemplate.Template.Oid
                                                      select tc).FirstOrDefault();

                // If a templateId constraint has not been defined at the top level for this template, then we should create
                // a templateId element for it, and add it to the output
                if (foundTemplateId == null)
                {
                    XmlElement templateIdElement = this.transformDoc.CreateElement(TransformHelper.IgNamespacePrefix, "templateId", this.schemaNamespace);
                    templateIdElement.Attributes.Append(
                        TransformHelper.CreateXsltAttribute(this.transformDoc, "root", greenTemplate.Template.Oid));
                    contextElement.AppendChild(templateIdElement);
                }

                rootTemplateConstraints.ForEach(y => CreateTemplateConstraint(contextElement, y));
            }
            else
            {
                rootTemplateConstraints.ForEach(y => CreateTemplateConstraint(templateElement, y));
            }
        }

        private void CreateTemplateConstraint(XmlElement parent, TemplateConstraint constraint)
        {
            GreenConstraint greenConstraint = null;
            XmlElement newElement = null;

            if (this.greenTemplates.ContainsKey(constraint.TemplateId))
            {
                long greenTemplateId = this.greenTemplates[constraint.TemplateId];
                greenConstraint = tdb.GreenConstraints.SingleOrDefault(y => y.GreenTemplateId == greenTemplateId && y.TemplateConstraintId == constraint.Id);
            }

            if (greenConstraint != null)
            {
                newElement = CreateGreenMapping(parent, greenConstraint);
            }
            else if (!string.IsNullOrEmpty(constraint.Context))
            {
                newElement = CreateNormativeMapping(parent, constraint);
            }
            else if (!string.IsNullOrEmpty(constraint.PrimitiveText))
            {
                XmlComment newComment = this.transformDoc.CreateComment("CONF# " + constraint.Id.ToString() + ":" + constraint.PrimitiveText);
                parent.AppendChild(newComment);
            }
            else
            {
                XmlComment newComment = this.transformDoc.CreateComment("CONF# " + constraint.Id.ToString() + " is not mapping via green, does not have a context " +
                    "and does not have narrative, therefore cannot be mapped in the transform.");
                parent.AppendChild(newComment);
            }

            if (newElement != null)
            {
                List<TemplateConstraint> childConstraints = (from tc in tdb.TemplateConstraints
                                                             where tc.TemplateId == constraint.TemplateId
                                                               && tc.ParentConstraintId == constraint.Id
                                                             select tc).ToList();
                childConstraints.ForEach(y => CreateTemplateConstraint(newElement, y));
            }
        }

        private XmlElement CreateNormativeMapping(XmlElement parent, TemplateConstraint constraint)
        {
            string context = Helper.NormalizeName(constraint.Context);

            if (!string.IsNullOrEmpty(constraint.Context) && constraint.Context.StartsWith("@"))
            {
                if (!string.IsNullOrEmpty(constraint.Value))
                {
                    XmlAttribute newAttribute = this.transformDoc.CreateAttribute(context);
                    newAttribute.Value = constraint.Value ;
                    parent.Attributes.Append(newAttribute);
                }
                else if (constraint.Cardinality.StartsWith("1.."))
                {
                    XmlComment newComment = this.transformDoc.CreateComment("CONF# " + constraint.Id.ToString() + " defines an attribute that is required, but has no green mapping.");
                    parent.AppendChild(newComment);
                }
            }
            else
            {
                if (context.Contains("/"))          // Non-schema-bound constraint.
                {
                    string[] contextSplit = context.Replace("//", "/").Split('/');
                    XmlElement parentContextElement = parent;

                    for (int i = 0; i < contextSplit.Length; i++)
                    {
                        string cContext = Helper.NormalizeName(contextSplit[i]);
                        bool isLast = i == contextSplit.Length-1;

                        if (cContext.Length == 0)
                            continue;

                        if (isLast && cContext.StartsWith("@"))           // Last context, and is attribute
                        {
                            if (!string.IsNullOrEmpty(constraint.Value))
                            {
                                XmlAttribute newAttribute = this.transformDoc.CreateAttribute(cContext.Substring(1));
                                newAttribute.Value = constraint.Value;
                                parentContextElement.Attributes.Append(newAttribute);
                            }
                            else if (constraint.Cardinality.StartsWith("1.."))
                            {
                                XmlComment newComment = this.transformDoc.CreateComment("CONF# " + constraint.Id.ToString() + " defines an attribute that is required, but has no green mapping.");
                                parentContextElement.AppendChild(newComment);
                            }
                        }
                        else if (!cContext.StartsWith("@"))         // Not an attribute, could be last
                        {
                            XmlElement newContextElement = this.transformDoc.CreateElement(TransformHelper.IgNamespacePrefix, cContext, this.schemaNamespace);
                            parentContextElement.AppendChild(newContextElement);

                            if (isLast && !string.IsNullOrEmpty(constraint.Value))
                                newContextElement.Value = constraint.Value;

                            parentContextElement = newContextElement;
                        }
                        else
                        {
                            XmlComment newComent = this.transformDoc.CreateComment("CONF# " + constraint.Id.ToString() + " defines a hierarchical context that has an attribute in the wrong place.");
                            parentContextElement.AppendChild(newComent);
                            break;
                        }
                    }
                }
                else
                {
                    XmlElement newMapping = this.transformDoc.CreateElement(TransformHelper.IgNamespacePrefix, context, this.schemaNamespace);
                    parent.AppendChild(newMapping);
                    return newMapping;
                }
            }

            return null;
        }

        private XmlElement CreateGreenMapping(XmlElement parent, GreenConstraint greenConstraint)
        {
            // If the constraint represents a contained template, we should simply apply the rules of the contained template here.
            if (greenConstraint.TemplateConstraint.References.Count(y => y.ReferenceType == ConstraintReferenceTypes.Template) > 0)
            {
                /* TODO: update to work with multiple contained templates/profiles
                if (allGreenTemplates.Exists(y => y.TemplateId == greenConstraint.TemplateConstraint.ContainedTemplateId))
                {
                    XmlElement applyTemplatesElement = TransformHelper.CreateXsltApplyTemplates(this.transformDoc, greenConstraint.Name);
                    parent.AppendChild(applyTemplatesElement);
                }
                else
                {
                    XmlComment newComent = this.transformDoc.CreateComment("CONF# " + greenConstraint.TemplateConstraintId.ToString() + " has a green constraint '" + greenConstraint.Name + "' that references a contained template, which does not have a green template defined for.");
                    parent.AppendChild(newComent);
                }
                */
            }
            else
            {
                if (greenConstraint.ChildGreenConstraints.Count > 0)
                {
                    // Create loop for child constraints
                    XmlElement forEachElement = TransformHelper.CreateXsltForEach(this.transformDoc, greenConstraint.Name);
                    parent.AppendChild(forEachElement);

                    XmlElement contextElement = this.transformDoc.CreateElement(TransformHelper.IgNamespacePrefix, greenConstraint.TemplateConstraint.Context, this.schemaNamespace);
                    forEachElement.AppendChild(contextElement);

                    if (greenConstraint.IsEditable == true)
                    {
                        // Identify if the editable green object is data-typed. If it is, apply the data-type template
                        string dataTypeTemplate = string.Empty;

                        if (!string.IsNullOrEmpty(greenConstraint.TemplateConstraint.DataType))
                            dataTypeTemplate = FindOrAddDataTypeTemplate(greenConstraint.TemplateConstraint.DataType);
                        
                        if (!string.IsNullOrEmpty(dataTypeTemplate))
                        {
                            XmlElement applyTemplatesElement = TransformHelper.CreateXsltApplyTemplates(this.transformDoc, dataTypeTemplate, null, "instance", greenConstraint.Name);
                            contextElement.AppendChild(applyTemplatesElement);
                        }
                        else
                        {
                            XmlElement textMappingElement = TransformHelper.CreateXsltValueOf(this.transformDoc, "value()");
                            contextElement.AppendChild(textMappingElement);
                        }
                    }

                    return contextElement;
                }
                else if (greenConstraint.Name.StartsWith("@"))           // TODO: Assuming that attributes are all editable for now
                {
                    XmlElement attributeMappingElement = TransformHelper.CreateXsltAttributeWithValueOf(
                        this.transformDoc,
                        greenConstraint.TemplateConstraint.Context.Replace("@", ""),
                        greenConstraint.Name);
                    parent.AppendChild(attributeMappingElement);
                }
                else if (greenConstraint.IsEditable == true)
                {
                    // Identify if the editable green object is data-typed. If it is, apply the data-type template
                    string dataTypeTemplate = string.Empty;

                    if (!string.IsNullOrEmpty(greenConstraint.TemplateConstraint.DataType))
                        dataTypeTemplate = FindOrAddDataTypeTemplate(greenConstraint.TemplateConstraint.DataType);

                    if (!string.IsNullOrEmpty(dataTypeTemplate))
                    {
                        XmlElement newElement = TransformHelper.CreateXsltElement(this.transformDoc, greenConstraint.TemplateConstraint.Context);
                        parent.AppendChild(newElement);

                        XmlElement applyTemplatesElement = TransformHelper.CreateXsltApplyTemplates(this.transformDoc, dataTypeTemplate, null, "instance", greenConstraint.Name);
                        newElement.AppendChild(applyTemplatesElement);
                    }
                    else
                    {
                        XmlElement attributeValueElement = TransformHelper.CreateXsltValueOf(this.transformDoc, greenConstraint.Name);
                        parent.AppendChild(attributeValueElement);
                    }
                }
                else
                {
                    XmlComment newComent = this.transformDoc.CreateComment("CONF# " + greenConstraint.TemplateConstraintId.ToString() + " has a green constraint '" + greenConstraint.Name + "' that is not editable and has no children.");
                    parent.AppendChild(newComent);
                }
            }

            return null;
        }

        #region DataTypes

        private string FindOrAddDataTypeTemplate(string dataTypeName)
        {
            string templateName = "dataType_" + dataTypeName;

            if (this.dataTypeTemplates.ContainsKey(dataTypeName))
                return templateName;

            SimpleSchema.SchemaObject simpleComplexType = this.simpleSchema.ComplexTypes.SingleOrDefault(y => y.Name == dataTypeName);

            bool dataTypeDefined = this.tdb.ImplementationGuideTypeDataTypes.Count(y =>
                y.ImplementationGuideTypeId == this.rootTemplate.ImplementationGuideTypeId &&
                y.DataTypeName == dataTypeName) > 0;

            if (simpleComplexType == null || !dataTypeDefined)
                return string.Empty;

            XmlElement dataTypeTemplate = TransformHelper.CreateXslTemplate(this.transformDoc, templateName, string.Empty);
            dataTypeTemplate.AppendChild(
                TransformHelper.CreateXsltTemplateParam(this.transformDoc, "instance"));

            CompleteDataTypeTemplate(dataTypeTemplate, simpleComplexType, "$instance");
            this.dataTypeTemplates.Add(dataTypeName, dataTypeTemplate);

            return templateName;
        }

        private void CompleteDataTypeTemplate(XmlElement parent, SimpleSchema.SchemaObject simpleObject, string currentXpath)
        {
            foreach (SimpleSchema.SchemaObject cChildObject in simpleObject.Children)
            {
                if (cChildObject.Type == SimpleSchema.ObjectTypes.Attribute)
                {
                    string cAttributeName = Helper.NormalizeName(cChildObject.Name);
                    string cAttributeXpath = !string.IsNullOrEmpty(currentXpath) ? currentXpath + "/@" + cAttributeName : "@" + cAttributeName;

                    XmlElement attributeIfEle = TransformHelper.CreateXsltIf(this.transformDoc, cAttributeXpath);
                    attributeIfEle.AppendChild(
                        TransformHelper.CreateXsltAttributeWithValueOf(this.transformDoc, cAttributeName, cAttributeXpath));

                    parent.AppendChild(attributeIfEle);
                }
                else if (cChildObject.Type == SimpleSchema.ObjectTypes.Element)
                {
                    string cChildXpath = !string.IsNullOrEmpty(currentXpath) ? currentXpath + "/" + cChildObject.Name : cChildObject.Name;

                    XmlElement newForeachEle = TransformHelper.CreateXsltForEach(this.transformDoc, cChildXpath);
                    XmlElement newChildElement = TransformHelper.CreateXsltElement(this.transformDoc, cChildObject.Name);

                    CompleteDataTypeTemplate(newChildElement, cChildObject, string.Empty);

                    newForeachEle.AppendChild(newChildElement);
                    parent.AppendChild(newForeachEle);
                }
            }

            if (simpleObject.Mixed)
            {
            string textXpath = !string.IsNullOrEmpty(currentXpath) ? currentXpath + "/text()" : "text()";
            XmlElement textValue = TransformHelper.CreateXsltValueOf(this.transformDoc, textXpath);
            parent.AppendChild(textValue);
                }
        }

        #endregion
    }
}
