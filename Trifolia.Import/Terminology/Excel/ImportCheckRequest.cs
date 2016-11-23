using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Import.Terminology.Excel
{
    public class ImportCheckRequest
    {
        public byte[] Data { get; set; }
        public bool FirstRowIsHeader { get; set; }
    }
}