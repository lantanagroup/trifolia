using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Import.Terminology.Excel
{
    public class ImportCheckResponse
    {
        public ImportCheckResponse()
        {
            this.Errors = new List<string>();
            this.ValueSets = new List<ImportValueSetChange>();
        }

        public List<string> Errors { get; set; }
        public List<ImportValueSetChange> ValueSets { get; set; }
    }
}