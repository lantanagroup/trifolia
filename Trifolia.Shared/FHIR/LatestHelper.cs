using System;
using System.Linq;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Logging;

namespace Trifolia.Shared.FHIR
{
    public class LatestHelper
    {
        public const string DEFAULT_IG_NAME = "Unowned FHIR Current Build Profiles";
        public const string DEFAULT_USER_NAME = "admin";
        public const string DEFAULT_ORG_NAME = "LCG";
        public const string STRUCDEF_NEW_IDENTIFIER_FORMAT = "https://trifolia.lantanagroup.com/Generated/{0}";

        public static string FormatIdentifier(string identifier)
        {
            if (identifier.StartsWith("http") || identifier.StartsWith("urn"))
                return identifier;

            return string.Format("urn:oid:{0}", identifier);
        }

        public static ImplementationGuideType GetImplementationGuideType(IObjectRepository tdb, bool throwError)
        {
            ImplementationGuideType found = tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name.ToLower() == Constants.IGTypeNames.FHIR_CURRENT_BUILD.ToLower());

            if (found == null && throwError)
            {
                string errorMsg = "No Latest FHIR IG Type is defined/configured";
                Log.For(typeof(LatestHelper)).Error(errorMsg);
                throw new Exception(errorMsg);
            }

            return found;
        }
    }
}
