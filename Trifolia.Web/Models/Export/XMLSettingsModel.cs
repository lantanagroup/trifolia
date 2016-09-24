using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Export
{
    public class XMLSettingsModel
    {
        public XMLSettingsModel()
        {
            this.TemplateIds = new List<int>();
            this.SelectedCategories = new List<string>();
        }

        public List<int> TemplateIds { get; set; }
        public int ImplementationGuideId { get; set; }
        public ExportTypes XmlType { get; set; }
        public List<string> SelectedCategories { get; set; }
        public bool? IncludeVocabulary { get; set; }    // Indicates to include vocabulary in the FHIR XML export

        public enum ExportTypes
        {
            Proprietary,
            FHIR,
            FHIRBuild,
            JSON,
            DSTU
        }
    }
}