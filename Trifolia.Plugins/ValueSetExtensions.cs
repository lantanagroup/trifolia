using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Plugins
{
    public static class ValueSetExtensions
    {
        public static string GetIdentifier(this ValueSet valueSet, IIGTypePlugin igTypePlugin)
        {
            if (igTypePlugin != null)
                return valueSet.GetIdentifier(igTypePlugin.DefaultIdentifierType);

            return valueSet.GetIdentifier();
        }
    }
}
