using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class MoveConstraint
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public bool IsDeleted { get; set; }
        public int Number { get; set; }
        public int Order { get; set; }
        public string DataType { get; set; }
        public string Context { get; set; }
        public string Conformance { get; set; }
        public string Cardinality { get; set; }
        public string Narrative { get; set; }
        public NumberReplacementTypes NumberReplacementType { get; set; }
    }
}