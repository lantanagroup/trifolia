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
using ExportModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using ExportTemplate = Trifolia.Shared.ImportExport.Model.TrifoliaTemplate;
using ExportImplementationGuide = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuide;
using Trifolia.Shared;

namespace Trifolia.Export.Native
{
    public class TemplateExporter
    {
        private const string ExportDisclaimer = "The export template XML file format definition is subject to change without notice, and continued availability should not be construed as a commitment by Lantana Consulting Group LLC. Lantana Consulting Group LLC assumes no responsibility or liability including incomplete data, errors, or impacts caused by changes to this XML file format definition.";

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

        public static string GenerateXMLExport(IObjectRepository tdb, List<TDBTemplate> templates, IGSettingsManager igSettings, bool verboseConstraints = false, List<string> categories = null)
        {
            TemplateExporter exporter = new TemplateExporter(tdb, templates, igSettings, verboseConstraints, categories);
            return exporter.GenerateXMLExport();
        }

        public static Trifolia.Shared.ImportExport.Model.Trifolia GenerateExport(IObjectRepository tdb, List<TDBTemplate> templates, IGSettingsManager igSettings, bool verboseConstraints = false, List<string> categories = null)
        {
            TemplateExporter exporter = new TemplateExporter(tdb, templates, igSettings, verboseConstraints, categories);
            return exporter.GenerateExport();
        }

        private bool FindImplementationGuide(List<ExportImplementationGuide> exportIgs, ImplementationGuide current)
        {
            int currentVersion = current.Version != null && current.Version.Value > 0 ? current.Version.Value : 1;
            return exportIgs.Count(y => y.name.ToLower() == current.Name.ToLower() && y.version == currentVersion) > 0;
        }

        public ExportModel GenerateExport()
        {
            List<ExportTemplate> exportTemplates = new List<ExportTemplate>();
            List<ExportImplementationGuide> exportImplementationGuides = new List<ExportImplementationGuide>();
            ExportModel export = new ExportModel()
            {
                Template = exportTemplates,
                ImplementationGuide = exportImplementationGuides
            };

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

                bool foundImplementationGuide = FindImplementationGuide(exportImplementationGuides, y.OwningImplementationGuide);

                if (!foundImplementationGuide)
                {
                    var exportImplementationGuide = y.OwningImplementationGuide.Export(this.tdb, this.igSettings);
                    exportImplementationGuides.Add(exportImplementationGuide);
                }
            });

            return export;
        }

        public string GenerateXMLExport()
        {
            var export = GenerateExport();
            string serializeContent = export.Serialize();

            using (StringWriter sw = new StringWriter())
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(serializeContent);

                XmlComment disclaimerComment = doc.CreateComment(ExportDisclaimer);
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
