using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class ValueSetModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string Code { get; set; }
        public bool IsIntentional { get; set; }
        public string IntentionalDefinition { get; set; }
        public string Description { get; set; }
        public string SourceUrl { get; set; }
        public bool IsComplete { get; set; }
    }
}