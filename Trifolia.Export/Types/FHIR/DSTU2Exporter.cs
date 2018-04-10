extern alias fhir_dstu2;
using System;
using System.Collections.Generic;
using System.Web;
using Trifolia.Config;
using Trifolia.Export.FHIR.DSTU2;
using Trifolia.Plugins;
using Trifolia.Shared;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;

namespace Trifolia.Export.Types.FHIR
{
    [ImplementationGuideTypePlugin("FHIR DSTU2")]
    public class DSTU2Exporter : BaseTypeExporter, ITypeExporter
    {
        public byte[] Export(DB.IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<DB.Template> templates, bool includeVocabulary, bool returnJson = true)
        {
            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);

            switch (format)
            {
                case ExportFormats.FHIR_Bundle:
                    ImplementationGuideExporter exporter = new ImplementationGuideExporter(tdb, schema, uri.Scheme, uri.Authority);
                    fhir_dstu2.Hl7.Fhir.Model.Bundle bundle = exporter.GetImplementationGuides(include: "ImplementationGuide:resource", implementationGuideId: igSettings.ImplementationGuideId);
                    return ConvertToBytes(fhir_dstu2.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToXml(bundle));
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
    }
}
