using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class ValueSetModel
    {
        public ValueSetModel()
        {
            this.Identifiers = new List<ValueSetIdentifierModel>();
        }

        public int? Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string Code { get; set; }
        public bool IsIntentional { get; set; }
        public string IntentionalDefinition { get; set; }
        public string Description { get; set; }
        public string SourceUrl { get; set; }
        public bool IsComplete { get; set; }
        public ValueSetImportSources? ImportSource { get; set; }
        public bool PermitModify { get; set; }
        public bool CanModify { get; set; }
        public bool CanOverride { get; set; }

        public List<ValueSetIdentifierModel> Identifiers { get; set; }
    }

    public class ValueSetIdentifierModel
    {
        public ValueSetIdentifierModel()
        {

        }

        public ValueSetIdentifierModel(ValueSetIdentifier valueSetIdentifier)
        {
            this.Id = valueSetIdentifier.Id;
            this.Identifier = valueSetIdentifier.Identifier;
            this.Type = valueSetIdentifier.Type;
            this.IsDefault = valueSetIdentifier.IsDefault;
        }

        public int? Id { get; set; }
        public string Identifier { get; set; }
        public ValueSetIdentifierTypes Type { get; set; }
        public bool IsDefault { get; set; }
        public bool ShouldRemove { get; set; }
    }
}