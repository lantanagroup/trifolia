using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Trifolia.Config
{
    public class ToolTipElement : ConfigurationElement
    {
        private const string ID_PROP = "id";
        private const string TEXT_PROP = "text";

        [ConfigurationProperty(ID_PROP, IsRequired = true)]
        public string Id
        {
            get { return (string)this[ID_PROP]; }
            set { this[ID_PROP] = value; }
        }

        [ConfigurationProperty(TEXT_PROP, IsRequired = true)]
        public string Text
        {
            get { return (string)this[TEXT_PROP]; }
            set { this[TEXT_PROP] = value; }
        }
    }

    public class ToolTipCollection : ConfigurationElementCollection
    {
        #region Ctor

        public ToolTipCollection()
        {

        }

        public ToolTipCollection(params ToolTipElement[] aElements)
        {
            foreach (ToolTipElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public new ToolTipElement this[int index]
        {
            get { return (ToolTipElement)this.BaseGet(index); }
        }

        public new ToolTipElement this[string id]
        {
            get { return (ToolTipElement)this.BaseGet(id); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ToolTipElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ToolTipElement).Id;
        }

        #endregion
    }
}
