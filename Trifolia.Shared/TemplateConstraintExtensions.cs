using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public static class TemplateConstraintExtensions
    {
        public static string GetFhirPath(this TemplateConstraint constraint)
        {
            string path = string.Empty;
            TemplateConstraint current = constraint;

            while (current != null)
            {
                if (current.IsPrimitive)
                    return string.Empty;

                if (!string.IsNullOrEmpty(path))
                    path = "." + path;

                path = current.Context + path;
                current = current.ParentConstraint;
            }

            return constraint.Template.TemplateType.RootContextType + "." + path;
        }
    }
}
