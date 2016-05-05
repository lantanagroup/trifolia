using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Config
{
    public class CodeSystemAlternativeElement : ConfigurationElement
    {
        #region Private Constants

        private const string CODE_SYSTEM_NAME_PROPERTY_NAME = "codeSystemName";
        private const string ALTERNATIVE_PROPERTY_NAME = "alternative";

        #endregion

        #region Public Properties

        [ConfigurationProperty(CODE_SYSTEM_NAME_PROPERTY_NAME, IsRequired = true)]
        public string CodeSystemName
        {
            get { return (string)this[CODE_SYSTEM_NAME_PROPERTY_NAME]; }
            set { this[CODE_SYSTEM_NAME_PROPERTY_NAME] = value; }
        }

        [ConfigurationProperty(ALTERNATIVE_PROPERTY_NAME, IsRequired = true)]
        public string Alternative
        {
            get { return (string)this[ALTERNATIVE_PROPERTY_NAME]; }
            set { this[ALTERNATIVE_PROPERTY_NAME] = value; }
        }

        #endregion
    }

    public class CodeSystemAlternativeCollection : ConfigurationElementCollection
    {
        #region Ctor

        public CodeSystemAlternativeCollection()
        {

        }

        public CodeSystemAlternativeCollection(params CodeSystemAlternativeElement[] aElements)
        {
            foreach (CodeSystemAlternativeElement lElement in aElements)
            {
                this.BaseAdd(lElement);
            }
        }
        
        #endregion

        #region Public Methods

        public CodeSystemAlternativeElement this[int index]
        {
            get { return (CodeSystemAlternativeElement)this.BaseGet(index); }
        }

        public CodeSystemAlternativeElement this[string alternative]
        {
            get { return (CodeSystemAlternativeElement)this.BaseGet(alternative); }
        }
        
        #endregion

        #region Overrides of ConfigurationElementCollection

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CodeSystemAlternativeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as CodeSystemAlternativeElement).Alternative;
        }

        #endregion
    }
}
