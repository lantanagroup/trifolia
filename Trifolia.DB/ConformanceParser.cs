using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Logging;

namespace Trifolia.DB
{
    public class ConformanceParser
    {
        public static Conformance Parse(string aConformanceString)
        {
            var conformance = Conformance.UNKNOWN;

            if (aConformanceString != null)
            {
                if (!System.Enum.TryParse<Conformance>(aConformanceString.Replace(" ", "_"), out conformance))
                {
                    Log.For(typeof(Conformance)).Error("Cannot parse conformance value '{0}'.", aConformanceString);
                }
            }

            return conformance;
        }
    }
}
