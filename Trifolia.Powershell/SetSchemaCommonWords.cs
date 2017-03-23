using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Trifolia.Authorization;
using System.Security.Principal;
using System.Xml;
using System.Xml.Linq;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Set, "TrifoliaSchemaCommonWords")]
    public class SetTrifoliaSchemaCommonWords : PSCmdlet
    {
        private const string AppInfoSource = "https://trifolia.lantanagroup.com/choiceName";
        private const string SchemaNamespace = "http://www.w3.org/2001/XMLSchema";

        [Parameter(
            Mandatory = true,
            HelpMessage = "The location of the schema to update")]
        public string SchemaLocation { get; set; }

        protected override void ProcessRecord()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(this.SchemaLocation);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("xs", SchemaNamespace);

            bool changedSchema = false;
            var choiceNodes = doc.SelectNodes("//xs:choice", nsManager);

            foreach (XmlElement choiceElement in choiceNodes)
            {
                bool foundNonElement = false;
                List<string[]> elementNameWords = new List<string[]>();

                foreach (XmlElement childNode in choiceElement.ChildNodes)
                {
                    if (childNode.LocalName == "annotation")
                        continue;

                    if (childNode.LocalName != "element" || childNode.Attributes["name"] == null)
                    {
                        foundNonElement = true;
                        break;
                    }

                    string[] nameWords = Shared.Helper.GetWords(childNode.Attributes["name"].Value);
                    elementNameWords.Add(nameWords);
                }

                if (foundNonElement)
                    continue;

                string commonName = Shared.Helper.FindCommonWord(elementNameWords);

                if (string.IsNullOrEmpty(commonName))
                    continue;

                commonName = commonName + "[x]";

                XmlElement annotationElement = choiceElement.SelectSingleNode("xs:annotation", nsManager) as XmlElement;

                if (annotationElement == null)
                {
                    annotationElement = doc.CreateElement("xs", "annotation", SchemaNamespace);

                    if (choiceElement.ChildNodes.Count > 0)
                        choiceElement.InsertBefore(annotationElement, choiceElement.FirstChild);
                    else
                        choiceElement.AppendChild(annotationElement);
                }

                string appInfoXpath = "xs:appinfo[@source='" + AppInfoSource + "']";
                XmlElement appInfoElement = annotationElement.SelectSingleNode(appInfoXpath, nsManager) as XmlElement;

                if (appInfoElement == null)
                {
                    appInfoElement = doc.CreateElement("xs", "appinfo", SchemaNamespace);

                    XmlAttribute sourceAttribute = doc.CreateAttribute("source");
                    sourceAttribute.Value = AppInfoSource;
                    appInfoElement.Attributes.Append(sourceAttribute);

                    if (annotationElement.ChildNodes.Count > 0)
                        annotationElement.InsertBefore(appInfoElement, annotationElement.FirstChild);
                    else
                        annotationElement.AppendChild(appInfoElement);

                    appInfoElement.InnerText = commonName;
                    changedSchema = true;
                }
            }

            if (changedSchema)
                doc.Save(this.SchemaLocation);
        }
    }
}
