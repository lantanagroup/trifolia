using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class ConstraintSample
    {
        #region Public Properties

        public int? Id { get; set; }
        public int? ConstraintId { get; set; }
        public string Name { get; set; }
        public string SampleText { get; set; }
        public bool IsDeleted { get; set; }

        #endregion
    }
}