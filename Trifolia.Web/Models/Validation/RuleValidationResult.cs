using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Validation
{
    public class RuleValidationResult
    {
        #region Public Properties

        public bool Pass { get; set; }
        public string Message { get; set; }

        #endregion
    }
}