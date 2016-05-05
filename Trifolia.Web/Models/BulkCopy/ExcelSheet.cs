using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class ExcelSheet
    {
        public ExcelSheet()
        {
            this.Columns = new List<ExcelColumn>();
            this.Rows = new List<ExcelRow>();
        }

        public string SheetName { get; set; }
        public List<ExcelColumn> Columns { get; set; }
        public List<ExcelRow> Rows { get; set; }
    }
}