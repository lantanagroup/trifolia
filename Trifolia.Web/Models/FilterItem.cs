using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models
{
    public class FilterItem
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }

        #endregion

        #region Comparer

        internal class Comparer : IEqualityComparer<FilterItem>
        {
            public bool Equals(FilterItem x, FilterItem y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                if ((x == null && y != null) || (y == null && x != null))
                    return false;
                else if (x == null && y == null)
                    return true;

                if (x.Name == null && y.Name != null)
                    return false;
                else if (x.Name != null && y.Name == null)
                    return false;
                else if (x.Name == null && y.Name == null)
                    return true;

                return x.Name.CompareTo(y.Name) == 0;
            }

            public int GetHashCode(FilterItem obj)
            {
                if (Object.ReferenceEquals(obj, null)) return 0;

                return obj.Name == null ? 0 : obj.Name.GetHashCode();
            }
        }

        #endregion
    }
}