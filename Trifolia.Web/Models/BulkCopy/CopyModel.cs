using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class CopyModel
    {
        public int BaseTemplateId { get; set; }
        public ExcelSheet TemplateSheet { get; set; }
        public ExcelSheet ConstraintSheet { get; set; }
    }
}