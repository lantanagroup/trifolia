using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared
{
    public static class Constants
    {
        public static class IGType
        {
            public const string HQMF_IG_TYPE = "HQMF R2";
            public const string EMEASURE_IG_TYPE = "eMeasure";
            public const string CDA_IG_TYPE = "CDA";
            public const string FHIR_DSTU2_IG_TYPE = "FHIR DSTU2";
            public const string FHIR_STU3_IG_TYPE = "FHIR STU3";
            public const string FHIR_CURRENT_BUILD_IG_TYPE = "FHIR Current Build";
        }
        
        public static class FHIRVersion
        {
            public const string FHIR_DSTU2_VERSION = "DSTU2";
            public const string FHIR_STU3_VERSION = "STU3";
            public const string FHIR_CURRENT_BUILD_VERSION = "Current Build";
        }
    }
}