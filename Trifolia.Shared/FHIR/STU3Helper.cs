using System;
using System.Linq;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Logging;

namespace Trifolia.Shared.FHIR
{
    public class STU3Helper
    {
        public const string DEFAULT_IG_NAME = "Unowned FHIR STU3 Profiles";
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
            ImplementationGuideType found = tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name.ToLower() == Constants.IGTypeNames.FHIR_STU3.ToLower());

            if (found == null && throwError)
            {
                string errorMsg = "No STU3 FHIR IG Type is defined/configured";
                Log.For(typeof(STU3Helper)).Error(errorMsg);
                throw new Exception(errorMsg);
            }

            return found;
        }
    }
}
