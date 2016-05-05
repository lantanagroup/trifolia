using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

using Trifolia.DB;
using TDBTemplate = Trifolia.DB.Template;
using TDBTemplateConstraint = Trifolia.DB.TemplateConstraint;
using ExportModel = Trifolia.Shared.ImportExport.Model.TemplateExport;
using ExportTemplate = Trifolia.Shared.ImportExport.Model.TemplateExportTemplate;
using Trifolia.Shared;

namespace Trifolia.Generation.XML
{
    public class TemplateExporter
    {
        private IObjectRepository tdb = null;
        private List<TDBTemplate> templates = null;
        private IGSettingsManager igSettings = null;
        private bool verboseConstraints = false;
        private Dictionary<int, SimpleSchema> schemas = null;
        private List<string> categories = null;

        public TemplateExporter(IObjectRepository tdb, List<TDBTemplate> templates, IGSettingsManager igSettings, bool verboseConstraints = false, List<string> categories = null)
        {
            this.tdb = tdb;
            this.templates = templates;
            this.igSettings = igSettings;
            this.verboseConstraints = verboseConstraints;
            this.categories = categories;

            if (verboseConstraints)
                this.schemas = new Dictionary<int, SimpleSchema>();
        }

        public static string GenerateExport(IObjectRepository tdb, List<TDBTemplate> templates, IGSettingsManager igSettings, bool verboseConstraints = false, List<string> categories = null)
        {
            TemplateExporter exporter = new TemplateExporter(tdb, templates, igSettings, verboseConstraints, categories);
            return exporter.GenerateExport();
        }

        public string GenerateExport()
        {
            List<ExportTemplate> exportTemplates = new List<ExportTemplate>();

            this.templates.ForEach(y =>
            {
                SimpleSchema schema = null;

                if (this.verboseConstraints)
                {
                    if (this.schemas.ContainsKey(y.ImplementationGuideTypeId))
                    {
                        schema = this.schemas[y.ImplementationGuideTypeId];
                    }
                    else
                    {
                        schema = y.ImplementationGuideType.GetSimpleSchema();
                        this.schemas.Add(y.ImplementationGuideTypeId, schema);
                    }
                }

                exportTemplates.Add(y.Export(this.tdb, this.igSettings, schema, categories));
            });

            ExportModel export = new ExportModel()
            {
                Template = exportTemplates.ToList()
            };

            string serializeContent = string.Empty;

            using (Utf8StringWriter sw = new Utf8StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ExportModel));
                serializer.Serialize(sw, export);

                serializeContent = sw.ToString();
            }

            using (StringWriter sw = new StringWriter())
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(serializeContent);

                XmlComment disclaimerComment = doc.CreateComment(Properties.Settings.Default.ExportXmlDisclaimer);
                doc.DocumentElement.InsertBefore(disclaimerComment, doc.DocumentElement.FirstChild);

                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.Indent = true;

                XmlWriter writer = XmlWriter.Create(sw, writerSettings);
                doc.WriteContentTo(writer);
                writer.Flush();

                return sw.ToString();
            }
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
