﻿extern alias fhir_stu3;
using System;
using System.Collections.Generic;
using System.Web;
using Trifolia.Config;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;

namespace Trifolia.Plugins.FHIR
{
    public class STU3Plugin : DefaultPlugin, IIGTypePlugin
    {
        public byte[] Export(DB.IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<DB.Template> templates, bool includeVocabulary, bool returnJson = true)
        {
            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);

            switch (format)
            {
                case ExportFormats.FHIR:
                    ImplementationGuideExporter exporter = new ImplementationGuideExporter(tdb, schema, uri.Scheme, uri.Authority);
                    fhir_stu3.Hl7.Fhir.Model.Bundle bundle = exporter.GetImplementationGuides(include: "ImplementationGuide:resource", implementationGuideId: igSettings.ImplementationGuideId);
                    return ConvertToBytes(fhir_stu3.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToXml(bundle));
                case ExportFormats.Proprietary:
                    NativeExporter proprietaryExporter = new NativeExporter(tdb, templates, igSettings, true, categories);
                    return ConvertToBytes(proprietaryExporter.GenerateXMLExport());
                case ExportFormats.TemplatesDSTU:
                    DecorExporter decorExporter = new DecorExporter(templates, tdb, igSettings.ImplementationGuideId);
                    return ConvertToBytes(decorExporter.GenerateXML());
                case ExportFormats.FHIRBuild:
                    BuildExporter buildExporter = new BuildExporter(tdb, igSettings.ImplementationGuideId, templates, returnJson);
                    var export = buildExporter.Export(includeVocabulary);
                    return export;
                default:
                    throw new Exception("Invalid export format for the specified implementation guide type");
            }
        }
    }
}
