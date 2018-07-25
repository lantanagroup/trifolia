using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Plugins
{
    public enum ExportFormats
    {
        Microsoft_Word_DOCX = 0,
        Web_HTML = 1,
        Snapshot_JSON = 2,
        Native_XML = 3,
        FHIR_Bundle = 4,
        FHIR_Build_Package = 5,
        Templates_DSTU_XML = 6,
        Schematron_SCH = 7,
        Vocabulary_XLSX = 8,
        Vocabulary_Native_XML = 9,
        Vocbulary_Single_SVS_XML = 10,
        Vocabulary_Multiple_SVS_XML = 11,
        Vocbulary_Bundle_FHIR_XML = 12
    }
}
