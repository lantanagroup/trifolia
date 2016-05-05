using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace TemplateDatabase.Shared
{
    [Serializable]
    public class SimplifiedSchema
    {
        public List<SimplifiedSchemaObject> Children = new List<SimplifiedSchemaObject>();
        public List<SimplifiedSchemaObject> ComplexTypes = new List<SimplifiedSchemaObject>();
        private List<string> recurringTypes = new List<string>();
        private Regex uniqueStringContextRegex = new Regex(@"(.+)\[(.*)\]");
        private XmlSchema schema = null;

        public SimplifiedSchema()
        { }

        public SimplifiedSchema(XmlSchema schema)
        {
            this.schema = schema;

            // Go through each actual element in the schema
            foreach (XmlSchemaObject cObject in schema.Items)
            {
                XmlSchemaElement cElement = cObject as XmlSchemaElement;

                if (cElement != null)
                    InitializeLevel(null, cElement, false);
            }
        }

        public static void Serialize(SimplifiedSchema schema, string location)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            
            using (FileStream fs = new FileStream(location, FileMode.Create))
            {
                serializer.Serialize(fs, schema);
            }
        }

        public static SimplifiedSchema Deserialize(string location)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            
            using (FileStream fs = new FileStream(location, FileMode.Open))
            {
                return serializer.Deserialize(fs) as SimplifiedSchema;
            }
        }

        public SimplifiedSchemaObject GetObjectFromString(string uniqueString)
        {
            string[] contextSplit = uniqueString.Split('/');
            SimplifiedSchemaObject lastFoundObject = null;

            foreach (string cContext in contextSplit)
            {
                if (string.IsNullOrEmpty(cContext))
                    continue;

                string cActualContext = cContext;
                string cActualDataType = string.Empty;

                if (this.uniqueStringContextRegex.IsMatch(cContext))
                {
                    Match uniqueStringMatch = this.uniqueStringContextRegex.Match(cContext);
                    cActualContext = uniqueStringMatch.Groups[1].Value;
                    cActualDataType = uniqueStringMatch.Groups[2].Value;
                }

                List<SimplifiedSchemaObject> searchChildren = this.Children;

                if (lastFoundObject != null)
                    searchChildren = lastFoundObject.Children;

                if (cActualContext.StartsWith("@"))
                    lastFoundObject = searchChildren.SingleOrDefault(y => y.Name == cActualContext.Substring(1) && y.IsAttribute);
                else
                    lastFoundObject = searchChildren.SingleOrDefault(y => y.Name == cActualContext);

                if (lastFoundObject == null)
                    return null;
            }

            return lastFoundObject;
        }

        public SimplifiedSchema GetSchemaFromContext(ContextTypes contextType, string context)
        {
            SimplifiedSchemaObject foundObject = null;

            if (contextType == ContextTypes.ComplexType)
                foundObject = GetFromComplexType(context);
            else if (contextType == ContextTypes.XPath)
                foundObject = GetFromXpath(context);

            if (foundObject != null)
            {
                SimplifiedSchema schema = new SimplifiedSchema();
                schema.Children.Add(foundObject);
                return schema;
            }

            return null;
        }

        private SimplifiedSchemaObject GetFromComplexType(string complexType)
        {
            foreach (SimplifiedSchemaObject schemaObject in this.ComplexTypes)
            {
                if (schemaObject.Name.ToLower() == complexType.ToLower())
                {
                    return schemaObject;
                }
            }

            return null;
        }

        private SimplifiedSchemaObject GetFromXpath(string xpath)
        {
            string[] xpathParts = xpath.Split('/');
            SimplifiedSchemaObject current = null;

            foreach (string cXpathPart in xpathParts)
            {
                if (current == null)
                    current = this.Children.SingleOrDefault(y => y.Name.ToLower() == cXpathPart);
                else
                    current = current.Children.SingleOrDefault(y => y.Name.ToLower() == cXpathPart);

                if (current == null)
                    return null;
            }

            return null;
        }

        private SimplifiedSchemaObject FindOrAddComplexType(XmlQualifiedName complexTypeName)
        {
            string simpleComplexTypeName = Helper.GetDataTypeName(complexTypeName.Name);
            SimplifiedSchemaObject foundComplexType = this.ComplexTypes.SingleOrDefault(y => y.Name == simpleComplexTypeName);

            if (foundComplexType == null)
            {
                XmlSchemaComplexType schemaComplexType = this.schema.SchemaTypes[complexTypeName] as XmlSchemaComplexType;
                foundComplexType = new SimplifiedSchemaObject(schemaComplexType);
                this.ComplexTypes.Add(foundComplexType);
                InitializeChildren(schemaComplexType, foundComplexType, true);
                return foundComplexType;
            }

            return foundComplexType;
        }

        private void InitializeLevel(SimplifiedSchemaObject parent, XmlSchemaElement currentSchemaElement, bool initializingComplexType)
        {
            string currentName = currentSchemaElement.Name;
            string currentDataType = currentSchemaElement.ElementSchemaType != null ? Helper.GetDataTypeName(currentSchemaElement.ElementSchemaType.Name) : string.Empty;

            // If we are initializing a complex type, we should check the complex type to se if it is recursively used. If so, add it to the recurring types.
            // Otherwise, use the IsRecurring method to improve performance by only checking for known recurring types
            if (initializingComplexType)
            {
                if (CheckRecursivelyRecurring(parent, currentName, currentDataType))
                {
                    this.recurringTypes.Add(currentDataType);
                    return;
                }
            }
            else
            {
                if (IsRecurring(parent, currentName, currentDataType))
                    return;
            }
            
            SimplifiedSchemaObject newElement = new SimplifiedSchemaObject(parent, currentSchemaElement);

            if (currentSchemaElement.SchemaTypeName != null)
                newElement.BaseComplexType = FindOrAddComplexType(currentSchemaElement.SchemaTypeName);

            if (parent == null)
                Children.Add(newElement);
            else
                parent.Children.Add(newElement);

            if (newElement.BaseComplexType == null)
                InitializeChildren(currentSchemaElement.ElementSchemaType as XmlSchemaComplexType, newElement, initializingComplexType);
        }

        private void InitializeChildren(XmlSchemaComplexType complexType, SimplifiedSchemaObject element, bool initializingComplexType)
        {
            if (complexType == null)
                return;

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

                    if (cAttribute != null)
                        element.Children.Add(
                            new SimplifiedSchemaObject(element, cAttribute));
                }
            }

            foreach (XmlSchemaObject cObject in complexType.Attributes)
            {
                XmlSchemaAttribute cAttribute = cObject as XmlSchemaAttribute;

                if (cAttribute != null)
                    element.Children.Add(
                        new SimplifiedSchemaObject(element, cAttribute));
            }

            //if (element.ToString() == "ClinicalDocument/recordTarget/patientRole")
            //    Console.WriteLine("Test");

            if (complexType.ContentTypeParticle is XmlSchemaSequence)
            {
                XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;

                foreach (XmlSchemaObject cObject in sequence.Items)
                {
                    // Check here if this element's name is the same as it's parent or grand-parents
                    XmlSchemaElement cElement = cObject as XmlSchemaElement;
                    XmlSchemaChoice cChoice = cObject as XmlSchemaChoice;

                    if (cElement != null)
                    {
                        InitializeLevel(element, cElement, initializingComplexType);
                    }
                    else if (cChoice != null)
                    {
                        foreach (XmlSchemaObject cChoiceObject in cChoice.Items)
                        {
                            XmlSchemaElement cChoiceElement = cChoiceObject as XmlSchemaElement;

                            if (cChoiceElement != null)
                                InitializeLevel(element, cChoiceElement, initializingComplexType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Because of the performance hit involved in recursively checking each parent node to determine if it
        /// is a recursively recurring element, IsRecurring is used to determine if the type in question is 
        /// actually a recurring type. When building complex types, the types that recursively occur
        /// are added to a list, and the list is checked first when the actual document's elements are being built.
        /// This reduces the number of times we have to recursively check parent nodes for re-occurance.
        /// </summary>
        public bool IsRecurring(SimplifiedSchemaObject parent, string compareName, string compareDataType)
        {
            if (this.recurringTypes.Contains(compareDataType))
            {
                compareName = !string.IsNullOrEmpty(compareName) ? compareName : null;
                compareDataType = !string.IsNullOrEmpty(compareDataType) ? compareDataType : null;

                return CheckRecursivelyRecurring(parent, compareName, compareDataType);
            }

            return false;
        }

        private bool CheckRecursivelyRecurring(SimplifiedSchemaObject parent, string compareName, string compareDataType)
        {
            if (parent == null)
                return false;

            string currentName = !string.IsNullOrEmpty(parent.Name) ? parent.Name : null;
            string currentDataType = !string.IsNullOrEmpty(parent.DataType) ? parent.DataType : null;

            if (currentName == compareName && currentDataType == compareDataType)
                return true;

            if (CheckRecursivelyRecurring(parent.Parent, compareName, compareDataType))
                return true;

            return false;
        }
    }

    [Serializable]
    public class SimplifiedSchemaObject
    {
        #region Properties

        private bool mixed = false;

        public bool Mixed
        {
            get { return mixed; }
            set { mixed = value; }
        }
        private SimplifiedSchemaObjectType type = SimplifiedSchemaObjectType.Element;

        public SimplifiedSchemaObjectType Type
        {
            get { return type; }
            set { type = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private List<SimplifiedSchemaObject> children = new List<SimplifiedSchemaObject>();

        public List<SimplifiedSchemaObject> Children
        {
            get
            {
                if (this.baseComplexType != null)
                    return this.baseComplexType.Children;
                else
                    return children;
            }
            set { children = value; }
        }
        private SimplifiedSchemaObject parent;

        public SimplifiedSchemaObject Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        private string conformance;

        public string Conformance
        {
            get { return conformance; }
            set { conformance = value; }
        }
        private string cardinality;

        public string Cardinality
        {
            get { return cardinality; }
            set { cardinality = value; }
        }
        private string dataType;

        public string DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }
        private string value;

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        private string fixedValue;

        public string FixedValue
        {
            get { return fixedValue; }
            set { fixedValue = value; }
        }
        private SimplifiedSchemaObject baseComplexType;

        public SimplifiedSchemaObject BaseComplexType
        {
            get { return baseComplexType; }
            set { baseComplexType = value; }
        }

        public bool IsAttribute
        {
            get { return this.Type == SimplifiedSchemaObjectType.Attribute; }
        }

        #endregion

        public SimplifiedSchemaObject(XmlSchemaComplexType complexType)
        {
            this.Name = Helper.GetDataTypeName(complexType.Name);
            this.Type = SimplifiedSchemaObjectType.ComplexType;
            this.Mixed = complexType.IsMixed;
        }

        public SimplifiedSchemaObject(SimplifiedSchemaObject parent, XmlSchemaElement element)
        {
            this.Name = element.Name;
            this.Type = Shared.SimplifiedSchemaObjectType.Element;
            this.Parent = parent;

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

            if (element.ElementSchemaType != null && !string.IsNullOrEmpty(element.ElementSchemaType.Name))
            {
                this.DataType = Helper.GetDataTypeName(element.ElementSchemaType.Name);
                this.Mixed = element.ElementSchemaType.IsMixed;
            }
        }

        public SimplifiedSchemaObject(SimplifiedSchemaObject parent, XmlSchemaAttribute attribute)
        {
            this.Name = attribute.Name;
            this.Type = SimplifiedSchemaObjectType.Attribute;
            this.Parent = parent;
            this.Value = attribute.DefaultValue;
            this.FixedValue = attribute.FixedValue;

            if (attribute.Use == XmlSchemaUse.Prohibited)
            {
                this.Conformance = "SHALL NOT";
                this.Cardinality = "0..0";
            }
            else if (attribute.Use == XmlSchemaUse.Required || !string.IsNullOrEmpty(FixedValue))
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

        public static List<SimplifiedSchemaObject> GetComplexTypesFromSchema(long implementationGuideTypeId)
        {
            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                ImplementationGuideType igType = tdb.ImplementationGuideTypes.Single(y => y.Id == implementationGuideTypeId);
                SimplifiedSchema schema = new SimplifiedSchema(
                    Helper.GetIGSchema(igType));
                return schema.ComplexTypes;
            }
        }

        public override string ToString()
        {
            string typeName = this.Type == SimplifiedSchemaObjectType.ComplexType ? this.Name : this.DataType;
            string outputName = string.Format("{0}[{1}]", this.Name, typeName);

            if (this.IsAttribute)
                outputName = "@" + outputName;
            else
                outputName += "/";

            return outputName;
        }
    }

    [Serializable]
    public enum SimplifiedSchemaObjectType
    {
        ComplexType,
        Element,
        Attribute
    }
}
