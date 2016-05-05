using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class XmlSample
    {
        #region Public Properties

        public int? Id { get; set; }
        public string Name { get; set; }
        public string SampleText { get; set; }
        public bool IsDeleted { get; set; }
        public int? TemplateId { get; set; }

        #endregion
    }
}