using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Type
{
    public class TypeModel
    {
        public TypeModel()
        {
            this.TemplateTypes = new List<TemplateTypeModel>();
            this.DeletedTemplateTypes = new List<TemplateTypeModel>();
            this.ComplexTypes = new List<string>();
            this.DataTypes = new List<string>();
        }

        public int? Id { get; set; }
        public string Name { get; set; }
        public byte[] SchemaFile { get; set; }
        public string SchemaFileContentType { get; set; }
        public string SchemaLocation { get; set; }
        public string SchemaPrefix { get; set; }
        public string SchemaUri { get; set; }
        public IEnumerable<TemplateTypeModel> TemplateTypes { get; set; }
        public IEnumerable<TemplateTypeModel> DeletedTemplateTypes { get; set; }
        public IEnumerable<string> ComplexTypes { get; set; }
        public IEnumerable<string> DataTypes { get; set; }

        public class TemplateTypeModel
        {
            public int? Id { get; set; }
            public int OutputOrder { get; set; }
            public string Name { get; set; }
            public string Context { get; set; }
            public string ContextType { get; set; }
            public int TemplateCount { get; set; }
        }
    }
}