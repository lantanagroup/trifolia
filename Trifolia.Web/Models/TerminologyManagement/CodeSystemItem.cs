using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class CodeSystemItem
    {
        public CodeSystemItem()
        {
            this.Identifiers = new List<CodeSystemIdentifierModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ConstraintCount { get; set; }
        public int MemberCount { get; set; }
        public bool PermitModify { get; set; }
        public bool CanModify { get; set; }
        public bool CanOverride { get; set; }
        public bool IsPublished { get; set; }

        public List<CodeSystemIdentifierModel> Identifiers { get; set; }
    }

    public class CodeSystemIdentifierModel
    {
        public int? Id { get; set; }
        public string Identifier { get; set; }
        public IdentifierTypes Type { get; set; }
        public bool IsDefault { get; set; }
        public bool IsRemoved { get; set; }
    }
}