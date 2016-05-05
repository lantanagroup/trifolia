using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Web.Models.Export
{
    public class SchematronSettingsModel
    {
        public SchematronSettingsModel()
        {
            this.SelectedCategories = new List<string>();
        }

        public enum ValueSetOutputFormats
        {
            Standard = 1,
            SVS_Multiple = 2,
            SVS_Single = 3
        }

        public int ImplementationGuideId { get; set; }
        public ValueSetOutputFormats ValueSetOutputFormat { get; set; }
        public bool IncludeCustomSchematron { get; set; }
        public string VocabularyFileName { get; set; }
        public List<int> TemplateIds { get; set; }
        public List<string> SelectedCategories { get; set; }
        public string DefaultSchematron { get; set; }
    }
}