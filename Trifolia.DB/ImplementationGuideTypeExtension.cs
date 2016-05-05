using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class ImplementationGuideType
    {
        public const string CDA_NAME = "CDA";
        public const string FHIR_DSTU1_NAME = "FHIR DSTU1";
        public const string FHIR_DSTU2_NAME = "FHIR DSTU2";

        public const string FHIR_NS = "http://hl7.org/fhir";

        public static List<ImplementationGuideType> GetAll()
        {
            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                return tdb.ImplementationGuideTypes.ToList();
            }
        }
    }
}
