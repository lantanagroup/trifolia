using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trifolia.Generation.Versioning;

namespace Trifolia.Generation.IG
{
    internal class IGDifferenceViewModel
    {
        #region Public Properties

        public string TemplateName { get; set; }

        public string TemplateOid { get; set; }

        public string TemplateBookmark { get; set; }

        public ComparisonResult Difference { get; set; }

        #endregion
    }
}