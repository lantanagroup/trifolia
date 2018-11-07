using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Import.Terminology.External
{
    public class ImportValueSet
    {
        public ImportValueSet()
        {
            this.Members = new List<ImportValueSetMember>();
        }

        public string ImportStatus { get; set; }
        public string ImportSource { get; set; }
        public string ImportSourceId { get; set; }
        public string Oid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string SourceUrl { get; set; }
        public List<ImportValueSetMember> Members { get; set; }
    }
}
