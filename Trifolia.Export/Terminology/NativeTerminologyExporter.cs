using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Export.Terminology
{
    public class NativeTerminologyExporter : BaseExporter<VocabularySystems>
    {
        public NativeTerminologyExporter(IObjectRepository tdb)
            : base(tdb)
        {

        }

        protected override VocabularySystems Convert(VocabularySystems systems)
        {
            return systems;
        }
    }
}
