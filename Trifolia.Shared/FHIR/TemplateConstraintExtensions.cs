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
        /// <summary>
        /// Builds a path in the form of elementX.elementY.elementZ.attributeA
        /// Call this method on the leaf constraint to build a path including all parents
        /// </summary>
        /// <remarks>
        /// Traverses the tree of parents
        /// Skips parents that are a choice to avoid paths like: Observation.extension.value[x].valueCodeableConcept
        /// Will not skip choices if the choice is the leaf node, ex: Observation.extension.value[x]
        /// </remarks>
        /// <param name="constraint">The lowest level constraint that the path should be built for</param>
        /// <param name="resourceType">If not null or empty, the type of resource that the path is being built for; used as the starting element in the path.</param>
        /// <returns></returns>
        public static string GetElementPath(this TemplateConstraint constraint, string resourceType)
        {
            string elementPath = "";

            // Element path
            var current = constraint;
            while (current != null)
            {
                if (!current.IsChoice || string.IsNullOrEmpty(elementPath))
                {
                    if (!string.IsNullOrEmpty(elementPath))
                        elementPath = "." + elementPath;

                    elementPath = current.Context.Replace("@", "") + elementPath;
                }

                current = current.ParentConstraint;
            }

            if (!string.IsNullOrEmpty(resourceType))
                return resourceType + "." + elementPath;

            return elementPath;
        }
    }
}
