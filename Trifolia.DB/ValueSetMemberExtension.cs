using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class ValueSetMember
    {
        public static List<ValueSetMember> GetValueSetMembers()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ValueSetMembers
                    .ToList();
            }
        }

        public static List<ValueSetMember> GetValueSetMembers(int valueSetId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ValueSetMembers
                    .Where(y => y.ValueSetId == valueSetId)
                    .ToList();
            }
        }
    }
}
