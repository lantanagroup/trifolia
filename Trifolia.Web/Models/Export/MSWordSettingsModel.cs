using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Export
{
    public enum TemplateSortOrderOptions
    {
        Alphabetically = 0,
        AlphaHierarchically = 1
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
        ConstraintOverview = 3
    }

    /// <summary>
    /// Models used to define settings for exporting to MS Word
    /// </summary>
    public class MSWordSettingsModel
    {
        public MSWordSettingsModel()
        {
            this.TemplateIds = new List<int>();
            this.SelectedCategories = new List<string>();
        }

        /// <summary>
        /// List of template ids included in the export
        /// </summary>
        public List<int> TemplateIds { get; set; }

        /// <summary>
        /// The id of the implementation guide being exported
        /// </summary>
        public int ImplementationGuideId { get; set; }

        /// <summary>
        /// Indicates the sort order of the templates exported
        /// </summary>
        public TemplateSortOrderOptions TemplateSortOrder { get; set; }

        /// <summary>
        /// Indicates what appendix tables should be included in the export
        /// </summary>
        public DocumentTableOptions DocumentTables { get; set; }

        /// <summary>
        /// Indicates what tables should be included for each template
        /// </summary>
        public TemplateTableOptions TemplateTables { get; set; }

        /// <summary>
        /// True to include samples in the export
        /// </summary>
        public bool IncludeXmlSample { get; set; }

        /// <summary>
        /// True if the export should include a table of changes. Only affects exports where the IG is a new version.
        /// </summary>
        public bool IncludeChangeList { get; set; }

        /// <summary>
        /// True if the status of templates should be included in the export
        /// </summary>
        public bool IncludeTemplateStatus { get; set; }

        /// <summary>
        /// True if value sets should be included in the export
        /// </summary>
        public bool GenerateValuesets { get; set; }

        /// <summary>
        /// Default number of maximum members to be used for value set exports
        /// </summary>
        public int MaximumValuesetMembers { get; set; }

        /// <summary>
        /// True if value sets should be exported in an appendix
        /// </summary>
        public bool ValuesetAppendix { get; set; }

        /// <summary>
        /// True if notes on templates and constraints should be included in the export
        /// </summary>
        public bool IncludeNotes { get; set; }

        /// <summary>
        /// List of value set identifiers included in this IG. Primarily used for the ValueSetMaxMembers property's order.
        /// </summary>
        public List<string> ValueSetOid { get; set; }

        /// <summary>
        /// List of numbers representing the maximum number of members that should be exported for each value set. The order of this list matches the order of the ValueSetOid list.
        /// </summary>
        public List<int> ValueSetMaxMembers { get; set; }

        /// <summary>
        /// Whether these settings should be remembered as default settings for future exports. Only editors of the IG can save default settings.
        /// </summary>
        public bool SaveAsDefaultSettings { get; set; }

        /// <summary>
        /// List of parent template IDs used to filter templates. Only templates that are used by these selected parent templates are included in the export
        /// </summary>
        public List<int> ParentTemplateIds { get; set; }
        
        /// <summary>
        /// Indicates if inferred templates should be included
        /// </summary>
        public bool Inferred { get; set; }

        /// <summary>
        /// Categories used to filter constraints exported in the ms word ig
        /// </summary>
        public List<string> SelectedCategories { get; set; }
    }
}