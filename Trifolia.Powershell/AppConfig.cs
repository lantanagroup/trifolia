using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Trifolia.Powershell
{
    public static class AppConfig
    {
        public static string ConfigLocation { get; set; }
        private static string oldConfig;

        public static void Change()
        {
            if (!string.IsNullOrEmpty(ConfigLocation))
            {
                oldConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", Path.GetFullPath(ConfigLocation));
                ResetConfigMechanism();
            }
        }

        public static void Revert()
        {
            if (!string.IsNullOrEmpty(ConfigLocation))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                ResetConfigMechanism();
            }
        }

        private static void ResetConfigMechanism()
        {
            typeof(ConfigurationManager)
                .GetField("s_initState", BindingFlags.NonPublic |
                                         BindingFlags.Static)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", BindingFlags.NonPublic |
                                            BindingFlags.Static)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName ==
                            "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", BindingFlags.NonPublic |
                                       BindingFlags.Static)
                .SetValue(null, null);
        }
    }
}