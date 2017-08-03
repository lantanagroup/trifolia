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
        /// <param name="stopConstraint">Stop building the element path when this constraint is reached</param>
        /// <returns></returns>
        public static string GetElementPath(this TemplateConstraint constraint, string resourceType, TemplateConstraint stopConstraint = null)
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

                if (current == stopConstraint)
                    break;
            }

            if (!string.IsNullOrEmpty(resourceType))
                return resourceType + "." + elementPath;

            return elementPath;
        }

        /// <summary>
        /// Checks the current constraint and all parent constraints to determine if any of them are a branch. If they are,
        /// constructs a unique "slice name" for the constraint combining the element's context and the order number of the constraint.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns>A string representing a unique name for the branch of the constraint, or an empty string if branches are found within the constraint</returns>
        public static string GetSliceName(this TemplateConstraint constraint)
        {
            string sliceName = string.Empty;
            TemplateConstraint current = constraint;

            while (current != null)
            {
                if (current.IsBranch)
                {
                    if (!string.IsNullOrEmpty(sliceName))
                        sliceName = "_" + sliceName;

                    string elementName = !string.IsNullOrEmpty(current.Context) ? current.Context : "slice";

                    sliceName = elementName + current.Order.ToString() + sliceName;
                }

                current = current.ParentConstraint;
            }

            return sliceName;
        }

        /// <summary>
        /// Gets the @id value of the element/constraint.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        /// <remarks>Creates the value based on the context of the constraint and each parent, and based on whether the parents are a branch</remarks>
        public static string GetElementId(this TemplateConstraint constraint)
        {
            string elementId = string.Empty;
            TemplateConstraint current = constraint;
            bool checkBranch = true;

            while (current != null)
            {
                if (!string.IsNullOrEmpty(elementId))
                    elementId = "." + elementId;

                if (checkBranch && current.IsBranch)
                {
                    elementId = current.GetElementPath(constraint.Template.PrimaryContextType) + ":" + current.GetSliceName() + elementId;
                    checkBranch = false;
                }
                else
                {
                    elementId = current.Context + elementId;
                }

                current = current.ParentConstraint;
            }

            return elementId;
        }
    }
}
