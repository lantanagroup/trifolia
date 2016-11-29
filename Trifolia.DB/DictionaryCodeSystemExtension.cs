using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class CodeSystem
    {
        public static List<CodeSystem> GetAllCodeSystems()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.CodeSystems.ToList();
            }
        }
    }
}
