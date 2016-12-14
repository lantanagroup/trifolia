using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Export.Schematron.Model
{
    public class DocumentTemplateElement
    {

        #region Constructors

        public DocumentTemplateElement(string aElementName)
        {
            this.ElementName   = aElementName;
            this.ChildElements = new List<DocumentTemplateElement>();
            this.Attributes    = new List<DocumentTemplateElementAttribute>();
        }

        public DocumentTemplateElement(string aElementName, string aValue)
        {
            this.ElementName   = aElementName;
            this.Value         = aValue;
            this.ChildElements = new List<DocumentTemplateElement>();
            this.Attributes    = new List<DocumentTemplateElementAttribute>();
        }

        #endregion

        #region Public Properties
        
        public DocumentTemplateElement Template { get; set; } //template this element belongs to

        public DocumentTemplateElement ParentElement { get; set; }  //parent element, allows us to do nesting
        
        public List<DocumentTemplateElement> ChildElements { get; set; }
        
        public List<DocumentTemplateElementAttribute> Attributes { get; set; }
        
        public string ElementName { get; set; }

        public string Value { get; set; }

        public Dictionary<string, string> ElementToAttributeOverrideMapping = new Dictionary<string,string>();

        public bool IsBranch { get; set; }

        public bool IsBranchIdentifier { get; set; }

        #endregion

        #region Protected Methods

        protected virtual void AddElementToChildCollection(DocumentTemplateElement aElement)
        {
            this.ChildElements.Add(aElement);
        }

        #endregion

        #region Private Methods
        private bool IsAttributeDefined(string aAttributeName)
        {
            foreach (var attr in this.Attributes)
            {
                if (attr.AttributeName == aAttributeName)
                    return true;
            }

            return false;
        }
        #endregion

        #region Public Methods

        //note, this is fluent for chaining calls, return this
        public DocumentTemplateElement AddElement(DocumentTemplateElement aElement)
        {
            aElement.ParentElement = this;
            AddElementToChildCollection(aElement);
            return this;
        }

        //note, this is fluent for chaining calls, return this
        public DocumentTemplateElement AddAttribute(DocumentTemplateElementAttribute aAttribute)
        {
            aAttribute.Element = this;
            this.Attributes.Add(aAttribute);
            return this;
        }

        public string GetAssertionStringIdentifier()
        {
            var sb = new StringBuilder();
            sb.Append(this.ElementName);
            if (!string.IsNullOrEmpty(this.Value))
            {
                if (ElementToAttributeOverrideMapping != null && ElementToAttributeOverrideMapping.Keys.Contains(this.ElementName) && !IsAttributeDefined(ElementToAttributeOverrideMapping[this.ElementName]))
                {
                    sb.AppendFormat("[@{0}='{1}']", ElementToAttributeOverrideMapping[this.ElementName], this.Value);
                }
                else
                {
                    sb.AppendFormat("[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{0}']", this.Value.ToLower()); //translate does case-insensitive compare in xpath 1.0
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
