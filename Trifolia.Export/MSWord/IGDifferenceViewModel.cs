using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trifolia.DB;
using Trifolia.Export.Versioning;

namespace Trifolia.Export.MSWord
{
    internal class IGDifferenceViewModel
    {
        public IGDifferenceViewModel()
        {

        }

        public IGDifferenceViewModel(ITemplate template)
        {
            this.TemplateName = template.Name;
            this.TemplateOid = template.Oid;
            this.TemplateBookmark = template.Bookmark;
            this.ShouldLink = template.Status != PublishStatus.RETIRED_STATUS;
        }

        #region Public Properties

        public string TemplateName { get; set; }

        public string TemplateOid { get; set; }

        public string TemplateBookmark { get; set; }

        public bool ShouldLink { get; set; }

        public ComparisonResult Difference { get; set; }

        #endregion
    }
}