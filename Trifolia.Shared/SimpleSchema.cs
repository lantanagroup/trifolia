using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public interface ISimpleSchema
    {
        List<Trifolia.Shared.SimpleSchema.SchemaObject> ComplexTypes { get; set; }
    }

    [Serializable]
    public class SimpleSchema : ISimpleSchema
    {
        public const string SchemaChoiceAppInfoUri = "https://trifolia.lantanagroup.com/choiceName";

        #region Private Fields

        private const int maxInitialDepth = 2;
        private List<SchemaObject> children = new List<SchemaObject>();
        private List<SchemaObject> complexTypes = new List<SchemaObject>();
        private bool initializing = false;
        [NonSerialized]
        private XmlSchema schema;
        private string schemaLocation;
        private Regex uniqueStringContextRegex = new Regex(@"(.+)\[(.*)\]");
        private bool isInContext = false;
        private Dictionary<string, string> namespaces = null;

        #endregion

        #region Properties

        public List<SchemaObject> Children
        {
            get
            {
                if (this.isInContext)
                    return children[0].Children;

                return children;
            }
        }

        public List<SchemaObject> ComplexTypes
        {
            get { return complexTypes; }
            set { complexTypes = value; }
        }

        public XmlSchema Schema
        {
            get
            {
                if (this.schema == null)
                    this.schema = Helper.GetSchema(this.schemaLocation, out this.namespaces);

                return this.schema;
            }
        }

        public Dictionary<string, string> Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }

        #endregion

        #region Ctor

        private SimpleSchema()
        { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new instance of <see cref="SimpleSchema"/>
        /// </summary>
        /// <param name="schemaLocation"></param>
        /// <returns></returns>
        public static SimpleSchema CreateSimpleSchema(string schemaLocation)
        {
            SimpleSchema lSchema = new SimpleSchema();
            lSchema.LoadSchema(schemaLocation);

            //lSchema.schemaLocation = schemaLocation;
            //lSchema.initializing = true;

            //// Load complex types
            //foreach (XmlSchemaObject cObject in lSchema.Schema.SchemaTypes.Values)
            //{
            //    XmlSchemaComplexType cComplexType = cObject as XmlSchemaComplexType;

            //    if (cComplexType != null)
            //        lSchema.InitializeComplexType(cComplexType);
            //}

            //// Pick up the full schema
            //foreach (XmlSchemaObject cObject in lSchema.Schema.Items)
            //{
            //    XmlSchemaElement cElement = cObject as XmlSchemaElement;

            //    if (cElement != null)
            //        lSchema.InitializeElement(null, cElement);
            //}

            //lSchema.initializing = false;

            return lSchema;
        }

        public class SimpleSchemaFactory
        {
            public virtual ISimpleSchema Create(string schemaLocation)
            {
                return this.OnCreate(schemaLocation);
            }

            protected virtual ISimpleSchema OnCreate(string schemaLocation)
            {
                var s = new SimpleSchema();
                s.LoadSchema(schemaLocation);
                return s;
            }
        }

        public void LoadSchema(string schemaLocation)
        {
            //SimpleSchema lSchema = new SimpleSchema();
            this.schemaLocation = schemaLocation;
            this.initializing = true;

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add(this.Schema);
            schemaSet.Compile();

            // Load complex types
            foreach (XmlSchemaObject cObject in schemaSet.GlobalTypes.Values)
            {
                XmlSchemaComplexType cComplexType = cObject as XmlSchemaComplexType;

                if (cComplexType != null)
                    this.InitializeComplexType(cComplexType);
            }

            // Pick up the full schema
            foreach (XmlSchemaObject cObject in this.Schema.Items)
            {
                XmlSchemaElement cElement = cObject as XmlSchemaElement;

                if (cElement != null)
                    this.InitializeElement(null, cElement);
            }

            this.initializing = false;

            //return lSchema;
        }

        /// <summary>
        /// Finds a SchemaObject element within the currently loaded schema based on the path specified.
        /// </summary>
        public SchemaObject FindFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string[] pathParts = path.Split('/');
            SchemaObject current = null;

            foreach (string cPathPart in pathParts)
            {
                string cActualContext = cPathPart;
                string cActualDataType = string.Empty;

                if (this.uniqueStringContextRegex.IsMatch(cPathPart))
                {
                    Match uniqueStringMatch = this.uniqueStringContextRegex.Match(cPathPart);
                    cActualContext = uniqueStringMatch.Groups[1].Value;
                    cActualDataType = uniqueStringMatch.Groups[2].Value;
                }

                bool isAttribute = cActualContext != null ? cActualContext.StartsWith("@") : false;

                if (isAttribute)
                    cActualContext = cActualContext.Substring(1);

                if (current == null)
                    current = this.Children.SingleOrDefault(y => y.Name == cActualContext && y.IsAttribute == isAttribute);
                else
                    current = current.Children.SingleOrDefault(y => y.Name == cActualContext && y.IsAttribute == isAttribute);

                if (current == null)
                {
                    SchemaObject complexType = null;

                    if (!string.IsNullOrEmpty(cActualDataType))
                        complexType = this.ComplexTypes.SingleOrDefault(y => y.Name == cActualDataType);
                    else if (!string.IsNullOrEmpty(cActualContext))
                        complexType = this.ComplexTypes.SingleOrDefault(y => y.Name == cActualContext);

                    if (complexType != null)
                    {
                        complexType = complexType.Clone(this);
                        complexType.Name = cActualContext;
                        complexType.Parent = current;
                        current = complexType;
                    }
                }

                if (current == null)
                    return null;
            }

            return current;
        }

        /// <summary>
        /// Finds the SchemaObject within the currently loaded schema based on the type
        /// </summary>
        /// <param name="type">The type to look for within the current schema. Expects it to be a complex type</param>
        public SchemaObject FindFromType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return null;

            SchemaObject complexType = this.complexTypes.SingleOrDefault(y => y.Name != null && y.Name.ToLower() == type.ToLower());

            if (complexType != null)
            {
                SchemaObject newComplexType = complexType.Clone(this);
                newComplexType.IsInitialized = false;
                return newComplexType;
            }

            return null;
        }

        public List<SchemaObject> GetDerivedTypes(string type)
        {
            if (string.IsNullOrEmpty(type) || type == "ANY")
                return this.ComplexTypes;

            SchemaObject simpleComplexType = this.ComplexTypes.SingleOrDefault(y => y.Name.ToLower() == type.ToLower());

            if (simpleComplexType == null)
                return null;

            XmlSchemaComplexType complexType = simpleComplexType.XmlObject as XmlSchemaComplexType;
            List<SchemaObject> derivedTypes = new List<SchemaObject>();

            complexType = this.Schema.SchemaTypes[complexType.QualifiedName] as XmlSchemaComplexType;

            foreach (XmlSchemaType globalType in this.Schema.SchemaTypes.Values)
            {
                if (XmlSchemaType.IsDerivedFrom(globalType, complexType, XmlSchemaDerivationMethod.None))
                {
                    string name = Helper.GetDataTypeName(this.Schema.TargetNamespace, globalType.Name);
                    derivedTypes.Add(this.ComplexTypes.Single(y => y.Name == name));
                }
            }

            return derivedTypes;
        }

        /// <summary>
        /// Gets a SimpleSchema based on a specific context within the current schema. This is for allowing
        /// a template to be of a different context than just the top-level of the entire schema.
        /// </summary>
        /// <param name="contextType">Indicates if the context is a full XPATH or just a complex type</param>
        /// <param name="context">The context to search for and return as its own schema</param>
        public SimpleSchema GetSchemaFromContext(string context)
        {
            if (string.IsNullOrEmpty(context))
                return this;

            SchemaObject foundObject = this.complexTypes.SingleOrDefault(y => y.Name.ToLower() == context.ToLower());

            if (foundObject != null)
            {
                SimpleSchema newSimpleSchema = new SimpleSchema();
                newSimpleSchema.namespaces = this.namespaces;
                newSimpleSchema.schemaLocation = this.schemaLocation;

                newSimpleSchema.Children.Add(foundObject);
                newSimpleSchema.ComplexTypes = this.ComplexTypes;
                newSimpleSchema.isInContext = true;
                return newSimpleSchema;
            }

            return null;
        }

        public string GetName(XmlQualifiedName name, XmlQualifiedName refName)
        {
            XmlQualifiedName qName = name != null && !string.IsNullOrEmpty(name.Name) ? name : refName;
            string localName = qName.Name;

            if (localName.Contains(".") && name.Namespace == "urn:hl7-org:v3")
                localName = localName.Substring(localName.LastIndexOf(".") + 1);

            if (qName == null || string.IsNullOrEmpty(qName.Name))
                throw new Exception("Can't identify a proper name for this node.");

            if (!string.IsNullOrEmpty(qName.Namespace) && qName.Namespace != this.Schema.TargetNamespace)
            {
                if (this.namespaces.ContainsKey(qName.Namespace))
                {
                    return string.Format("{0}:{1}",
                        this.namespaces[qName.Namespace],
                        localName);
                }
                else
                {
                    return string.Format("{{{0}}}{1}", qName.Namespace, localName);
                }
            }

            return localName;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new SchemaObject as ObjectTypes.ComplexType
        /// </summary>
        /// <param name="complexType">The XSD complext ype that the new SchemaObject should be based on</param>
        private SchemaObject InitializeComplexType(XmlSchemaComplexType complexType)
        {
            SchemaObject newSimpleObject = new SchemaObject(this, complexType, false)
            {
                Name = this.GetName(complexType.QualifiedName, null),
                DataType = complexType.BaseXmlSchemaType != null && !string.IsNullOrEmpty(complexType.BaseXmlSchemaType.Name) ? 
                    this.GetName(complexType.BaseXmlSchemaType.QualifiedName, null) : 
                    string.Empty,
                Type = ObjectTypes.ComplexType
            };

            // TODO: More complicated logic to determine if a restriction is being applied
            if (this.complexTypes.Count(y => y.Name == newSimpleObject.Name) > 0)
                return this.complexTypes.SingleOrDefault(y => y.Name == newSimpleObject.Name);

            if (IsRecurring(newSimpleObject))
                return null;

            if (!this.initializing || newSimpleObject.Depth <= maxInitialDepth)
                newSimpleObject.InitializeChildren(newSimpleObject, complexType);

            this.complexTypes.Add(newSimpleObject);

            return newSimpleObject;
        }

        /// <summary>
        /// Determines ift he schema object specified is recurring (by comparing the data type to its parents).
        /// </summary>
        private bool IsRecurring(SchemaObject theObject)
        {
            SchemaObject cParent = theObject.Parent;
            bool isRecurring = false;

            while (cParent != null)
            {
                if (cParent.DataType == theObject.DataType)
                {
                    isRecurring = true;
                    break;
                }

                cParent = cParent.Parent;
            }

            return isRecurring;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initializes an element within the schema.
        /// </summary>
        /// <remarks>Detects if the element is recurring. If so, does not continue.</remarks>
        /// <param name="parent">The parent schema object that the new schemaobject (derived from the element parameter) should be attached to.</param>
        /// <param name="element">The XSD element that the new schema object should be created based on.</param>
        /// <returns>The newly created schema object, which is already attached as a child to the parent (or the document as a whole if no parent is specified)</returns>
        protected SchemaObject InitializeElement(SchemaObject parent, XmlSchemaElement element, bool update = false)
        {
            List<SchemaObject> siblings = parent == null ? this.Children : parent.Children;

            SchemaObject newSimpleObject = new SchemaObject(this, element, parent == null || parent.IsChoice)
            {
                Parent = parent,
                Name = this.GetName(element.QualifiedName, element.RefName),
                DataType = element.ElementSchemaType != null && !string.IsNullOrEmpty(element.ElementSchemaType.Name) ? 
                    this.GetName(element.ElementSchemaType.QualifiedName, null) : 
                    null,
                Type = ObjectTypes.Element
            };

            // Make sure the element doesn't already exist
            var foundExisting = siblings.SingleOrDefault(y => y.Name == newSimpleObject.Name);

            if (update && foundExisting != null)
            {
                var foundExistingIndex = siblings.IndexOf(foundExisting);
                siblings.Remove(foundExisting);
                siblings.Insert(foundExistingIndex, newSimpleObject);
            }
            else
            {
                siblings.Add(newSimpleObject);
            }

            if (this.initializing && newSimpleObject.Depth <= maxInitialDepth)
                newSimpleObject.InitializeChildren(newSimpleObject, element);

            return newSimpleObject;
        }

        #endregion

        #region Nested Types

        [Serializable]
        public class SchemaObject
        {
            #region Private Fields

            private ObjectTypes type = ObjectTypes.ComplexType;
            private List<SchemaObject> children = new List<SchemaObject>();
            private bool isInitialized = false;
            [NonSerialized]
            private XmlSchemaObject xmlObject;
            private SimpleSchema simpleSchema;

            #endregion

            #region Properties

            public string Value { get; set; }
            public string FixedValue { get; set; }
            public string Conformance { get; set; }
            public string Cardinality { get; set; }
            public bool Mixed { get; set; }
            public string Name { get; set; }
            public string DataType { get; set; }
            public string ChoiceId { get; set; }
            public bool IsChoice { get; set; }
            public SchemaObject Parent { get; set; }

            public ObjectTypes Type
            {
                get { return type; }
                set { type = value; }
            }

            public bool IsInitialized
            {
                get
                {
                    if (this.type == ObjectTypes.Attribute)
                        return true;

                    return isInitialized;
                }
                set { isInitialized = value; }
            }

            public int LowerBound
            {
                get
                {
                    DB.Cardinality card = CardinalityParser.Parse(this.Cardinality);
                    return card.Left;
                }
            }

            public int UpperBound
            {
                get
                {
                    DB.Cardinality card = CardinalityParser.Parse(this.Cardinality);
                    return card.Right;
                }
            }

            [XmlIgnore]
            public XmlSchemaObject XmlObject
            {
                get { return xmlObject; }
                set { xmlObject = value; }
            }

            public SimpleSchema SimpleSchema
            {
                get { return simpleSchema; }
                set { simpleSchema = value; }
            }

            public List<SchemaObject> Children
            {
                get
                {
                    if (!this.IsInitialized)
                        InitializeChildren(this, this.xmlObject);

                    return children;
                }
                set { children = value; }
            }

            public bool IsAttribute
            {
                get
                {
                    return this.Type == ObjectTypes.Attribute;
                }
            }

            public int Depth
            {
                get
                {
                    int count = 1;
                    SchemaObject cParent = this.Parent;

                    while (cParent != null)
                    {
                        count++;
                        cParent = cParent.Parent;
                    }

                    return count;
                }
            }

            public string Path
            {
                get
                {
                    if (this.Type != ObjectTypes.ComplexType)
                    {
                        string path = this.ToString();
                        SchemaObject cParent = this.Parent;

                        while (cParent != null)
                        {
                            path = path.Insert(0, cParent.ToString() + "/");
                            cParent = cParent.Parent;
                        }

                        return path;
                    }

                    return string.Empty;
                }
            }

            #endregion

            #region Public Methods

            public SchemaObject(SimpleSchema simpleSchema, XmlSchemaObject xmlObject, bool childOfChoice)
            {
                this.simpleSchema = simpleSchema;
                this.xmlObject = xmlObject;

                if (xmlObject is XmlSchemaElement)
                    InitializeElementValues(xmlObject as XmlSchemaElement, childOfChoice);
                else if (xmlObject is XmlSchemaAttribute)
                    InitializeAttributeValues(xmlObject as XmlSchemaAttribute);
                else if (xmlObject is XmlSchemaChoice)
                    InitializeChoice(xmlObject as XmlSchemaChoice);
            }

            public void InitializeChoice(XmlSchemaChoice schemaChoice)
            {
                this.IsChoice = true;

                // Name
                this.SetChoiceName(schemaChoice);

                // Cardinality
                string minOccurs = schemaChoice.MinOccurs.ToString();
                string maxOccurs = schemaChoice.MaxOccurs.ToString();

                this.Cardinality = string.Format("{0}..{1}",
                    minOccurs != "79228162514264337593543950335" ? minOccurs : "*",
                    maxOccurs != "79228162514264337593543950335" ? maxOccurs : "*");

                // Conformance (default based on cardinality)
                if (this.Cardinality == "0..0")
                    this.Conformance = "SHALL NOT";
                else if (this.Cardinality.StartsWith("0.."))
                    this.Conformance = "MAY";
                else if (this.Cardinality.StartsWith("1.."))
                    this.Conformance = "SHALL";
            }

            public void InitializeChildren(SchemaObject theObject, XmlSchemaObject xmlObject)
            {
                XmlSchemaComplexType complexType = xmlObject as XmlSchemaComplexType;

                if (complexType == null && xmlObject is XmlSchemaElement)
                    complexType = ((XmlSchemaElement)xmlObject).ElementSchemaType as XmlSchemaComplexType;

                // if we don't have a complex type, we don't have any children to work with
                if (complexType == null)
                {
                    theObject.IsInitialized = true;
                    return;
                }

                XmlSchemaComplexType baseComplexType = complexType.BaseXmlSchemaType as XmlSchemaComplexType;

                // Load the base type info
                if (baseComplexType != null)
                    InitializeChildren(theObject, baseComplexType);

                theObject.IsInitialized = true;

                XmlSchemaObjectCollection attributes = complexType.Attributes;
                XmlSchemaSequence complexTypeSequence = complexType.ContentTypeParticle as XmlSchemaSequence;
                XmlSchemaChoice complexTypeChoice = complexType.ContentTypeParticle as XmlSchemaChoice;

                // Add attributes
                XmlSchemaComplexContentRestriction restriction = complexType.ContentModel != null ? complexType.ContentModel.Content as XmlSchemaComplexContentRestriction : null;
                XmlSchemaComplexContentExtension extension = complexType.ContentModel != null ? complexType.ContentModel.Content as XmlSchemaComplexContentExtension : null;
                XmlSchemaObjectCollection baseAttributes = null;

                if (restriction != null)
                    baseAttributes = restriction.Attributes;
                else if (extension != null)
                    baseAttributes = extension.Attributes;

                if (baseAttributes != null)
                {
                    foreach (XmlSchemaObject cObject in baseAttributes)
                    {
                        XmlSchemaAttribute cAttribute = cObject as XmlSchemaAttribute;

                        // TODO: Do something more complicated when cObject is an attribute group reference
                        if (cAttribute == null)
                            continue;

                        var foundChild = theObject.Children.SingleOrDefault(y => y.Name == cAttribute.Name);
                        var newChild = new SchemaObject(theObject.simpleSchema, cAttribute, theObject.IsChoice)
                        {
                            Parent = theObject,
                            Name = this.SimpleSchema.GetName(cAttribute.QualifiedName, cAttribute.RefName),
                            Type = ObjectTypes.Attribute
                        };

                        if (foundChild != null && restriction != null)
                        {
                            var foundIndex = theObject.Children.IndexOf(foundChild);
                            theObject.Children.RemoveAt(foundIndex);
                            theObject.Children.Insert(foundIndex, newChild);
                        }
                        else if (foundChild == null)
                        {
                            theObject.Children.Add(newChild);
                        }
                    }
                }

                foreach (XmlSchemaObject cObject in complexType.Attributes)
                {
                    XmlSchemaAttribute cAttribute = cObject as XmlSchemaAttribute;

                    // TODO: More complicated logic to determine if a restriction is being applied
                    if (cAttribute != null && !theObject.Children.Exists(y => y.Name == cAttribute.Name))
                        theObject.Children.Add(
                            new SchemaObject(theObject.simpleSchema, cAttribute, theObject.IsChoice)
                            {
                                Parent = theObject,
                                Name = this.SimpleSchema.GetName(cAttribute.QualifiedName, cAttribute.RefName),
                                Type = ObjectTypes.Attribute
                            });
                }

                bool update = extension != null || restriction != null;

                // Add child elements (un-initialized)
                if (complexTypeSequence != null)
                    InitializeXmlSequence(theObject, complexTypeSequence, update);

                // Add all choice items
                if (complexTypeChoice != null)
                    InitializeXmlChoice(theObject, complexTypeChoice, update);
            }

            public override string ToString()
            {
                if (this.DataType != null)
                    return string.Format("{0}[{1}]", this.Name, this.DataType);

                return this.Name;
            }

            public SchemaObject Clone(SimpleSchema schema)
            {
                return Copy(this, schema);
            }

            #endregion

            #region Private Methods

            private string GetChoiceAppInfoText(XmlSchemaAnnotation annotation)
            {
                XmlSchemaAppInfo appInfo = annotation.Items.OfType<XmlSchemaAppInfo>().SingleOrDefault(y => y.Source == SimpleSchema.SchemaChoiceAppInfoUri);

                if (appInfo != null)
                {
                    XmlText nameText = appInfo.Markup.OfType<XmlText>().FirstOrDefault();

                    if (nameText != null && !string.IsNullOrEmpty(nameText.Value))
                    {
                        return nameText.Value;
                    }
                }

                return string.Empty;
            }

            private void SetChoiceName(XmlSchemaChoice schemaChoice)
            {
                // See if the schema defines the name of the choice element
                if (schemaChoice.Annotation != null)
                {
                    this.Name = GetChoiceAppInfoText(schemaChoice.Annotation);
                }
                else if (schemaChoice.Items.Count > 0 && ((XmlSchemaChoice)schemaChoice.Items[0].Parent).Annotation != null)
                {
                    // Weird bug in System.Xml.Schema where annotations stored in the schema are reflected by the children's parent,
                    // and are never directly on schemaChoice.Annotation
                    this.Name = GetChoiceAppInfoText(((XmlSchemaChoice)schemaChoice.Items[0].Parent).Annotation);
                }

                if (!string.IsNullOrEmpty(this.Name))
                    return;

                this.Name = Shared.Helper.GetChoiceCommonName(schemaChoice, this.SimpleSchema.Schema.TargetNamespace);

                // Save the choice name to the schema
                XmlSchemaAppInfo newAppInfo = new XmlSchemaAppInfo();
                newAppInfo.Source = SimpleSchema.SchemaChoiceAppInfoUri;
                newAppInfo.Markup = new XmlNode[] { new XmlDocument().CreateTextNode(this.Name) };

                if (schemaChoice.Annotation == null)
                    schemaChoice.Annotation = new XmlSchemaAnnotation();

                schemaChoice.Annotation.Items.Add(newAppInfo);
            }

            private void InitializeXmlSequence(SchemaObject parent, XmlSchemaSequence sequence, bool update = false)
            {
                foreach (XmlSchemaObject cChildType in sequence.Items)
                {
                    XmlSchemaElement cSequenceElement = cChildType as XmlSchemaElement;
                    XmlSchemaChoice cSequenceChoice = cChildType as XmlSchemaChoice;

                    if (cSequenceElement != null)
                    {
                        this.simpleSchema.InitializeElement(parent, cSequenceElement, update);
                    }
                    else if (cSequenceChoice != null)
                    {
                        InitializeXmlChoice(parent, cSequenceChoice, update);
                    }
                }
            }

            private void InitializeXmlChoice(SchemaObject parent, XmlSchemaChoice choice, bool update = false)
            {
                SchemaObject choiceObject = new SchemaObject(this.simpleSchema, choice, false);
                choiceObject.Parent = parent;
                parent.Children.Add(choiceObject);

                foreach (XmlSchemaObject cChoiceObject in choice.Items)
                {
                    XmlSchemaElement cChoiceElement = cChoiceObject as XmlSchemaElement;
                    XmlSchemaSequence cChoiceSequence = cChoiceObject as XmlSchemaSequence;

                    if (cChoiceElement != null)
                    {
                        this.simpleSchema.InitializeElement(choiceObject, cChoiceElement, update);
                    }
                    else if (cChoiceSequence != null)
                    {
                        InitializeXmlSequence(choiceObject, cChoiceSequence, update);
                    }
                }
            }

            private void InitializeAttributeValues(XmlSchemaAttribute attribute)
            {
                this.Value = attribute.DefaultValue;
                this.FixedValue = attribute.FixedValue;

                if (attribute.Use == XmlSchemaUse.Prohibited)
                {
                    this.Conformance = "SHALL NOT";
                    this.Cardinality = "0..0";
                }
                else if (attribute.Use == XmlSchemaUse.Required)
                {
                    this.Conformance = "SHALL";
                    this.Cardinality = "1..1";
                }
                else
                {
                    this.Conformance = "MAY";
                    this.Cardinality = "0..1";
                }

                if (attribute.SchemaTypeName != null && !string.IsNullOrEmpty(attribute.SchemaTypeName.Name))
                {
                    if (attribute.SchemaTypeName.Name.Contains("."))
                        this.DataType = attribute.SchemaTypeName.Name.Substring(attribute.SchemaTypeName.Name.LastIndexOf(".") + 1);
                    else
                        this.DataType = attribute.SchemaTypeName.Name;
                }
            }

            private void InitializeElementValues(XmlSchemaElement element, bool childOfChoice)
            {
                if (!childOfChoice)
                {
                    string minOccurs = element.MinOccurs.ToString();
                    string maxOccurs = element.MaxOccurs.ToString();

                    this.Cardinality = string.Format("{0}..{1}",
                        minOccurs != "79228162514264337593543950335" ? minOccurs : "*",
                        maxOccurs != "79228162514264337593543950335" ? maxOccurs : "*");

                    if (this.Cardinality == "0..0")
                        this.Conformance = "SHALL NOT";
                    else if (this.Cardinality.StartsWith("0.."))
                        this.Conformance = "MAY";
                    else if (this.Cardinality.StartsWith("1.."))
                        this.Conformance = "SHALL";
                }

                if (element.ElementSchemaType != null && !string.IsNullOrEmpty(element.ElementSchemaType.Name))
                {
                    if (element.ElementSchemaType.Name.Contains("."))
                        this.DataType = element.ElementSchemaType.Name.Substring(element.ElementSchemaType.Name.LastIndexOf(".") + 1);
                    else
                        this.DataType = element.ElementSchemaType.Name;

                    this.Mixed = element.ElementSchemaType.IsMixed;
                }
            }

            private SchemaObject Copy(SchemaObject source, SimpleSchema schema)
            {
                SchemaObject newSchemaObject = new SchemaObject(schema, source.XmlObject, source.Parent == null || source.Parent.IsChoice)
                {
                    Cardinality = source.Cardinality,
                    Conformance = source.Conformance,
                    DataType = source.DataType,
                    Name = source.Name,
                    Type = source.Type,
                    Mixed = source.Mixed,
                    Value = source.Value,
                    XmlObject = source.XmlObject,
                    FixedValue = source.FixedValue
                };

                return newSchemaObject;
            }

            #endregion
        }

        [Serializable]
        public enum ObjectTypes
        {
            ComplexType,
            Element,
            Attribute
        }

        #endregion

    }
}
