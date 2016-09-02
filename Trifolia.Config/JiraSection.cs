using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Trifolia.Config
{
    public class JiraSection : ConfigurationSection
    {
        private const string PRIORITIES_PROP_NAME = "priorities";
        private const string TYPES_PROP_NAME = "types";
        private const string CONFIG_SECTION_NAME = "jira";

        private static JiraSection _configSection;

        public static JiraSection GetSection()
        {
            return _configSection ?? (_configSection = ConfigurationManager.GetSection(CONFIG_SECTION_NAME) as JiraSection);
        }

        [ConfigurationProperty(PRIORITIES_PROP_NAME, IsDefaultCollection = true)]
        public JiraMappingCollection Priorities
        {
            get { return (JiraMappingCollection)base[PRIORITIES_PROP_NAME]; }
        }

        [ConfigurationProperty(TYPES_PROP_NAME, IsDefaultCollection = true)]
        public JiraMappingCollection Types
        {
            get { return (JiraMappingCollection)base[TYPES_PROP_NAME]; }
        }
    }
}
