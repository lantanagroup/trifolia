using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared
{
    [Serializable]
    public class LookupValueSet
    {
        #region Properties

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string oid;

        public string Oid
        {
            get { return oid; }
            set { oid = value; }
        }

        #endregion

        public static List<LookupValueSet> GetValuesets()
        {
            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                return GetValuesets(tdb);
            }
        }

        public static List<LookupValueSet> GetValuesets(IObjectRepository tdb)
        {
             return (from v in tdb.ValueSets
                    select new LookupValueSet()
                    {
                        Id = v.Id,
                        Name = v.Name,
                        Oid = v.Oid
                    }).ToList();
        }
    }
}
