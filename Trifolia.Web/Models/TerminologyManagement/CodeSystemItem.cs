using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class CodeSystemItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string Description { get; set; }
        public int ConstraintCount { get; set; }
        public int MemberCount { get; set; }
        public bool PermitModify { get; set; }
        public bool CanModify { get; set; }
        public bool CanOverride { get; set; }
        public bool IsPublished { get; set; }
    }
}