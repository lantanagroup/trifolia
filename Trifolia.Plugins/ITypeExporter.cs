using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.Shared;

namespace Trifolia.Plugins
{
    public interface ITypeExporter
    {
        byte[] Export(
            DB.IObjectRepository tdb, 
            SimpleSchema schema, 
            ExportFormats format, 
            IGSettingsManager igSettings, 
            List<string> categories, 
            List<DB.Template> templates, 
            bool includeVocabulary, 
            bool returnJson = true);
    }
}
