using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Generation.Schematron.Model;

namespace Trifolia.Generation.Schematron
{
    public class ContextBuilder
    {
        internal class Sentinels
        {
            public const string ATTRIBUTE_TOKEN = "%ATTRIBUTE%";
        }

        DocumentTemplateElement _element;
        DocumentTemplateElementAttribute _attribute;
        string _prefix;
        bool _isBranch;
        bool _includeLeafElement;

        public ContextBuilder(DocumentTemplateElement aElement, string prefix, bool aIsBranch = false, bool aIncludeLeafElement = false)
        {
            _element = aElement;
            _prefix = prefix;
            _isBranch = aIsBranch;
            _includeLeafElement = aIncludeLeafElement;
        }

        public ContextBuilder(DocumentTemplateElementAttribute aAttribute, string aNamespace)
        {
            _attribute = aAttribute;
            _prefix = aNamespace;
        }

        public static ContextBuilder CreateFromElementAndAttribute(DocumentTemplateElement aElement, DocumentTemplateElementAttribute aAttribute, string aNameSpace)
        {
            if (aAttribute != null)
                return new ContextBuilder(aAttribute, aNameSpace);
            if ( aElement != null)
                return new ContextBuilder(aElement, aNameSpace);

            throw new ArgumentException("Must provide a DocumentTemplate Element or an Attribute.");
        }

        /// <summary>
        /// Returns the context of the current element or attribute, but does not walk the parent tree to get the full context.
        /// For instance, given "cda:entryRelationship/cda:observation/cda:code", GetRelativeContextString() will return cda:code.
        /// </summary>
        /// <returns>
        /// Current element or attribute context string.
        /// </returns>
        public string GetRelativeContextString()
        {
            var sb = new StringBuilder();
            var targetElement = _attribute != null ? _attribute.Element : _element;
            if (targetElement != null)
            {
                if (targetElement.ElementName.IndexOf(':') < 0)
                    sb.AppendFormat("{0}:", this._prefix);

                sb.Append(targetElement.ElementName);

                if (_attribute != null)
                {
                    sb.AppendFormat("[@{0}='{1}']", _attribute.AttributeName, _attribute.SingleValue);
                }
            }


            return sb.ToString();
        }

        private string BuildSingleAttributeString(DocumentTemplateElementAttribute aAttribute)
        {
            if (string.IsNullOrEmpty(aAttribute.SingleValue))
            {
                return string.Format("[@{0}]", aAttribute.AttributeName);
            }
            else
            {
                return string.Format("[@{0}='{1}']", aAttribute.AttributeName, aAttribute.SingleValue);
            }
        }

        /// <summary>
        /// Iterate through the collection of attributes for an element and return the 
        /// path strings.
        /// </summary>
        /// <param name="aAttributes">
        /// Collection of attributes to iterate
        /// </param>
        /// <returns>
        /// Formatted string for all attributes in collection
        /// </returns>
        private string BuildStringForAttributes(IEnumerable<DocumentTemplateElementAttribute> aAttributes)
        {
            var sb = new StringBuilder();

            foreach (var attribute in aAttributes)
            {
                sb.Append(BuildSingleAttributeString(attribute));
            }

            return sb.ToString();
        }

        private string BuildSingleSiblingElementString(DocumentTemplateElement aElement)
        {
            var elementAndAttributeString = string.Format("{0}{1}", aElement.ElementName, BuildStringForAttributes(aElement.Attributes));

            if (!string.IsNullOrEmpty(elementAndAttributeString))
            {
                if (elementAndAttributeString.IndexOf(':') < 0)
                    return string.Format("[{0}:{1}]", _prefix, elementAndAttributeString);
                else if (!string.IsNullOrEmpty(elementAndAttributeString))
                    return string.Format("[{0}]", elementAndAttributeString);
            }

            return string.Empty;
        }

