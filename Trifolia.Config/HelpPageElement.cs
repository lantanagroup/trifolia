using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Trifolia.Config
{
    public class HelpPageElement : ConfigurationElement
    {
        private const string PAGE_TYPE_PROP = "pageType";
        private const string HELP_LOCATION_PROP = "helpLocation";
        private const string WINDOW_TYPE_PROP = "windowType";

        [ConfigurationProperty(PAGE_TYPE_PROP, IsRequired = true)]
        public string PageType
        {
            get { return (string)this[PAGE_TYPE_PROP]; }
            set { this[PAGE_TYPE_PROP] = value; }
        }

        [ConfigurationProperty(HELP_LOCATION_PROP, IsRequired = true)]
        public string HelpLocation
        {
            get { return (string)this[HELP_LOCATION_PROP]; }
            set { this[HELP_LOCATION_PROP] = value; }
        }

        [ConfigurationProperty(WINDOW_TYPE_PROP, IsRequired = false)]
        public WindowTypes WindowType
        {
            get { return (WindowTypes)this[WINDOW_TYPE_PROP]; }
            set { this[WINDOW_TYPE_PROP] = value; }
        }

        public enum WindowTypes
        {
            PopupWindow,
            NewWindow
        }
    }

    public class HelpPageCollection : ConfigurationElementCollection
    {
        #region Ctor

        public HelpPageCollection()
        {

        }

        public HelpPageCollection(params HelpPageElement[] aElements)
        {
            foreach (HelpPageElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public new HelpPageElement this[int index]
        {
            get { return (HelpPageElement)this.BaseGet(index); }
        }

        public new HelpPageElement this[string pageType]
        {
            get { return (HelpPageElement)this.BaseGet(pageType); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HelpPageElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as HelpPageElement).PageType;
        }

        #endregion
    }
}
