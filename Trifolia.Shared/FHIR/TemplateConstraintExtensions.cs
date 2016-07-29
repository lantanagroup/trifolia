using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Shared.FHIR
{
    public static class TemplateConstraintExtensions
    {
        public static string GetElementPath(this TemplateConstraint constraint, string resourceType)
        {
            string elementPath = "";

            // Element path
            var current = constraint;
            while (current != null)
            {
                if (!string.IsNullOrEmpty(elementPath))
                    elementPath = "." + elementPath;

                elementPath = current.Context.Replace("@", "") + elementPath;
                current = current.ParentConstraint;
            }

            return (string.IsNullOrEmpty(resourceType) ? "Resource" : resourceType) + "." + elementPath;
        }
    }
}
