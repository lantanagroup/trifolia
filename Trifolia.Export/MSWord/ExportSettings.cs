using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Export.MSWord
{
    public class ExportSettings
    {
        public bool GenerateTemplateConstraintTable { get; set; }
        public bool GenerateTemplateContextTable { get; set; }
        public bool GenerateDocTemplateListTable { get; set; }
        public bool GenerateDocContainmentTable { get; set; }
        public bool AlphaHierarchicalOrder { get; set; }
        public int DefaultValueSetMaxMembers { get; set; }
        public bool GenerateValueSetAppendix { get; set; }
        public bool IncludeXmlSamples { get; set; }
        public bool IncludeChangeList { get; set; }
        public bool IncludeTemplateStatus { get; set; }
        public bool IncludeNotes { get; set; }
        public bool IncludeVolume1 { get; set; }
        public Dictionary<string, int> ValueSetMaxMembers { get; set; }
        public List<string> SelectedCategories { get; set; }
        public bool GenerateRequiredAndOptionalSectionsTable { get; set; }
        public int? DocumentTemplateTypeId { get; set; }
        public int? SectionTemplateTypeId { get; set; }
    }
}