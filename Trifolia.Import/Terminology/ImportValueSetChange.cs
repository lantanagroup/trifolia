using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Import.Terminology
{
    public class ImportValueSetChange
    {
        public ImportValueSetChange()
        {
            this.Concepts = new List<ConceptChange>();
        }

        /// <summary>
        /// The ValueSet in the database that was matched to the valueset in the import file
        /// </summary>
        /// <remarks>This is marked as internal so that the property does not get serialized by the REST API</remarks>
        internal ValueSet ValueSet { get; set; }
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