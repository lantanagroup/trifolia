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
using System.Web;

namespace Trifolia.Export.Types
{
    [ImplementationGuideTypePlugin(Constants.IGType.HQMF_IG_TYPE)]
    public class HQMFExporter : ITypeExporter
    {
        public byte[] Export(DB.IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<DB.Template> templates, bool includeVocabulary, bool returnJson = true)
        {
            string requestScheme = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url.Scheme : null;
            string requestAuthority = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url.Authority : null;

            switch (format)
            {
                case ExportFormats.FHIR_Bundle:
                    throw new NotImplementedException();
                case ExportFormats.Native_XML:
                    NativeExporter nativeExporter = new NativeExporter(tdb, templates, igSettings, true, categories);
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
