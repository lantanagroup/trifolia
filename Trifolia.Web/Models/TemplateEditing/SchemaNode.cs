using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateEditing
{
    public class SchemaNode
    {
        public string Context { get; set; }
        public string Conformance { get; set; }
        public string Cardinality { get; set; }
        public string DataType { get; set; }
        public bool HasChildren { get; set; }
        public bool IsChoice { get; set; }
    }
}