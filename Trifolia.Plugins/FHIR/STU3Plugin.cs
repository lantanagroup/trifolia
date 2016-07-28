extern alias fhir_stu3;
using System;
using System.Collections.Generic;
using System.Web;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;

namespace Trifolia.Plugins.FHIR
{
    public class STU3Plugin : DefaultPlugin, IIGTypePlugin
    {
        public string Export(DB.IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<DB.Template> templates, bool includeVocabulary, bool returnJson = true)
        {
            string requestScheme = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url.Scheme : null;
            string requestAuthority = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url.Authority : null;

            switch (format)
            {
                case ExportFormats.FHIR:
                    string baseProfilePath = HttpContext.Current.Server.MapPath("~/App_Data/FHIR/STU3/");
                    ImplementationGuideExporter exporter = new ImplementationGuideExporter(tdb, schema, baseProfilePath, requestScheme, requestAuthority);
                    fhir_stu3.Hl7.Fhir.Model.Bundle bundle = exporter.GetImplementationGuides(include: "ImplementationGuide:resource", implementationGuideId: igSettings.ImplementationGuideId);
                    return fhir_stu3.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToXml(bundle);
                case ExportFormats.Proprietary:
                    NativeExporter proprietaryExporter = new NativeExporter(tdb, templates, igSettings, true, categories);
                    return proprietaryExporter.GenerateXMLExport();
                case ExportFormats.TemplatesDSTU:
                    DecorExporter decorExporter = new DecorExporter(templates, tdb, igSettings.ImplementationGuideId);
                    return decorExporter.GenerateXML();
                default:
                    throw new Exception("Invalid export format for the specified implementation guide type");
            }
        }
    }
}
