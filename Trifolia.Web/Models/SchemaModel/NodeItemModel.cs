using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.SchemaModel
{
    [Serializable]
    public class NodeItemModel
    {
        public string Name { get; set; }
        public string DefaultCardinality { get; set; }
        public string DefaultConformance { get; set; }
        public string DefaultDataType { get; set; }
        public string DefaultValue { get; set; }
        public bool HasChildren { get; set; }
    }
}