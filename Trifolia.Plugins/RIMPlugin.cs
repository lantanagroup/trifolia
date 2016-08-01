using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;

namespace Trifolia.Plugins
{
    public class RIMPlugin : IIGTypePlugin
    {
        public string TemplateIdentifierXpath
        {
            get
            {
                return "{0}templateId[@root='{1}']";
            }
        }

        public string TemplateVersionIdentifierXpath
        {
            get
            {
                return "{0}templateId[@root='{1}' and @extension = '{2}']";
            }
        }

        public string ClosedTemplateXpath
        {
            get
            {
                return "count(.//{0}templateId[{1}])=0";
            }
        }

        public string ClosedTemplateIdentifierXpath
        {
            get
            {
                return "@root != '{1}'";
            }
        }

        public string ClosedTemplateVersionIdentifierXpath
        {
            get
            {
                return "not(@root = '{1}' and @extension = '{2}')";
            }
        }

        public void FillSampleData(XmlElement element)
        {
            switch (element.Name)
            {
                case "birthTime":
                    CreateSampleAttribute(element, "value", FormatDate(DateTime.Now.AddYears(-50), true));
                    break;
                case "id":
                case "setId":
                    CreateSampleAttribute(element, "root", Guid.NewGuid().ToString());
                    break;
                case "versionNumber":
                    CreateSampleAttribute(element, "value", "1");
                    break;
                case "confidentialityCode":
                    CreateSampleAttribute(element, "codeSystem", "2.16.840.1.113883.5.25");
                    CreateSampleAttribute(element, "code", "N");
                    break;
                case "effectiveTime":
                case "time":
                    CreateSampleAttribute(element, "value", FormatDate(DateTime.Now, true));
                    break;
                case "administrativeGenderCode":
                    CreateSampleAttribute(element, "code", "F");
                    CreateSampleAttribute(element, "displayName", "Female");
                    CreateSampleAttribute(element, "codeSystem", "2.16.840.1.113883.5.1");
                    CreateSampleAttribute(element, "codeSystemName", "AdministrativeGender");
                    break;
                case "typeId":
                    CreateSampleAttribute(element, "extension", "POCD_HD000040");
                    break;
            }
        }

        public void AddTemplateIdentifierToSample(XmlElement templateElement, Template template)
        {
            string searchXpath = null;
            string oid, root, extension, uri;

            if (template.GetIdentifierOID(out oid))
                searchXpath = string.Format("templateId[@root='{0}']", oid);
            else if (template.GetIdentifierII(out root, out extension))
                searchXpath = string.Format("templateId[@root='{0}'][@extension='{1}']", root, extension);
            else if (template.GetIdentifierURI(out uri))
                searchXpath = string.Format("templateId[@root='{0}']", uri);
            else
                throw new Exception("Unexpected identifier format!");

            XmlNode identifierNode = templateElement.SelectSingleNode(searchXpath);

            if (identifierNode == null)
            {
                identifierNode = templateElement.OwnerDocument.CreateElement("templateId");

                if (templateElement.ChildNodes.Count > 0)
                    templateElement.InsertBefore(identifierNode, templateElement.ChildNodes[0]);
                else
                    templateElement.AppendChild(identifierNode);

                if (template.GetIdentifierOID(out oid))
                {
                    XmlAttribute rootAttr = templateElement.OwnerDocument.CreateAttribute("root");
                    rootAttr.Value = oid;
                    identifierNode.Attributes.Append(rootAttr);
                }
                else if (template.GetIdentifierII(out root, out extension))
                {
                    XmlAttribute rootAttr = templateElement.OwnerDocument.CreateAttribute("root");
                    rootAttr.Value = root;
                    identifierNode.Attributes.Append(rootAttr);

                    XmlAttribute extensionAttr = templateElement.OwnerDocument.CreateAttribute("extension");
                    extensionAttr.Value = extension;
                    identifierNode.Attributes.Append(extensionAttr);
                }
                else if (template.GetIdentifierURI(out uri))
                {
                    XmlAttribute rootAttr = templateElement.OwnerDocument.CreateAttribute("root");
                    rootAttr.Value = uri;
                    identifierNode.Attributes.Append(rootAttr);
                }
            }
        }

        public string ParseIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return identifier;

            string oidDef = "urn:oid:";

            if (identifier.StartsWith(oidDef))
                return identifier.Substring(oidDef.Length);

            return identifier;
        }

        private static string FormatDate(DateTime date, bool includeTime = false)
        {
            if (includeTime)
                return date.ToString("yyyyMMddHHmmss");

            return date.ToString("yyyyMMdd");
        }

        private static void CreateSampleAttribute(XmlElement parent, string attributeName, string value)
        {
            if (parent.Attributes[attributeName] != null && parent.Attributes[attributeName].Value != "XXXX")
                return;

            var attribute = parent.Attributes[attributeName];

            if (attribute == null)
            {
                attribute = parent.OwnerDocument.CreateAttribute(attributeName);
                parent.Attributes.Append(attribute);
            }

            attribute.Value = value;

            return;
        }

        public string Export(DB.IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<DB.Template> templates, bool includeVocabulary, bool returnJson = true)
        {
            string requestScheme = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url.Scheme : null;
            string requestAuthority = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url.Authority : null;

            switch (format)
            {
                case ExportFormats.FHIR:
                    throw new NotImplementedException();
                case ExportFormats.Proprietary:
                    NativeExporter nativeExporter = new NativeExporter(tdb, templates, igSettings, true, categories);
                    return nativeExporter.GenerateXMLExport();
                case ExportFormats.TemplatesDSTU:
                    DecorExporter decorExporter = new DecorExporter(templates, tdb, igSettings.ImplementationGuideId);
                    return decorExporter.GenerateXML();
                default:
                    throw new Exception("Invalid export format for the specified implementation guide type");
            }
        }

        public string GenerateSample(DB.IObjectRepository tdb, DB.Template template)
        {
            DefaultSampleGenerator sampleGenerator = DefaultSampleGenerator.CreateDefaultSampleGenerator(tdb, template);
            return sampleGenerator.GenerateSample();
        }
    }
}
