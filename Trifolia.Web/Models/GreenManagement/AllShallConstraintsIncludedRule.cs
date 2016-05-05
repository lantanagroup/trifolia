using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Web.Models.Validation;

namespace Trifolia.Web.Models.GreenManagement
{
    public class AllShallConstraintsIncludedRule : IValidationRule<GreenTemplateViewModel>
    {
        #region Private Fields

        private DB.Template _underlyingTemplate = null;

        #endregion

        #region Ctor

        public AllShallConstraintsIncludedRule(DB.Template aUnderlyingTemplate)
        {
            _underlyingTemplate = aUnderlyingTemplate;
        }

        #endregion

        #region Public Properties

        public string FailureMessage
        {
            get { return "All SHALL constraints that have editable data must be included in the green model"; }
        }

        #endregion

        #region Public Methods

        public bool RunRule(GreenTemplateViewModel aModel)
        {
            var lConstraints = from c in _underlyingTemplate.ChildConstraints
                               where c.Conformance == "SHALL"
                               && c.ParentConstraintId == null
                               && string.IsNullOrEmpty(c.Value)
                               select c;

            if (lConstraints.Count() > 0)
            {
                foreach (DB.TemplateConstraint lConstraint in lConstraints)
                {
                    if (!this.AssertRequiredShallConstraints(lConstraint, aModel)) return false;
                }
            }

            return true;
        }

        #endregion

        #region Private Methods

        private bool AssertRequiredShallConstraints(DB.TemplateConstraint aRootConstraint, GreenTemplateViewModel aModel)
        {
            foreach (DB.TemplateConstraint lChildConstraint in aRootConstraint.ChildConstraints.Where(tc=>tc.Conformance == "SHALL"))
            {
                if (!AssertRequiredShallConstraints(lChildConstraint, aModel))
                {
                    return false;
                }
            }

            if (aRootConstraint.IsPrimitive) return true;
            if (!string.IsNullOrEmpty(aRootConstraint.Value)) return true;
            if (aRootConstraint.ChildConstraints.Count > 0) return true;

            ConstraintViewModel lFoundModel = null;

            foreach (ConstraintViewModel lViewConstraint in aModel.childConstraints)
            {
                lFoundModel = this.FindConstraint(lViewConstraint, c=>c.id == aRootConstraint.Id);
                if (lFoundModel != null) break;
            }

            if (lFoundModel == null) return false;
            if (lFoundModel.isDeleted) return false;
            return lFoundModel.hasGreenConstraint;
        }

        private ConstraintViewModel FindConstraint(ConstraintViewModel aStartConstraint, Func<ConstraintViewModel, bool> predicate)
        {
            foreach (ConstraintViewModel lChild in aStartConstraint.children)
            {
                ConstraintViewModel lModel = FindConstraint(lChild, predicate);
                if (lModel != null)
                {
                    return lModel;
                }
                else
                {
                    continue;
                }
            }

            if (predicate(aStartConstraint))
            {
                return aStartConstraint;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}