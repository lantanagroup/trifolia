using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Web.Models.Validation;

namespace Trifolia.Web.Models.GreenManagement
{
    public class GreenTemplateSaveResult
    {
        #region Public Properties

        public GreenTemplateViewModel ViewModel { get; set; }
        public bool FailedValidation { get; set; }
        public string ValidationMessage { get; set; }

        #endregion
    }
}