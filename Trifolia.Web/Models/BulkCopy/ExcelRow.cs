using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class ExcelRow
    {
        public ExcelRow()
        {
            this.Cells = new List<ExcelCell>();
        }

        public int RowNumber { get; set; }
        public List<ExcelCell> Cells { get; set; }
    }
}