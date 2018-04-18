using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Plugins
{
    public class DefaultSampleGenerator
    {
        #region Private Fields

        private XmlDocument _currentDocument;
        private XmlNamespaceManager _nsManager;
        private IObjectRepository tdb = null;
        private SimpleSchema simpleSchema;
        private bool isSchemaValid = true;
        private Template template;
        private ImplementationGuideType igType;
        private IIGTypePlugin igTypePlugin = null;
        
        #endregion

        #region Ctor

        private DefaultSampleGenerator(IObjectRepository tdb, Template template)
        {
            this.tdb = tdb;
            this.template = template;

            this.simpleSchema =
                SimpleSchema.CreateSimpleSchema(
                    Helper.GetIGSimplifiedSchemaLocation(template.ImplementationGuideType));

            if (string.IsNullOrEmpty(this.template.PrimaryContextType))
                this.simpleSchema = this.simpleSchema.GetSchemaFromContext(template.TemplateType.RootContextType);
            else
                this.simpleSchema = this.simpleSchema.GetSchemaFromContext(template.PrimaryContextType);

            this.igType = template.ImplementationGuideType;
            this.igTypePlugin = this.igType.GetPlugin();
        }

        #endregion

        #region Public Properties

        public bool IsSchemaValid
        {
            get { return isSchemaValid; }
        }

        #endregion

        #region Public Methods

        public static DefaultSampleGenerator CreateDefaultSampleGenerator(IObjectRepository tdb, Template template)
        {
            DefaultSampleGenerator lGenerator = new DefaultSampleGenerator(tdb, template);
            return lGenerator;
        }

        public string GenerateSample()
        {
            //determine exactly how XML sample is built
            this._currentDocument = new XmlDocument();
            XmlElement rootEle = null;

            this._nsManager = new XmlNamespaceManager(this._currentDocument.NameTable);

            foreach (string cNamespace in this.simpleSchema.Namespaces.Keys)
            {
                if (this.simpleSchema.Namespaces[cNamespace] == "xmlns")
                    continue;

                this._nsManager.AddNamespace(this.simpleSchema.Namespaces[cNamespace], cNamespace);
            }

            if (string.IsNullOrEmpty(this.template.PrimaryContext))
            {
                rootEle = _currentDocument.CreateElement(this.template.TemplateType.RootContext);
            }
            else
            {
                rootEle = _currentDocument.CreateElement(this.template.PrimaryContext);
            }

            _currentDocument.AppendChild(rootEle);

            List<TemplateConstraint> childConstraints = this.template.ChildConstraints
                .Where(y => y.ParentConstraint == null)
                .OrderBy(y => y.Order)
                .ToList();

            if (this.template.ImpliedTemplateId.HasValue)
            {
                Template lImpliedTemplate = tdb.Templates.Single(t => t.Id == this.template.ImpliedTemplateId.Value);
                List<TemplateConstraint> lImpliedConstraints
                    = lImpliedTemplate.ChildConstraints
                    .Where(tc => tc.ParentConstraintId.HasValue == false)
                    .OrderBy(tc => tc.Order)
                    .ToList();

                foreach (TemplateConstraint lImpliedConstraint in lImpliedConstraints)
                {
                    if (lImpliedConstraint.IsPrimitive || !childConstraints.Exists(tc => tc.Context == lImpliedConstraint.Context))
                    {
                        childConstraints.Add(lImpliedConstraint);
                    }
                }
            }

            childConstraints.Sort(delegate(TemplateConstraint a, TemplateConstraint b)
            {
                return a.Order.CompareTo(b.Order);
            });

            childConstraints.ForEach(y => AddConstraint(rootEle, this.simpleSchema.Children, y));
            this.AddMissingRequiredData(rootEle, this.simpleSchema.Children);

            // Fill in sample data for stuff that isn't set already
            foreach (var childNode in rootEle.ChildNodes)
            {
                if (childNode is XmlElement)
                    this.igTypePlugin.FillSampleData((XmlElement)childNode);
            }

            // Add the template identifier to the sample
            this.igTypePlugin.AddTemplateIdentifierToSample(rootEle, this.template);

            return this.GetSampleXML(_currentDocument);
        }

        #endregion

        #region Private Methods

        private void AddMissingRequiredData(XmlNode parent, List<SimpleSchema.SchemaObject> siblingSchemaObjects)
        {
            // Get the list of sibling schema objects that are required
            var requiredObjects = siblingSchemaObjects.Where(y => y.LowerBound >= 1);

            foreach (var requiredSchemaObject in requiredObjects)
            {
                // Determine if we are trying to fill in data in an endless loop
                if (!requiredSchemaObject.IsAttribute)
                {
                    var nextParent = parent.ParentNode;
                    bool isRecursive = false;

                    while (nextParent != null)
                    {
                        if (nextParent.Name == requiredSchemaObject.Name)
                            isRecursive = true;

                        nextParent = nextParent.ParentNode;
                    }

                    if (isRecursive)
                        continue;
                }

                // Determine if a node already exists for the required schema object
                var foundInSample = parent.SelectNodes((requiredSchemaObject.IsAttribute ? "@" : "") + requiredSchemaObject.Name, this._nsManager).Count > 0;

                if (!foundInSample)
                {
                    // If the required node is an attribute, create an attribute.
                    // Detect whether or not there is a fixed/default value for the attribute, and if not, fill in "XXXX"
                    // Attributes within schemas don't have a required order, so just append new attributes to the end
                    if (requiredSchemaObject.IsAttribute)
                    {
                        XmlAttribute attribute = parent.OwnerDocument.CreateAttribute(requiredSchemaObject.Name);

                        if (!string.IsNullOrEmpty(requiredSchemaObject.FixedValue))
                            attribute.Value = requiredSchemaObject.FixedValue;
                        else if (!string.IsNullOrEmpty(requiredSchemaObject.Value))
                            attribute.Value = requiredSchemaObject.Value;
                        else
                            attribute.Value = "XXXX";

                        parent.Attributes.Append(attribute);
                    }
                    // If the required node is an element, determine where to place the new element within the parent
                    // and determine if the element is part of a choice (in which case, only spit out the first option of the choices)
                    else
                    {
                        XmlNode previousSibling = null;
                        bool previousSiblingIsChoice = false;

                        // Loop backwards in siblings to determine where the last valid schema node is
                        for (var i = siblingSchemaObjects.IndexOf(requiredSchemaObject) - 1; i >= 0; i--)
                        {
                            var currentSiblingSchemaObject = siblingSchemaObjects[i];

                            if (currentSiblingSchemaObject.IsAttribute)
                                continue;

                            var foundPreviousNode = parent.SelectNodes(currentSiblingSchemaObject.Name, this._nsManager);

                            if (foundPreviousNode.Count > 0)
                            {
                                previousSibling = foundPreviousNode[foundPreviousNode.Count - 1];
                                previousSiblingIsChoice = currentSiblingSchemaObject.IsChoice;
                                break;
                            }
                        }

                        // If the previous sibling is part of a choice, we have already satisfied the choice, and should skip ahead
                        if (requiredSchemaObject.IsChoice && previousSiblingIsChoice)
                            continue;

                        XmlElement newElement = parent.OwnerDocument.CreateElement(requiredSchemaObject.Name);

                        // Fill in defaults from the implementation guide type
                        this.igTypePlugin.FillSampleData(newElement);

                        // If the previous sibling is not null, insert the new element right after the last found valid sibling
                        if (previousSibling != null)
                            parent.InsertAfter(newElement, previousSibling);
                        // If the new schema object is the first in the list of required and there is no previous sibling, and the parent doesn't have any
                        // children yet, append the new element to the end
                        else if (requiredObjects.First() == requiredSchemaObject && parent.ChildNodes.Count == 0)
                            parent.AppendChild(newElement);
                        // If the new schema object is the first in the list of required and there ARE children within the parent,
                        // insert the node before the first of the parent's children
                        else if (requiredObjects.First() == requiredSchemaObject && parent.ChildNodes.Count > 0)
                            parent.InsertBefore(newElement, parent.ChildNodes[0]);
                        // Otherwise, just add it to the end
                        else
                            parent.AppendChild(newElement);

                        // Recursively add missing required data
                        this.AddMissingRequiredData(newElement, requiredSchemaObject.Children);
                    }
                }
            }
        }

        private string GetSampleXML(XmlDocument aDocument)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                aDocument.Save(ms);

                ms.Position = 0;

                StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }

        private void AddConstraint(XmlNode aParent, List<SimpleSchema.SchemaObject> siblingSchemaObjects, TemplateConstraint aConstraint)
        {
            // If the constraint is a primitive, add a comment to the sample
            if (aConstraint.IsPrimitive)
            {
                XmlComment comment = _currentDocument.CreateComment("PRIMITIVE: " + aConstraint.PrimitiveText);
                XmlNode nonAttributeParent = GetNonAttributeParent(aParent);

                if (nonAttributeParent != null)
                    nonAttributeParent.AppendChild(comment);
            }
            // If the constraint is not primitive and does not have a context, add an error comment to the sample
            else if (!aConstraint.IsPrimitive && string.IsNullOrEmpty(aConstraint.Context))
            {
                XmlComment comment =
                    _currentDocument.CreateComment("ERROR: Constraint " + aConstraint.Id.ToString() +
                                                   " does not have a context.");
                aParent.AppendChild(comment);
            }
            else
            {
                // Splitting on / in case legacy data is exported that makes use of xpath in constraint contexts
                string[] lContextSplit = aConstraint.Context.Split('/');
                XmlNode lCurrentNode = aParent;
                SimpleSchema.SchemaObject lSchemaObject = null;

                foreach (string lCurrentContextSplit in lContextSplit)
                {
                    bool lIsAttribute = lCurrentContextSplit.StartsWith("@");
                    string lStrippedContext = lCurrentContextSplit.Replace("@", "");

                    // Add an error comment if an attribute is a child of another attribute (ex: "@root/@extension")
                    if (lIsAttribute && lCurrentNode is XmlAttribute)
                    {
                        XmlComment comment =
                            _currentDocument.CreateComment(
                                "ERROR: An attribute cannot be a child of another attribute for constraint " +
                                aConstraint.Id.ToString());
                        lCurrentNode.AppendChild(comment);
                        break;
                    }

                    // Find the schema object associated with this constraint
                    if (siblingSchemaObjects != null)
                    {
                        lSchemaObject =
                            siblingSchemaObjects.SingleOrDefault(
                                y => y.Name == lStrippedContext && y.IsAttribute == lIsAttribute);
                    }

                    // If the constraint is an attribute, add an attribute to the parent
                    if (lIsAttribute)
                    {
                        XmlAttribute attribute = lCurrentNode.Attributes[lStrippedContext];

                        if (attribute == null)
                        {
                            attribute = _currentDocument.CreateAttribute(lStrippedContext);
                            lCurrentNode.Attributes.Append(attribute);
                        }

                        if (!string.IsNullOrEmpty(aConstraint.Value))
                            attribute.Value = aConstraint.Value;
                        else
                            attribute.Value = "XXX";

                        lCurrentNode = attribute;
                    }
                    // Otherwise, create an element and add it to the parent
                    else
                    {
                        XmlElement lNewContextElement = _currentDocument.CreateElement(lCurrentContextSplit);

                        if (lSchemaObject != null)
                        {
                            foreach (SimpleSchema.SchemaObject lCurrentSchemaAttribute in lSchemaObject.Children.Where(y => y.IsAttribute))
                            {
                                if (!lCurrentSchemaAttribute.Cardinality.StartsWith("1.."))
                                    continue;

                                XmlAttribute newAttribute = _currentDocument.CreateAttribute(lCurrentSchemaAttribute.Name);
                                lNewContextElement.Attributes.Append(newAttribute);

                                if (!string.IsNullOrEmpty(lCurrentSchemaAttribute.FixedValue))
                                    newAttribute.Value = lCurrentSchemaAttribute.FixedValue;
                                else if (!string.IsNullOrEmpty(lCurrentSchemaAttribute.Value))
                                    newAttribute.Value = lCurrentSchemaAttribute.Value;
                                else
                                    newAttribute.Value = "YYY";
                            }
                        }

                        // Do not add elements to attributes
                        if (!(lCurrentNode is XmlAttribute))
                        {
                            lCurrentNode.AppendChild(lNewContextElement);
                            lCurrentNode = lNewContextElement;
                        }
                        else
                        {
                            // Add a comment to the generated sample that indicates the problem
                            string commentText = string.Format(
                                "Error: Attribute contains an element. (CONF #: {0}, context: '{1}')",
                                aConstraint.Id,
                                !string.IsNullOrEmpty(aConstraint.Context) ? aConstraint.Context : "N/A");
                            XmlComment comment = this._currentDocument.CreateComment(commentText);

                            if (lCurrentNode.ParentNode != null)
                                lCurrentNode.ParentNode.AppendChild(comment);
                            else
                                this._currentDocument.DocumentElement.AppendChild(comment);
                        }
                    }

                    if (lSchemaObject == null)
                        this.isSchemaValid = false;
                }

                List<TemplateConstraint> childConstraints = aConstraint.ChildConstraints
                    .OrderBy(y => y.Order)
                    .ToList();
                childConstraints.ForEach(y => AddConstraint(lCurrentNode, lSchemaObject != null ? lSchemaObject.Children : null, y));

                if (lSchemaObject != null)
                    this.AddMissingRequiredData(lCurrentNode, lSchemaObject.Children);

                if (lCurrentNode is XmlElement)
                {
                    foreach (XmlNode childNode in lCurrentNode.ChildNodes)
                    {
                        if (childNode is XmlElement)
                            this.igTypePlugin.FillSampleData((XmlElement)childNode);
                    }
                }
            }
        }

        private XmlNode GetNonAttributeParent(XmlNode startingNode)
        {
            if (startingNode.NodeType == XmlNodeType.Attribute)
            {
                XmlAttribute cAttribute = startingNode as XmlAttribute;
                return cAttribute.OwnerElement;
            }

            return startingNode;
        }

        #endregion
    }
}