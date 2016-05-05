using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Config
{
    public class IGTypeSection : ConfigurationSection
    {
        private const string PLUGINS_PROP = "plugins";
        private const string CONFIG_SECTION_NAME = "igTypes";

        private static IGTypeSection _configSection;

        public static IGTypeSection GetSection()
        {
            return _configSection ?? (_configSection = ConfigurationManager.GetSection(CONFIG_SECTION_NAME) as IGTypeSection);
        }

        [ConfigurationProperty(PLUGINS_PROP, IsDefaultCollection = true)]
        public IGTypePluginCollection Plugins
        {
            get { return (IGTypePluginCollection)base[PLUGINS_PROP]; }
        }
    }
}
