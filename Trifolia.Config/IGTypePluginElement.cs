using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Config
{
    public class IGTypePluginElement : ConfigurationElement
    {
        #region Private Constants

        private const string IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME = "igTypeName";
        private const string PLUGIN_ASSEMBLY_PROPERTY_NAME = "pluginAssembly";

        #endregion

        #region Public Properties

        [ConfigurationProperty(IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME, IsRequired = true)]
        public string ImplementationGuideTypeName
        {
            get { return (string)this[IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME]; }
            set { this[IMPLEMENTATION_GUIDE_TYPE_PROPERTY_NAME] = value; }
        }

        [ConfigurationProperty(PLUGIN_ASSEMBLY_PROPERTY_NAME, IsRequired = true)]
        public string PluginAssembly
        {
            get { return (string)this[PLUGIN_ASSEMBLY_PROPERTY_NAME]; }
            set { this[PLUGIN_ASSEMBLY_PROPERTY_NAME] = value; }
        }

        #endregion
    }

    public class IGTypePluginCollection : ConfigurationElementCollection
    {
        #region Ctor

        public IGTypePluginCollection()
        {

        }

        public IGTypePluginCollection(params IGTypePluginElement[] aElements)
        {
            foreach (IGTypePluginElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public IGTypePluginElement this[int index]
        {
            get { return (IGTypePluginElement)this.BaseGet(index); }
        }

        public IGTypePluginElement this[string igTypeNamespace]
        {
            get { return (IGTypePluginElement)this.BaseGet(igTypeNamespace); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IGTypePluginElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as IGTypePluginElement).ImplementationGuideTypeName;
        }

        #endregion
    }
}
