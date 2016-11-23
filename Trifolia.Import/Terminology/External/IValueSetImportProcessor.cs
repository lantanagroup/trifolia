using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Import.Terminology.External
{
    public interface IValueSetImportProcessor<T, V>
            where T : ImportValueSet
            where V : ImportValueSetMember
    {
        T FindValueSet(IObjectRepository tdb, string oid);

        void SaveValueSet(IObjectRepository tdb, ImportValueSet valueSet);
    }
}
