using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class ConceptItem
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public int? CodeSystemId { get; set; }
        public string CodeSystemName { get; set; }
        public string CodeSystemOid { get; set; }
        public string Status { get; set; }
        public DateTime? StatusDate { get; set; }
    }
}