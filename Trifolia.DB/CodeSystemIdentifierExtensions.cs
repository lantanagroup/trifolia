using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public partial class CodeSystemIdentifier
    {
        public CodeSystemIdentifier(string identifier)
        {
            if (identifier.StartsWith("urn:oid:"))
                this.Type = IdentifierTypes.Oid;
            else if (identifier.StartsWith("urn:hl7ii:"))
                this.Type = IdentifierTypes.HL7II;
            else if (identifier.StartsWith("http://") || identifier.StartsWith("https://"))
                this.Type = IdentifierTypes.HTTP;
            else
                throw new Exception("Unexpected identifier format");

            this.Identifier = identifier;
        }
    }
}
