using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using Trifolia.DB;
using Trifolia.Shared;
using LantanaGroup.ValidationUtility;

namespace Trifolia.Generation.XML.FHIR.DSTU1
{
    public class FHIRExporter
    {
        private const string StylesheetResource = "Trifolia.Generation.XML.TemplateExportToFHIRProfile.xslt";
        private const string StylesheetUri = "http://www.w3.org/1999/XSL/Transform";

        private IObjectRepository tdb = null;
        private List<Template> templates = null;
        private IGSettingsManager igSettings = null;
        private List<string> categories = null;

        public FHIRExporter(IObjectRepository tdb, List<Template> templates, IGSettingsManager igSettings, List<string> categories = null)
        {
            this.tdb = tdb;
            this.templates = templates;
            this.igSettings = igSettings;
            this.categories = categories;
        }

        public static string GenerateExport(IObjectRepository tdb, List<Template> templates, IGSettingsManager igSettings, List<string> categories = null)
        {
            FHIRExporter exporter = new FHIRExporter(tdb, templates, igSettings, categories);
            return exporter.GenerateExport();
        }

        public string GenerateExport()
        {
            string templateExport = TemplateExporter.GenerateXMLExport(this.tdb, this.templates, this.igSettings, true, this.categories);
            LantanaXmlResolver resolver = new LantanaXmlResolver();
            string stylesheetContent = string.Empty;
            
            using (StreamReader stylesheetReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(StylesheetResource)))
            {
                stylesheetContent = stylesheetReader.ReadToEnd();
            }

            return TransformFactory.Transform(templateExport, stylesheetContent, StylesheetUri, resolver);
        }
    }
}
