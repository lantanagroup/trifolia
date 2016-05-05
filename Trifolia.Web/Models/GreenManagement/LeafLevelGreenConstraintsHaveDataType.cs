using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Web.Models.Validation;

namespace Trifolia.Web.Models.GreenManagement
{
    public class LeafLevelGreenConstraintsHaveDataType : IValidationRule<GreenTemplateViewModel>
    {
        public bool RunRule(GreenTemplateViewModel aModel)
        {
            bool isValid = true;

            foreach (var rootConstraint in aModel.childConstraints)
            {
                if (!IsValid(rootConstraint))
                    isValid = false;
            }

            return isValid;
        }

        private bool IsValid(ConstraintViewModel greenConstraint)
        {
            if (greenConstraint.hasGreenConstraint && !HasGreenChildren(greenConstraint) && greenConstraint.datatypeId == null)
                return false;

            foreach (var child in greenConstraint.children)
            {
                if (!IsValid(child))
                    return false;
            }

            return true;
        }

        private bool HasGreenChildren(ConstraintViewModel greenConstraint)
        {
            if (greenConstraint.children.Count(y => y.hasGreenConstraint) > 0)
                return true;

            bool ret = false;

            foreach (var child in greenConstraint.children)
            {
                if (HasGreenChildren(child))
                    ret = true;
            }

            return ret;
        }

        public string FailureMessage
        {
            get { return "All leaf-level green constraints must have a data-type."; }
        }
    }
}