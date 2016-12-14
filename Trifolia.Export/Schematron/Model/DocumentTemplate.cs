using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Export.Schematron.Model
{
    public class DocumentTemplate
        : DocumentTemplateElement
    {

        #region Constructors
        
        public DocumentTemplate(string aNamespace)
            : base("ClinicalDocument")
        {
            this.Namespace = aNamespace;
            this.Template = this;
        }

        #endregion

        #region Public Properties

        public string Namespace { get; set; }

        #endregion

        #region Overrides 
        
        protected override void AddElementToChildCollection(DocumentTemplateElement aElement)
        {
            aElement.Template = this;
            base.AddElementToChildCollection(aElement);
        }

        #endregion
    }
}
