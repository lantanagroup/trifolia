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
            string id = valueSet.Id.ToString();

            if (valueSet.Oid.LastIndexOf("/") > 0)
                id = valueSet.Oid.Substring(valueSet.Oid.LastIndexOf("/") + 1);

            return id;
        }
    }
}
