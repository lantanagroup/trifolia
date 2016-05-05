using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared.ImportExport
{
    public interface IValueSetImportProcessor<T, V>
            where T : Model.ImportValueSet
            where V : Model.ImportValueSetMember
    {
        T FindValueSet(IObjectRepository tdb, string oid);

        void SaveValueSet(IObjectRepository tdb, Model.ImportValueSet valueSet);
    }
}
