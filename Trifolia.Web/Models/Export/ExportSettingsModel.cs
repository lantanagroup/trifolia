using Newtonsoft.Json;
using System.Collections.Generic;
using Trifolia.Plugins;

namespace Trifolia.Web.Models.Export
{
    public class ExportSettingsModel
    {
        private string vocabularyFileName = string.Empty;

        public ExportSettingsModel()
        {
            this.ValueSetOid = new List<string>();
            this.ValueSetMaxMembers = new List<int>();
            this.ParentTemplateIds = new List<int>();
            this.TemplateIds = new List<int>();
            this.SelectedCategories = new List<string>();
        }

        [JsonIgnore]
        public int ImplementationGuideId { get; set; }
        [JsonIgnore]
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
        public List<string> ValueSetOid { get; set; }

        /// <summary>
        /// An ordered array indicating the maximum members for each of the value sets
        /// </summary>
        /// <remarks>ValueSetMaxMembers[i] is the maximum number of members for ValueSetOid[i]</remarks>
        public List<int> ValueSetMaxMembers { get; set; }

        // SCH Export Settings
        public string VocabularyFileName
        {
            get
            {
                // Always return something for the vocabulary file name
                if (string.IsNullOrEmpty(this.vocabularyFileName))
                    return "voc.xml";

                return this.vocabularyFileName;
            }
            set
            {
                this.vocabularyFileName = value;
            }
        }

        public bool IncludeVocabulary { get; set; }
        public bool IncludeCustomSchematron { get; set; }
        public string DefaultSchematron { get; set; }

        // Vocabulary Settings
        public EncodingOptions Encoding { get; set; }

        /// <summary>
        /// The default maximum members for each value set, if not specified by ValueSetMaxMembers[i]
        /// </summary>
        public int MaximumValueSetMembers { get; set; }
        public bool ValueSetAppendix { get; set; }

        // General Settings
        public List<int> ParentTemplateIds { get; set; }
        public List<int> TemplateIds { get; set; }
        public bool IncludeInferred { get; set; }
        public List<string> SelectedCategories { get; set; }
        public bool ReturnJson { get; set; }
    }
}