using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Trifolia.DB
{
    public static class DBContext
    {
        public static IObjectRepository Instance { get; set; }

        public static IObjectRepository Create()
        {
            if (Instance != null)
                return Instance;

            Type dbContextType = Type.GetType(Properties.Settings.Default.DBContextType);
            IObjectRepository dbContext = (IObjectRepository) Activator.CreateInstance(dbContextType);
            return dbContext;
        }
    }
}
