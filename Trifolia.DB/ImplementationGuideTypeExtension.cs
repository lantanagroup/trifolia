using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class ImplementationGuideType
    {
        public const string FHIR_NS = "http://hl7.org/fhir";

        public static List<ImplementationGuideType> GetAll()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ImplementationGuideTypes.ToList();
            }
        }
    }
}
