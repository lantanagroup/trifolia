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
            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                return tdb.CodeSystems.ToList();
            }
        }
    }
}
