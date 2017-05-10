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
        T FindValueSet(string oid);

        void SaveValueSet(ImportValueSet valueSet);
    }
}
