using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Config
{
    public class IGTypeFhirElement : ConfigurationElement
    {
        #region Private Constants

        private const string IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME = "igTypeName";
        private const string VERSION_PROPERTY_NAME = "version";

        #endregion

        #region Public Properties

        [ConfigurationProperty(IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME, IsRequired = true)]
        public string ImplementationGuideTypeName
        {
            get { return (string)this[IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME]; }
            set { this[IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME] = value; }
        }

        [ConfigurationProperty(VERSION_PROPERTY_NAME, IsRequired = true)]
        public string Version
        {
            get { return (string)this[VERSION_PROPERTY_NAME]; }
            set { this[VERSION_PROPERTY_NAME] = value; }
        }

        #endregion
    }

    public class IGTypeFhirCollection : ConfigurationElementCollection
    {
        #region Ctor

        public IGTypeFhirCollection()
        {

        }

        public IGTypeFhirCollection(params IGTypeFhirElement[] aElements)
        {
            foreach (IGTypeFhirElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public IGTypeFhirElement this[int index]
        {
            get { return (IGTypeFhirElement)this.BaseGet(index); }
        }

        public new IGTypeFhirElement this[string igTypeNamespace]
        {
            get { return (IGTypeFhirElement)this.BaseGet(igTypeNamespace); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IGTypeFhirElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as IGTypeFhirElement).ImplementationGuideTypeName;
        }

        #endregion
    }
}
