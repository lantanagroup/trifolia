using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Export.Schematron.Model
{
    public class DocumentTemplateElementAttribute
    {

        #region Constructors

        public DocumentTemplateElementAttribute(string aAttributeName, string aSingleValue, string aValueSet, string aCodeSystemName)
        {
            this.AttributeName = aAttributeName;
            this.SingleValue = aSingleValue;
            this.ValueSet = aValueSet;
            this.CodeSystemName = aCodeSystemName;
        }

        public DocumentTemplateElementAttribute(string aAttributeName, string aSingleValue, string aValueSet)
        {
            this.AttributeName = aAttributeName;
            this.SingleValue = aSingleValue;
            this.ValueSet = aValueSet;
        }

        public DocumentTemplateElementAttribute(string aAttributeName, string aSingleValue, bool aAllowCodeSystemNullFlavor=false)
        {
            this.AttributeName = aAttributeName;
            this.SingleValue = aSingleValue;
            this.AllowCodeSystemNullFlavor = aAllowCodeSystemNullFlavor;
        }

        public DocumentTemplateElementAttribute(string aAttributeName)
        {
            this.AttributeName = aAttributeName;
        }

        #endregion

        #region Public Properties

        public DocumentTemplateElement Element { get; set; }  //element this attribute applies to

        public string AttributeName { get; set; }

        public string SingleValue { get; set; }

        public string ValueSet { get; set; }

        public string CodeSystemName { get; set; }

        public string DataType { get; set; }

        public string CodeSystemOid { get; set; }

        public bool AllowCodeSystemNullFlavor { get; set; }

        #endregion

        #region Public Methods
        
        public string GetAssertionStringIdentifier(string aConcatWithValueString=null, bool aForceBrackets=false)
        {
            string prefix = this.Element == null ? string.Empty : "[";
            string postfix = this.Element == null ? string.Empty : "]";
            string datatype = string.IsNullOrEmpty(this.DataType) ? string.Empty : string.Format("@xsi:type='{0}'", this.DataType);
            string codeSystemNullFlavor = this.AllowCodeSystemNullFlavor ? " or @nullFlavor" : string.Empty;
            string codeSystem = string.IsNullOrEmpty(this.CodeSystemName) ? string.Empty : string.Format("@codeSystem='{0}'{1}", this.CodeSystemName, codeSystemNullFlavor);
            string codeSystemOid = string.IsNullOrEmpty(this.CodeSystemOid) ? string.Empty : string.Format(" and @codeSystem='{0}{1}'", this.CodeSystemOid, codeSystemNullFlavor );
            string value = string.IsNullOrEmpty(this.SingleValue) ? string.Empty : string.Format("='{0}'", this.SingleValue);
            if (string.IsNullOrEmpty(codeSystem) && string.IsNullOrEmpty(codeSystemOid) && !string.IsNullOrEmpty(codeSystemNullFlavor))
            {
                value += codeSystemNullFlavor;
            }

            aConcatWithValueString = aConcatWithValueString == null ? string.Empty : aConcatWithValueString;

            if (!string.IsNullOrEmpty(datatype) || !string.IsNullOrEmpty(codeSystem) || aForceBrackets)
            {
                prefix = "[";
                postfix = "]";
            }

            return prefix + "@" + this.AttributeName + value + codeSystemOid + aConcatWithValueString + postfix + codeSystem ;
        }

        public int GetNumberOfValuesDefined()
        {
            int ct = 0;
            ct += (string.IsNullOrEmpty(SingleValue) ? 0 : 1);
            ct += (string.IsNullOrEmpty(CodeSystemName) ? 0 : 1);
            ct += (string.IsNullOrEmpty(DataType) ? 0 : 1);
            return ct;
        }
        
        #endregion

    }
}
