using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Trifolia.Config
{
    public class JiraMappingElement : ConfigurationElement
    {
        private const string ID_PROP = "id";
        private const string NAME_PROP = "name";

        [ConfigurationProperty(ID_PROP, IsRequired = true)]
        public string Id
        {
            get { return (string)this[ID_PROP]; }
            set { this[ID_PROP] = value; }
        }

        [ConfigurationProperty(NAME_PROP, IsRequired = true)]
        public string Name
        {
            get { return (string)this[NAME_PROP]; }
            set { this[NAME_PROP] = value; }
        }
    }

    public class JiraMappingCollection : ConfigurationElementCollection
    {
        #region Ctor

        public JiraMappingCollection()
        {

        }

        public JiraMappingCollection(params JiraMappingElement[] aElements)
        {
            foreach (JiraMappingElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public JiraMappingElement this[int index]
        {
            get { return (JiraMappingElement)this.BaseGet(index); }
        }

        public new JiraMappingElement this[string name]
        {
            get { return (JiraMappingElement)this.BaseGet(name); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new JiraMappingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as JiraMappingElement).Name;
        }

        #endregion
    }
}
