using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared.Validation
{
    public class TemplateValidationResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public List<ValidationResult> Items { get; set; }
    }
}
