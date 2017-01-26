using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Trifolia.DB;

namespace Trifolia.Plugins.FHIR
{
    public class DefaultPlugin
    {
        public string TemplateIdentifierElementName
        {
            get { return "meta/profile"; }
        }

        public string TemplateIdentifierRootName
        {
            get { return "@value"; }
        }

        public string TemplateIdentifierExtensionName
        {
            get { return null; }
        }

        public string ClosedTemplateXpath
        {
            get
            {
                return "count(.//{0}profile[{1}])=0";
            }
        }

        public string ClosedTemplateIdentifierXpath
        {
            get
            {
                return "@value != '{1}'";
            }
        }

        public string ClosedTemplateVersionIdentifierXpath
        {
            get
            {
                return "not(@value = '{1}')";
            }
        }

        public void FillSampleData(XmlElement element)
        {
        }

        public string ParseIdentifier(string identifier)
        {
            if (identifier == null || !identifier.StartsWith("http"))
                throw new ArgumentException("identifier must be an http identifier for FHIR implementation guides");

            return identifier;
        }

        public void AddTemplateIdentifierToSample(XmlElement templateElement, Template template)
        {
            string searchXpath = null;
            string uri = null;

            if (template.GetIdentifierURL(out uri))
                searchXpath = string.Format("meta/profile[@value='{0}']", uri);
            else
                throw new Exception("Unexpected identifier format!");

            XmlNode profileNode = templateElement.SelectSingleNode(searchXpath);

            if (profileNode == null)
            {
                var metaNode = templateElement.SelectSingleNode("meta");

                if (metaNode == null)
                {
                    metaNode = templateElement.OwnerDocument.CreateElement("meta");

                    if (templateElement.ChildNodes.Count > 0)
                        templateElement.InsertBefore(metaNode, templateElement.ChildNodes[0]);
                    else
                        templateElement.AppendChild(metaNode);
                }

                profileNode = templateElement.OwnerDocument.CreateElement("profile");

                XmlAttribute rootAttr = templateElement.OwnerDocument.CreateAttribute("value");
                rootAttr.Value = uri;
                profileNode.Attributes.Append(rootAttr);
            }
        }

        public string GenerateSample(DB.IObjectRepository tdb, DB.Template template)
        {
            DefaultSampleGenerator sampleGenerator = DefaultSampleGenerator.CreateDefaultSampleGenerator(tdb, template);
            return sampleGenerator.GenerateSample();
        }

        protected byte[] ConvertToBytes(string content)
        {
            return System.Text.Encoding.UTF8.GetBytes(content);
        }
    }
}
