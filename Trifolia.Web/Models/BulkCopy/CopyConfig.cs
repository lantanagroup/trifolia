using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class CopyConfig
    {
        public CopyConfig()
        {
            this.TemplateColumns = new List<ColumnConfig>();
            this.ConstraintColumns = new List<ColumnConfig>();
        }

        public string TemplateMetaDataSheet { get; set; }
        public string ConstraintChangesSheet { get; set; }
        public List<ColumnConfig> TemplateColumns { get; set; }
        public List<ColumnConfig> ConstraintColumns { get; set; }

        public class ColumnConfig
        {
            public string Letter { get; set; }
            public string MappedField { get; set; }
        }
    }
}