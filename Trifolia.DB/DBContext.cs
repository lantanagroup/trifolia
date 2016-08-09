using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Trifolia.Config;

namespace Trifolia.DB
{
    public static class DBContext
    {
        internal static IObjectRepository Instance { get; set; }

        public static IObjectRepository Create()
        {
            if (Instance != null)
                return Instance;

            Type dbContextType = Type.GetType(Properties.Settings.Default.DBContextType);
            IObjectRepository dbContext = null;

            if (!string.IsNullOrEmpty(AppSettings.DatabaseConnectionString))
                dbContext = (IObjectRepository)Activator.CreateInstance(dbContextType);
            else
                dbContext = (IObjectRepository)Activator.CreateInstance(dbContextType, AppSettings.DatabaseConnectionString);

            return dbContext;
        }
    }
}
