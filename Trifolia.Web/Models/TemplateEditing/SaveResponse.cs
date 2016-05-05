using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateEditing
{
    public class SaveResponse
    {
        public string Error { get; set; }
        public int TemplateId { get; set; }
        public IEnumerable<ConstraintModel> Constraints { get; set; }
        public IEnumerable<dynamic> ValidationResults { get; set; }
    }
}