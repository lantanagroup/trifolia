using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Plugins;
using Trifolia.Shared;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;

namespace Trifolia.Export.Types
{
    [ImplementationGuideTypePlugin(Constants.IGTypeNames.EMEASURE)]
    [ImplementationGuideTypePlugin(Constants.IGTypeNames.CDA)]
    public class RIMExporter : ITypeExporter
    {
        public byte[] Export(
            DB.IObjectRepository tdb,
            SimpleSchema schema,
            ExportFormats format,
            IGSettingsManager igSettings,
            List<string> categories,
            List<DB.Template> templates,
            bool includeVocabulary,
            bool returnJson = true)
        {
            var templateIds = templates.Select(y => y.Id);

            switch (format)
            {
                case ExportFormats.Microsoft_Word_DOCX:
                    ImplementationGuide ig = tdb.ImplementationGuides.Single(y => y.Id == igSettings.ImplementationGuideId);
                    MSWord.ImplementationGuideGenerator docxGenerator = new MSWord.ImplementationGuideGenerator(tdb, igSettings.ImplementationGuideId, templateIds);

                    // TODO: Re-factor ITypeExporter to accept ExportSettings
                    MSWord.ExportSettings exportConfig = new MSWord.ExportSettings();
                    exportConfig.Use(c =>
                    {
                        c.GenerateTemplateConstraintTable = true;
                        c.GenerateTemplateContextTable = true;
                        c.GenerateDocTemplateListTable = true;
                        c.GenerateDocContainmentTable = true;
                        c.AlphaHierarchicalOrder = true;
                        c.DefaultValueSetMaxMembers = 100;
                        c.GenerateValueSetAppendix = true;
                        c.IncludeXmlSamples = true;
                        c.IncludeChangeList = true;
                        c.IncludeTemplateStatus = true;
                        c.IncludeNotes = false;
                        c.SelectedCategories = categories;
                    });

                    docxGenerator.BuildImplementationGuide(exportConfig, ig.ImplementationGuideType.GetPlugin());
                    return docxGenerator.GetDocument();
                case ExportFormats.FHIR_Bundle:
                    throw new NotImplementedException();
                case ExportFormats.Native_XML:
                    NativeExporter nativeExporter = new NativeExporter(tdb, templates, igSettings, true, categories);

                    if (returnJson)
                        return System.Text.Encoding.UTF8.GetBytes(nativeExporter.GenerateJSONExport());
                    else
                        return System.Text.Encoding.UTF8.GetBytes(nativeExporter.GenerateXMLExport());
                case ExportFormats.Templates_DSTU_XML:
                    DecorExporter decorExporter = new DecorExporter(templates, tdb, igSettings.ImplementationGuideId);
                    return System.Text.Encoding.UTF8.GetBytes(decorExporter.GenerateXML());
                default:
                    throw new Exception("Invalid export format for the specified implementation guide type");
            }
        }
    }
}
