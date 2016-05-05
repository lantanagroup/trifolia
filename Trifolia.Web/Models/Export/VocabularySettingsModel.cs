using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Export
{
    public class VocabularySettingsModel
    {
        public enum ExportFormatTypes
        {
            Standard = 1,
            SVS = 2,
            Excel = 3,
            FHIR = 4
        }

        public int ImplementationGuideId { get; set; }
        public ExportFormatTypes ExportFormat { get; set; }
        public string Encoding { get; set; }
        public int MaximumMembers { get; set; }
    }
}