using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.BulkCopy
{
    public class ExcelUpload
    {
        public byte[] ExcelFile { get; set; }
        public byte[] ConfigFile { get; set; }
        public bool FirstRowIsHeader { get; set; }
    }
}