using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared
{
    [Serializable]
    public class LookupImplementationGuideType
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

        #endregion

        public static List<LookupImplementationGuideType> GetImplementationGuideTypes()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return (from igt in tdb.ImplementationGuideTypes
                        select new LookupImplementationGuideType()
                        {
                            Id = igt.Id,
                            Name = igt.Name
                        }).ToList();
            }
        }
    }
}
