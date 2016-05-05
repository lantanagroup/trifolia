using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Shared;

namespace Trifolia.Web.Models
{
    public class ConstraintDataTypeResolver
    {
        #region Public Methods

        public string GetConstraintDataType(DB.TemplateConstraint aConstraint, string aConstraintXpath)
        {
            var templateType = aConstraint.Template.TemplateType;
            string context = !string.IsNullOrEmpty(aConstraint.Template.PrimaryContextType) ?
                aConstraint.Template.PrimaryContextType : templateType.RootContextType;

            var schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, templateType.ImplementationGuideType);
            schema = schema.GetSchemaFromContext(context);

            SimpleSchema.SchemaObject lSchemaObject = schema.FindFromPath(aConstraintXpath);
            return lSchemaObject.DataType;
        }

        #endregion
    }
}