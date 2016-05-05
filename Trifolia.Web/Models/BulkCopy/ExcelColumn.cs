using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class ExcelColumn
    {
        public string Letter { get; set; }
        public string Name { get; set; }
        public Fields MappedField { get; set; }
    }
}