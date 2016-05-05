using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Config
{
    public class BookmarkTemplateTypeElement : ConfigurationElement
    {
        #region Private Constants

        private const string TEMPLATE_TYPE_NAME_PROPERTY_NAME = "templateType";
        private const string BOOKMARK_ABBREVIATION_PROPERTY_NAME = "bookmarkAbbreviation";

        #endregion

        #region Public Properties

        [ConfigurationProperty(TEMPLATE_TYPE_NAME_PROPERTY_NAME, IsRequired = true)]
        public string TemplateTypeName
        {
            get { return (string)this[TEMPLATE_TYPE_NAME_PROPERTY_NAME]; }
            set { this[TEMPLATE_TYPE_NAME_PROPERTY_NAME] = value; }
        }

        [ConfigurationProperty(BOOKMARK_ABBREVIATION_PROPERTY_NAME, IsRequired = true)]
        public string BookmarkAbbreviation
        {
            get { return (string)this[BOOKMARK_ABBREVIATION_PROPERTY_NAME]; }
            set { this[BOOKMARK_ABBREVIATION_PROPERTY_NAME] = value; }
        }

        #endregion
    }

    public class BookmarkTemplateTypeCollection : ConfigurationElementCollection
    {
        #region Ctor

        public BookmarkTemplateTypeCollection()
        {

        }

        public BookmarkTemplateTypeCollection(params BookmarkTemplateTypeElement[] aElements)
        {
            foreach (BookmarkTemplateTypeElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public BookmarkTemplateTypeElement this[int index]
        {
            get { return (BookmarkTemplateTypeElement)this.BaseGet(index); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BookmarkTemplateTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as BookmarkTemplateTypeElement).TemplateTypeName;
        }

        #endregion
    }
}
