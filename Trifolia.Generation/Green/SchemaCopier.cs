using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Trifolia.Generation.Green
{
    public class SchemaCopier : SchemaGenerationBase
    {
        private XmlSchema sourceSchema;
        private XmlSchema destinationSchema;
        private string ns;

        public SchemaCopier(XmlSchema sourceSchema, XmlSchema destinationSchema, string ns)
        {
            this.sourceSchema = sourceSchema;
            this.destinationSchema = destinationSchema;
            this.ns = ns;
        }

        public void CopyDataType(string dataTypeName)
        {
            foreach (var baseSchemaObject in sourceSchema.SchemaTypes.Values)
            {
                XmlSchemaComplexType baseSchemaComplexType = baseSchemaObject as XmlSchemaComplexType;

                if (baseSchemaComplexType == null || baseSchemaComplexType.Name != dataTypeName)
                    continue;

                CopyDataType(baseSchemaComplexType);
            }
        }

        private void CopyDataType(XmlSchemaComplexType complexType)
        {
            if (FindDataType(destinationSchema, complexType.Name) != null)
                return;

            XmlSchemaComplexType newComplexType = new XmlSchemaComplexType()
            {
                Name = complexType.Name,
                IsMixed = complexType.IsMixed,
                IsAbstract = complexType.IsAbstract,
                
            };
            destinationSchema.Items.Add(newComplexType);

            // Add annotation to indicate where it was copied from
            newComplexType.Annotation = CreateAnnotation("Data type copied from base schema (" + sourceSchema.TargetNamespace + ")");

            // Copy children
            XmlSchemaSequence particleSequence = complexType.Particle as XmlSchemaSequence;
            XmlSchemaComplexContent complexContent = complexType.ContentModel as XmlSchemaComplexContent;

            if (particleSequence != null)
            {
                XmlSchemaSequence newParticleSequence = new XmlSchemaSequence();
                newComplexType.Particle = newParticleSequence;

                foreach (var currentObject in particleSequence.Items)
                {
                    if (currentObject is XmlSchemaElement)
                    {
                        XmlSchemaElement newElement = Copy(currentObject as XmlSchemaElement);
                        newParticleSequence.Items.Add(newElement);
                    }
                }
            }
            else if (complexContent != null)
            {
                XmlSchemaComplexContent newComplexContent = new XmlSchemaComplexContent();
                newComplexType.ContentModel = newComplexContent;
                newComplexContent.IsMixed = complexContent.IsMixed;

                if (complexContent.Content != null)
                {
                    XmlSchemaContent newContent = Copy(complexContent.Content);
                    newComplexContent.Content = newContent;
                }
            }

            foreach (var attribute in complexType.Attributes.Cast<XmlSchemaAttribute>())
            {
                XmlSchemaAttribute newAttribute = this.Copy(attribute);
                newComplexType.Attributes.Add(newAttribute);
            }
        }

        private XmlSchemaContent Copy(XmlSchemaContent content)
        {
            XmlSchemaComplexContentExtension extension = content as XmlSchemaComplexContentExtension;
            XmlSchemaComplexContentRestriction restriction = content as XmlSchemaComplexContentRestriction;

            if (extension != null)
            {
                XmlSchemaComplexContentExtension newExtension = new XmlSchemaComplexContentExtension();

                if (extension.BaseTypeName != null)
                {
                    CopyDataType(extension.BaseTypeName.Name);
                    newExtension.BaseTypeName = new XmlQualifiedName(extension.BaseTypeName.Name, this.ns);
                }

                if (extension.Particle != null)
                {
                    XmlSchemaParticle newParticle = Copy(extension.Particle);
                    newExtension.Particle = newParticle;
                }

                foreach (var attribute in extension.Attributes.Cast<XmlSchemaAttribute>())
                {
                    XmlSchemaAttribute newAttribute = this.Copy(attribute);
                    newExtension.Attributes.Add(newAttribute);
                }

                return newExtension;
            }

            if (restriction != null)
            {
                XmlSchemaComplexContentRestriction newRestriction = new XmlSchemaComplexContentRestriction();

                if (restriction.BaseTypeName != null)
                {
                    CopyDataType(restriction.BaseTypeName.Name);
                    newRestriction.BaseTypeName = new XmlQualifiedName(restriction.BaseTypeName.Name, this.ns);
                }

                if (restriction.Particle != null)
                {
                    XmlSchemaParticle newParticle = Copy(restriction.Particle);
                    newRestriction.Particle = newParticle;
                }

                foreach (var attribute in restriction.Attributes.Cast<XmlSchemaAttribute>())
                {
                    XmlSchemaAttribute newAttribute = this.Copy(attribute);
                    newRestriction.Attributes.Add(newAttribute);
                }

                return newRestriction;
            }

            throw new Exception("Unexpected XmlSchemaComplexContent type (not extension or restriction)");
        }

        private XmlSchemaParticle Copy(XmlSchemaParticle particle)
        {
            XmlSchemaSequence particleSequence = particle as XmlSchemaSequence;
            XmlSchemaChoice particleChoice = particle as XmlSchemaChoice;

            if (particleSequence != null)
            {
                XmlSchemaSequence newSequence = new XmlSchemaSequence();

                foreach (var item in particleSequence.Items)
                {
                    XmlSchemaElement element = item as XmlSchemaElement;
                    XmlSchemaChoice choice = item as XmlSchemaChoice;

                    if (element != null)
                    {
                        XmlSchemaElement newElement = Copy(element);
                        newSequence.Items.Add(newElement);
                    }

                    if (choice != null)
                    {
                        XmlSchemaChoice newChoice = Copy(choice);
                        newSequence.Items.Add(newChoice);
                    }
                }

                return newSequence;
            }
            else if (particleChoice != null)
            {
                return Copy(particleChoice);
            }

            throw new Exception("Unexpected XmlSchemaParticle type (not sequence)");
        }

        private XmlSchemaChoice Copy(XmlSchemaChoice choice)
        {
            XmlSchemaChoice newChoice = new XmlSchemaChoice();
            newChoice.MinOccurs = choice.MinOccurs;
            newChoice.MaxOccurs = choice.MaxOccurs;
            
            foreach (var item in choice.Items)
            {
                XmlSchemaElement element = item as XmlSchemaElement;

                if (element != null)
                {
                    XmlSchemaElement newElement = Copy(element);
                    newChoice.Items.Add(newElement);
                }
            }

            return newChoice;
        }

        private XmlSchemaElement Copy(XmlSchemaElement element)
        {
            XmlSchemaObject schemaTypeObject = element.SchemaTypeName != null ? 
                FindDataType(sourceSchema, element.SchemaTypeName.Name) : 
                null;

            XmlSchemaElement newElement = new XmlSchemaElement();
            newElement.Name = element.Name;
            newElement.MinOccurs = element.MinOccurs;
            newElement.MaxOccurs = element.MaxOccurs;
            newElement.IsAbstract = element.IsAbstract;
            newElement.IsNillable = element.IsNillable;
            newElement.RefName = element.RefName != null ? new XmlQualifiedName(element.RefName.Name) : null;

            if (schemaTypeObject != null)
            {
                if (schemaTypeObject is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType baseComplexType = schemaTypeObject as XmlSchemaComplexType;

                    CopyDataType(baseComplexType.Name);

                    newElement.SchemaTypeName = new XmlQualifiedName(baseComplexType.Name, this.ns);
                }
                else if (schemaTypeObject is XmlSchemaSimpleType)
                {
                    newElement.SchemaTypeName = new XmlQualifiedName("xs:string");
                }
            }

            return newElement;
        }

        private XmlSchemaAttribute Copy(XmlSchemaAttribute attribute)
        {
            XmlSchemaAttribute newAttribute = new XmlSchemaAttribute();
            newAttribute.Use = attribute.Use;
            newAttribute.Name = attribute.Name;

            if (string.IsNullOrEmpty(newAttribute.Name))
                newAttribute.Name = attribute.RefName != null ? attribute.RefName.Name : string.Empty;

            return newAttribute;
        }

        private static XmlSchemaObject FindDataType(XmlSchema schema, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (var item in schema.SchemaTypes.Values)
            {
                XmlSchemaComplexType itemComplexType = item as XmlSchemaComplexType;
                XmlSchemaSimpleType itemSimpleType = item as XmlSchemaSimpleType;

                if (itemComplexType != null && itemComplexType.Name.CompareTo(name) == 0)
                    return itemComplexType;

                if (itemSimpleType != null && itemSimpleType.Name.CompareTo(name) == 0)
                    return itemSimpleType;
            }

            foreach (var item in schema.Items)
            {
                XmlSchemaComplexType itemComplexType = item as XmlSchemaComplexType;
                XmlSchemaSimpleType itemSimpleType = item as XmlSchemaSimpleType;

                if (itemComplexType != null && itemComplexType.Name.CompareTo(name) == 0)
                    return itemComplexType;

                if (itemSimpleType != null && itemSimpleType.Name.CompareTo(name) == 0)
                    return itemSimpleType;
            }

            return null;
        }
    }
}
