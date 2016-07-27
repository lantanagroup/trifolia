extern alias fhir_dstu1;
using System;
using System.Collections.Generic;
using Trifolia.Export.FHIR.DSTU1;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;

namespace Trifolia.Plugins.FHIR
{
    public class DSTU1Plugin : DefaultPlugin, IIGTypePlugin
    {
        public string Export(DB.IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<DB.Template> templates, bool includeVocabulary, bool returnJson = true)
        {
            switch (format)
            {
                case ExportFormats.FHIR:
                    string export = FHIRExporter.GenerateExport(tdb, templates, igSettings, categories, includeVocabulary);

                    if (returnJson)
                    {
                        fhir_dstu1.Hl7.Fhir.Model.Bundle bundle = fhir_dstu1.Hl7.Fhir.Serialization.FhirParser.ParseBundleFromXml(export);
                        export = fhir_dstu1.Hl7.Fhir.Serialization.FhirSerializer.SerializeBundleToJson(bundle);
                    }

                    return export;
                case ExportFormats.Proprietary:
                    NativeExporter proprietaryExporter = new NativeExporter(tdb, templates, igSettings, true, categories);
                    return proprietaryExporter.GenerateXMLExport();
                case ExportFormats.TemplatesDSTU:
                    DecorExporter decorExporter = new DecorExporter(templates, tdb, igSettings.ImplementationGuideId);
                    return decorExporter.GenerateXML();
                case ExportFormats.Snapshot:

                default:
                    throw new Exception("Invalid export format for the specified implementation guide type");
            }
        }
    }
}
