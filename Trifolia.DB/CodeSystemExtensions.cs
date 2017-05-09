using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public partial class CodeSystem
    {
        public CodeSystem(string name)
        {
            this.Name = name;
        }

        public CodeSystemIdentifier GetIdentifier(IdentifierTypes? preferred = null)
        {
            CodeSystemIdentifier identifier = null;
            var identifiers = this.Identifiers.OrderByDescending(y => y.IsDefault);

            if (preferred != null)
                identifier = identifiers.Where(y => y.Type == preferred).FirstOrDefault();

            if (identifier == null)
                identifier = identifiers.FirstOrDefault();

            return identifier;
        }

        public string GetIdentifierValue()
        {
            return this.GetIdentifierValue(null);
        }

        public string GetIdentifierValue(IdentifierTypes? preferred)
        {
            CodeSystemIdentifier identifier = this.GetIdentifier(preferred);

            return (identifier != null ? identifier.Identifier : string.Empty);
        }
    }
}
