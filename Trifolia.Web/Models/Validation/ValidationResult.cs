using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Validation
{
    public class ValidationResult
    {
        #region Private Fields

        private List<RuleValidationResult> _results = new List<RuleValidationResult>();

        #endregion

        #region Public Properties

        public bool Pass { get; set; }
        public List<RuleValidationResult> Results { get { return _results; } }

        #endregion
    }
}