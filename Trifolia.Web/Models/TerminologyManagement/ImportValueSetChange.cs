using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class ImportValueSetChange
    {
        public ImportValueSetChange()
        {
            this.Concepts = new List<ConceptChange>();
        }

        public int? Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public ChangeTypes ChangeType { get; set; }

        public List<ConceptChange> Concepts { get; set; }

        public class ConceptChange
        {
            public int? Id { get; set; }
            public ChangeTypes ChangeType { get; set; }

            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string CodeSystemOid { get; set; }
            public string CodeSystemName { get; set; }
            public string Status { get; set; }
            public DateTime? StatusDate { get; set; }
        }

        public enum ChangeTypes
        {
            None = 0,
            Add = 1,
            Update = 2
        }
    }
}