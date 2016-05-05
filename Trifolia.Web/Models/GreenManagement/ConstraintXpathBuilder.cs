using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Trifolia.Web.Models.GreenManagement
{
    public class ConstraintXpathBuilder
    {
        #region Private Fields

        private DB.IObjectRepository _tdb;

        #endregion

        #region Ctor

        public ConstraintXpathBuilder(DB.IObjectRepository tdb)
        {
            _tdb = tdb;
        }

        #endregion

        #region Public Methods

        public string GenerateXpath(DB.TemplateConstraint aStartConstraint)
        {
            if (!aStartConstraint.ParentConstraintId.HasValue) return aStartConstraint.Context;

            StringBuilder lXpathBuilder = new StringBuilder();
            lXpathBuilder.Append(aStartConstraint.Context);

            DB.TemplateConstraint lParentConstraint = aStartConstraint.ParentConstraint;

            do
            {
                string addition = lParentConstraint.Context;

                if (!string.IsNullOrEmpty(lParentConstraint.DataType))
                    addition += string.Format("[{0}]", lParentConstraint.DataType);

                addition += "/";

                lXpathBuilder.Insert(0, addition);

                lParentConstraint = _tdb.TemplateConstraints.DefaultIfEmpty(null)
                    .SingleOrDefault(tc => tc.Id == (lParentConstraint.ParentConstraintId ?? -1));
            } while (lParentConstraint != null);

            return lXpathBuilder.ToString();
        }

        #endregion
    }
}