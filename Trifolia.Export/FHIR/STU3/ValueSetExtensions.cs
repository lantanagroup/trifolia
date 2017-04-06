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
            ValueSetIdentifier httpIdentifier = valueSet.Identifiers.FirstOrDefault(y => y.Type == ValueSetIdentifierTypes.HTTP);

            if (httpIdentifier != null)
                return httpIdentifier.Identifier.Substring(httpIdentifier.Identifier.LastIndexOf("/") + 1);

            return valueSet.Id.ToString();
        }
    }
}
