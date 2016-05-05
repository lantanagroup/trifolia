using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Config
{
    public class CodeSystemsSection : ConfigurationSection
    {
        private const string ALTERNATIVES_PROP = "alternatives";
        private const string CONFIG_SECTION_NAME = "codeSystems";

        private static CodeSystemsSection _configSection;

        public static CodeSystemsSection GetSection()
        {
            return _configSection ?? (_configSection = ConfigurationManager.GetSection(CONFIG_SECTION_NAME) as CodeSystemsSection);
        }

        [ConfigurationProperty(ALTERNATIVES_PROP, IsDefaultCollection = true)]
        public CodeSystemAlternativeCollection Alternatives
        {
            get { return (CodeSystemAlternativeCollection)base[ALTERNATIVES_PROP]; }
        }
    }
}
