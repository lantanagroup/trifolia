using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Shared;

namespace Trifolia.Web.Models.IGManagement
{
    public class ViewModel
    {
        public ViewModel()
        {
            this.TemplateTypes = new List<ViewTemplateType>();
            this.Templates = new List<TemplateItem>();
            this.WebPublications = new List<string>();
        }

        public int Id { get; set; }
        public int? PreviousVersionImplementationGuideId { get; set; }
        public int? NextVersionImplementationGuideId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string Organization { get; set; }
        public string Status { get; set; }
        public string PublishDate { get; set; }
        public bool ViewAuditTrail { get; set; }
        public bool ViewNotes { get; set; }
        public bool ViewPrimitives { get; set; }
        public bool ViewFiles { get; set; }
        public bool HasPreviousVersion { get; set; }
        public string PreviousVersionIgName { get; set; }
        public string UserPrompt { get; set; }

        // Button enabling
        public bool ShowExportSchematron { get; set; }
        public bool ShowExportVocabulary { get; set; }
        public bool ShowExportMSWord { get; set; }
        public bool ShowExportGreen { get; set; }
        public bool ShowExportXML { get; set; }
        public bool ShowEditIG { get; set; }
        public bool ShowEditTemplate { get; set; }
        public bool ShowEditBookmarks { get; set; }
        public bool ShowPublish { get; set; }
        public bool ShowNewVersion { get; set; }
        public bool ShowManageFiles { get; set; }
        public bool ShowDelete { get; set; }
        public bool ViewWebIG { get; set; }
        public bool EnableNewVersion { get; set; }

        public List<ViewTemplateType> TemplateTypes { get; set; }
        public List<TemplateItem> Templates { get; set; }
        public List<string> WebPublications { get; set; }

        public class TemplateItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Oid { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public bool HasGreenModel { get; set; }
            public bool CanEdit { get; set; }
        }
    }
}