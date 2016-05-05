using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;
using Trifolia.Authorization;

namespace Trifolia.Shared
{
    public class FilterOrganizationHelper
    {
        public static List<T> Filter<T>(IEnumerable<T> list)
            where T : IFilterOrganization
        {
            if (CheckPoint.Instance.IsDataAdmin)
                return list.ToList();

            List<T> filteredList = new List<T>();
            string organizationName = CheckPoint.Instance.OrganizationName;

            foreach (T listItem in list)
            {
                if (listItem.OrganizationName != null && listItem.OrganizationName.ToLower() == organizationName.ToLower())
                    filteredList.Add(listItem);
            }

            return filteredList;
        }
    }
}
