using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Validation
{
    public class ValidationRunner<TModel>
    {
        #region Private Fields

        private List<IValidationRule<TModel>> _rules = new List<IValidationRule<TModel>>();

        #endregion

        #region Ctor

        public ValidationRunner(params IValidationRule<TModel> [] rules)
        {
            _rules.AddRange(rules);
        }

        #endregion

        #region Public Methods

        public ValidationResult RunValidation(TModel aModel)
        {
            ValidationResult lResult = new ValidationResult() { Pass = true };

            if (this.Rules.Count == 0)
            {
                return lResult;
            }

            foreach (IValidationRule<TModel> lRule in this.Rules)
            {
                if (!lRule.RunRule(aModel))
                {
                    lResult.Pass = false;

                    lResult.Results.Add(new RuleValidationResult()
                    {
                        Pass = false,
                        Message = lRule.FailureMessage
                    });
                }
            }

            return lResult;
        }

        #endregion

        #region Public Properties

        public List<IValidationRule<TModel>> Rules { get { return _rules; } }

        #endregion
    }
}