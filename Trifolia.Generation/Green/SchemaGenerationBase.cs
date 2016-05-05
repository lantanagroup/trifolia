using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Schema;

namespace Trifolia.Generation.Green
{
    public abstract class SchemaGenerationBase
    {
        private XmlDocument annotationDoc;

        protected XmlSchemaAnnotation CreateAnnotation(string annotationText)
        {
            if (this.annotationDoc == null)
                this.annotationDoc = new XmlDocument();

            XmlSchemaAnnotation annotation = new XmlSchemaAnnotation();
            XmlText annotationTextNode = this.annotationDoc.CreateTextNode(annotationText);
            annotation.Items.Add(new XmlSchemaDocumentation()
            {
                Markup = new XmlNode[] { annotationTextNode }
            });

            return annotation;
        }
    }
}
