using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Validation
{
    public interface IValidationRule<TModel>
    {
        #region Methods

        bool RunRule(TModel aModel);

        #endregion

        #region Properties

        string FailureMessage { get; }

        #endregion
    }
}