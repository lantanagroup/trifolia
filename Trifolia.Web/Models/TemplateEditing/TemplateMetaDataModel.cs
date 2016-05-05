using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.TemplateEditing
{
    public class TemplateMetaDataModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public bool IsOpen { get; set; }
        public int OwningImplementationGuideId { get; set; }
        public int TemplateTypeId { get; set; }
        public string PrimaryContext { get; set; }
        public string PrimaryContextType { get; set; }
        public string Bookmark { get; set; }
        public int? ImpliedTemplateId { get; set; }
        public int? StatusId { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }

        public string PreviousVersionLink { get; set; }
        public string PreviousVersionName { get; set; }
        public string Author { get; set; }
        public string OrganizationName { get; set; }
        public string MoveUrl { get; set; }
        public string TemplateTypeAbbreviation { get; set; }
        public bool Locked { get; set; }

        public IEnumerable<dynamic> ValidationResults { get; set; }
        public IEnumerable<TemplateReference> ContainedByTemplates { get; set; }
        public IEnumerable<TemplateReference> ImpliedByTemplates { get; set; }
        public IEnumerable<TemplateExtension> Extensions { get; set; }

        public class TemplateExtension
        {
            public string Identifier { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
        }

        public class TemplateReference
        {
            public string EditUrl { get; set; }
            public string ViewUrl { get; set; }
            public string Name { get; set; }
            public string ImplementationGuide { get; set; }
        }
    }
}