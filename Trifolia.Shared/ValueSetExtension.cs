using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public static class ValueSetExtension
    {
        public static bool IsIdentifierII(this ValueSet valueSet)
        {
            return IdentifierHelper.IsIdentifierII(valueSet.Oid);
        }

        public static bool GetIdentifierII(this ValueSet valueSet, out string root, out string extension)
        {
            return IdentifierHelper.GetIdentifierII(valueSet.Oid, out root, out extension);
        }

        public static bool IsIdentifierOID(this ValueSet valueSet)
        {
            return IdentifierHelper.IsIdentifierOID(valueSet.Oid);
        }

        public static bool GetIdentifierOID(this ValueSet valueSet, out string oid)
        {
            return IdentifierHelper.GetIdentifierOID(valueSet.Oid, out oid);
        }
    }
}