        private string BuildStringForSiblingElements(DocumentTemplateElement aElement)
        {
            var sb = new StringBuilder();

            if (aElement.ParentElement != null)
            {
                foreach (var sibling in aElement.ParentElement.ChildElements)
                {
                    if (sibling != aElement)
                        sb.Append(BuildSingleSiblingElementString(sibling));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the full context (parent tree included) of the current element or attribute.
        /// For instance, given "cda:entryRelationship/cda:observation/cda:code", GetFullyQualifiedContextString() will return cda:entryRelationship/cda:observation/cda:code.
        /// </summary>
        /// <returns></returns>
        public string GetFullyQualifiedContextString()
        {
            var sb = new StringBuilder();

            var targetElement = _attribute != null ? _attribute.Element : _element;

            if (targetElement != null)
            {
                DocumentTemplateElement current = targetElement;  //a branch context builder always starts from the leaf node so that sibling nodes can be added to path
                DocumentTemplateElement root = current;
                int maxloop = 10000;
                int i = 0;
                bool isBranchNodeWithSiblings = false;
                bool isEmptyBranch = false;

                //were we given an attribute? if so then use the attribute and advance the current to the next parent
                if ((current != null) && (this._attribute != null))
                {
                    sb.AppendFormat("[{0}/{1}]", current.ElementName, ContextBuilder.Sentinels.ATTRIBUTE_TOKEN);
                    current = current.ParentElement;
                }
                //walk the parent chain and build up string from leaf to root
                while ((current != null) && (!(current is DocumentTemplate)) && (i < maxloop))
                {
                    string elementString = current.ElementName;
                    
                    if (elementString.IndexOf(":") < 0)
                        elementString = _prefix + ":" + elementString;

                    var siblingElementString = BuildStringForSiblingElements(current);                    
                    var attributeString = BuildStringForAttributes(current.Attributes);
                    isBranchNodeWithSiblings = false;
                    isEmptyBranch = false;
                    if (_isBranch && current == _element) //is this the leaf node on a branch
                    {                        
                        if (!string.IsNullOrEmpty(siblingElementString))  //do we have siblings as part of the branch
                        {
                            isBranchNodeWithSiblings = true;
                            sb.Insert(0, siblingElementString + attributeString); //siblings will have all the nodes inclusive, no need to use current element
                        }
                        else if (!string.IsNullOrEmpty(attributeString)) //do we have attributes as part of the branch
                        {
                            sb.Insert(0, elementString + attributeString);  //attributes, just print element + attribute
                        }
                        else if (_includeLeafElement)
                        {
                            sb.Insert(0, elementString);
                        }
                        else
                        {
                            //no siblings, no attributes, it's empty
                            isEmptyBranch = true;
                        }
                    }
                    else
                    {
                        string currentString = elementString + attributeString;
                        if (current.ParentElement != null && _isBranch)
                        {
                            currentString = "[" + currentString + "]";
                        }
                        currentString += siblingElementString; 
                        sb.Insert(0, currentString);
                    }
                    root = current;  //set root as the last processed element
                    current = current.ParentElement;
                    if ((current != null) && (!isBranchNodeWithSiblings) && (!isEmptyBranch)
                        && !(current is DocumentTemplate))  //if this is not last element and not the first element of a branch and the branch was not empty
                    {
                        if (!current.IsBranchIdentifier && !current.IsBranch)  //if this is a branch identifier then we've wrapped it in a predicate "[..]"
                        {
                            sb.Insert(0, "/");
                        }
                    }
                    i++;
                }

                if (i >= maxloop)
                {
                    throw new InvalidOperationException(string.Format("Infinite loop detected on {0}", this._element.ElementName));
                }

            }

            if (_attribute != null)
            {
                if (sb.ToString().Contains(ContextBuilder.Sentinels.ATTRIBUTE_TOKEN))  //if we already have a placeholder for the attribute, then put it there
                {
                    sb.Replace(ContextBuilder.Sentinels.ATTRIBUTE_TOKEN, string.Format("@{0}='{1}'", _attribute.AttributeName, _attribute.SingleValue));
                }
                else 
                {
                    if (!string.IsNullOrEmpty(_attribute.SingleValue))
                    {
                        sb.AppendFormat("[@{0}='{1}']", _attribute.AttributeName, _attribute.SingleValue);
                    }
                    else
                    {
                        sb.AppendFormat("[@{0}]", _attribute.AttributeName);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
