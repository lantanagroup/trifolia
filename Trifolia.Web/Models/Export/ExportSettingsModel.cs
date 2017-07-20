using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Shared.Plugins;

namespace Trifolia.Web.Models.Export
{
    public class ExportSettingsModel
    {
        public int ImplementationGuideId { get; set; }
        public ExportFormats ExportFormat { get; set; }

        // MS Word Export Settings
        public bool IncludeXmlSample { get; set; }
        public bool IncludeChangeList { get; set; }
        public bool IncludeTemplateStatus { get; set; }
        public bool IncludeNotes { get; set; }
        public TemplateTableOptions TemplateTables { get; set; }
        public DocumentTableOptions DocumentTables { get; set; }
        public TemplateSortOrderOptions TemplateSortOrder { get; set; }
        public bool ValueSetTables { get; set; }
        public string[] ValueSetOid { get; set; }
        public int[] ValueSetMaxMembers { get; set; }

        // SCH Export Settings
        public string VocabularyFileName { get; set; }
        public bool IncludeVocabulary { get; set; }
        public bool IncludeCustomSchematron { get; set; }
        public string DefaultSchematron { get; set; }

        // Vocabulary Settings
        public EncodingOptions Encoding { get; set; }
        public int MaximumValueSetMembers { get; set; }
        public bool ValueSetAppendix { get; set; }

        // General Settings
        public int[] TemplateIds { get; set; }
        public List<string> SelectedCategories { get; set; }
        public bool ReturnJson { get; set; }

        public enum EncodingOptions
        {
            UTF8 = 0,
            UNICODE = 1
        }

        public enum TemplateSortOrderOptions
        {
            AlphaHierarchical = 0,
            Alphabetical = 1
        }

        public enum DocumentTableOptions
        {
            None = 0,
            Both = 1,
            List = 2,
            Containment = 3
        }

        public enum TemplateTableOptions
        {
            None = 0,
            Both = 1,
            Context = 2,
            Overview = 3
        }
    }
}