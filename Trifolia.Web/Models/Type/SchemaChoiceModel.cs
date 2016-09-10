using System.Collections.Generic;

namespace Trifolia.Web.Models.Type
{
    public class SchemaChoiceModel
    {
        public string ComplexTypeName { get; set; }
        public string CalculatedName { get; set; }
        public string DefinedName { get; set; }
        public IEnumerable<string> ChildrenElements { get; set; }
        public string SourceUri { get; set; }
        public string XPath { get; set; }
        public string Documentation { get; set; }
    }
}