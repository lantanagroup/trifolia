using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Export.FHIR.STU3
{
    public static class ValueSetExtensions
    {
        public static string GetFhirId(this ValueSet valueSet)
        {
            ValueSetIdentifier identifier = valueSet.Identifiers.FirstOrDefault(y => y.Type == IdentifierTypes.HTTP);

            if (identifier == null && valueSet.Identifiers.Count > 0)
                identifier = valueSet.Identifiers.First();

            if (identifier != null)
            {
                if (identifier.Type == IdentifierTypes.Oid)
                    return identifier.Identifier.Substring(8);
                else if (identifier.Type == IdentifierTypes.HL7II)
                    return identifier.Identifier.Substring(10);
                else if (identifier.Type == IdentifierTypes.HTTP)
                    return identifier.Identifier.Substring(identifier.Identifier.LastIndexOf("/") + 1);
            }

            return valueSet.Id.ToString();
        }
    }
}
