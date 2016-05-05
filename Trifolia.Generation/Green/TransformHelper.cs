using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Trifolia.Shared;

namespace Trifolia.Generation.Green
{
    public class TransformHelper
    {
        public const string XslNamespaceUri = "http://www.w3.org/1999/XSL/Transform";
        public const string XslNamespacePrefix = "xsl";
        public const string GreenNamespaceUri = "urn:hl7-org:v3:green";
        public const string GreenNamespacePrefix = "green";
        public const string IgNamespacePrefix = "ig";

        public static void InitializeTransform(XmlDocument doc, string schemaNamespace)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace(TransformHelper.GreenNamespacePrefix, TransformHelper.GreenNamespaceUri);
            nsManager.AddNamespace(TransformHelper.XslNamespacePrefix, TransformHelper.XslNamespaceUri);
            nsManager.AddNamespace(TransformHelper.IgNamespacePrefix, schemaNamespace);

            XmlElement stylesheetElement = doc.CreateElement(TransformHelper.XslNamespacePrefix, "stylesheet", TransformHelper.XslNamespaceUri);
            stylesheetElement.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "version", "1.0"));
            doc.AppendChild(stylesheetElement);

            XmlElement outputElement = doc.CreateElement(XslNamespacePrefix, "output", XslNamespaceUri);
            outputElement.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "method", "xml"));
            outputElement.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "version", "1.0"));
            outputElement.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "encoding", "UTF-8"));
            outputElement.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "indent", "yes"));
            stylesheetElement.AppendChild(outputElement);
        }

        public static XmlElement CreateXslTemplate(XmlDocument doc, string name, string match=null)
        {
            XmlElement templateElement = doc.CreateElement(XslNamespacePrefix, "template", XslNamespaceUri);

            templateElement.Attributes.Append(
                CreateXsltAttribute(doc, "name", name));

            if (!string.IsNullOrEmpty(match))
            {
                templateElement.Attributes.Append(
                    CreateXsltAttribute(doc, "match", match));
            }

            return templateElement;
        }

        public static XmlAttribute CreateXsltAttribute(XmlDocument doc, string name, string value)
        {
            XmlAttribute newAttribute = doc.CreateAttribute(name);
            newAttribute.Value = value;

            return newAttribute;
        }

        public static XmlElement CreateXsltTemplateParam(XmlDocument doc, string name)
        {
            XmlElement instanceParam = doc.CreateElement(TransformHelper.XslNamespacePrefix, "param", TransformHelper.XslNamespaceUri);
            XmlAttribute instanceParamNameAttr = TransformHelper.CreateXsltAttribute(doc, "name", name);
            instanceParam.Attributes.Append(instanceParamNameAttr);
            return instanceParam;
        }

        public static XmlElement CreateXsltValueOf(XmlDocument doc, string selectXpath)
        {
            XmlElement valueOfElement = doc.CreateElement(TransformHelper.XslNamespacePrefix, "value-of", TransformHelper.XslNamespaceUri);
            XmlAttribute selectAttr = doc.CreateAttribute("select");
            selectAttr.Value = selectXpath;
            valueOfElement.Attributes.Append(selectAttr);
            return valueOfElement;
        }

        public static XmlElement CreateXsltApplyTemplates(XmlDocument doc, string name=null, string select=null, string paramName=null, string paramSelect=null)
        {
            string xslName = name != null && select == null ? "call-template" : "apply-template";

            XmlElement applyTemplatesElement = doc.CreateElement(TransformHelper.XslNamespacePrefix, xslName, TransformHelper.XslNamespaceUri);

            if (name != null)
            {
                applyTemplatesElement.Attributes.Append(
                    TransformHelper.CreateXsltAttribute(doc, "name", Helper.NormalizeName(name)));
            }

            if (select != null)
            {
                applyTemplatesElement.Attributes.Append(
                    TransformHelper.CreateXsltAttribute(doc, "select", select));
            }

            if (paramName != null && paramSelect != null)
            {
                XmlElement applyTemplatesWithParam = doc.CreateElement(TransformHelper.XslNamespacePrefix, "with-param", TransformHelper.XslNamespaceUri);
                applyTemplatesWithParam.Attributes.Append(
                    CreateXsltAttribute(doc, "name", paramName));
                applyTemplatesWithParam.Attributes.Append(
                    CreateXsltAttribute(doc, "select", paramSelect));
                applyTemplatesElement.AppendChild(applyTemplatesWithParam);
            }

            return applyTemplatesElement;
        }

        public static XmlElement CreateXsltIf(XmlDocument doc, string test)
        {
            XmlElement attributeIfEle = doc.CreateElement(TransformHelper.XslNamespacePrefix, "if", TransformHelper.XslNamespaceUri);
            attributeIfEle.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "test", test));
            return attributeIfEle;
        }

        public static XmlElement CreateXsltAttributeWithValueOf(XmlDocument doc, string attributeName, string attributeValueXpath)
        {
            XmlElement attributeEle = doc.CreateElement(TransformHelper.XslNamespacePrefix, "attribute", TransformHelper.XslNamespaceUri);
            attributeEle.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "name", attributeName));
            attributeEle.AppendChild(
                TransformHelper.CreateXsltValueOf(doc, attributeValueXpath));

            return attributeEle;
        }

        public static XmlElement CreateXsltElement(XmlDocument doc, string elementName)
        {
            XmlElement newElement = doc.CreateElement(TransformHelper.XslNamespacePrefix, "element", TransformHelper.XslNamespaceUri);
            newElement.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "name", elementName));
            return newElement;
        }

        public static XmlElement CreateXsltForEach(XmlDocument doc, string select)
        {
            XmlElement newForeachEle = doc.CreateElement(TransformHelper.XslNamespacePrefix, "for-each", TransformHelper.XslNamespaceUri);
            newForeachEle.Attributes.Append(
                TransformHelper.CreateXsltAttribute(doc, "select", select));
            return newForeachEle;
        }
    }
}
