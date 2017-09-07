using LantanaGroup.ValidationUtility;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Trifolia.DB;
using Trifolia.Export.Native;
using Trifolia.Shared;

namespace Trifolia.Export.FHIR.DSTU1
{
    public class FHIRExporter
    {
        private const string StylesheetResource = "Trifolia.Export.FHIR.DSTU1.TemplateExportToFHIRProfile.xslt";
        private const string StylesheetUri = "http://www.w3.org/1999/XSL/Transform";

        private IObjectRepository tdb = null;
        private List<Template> templates = null;
        private IGSettingsManager igSettings = null;
        private List<string> categories = null;
        private bool includeVocabulary = false;

        private FHIRExporter(IObjectRepository tdb, List<Template> templates, IGSettingsManager igSettings, List<string> categories = null, bool includeVocabulary = false)
        {
            this.tdb = tdb;
            this.templates = templates;
            this.igSettings = igSettings;
            this.categories = categories;
        }

        public static string GenerateExport(IObjectRepository tdb, List<Template> templates, IGSettingsManager igSettings, List<string> categories = null, bool includeVocabulary = false)
        {
            FHIRExporter exporter = new FHIRExporter(tdb, templates, igSettings, categories, includeVocabulary);
            return exporter.GenerateExport();
        }

        private string GenerateExport()
        {
            string templateExport = TemplateExporter.GenerateXMLExport(this.tdb, this.templates, this.igSettings, true, this.categories);
            LantanaXmlResolver resolver = new LantanaXmlResolver();
            string stylesheetContent = string.Empty;
            
            using (StreamReader stylesheetReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(StylesheetResource)))
            {
                stylesheetContent = stylesheetReader.ReadToEnd();
            }

            var export = TransformFactory.Transform(templateExport, stylesheetContent, StylesheetUri, resolver);

            // No longer supporting DSTU1. If needed in future, replace with Svs2FhirValueSet.xslt execution against MultipleSvsExporter.GetExport()
            /*
            if (includeVocabulary)
            {
                // Export the vocabulary for the implementation guide in SVS format
                VocabularyService vocService = new VocabularyService(tdb, false);
                string vocXml = vocService.GetImplementationGuideVocabulary(igSettings.ImplementationGuideId, 1000, 4, "utf-8");

                // Merge the two ATOM exports together
                XmlDocument exportDoc = new XmlDocument();
                exportDoc.LoadXml(export);

                // Remove extra xmlns attributes from vocabulary xml
                System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Parse(vocXml);
                foreach (var descendant in doc.Root.Descendants())
                {
                    var namespaceDeclarations = descendant.Attributes().Where(y => y.IsNamespaceDeclaration && y.Name.LocalName == "atom");
                    foreach (var namespaceDeclaration in namespaceDeclarations)
                    {
                        namespaceDeclaration.Remove();
                    }
                }
                vocXml = doc.ToString();

                XmlDocument vocDoc = new XmlDocument();
                vocDoc.LoadXml(vocXml);

                XmlNamespaceManager vocNsManager = new XmlNamespaceManager(vocDoc.NameTable);
                vocNsManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");

                XmlNodeList vocEntryNodes = vocDoc.SelectNodes("/atom:feed/atom:entry", vocNsManager);

                foreach (XmlNode vocEntryNode in vocEntryNodes)
                {
                    XmlNode clonedVocEntryNode = exportDoc.ImportNode(vocEntryNode, true);
                    exportDoc.DocumentElement.AppendChild(clonedVocEntryNode);
                }

                // Format the XmlDocument and save it as a string
                using (StringWriter sw = new StringWriter())
                {
                    XmlTextWriter xtw = new XmlTextWriter(sw);
                    xtw.Formatting = Formatting.Indented;

                    exportDoc.WriteContentTo(xtw);
                    export = sw.ToString();
                }
            }
            */

            return export;
        }
    }
}
