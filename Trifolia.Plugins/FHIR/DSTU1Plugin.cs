extern alias fhir_dstu1;
using System;
using System.Collections.Generic;
using Trifolia.DB;
using Trifolia.Export.FHIR.DSTU1;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using Trifolia.Shared.Validation;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;

namespace Trifolia.Plugins.FHIR
{
    public class DSTU1Plugin : DefaultPlugin, IIGTypePlugin
    {
        public byte[] Export(DB.IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<DB.Template> templates, bool includeVocabulary, bool returnJson = true)
        {
            switch (format)
            {
                case ExportFormats.FHIR_Bundle:
                    string export = FHIRExporter.GenerateExport(tdb, templates, igSettings, categories, includeVocabulary);

                    if (returnJson)
                    {
                        fhir_dstu1.Hl7.Fhir.Model.Bundle bundle = fhir_dstu1.Hl7.Fhir.Serialization.FhirParser.ParseBundleFromXml(export);
                        export = fhir_dstu1.Hl7.Fhir.Serialization.FhirSerializer.SerializeBundleToJson(bundle);
                    }

                    return ConvertToBytes(export);
                case ExportFormats.Native_XML:
                    NativeExporter proprietaryExporter = new NativeExporter(tdb, templates, igSettings, true, categories);

                    if (returnJson)
                        return ConvertToBytes(proprietaryExporter.GenerateJSONExport());
                    else
                        return ConvertToBytes(proprietaryExporter.GenerateXMLExport());
                case ExportFormats.Templates_DSTU_XML:
                    DecorExporter decorExporter = new DecorExporter(templates, tdb, igSettings.ImplementationGuideId);
                    return ConvertToBytes(decorExporter.GenerateXML());
                default:
                    throw new Exception("Invalid export format for the specified implementation guide type");
            }
        }

        public string GetFHIRResourceInstanceJson(string content)
        {
            throw new NotImplementedException();
        }

        public string GetFHIRResourceInstanceXml(string content)
        {
            throw new NotImplementedException();
        }

        public IValidator GetValidator(IObjectRepository tdb)
        {
            return new Validation.RIMValidator(tdb);
        }
    }
}
