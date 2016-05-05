using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Config
{
    public class BookmarkSection : ConfigurationSection
    {
        private const string TEMPLATE_TYPES_PROP = "templateTypes";
        private const string CONFIG_SECTION_NAME = "bookmark";

        private static BookmarkSection _configSection;

        public static BookmarkSection GetSection()
        {
            return _configSection ?? (_configSection = ConfigurationManager.GetSection(CONFIG_SECTION_NAME) as BookmarkSection);
        }

        [ConfigurationProperty(TEMPLATE_TYPES_PROP, IsDefaultCollection = true)]
        public BookmarkTemplateTypeCollection TemplateTypes
        {
            get { return (BookmarkTemplateTypeCollection)base[TEMPLATE_TYPES_PROP]; }
        }
    }
}
