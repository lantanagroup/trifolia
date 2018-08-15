using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared
{
    public static class Constants
    {
        public static class IGTypeNames
        {
            public const string HQMF = "HQMF R2";
            public const string EMEASURE = "eMeasure";
            public const string CDA = "CDA";
            public const string FHIR_DSTU2 = "FHIR DSTU2";
            public const string FHIR_STU3 = "FHIR STU3";
            public const string FHIR_LATEST = "FHIR Current Build";
        }

        public static class IGTypeSchemaLocations
        {
            public const string CDA = "CDA_SDTC.xsd";
            public const string HQMF = "schemas\\EMeasure.xsd";
        }

        public static class IGTypePrefixes
        {
            public const string CDA = "cda";
            public const string HQMF = "hqmf";
        }

        public static class IGTypeNamespaces
        {
            public const string CDA = "urn:hl7-org:v3";
            public const string HQMF = "urn:hl7-org:v3";
        }
        
        public static class FHIRVersion
        {
            public const string FHIR_DSTU2_VERSION = "DSTU2";
            public const string FHIR_STU3_VERSION = "STU3";
            public const string FHIR_CURRENT_BUILD_VERSION = "Current Build";
        }
    }
}