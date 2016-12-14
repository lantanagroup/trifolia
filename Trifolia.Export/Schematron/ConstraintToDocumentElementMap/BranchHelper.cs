using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Export.Schematron.Model;
using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Export.Schematron.ConstraintToDocumentElementMap
{
    static internal class BranchHelper
    {

        /// <summary>
        /// If the template constraint is a branch then add the appropriate attributes to the element passed in.
        /// </summary>
        /// <param name="aElement">
        /// Element to create the attributes on
        /// </param>
        /// <param name="aTemplateConstraint">
        /// Constraint to evaluate
        /// </param>
        static public void CheckForBranchedAttributes(DocumentTemplateElement aElement, IConstraint aTemplateConstraint)
        {
            if (aTemplateConstraint.IsBranch || aTemplateConstraint.IsBranchIdentifier) //add attributes
            {
                AddBranchedAttributes(aElement, aTemplateConstraint);
            }
        }

        /// <summary>
        /// Checks parent to see if IsBranch attribute is set. If true then adds the sibling elements.
        /// </summary>
        /// <param name="aChildElement">Target element that we need to locate siblings for</param>
        /// <param name="aParentTemplateConstraint">Parent Constraint of the aChildElement</param>
        /// <param name="aParentElement">Parent Element for the aChildElement</param>
        /// <param name="aAddedConstraints">Constraints that have already been added to the child collection, we don't need to parse these.</param>
        static public void CheckForBranchedSiblingElements(DocumentTemplateElement aChildElement, DocumentTemplateElement aParentElement, IConstraint aParentTemplateConstraint, Dictionary<DocumentTemplateElement, IConstraint> aConstraintMap)
        {
            if ((aChildElement == null)
                || (aParentElement == null)
                || (aParentTemplateConstraint == null))
                return;

            if (aParentTemplateConstraint.IsBranch || aParentTemplateConstraint.IsBranchIdentifier)
            {
                AddSiblingElements(aChildElement, aParentTemplateConstraint, aParentElement, aConstraintMap);
            }
        }

        /// <summary>
        /// Takes a given child element (aChildElement) and parent constraint (aParentTemplateConstraint) and steps through the Children of the parent constraint
        /// to determine if any siblings of the child element exist. 
        /// </summary>
        /// <param name="aChildElement">Target element that we need to locate siblings for</param>
        /// <param name="aParentTemplateConstraint">Parent Constraint of the aChildElement</param>
        /// <param name="aParentElement">Parent Element for the aChildElement</param>
        /// <param name="aAddedConstraints">Constraints that have already been added to the child collection, we don't need to parse these.</param>
        static private void AddSiblingElements(DocumentTemplateElement aChildElement, IConstraint aParentTemplateConstraint, DocumentTemplateElement aParentElement, Dictionary<DocumentTemplateElement, IConstraint> aConstraintMap)
        {
            //look at parent to get the siblings
            if (aParentTemplateConstraint.Children != null)
            {
                DocumentTemplateElement parsedElement = null;
                DocumentTemplateElementAttribute parsedAttribute = null;
                //walk through the siblings
                foreach (var sibling in aParentTemplateConstraint.Children)
                {
                    //have we already added this constraint in a previous iteration (e.g. it's on the main path from the leaf to the root)
                    if (sibling.IsBranchIdentifier && !aConstraintMap.ContainsValue(sibling))
                    {
                        //parse the context
                        var cp = new ContextParser(sibling.Context);
                        cp.Parse(out parsedElement, out parsedAttribute);
                        //is this an element or an attribute?
                        if ((parsedElement != null) && (!string.IsNullOrEmpty(parsedElement.ElementName)))
                        {
                            parsedElement.IsBranchIdentifier = sibling.IsBranchIdentifier;
                            parsedElement.IsBranch = sibling.IsBranch;
                            //element, let's add it to the parent element's children so it becomes a sibling of aChildElement
                            parsedElement.Value = sibling.Value;
                            aParentElement.AddElement(parsedElement);
                            AddBranchedAttributes(parsedElement, sibling);
                            aConstraintMap.Add(parsedElement, sibling);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method which steps through the children for a given constraint (aTemplateConstraint) and attaches any attributes to the
        /// given element (aElement)).
        /// </summary>
        /// <param name="aElement">Element to add the attributes to</param>
        /// <param name="aTemplateConstraint">Constraint to walk the children to find attributes</param>
        static private void AddBranchedAttributes(DocumentTemplateElement aElement, IConstraint aTemplateConstraint)
        {
            if (aTemplateConstraint.Children != null)
            {
                DocumentTemplateElement parsedElement = null;
                DocumentTemplateElementAttribute parsedAttribute = null;
                foreach (var child in aTemplateConstraint.Children)
                {
                    var cp = new ContextParser(child.Context);
                    cp.Parse(out parsedElement, out parsedAttribute);
                    if (parsedElement != null)
                    {
                        parsedElement.IsBranch = child.IsBranch;
                        parsedElement.IsBranchIdentifier = child.IsBranchIdentifier;
                    }
                    if (parsedAttribute != null) //we are only looking for attributes
                    {
                        parsedAttribute.SingleValue = child.Value;
                        aElement.AddAttribute(parsedAttribute);
                    }
                }
            }
        }
    }
}
