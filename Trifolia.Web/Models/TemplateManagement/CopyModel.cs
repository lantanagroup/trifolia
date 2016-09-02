using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class CopyModel
    {
        public CopyModel()
        {
            this.SubmitEnabled = true;
            this.Constraints = new List<Constraint>();
        }

        public int TemplateId { get; set; }
        public int? ImplementationGuideId { get; set; }
        public string OriginalName { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string Bookmark { get; set; }
        public string Type { get; set; }
        public int? ImpliedTemplateId { get; set; }
        public bool IsNewVersion { get; set; }
        public string NewVersionImplementationGuideName { get; set; }
        public string Message { get; set; }
        public bool SubmitEnabled { get; set; }

        public List<Constraint> Constraints { get; set; }

        public class Constraint
        {
            public bool IsDeleted { get; set; }
            public int Number { get; set; }
            public int? ParentNumber { get; set; }
            public int Order { get; set; }
            public string DataType { get; set; }
            public string Context { get; set; }
            public string Conformance { get; set; }
            public string Cardinality { get; set; }
            public string Narrative { get; set; }
            public NumberReplacementTypes NumberReplacementType { get; set; }
        }
    }
}