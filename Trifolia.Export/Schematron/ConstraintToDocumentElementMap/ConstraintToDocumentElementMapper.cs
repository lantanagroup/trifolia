using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Export.Schematron.Model;
using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Export.Schematron.ConstraintToDocumentElementMap
{
    public class ConstraintToDocumentElementMapper
    {
        /// <summary>
        /// Uses back-tracing algorithm to go backwards through the tree
        /// Helper function which builds the full parent context for a given template constraint. For example, for the template constraint @code with cda:entryRelationship/cda:observation/cda:code[@code]
        /// this function returns the cda:entryRelationship/cda:observation/cda:code.
        /// </summary>
        /// <param name="aElement">current element to start from</param>
        /// <param name="aTemplateConstraint">constraint which will have its parent chain walked to form path</param>
        /// <param name="aIncludeElementInPath">determines whether we start the path with the element passed in (true) or its parent (false)</param>
        /// <returns>full context string</returns>
        public static string CreateFullParentContext(string aPrefix, IConstraint aTemplateConstraint)
        {
            if (aTemplateConstraint == null)
                return string.Empty;

            DocumentTemplateElement firstElement = null;
            DocumentTemplateElement newElement = null;
            DocumentTemplateElement previousElement = null;
            DocumentTemplateElementAttribute newAttribute = null;
            IConstraint currentConstraint = aTemplateConstraint.Parent;
            while (currentConstraint != null)
            {
                //parse the context to determine whether this is element or attribute
                var contextParser = new ContextParser(currentConstraint.Context);
                contextParser.Parse(out newElement, out newAttribute);
                newElement.Attributes.Clear();
                if (currentConstraint.IsBranch) //if we hit a branch then we stop b/c we are in the branch's context
                    break;
                if (newElement == null)
                    break;  //there is a broken chain, we have null parent
                //add value and data type (if present)
                ConstraintToDocumentElementHelper.AddElementValueAndDataType(aPrefix, newElement, currentConstraint);
                //chain the previous element to the child collection of this new one
                if (previousElement != null)
                    newElement.AddElement(previousElement);
                //get the leaf node
                if (firstElement == null)
                    firstElement = newElement;
                previousElement = newElement;
                //walk the parent chain
                currentConstraint = currentConstraint.Parent;
            }
            if (firstElement == null)
            {
                return string.Empty;
            }
            else
            {
                var contextBuilder = new ContextBuilder(firstElement, aPrefix);
                return contextBuilder.GetFullyQualifiedContextString();
            }
        }

        #region Public methods
        public DocumentTemplateElement BuildDocumentTemplateElementTreeFromBranchIdentifiers()
        {
            return null;
        }

        public string GetFullContext()
        {
            string fullContext = string.Empty;
            return fullContext;
        }
        #endregion
    }
}
