using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Trifolia.Config
{
    public class HelpContextElement : ConfigurationElement
    {
        private const string KEY_PROP = "key";
        private const string HELP_LOCATION_PROP = "helpLocation";

        [ConfigurationProperty(KEY_PROP, IsRequired = true)]
        public string Key
        {
            get { return (string)this[KEY_PROP]; }
            set { this[KEY_PROP] = value; }
        }

        [ConfigurationProperty(HELP_LOCATION_PROP, IsRequired = true)]
        public string HelpLocation
        {
            get { return (string)this[HELP_LOCATION_PROP]; }
            set { this[HELP_LOCATION_PROP] = value; }
        }
    }

    public class HelpContextCollection : ConfigurationElementCollection
    {
        #region Ctor

        public HelpContextCollection()
        {

        }

        public HelpContextCollection(params HelpContextElement[] aElements)
        {
            foreach (HelpContextElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public new HelpContextElement this[int index]
        {
            get { return (HelpContextElement)this.BaseGet(index); }
        }

        public new HelpContextElement this[string key]
        {
            get { return (HelpContextElement)this.BaseGet(key); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HelpContextElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as HelpContextElement).Key;
        }

        #endregion
    }
}
