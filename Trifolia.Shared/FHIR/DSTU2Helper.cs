using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Logging;

namespace Trifolia.Shared.FHIR
{
    public class DSTU2Helper
    {
        public const string DEFAULT_IG_NAME = "Unowned FHIR DSTU2 Profiles";
        public const string DEFAULT_USER_NAME = "admin";
        public const string STRUCDEF_NEW_IDENTIFIER_FORMAT = "https://trifolia.lantanagroup.com/Generated/{0}";

        public static string FormatIdentifier(string identifier)
        {
            if (identifier.StartsWith("http") || identifier.StartsWith("urn"))
                return identifier;

            return string.Format("urn:oid:{0}", identifier);
        }

        public static ImplementationGuideType GetImplementationGuideType(IObjectRepository tdb, bool throwError)
        {
            ImplementationGuideType found = tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name.ToLower() == Constants.IGType.FHIR_DSTU2_IG_TYPE.ToLower());
             
            if (found == null && throwError)
            {
                string errorMsg = "No DSTU2 FHIR IG Type is defined/configured";
                Log.For(typeof(DSTU2Helper)).Error(errorMsg);
                throw new Exception(errorMsg);
            }

            return found;
        }
    }
}
