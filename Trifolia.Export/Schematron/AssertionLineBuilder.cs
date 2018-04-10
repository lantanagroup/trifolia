using System;
using System.Collections.Generic;
using System.Text;
using Trifolia.DB;
using Trifolia.Export.Schematron.Model;
using Trifolia.Shared;

namespace Trifolia.Export.Schematron
{
    public enum ContextWrapper { Bracket, Slash };

    public class AssertionLineBuilder
    {
        #region internal Sentinel Strings
        internal class Sentinels
        {
            public const string CONTEXT_TOKEN                  = "%CONTEXT%";
            public const string ELEMENT_TOKEN                  = "%ELEMENT%";
            public const string ATTRIBUTE_TOKEN                = "%ATTRIBUTE%";
            public const string ATTRIBUTE_IMMUTABLE_TOKEN      = "%ATTRIBUTE_IM%";
            public const string CHILDELEMENT_TOKEN             = "%CHILDELEMENT%";
            public const string VALUESET_TOKEN                 = "%VALUESET%";
            public const string CODE_ATTRIBUTE_NAME            = "code";
        }
        #endregion

        #region Private Member Vars
        DocumentTemplateElement _element;
        DocumentTemplateElementAttribute _attribute;
        Cardinality _cardinality;
        Conformance _conformance;
        string _context;
        ContextWrapper _contextWrapper = ContextWrapper.Bracket;
        string _valueSetOid;
        string _valueSetFileName;
        Dictionary<string, string> containedTemplates = new Dictionary<string, string>();       // Identifier, PrimaryContextType
        string _prefix;
        string _parentContext;
        ImplementationGuideType _igType;
        Dictionary<DocumentTemplateElement, AssertionLineBuilder> _childElementAssertionBuilders = new Dictionary<DocumentTemplateElement, AssertionLineBuilder>();
        bool _isBranchRoot;
        bool _outputPathOnly;
        bool _hasOptionalParentContext;
        VocabularyOutputType _vocabularyOutputType;
        bool _includeValueSetNullFlavor = false;
        IObjectRepository _tdb = null;
        private SimpleSchema _igTypeSchema;

        #endregion

        #region ctor
        protected AssertionLineBuilder(IObjectRepository tdb, ImplementationGuideType igType, SimpleSchema igTypeSchema, string prefix)
        {
            this._igTypeSchema = igTypeSchema;
            this._tdb = tdb;
            this._prefix = prefix;
            this._igType = igType;
        }

        public AssertionLineBuilder(IObjectRepository tdb, DocumentTemplateElement aElement, ImplementationGuideType igType, SimpleSchema igTypeSchema, string prefix = null)
            : this(tdb, igType, igTypeSchema, igType.SchemaPrefix)
        {
            _element = aElement;

            if (!string.IsNullOrEmpty(prefix))
                this._prefix = prefix;
        }

        public AssertionLineBuilder(IObjectRepository tdb, DocumentTemplateElementAttribute aAttribute, ImplementationGuideType igType, SimpleSchema igTypeSchema, string prefix = null)
            : this(tdb, igType, igTypeSchema, igType.SchemaPrefix)
        {
            _attribute = aAttribute;
            _element = _attribute.Element;

            if (!string.IsNullOrEmpty(prefix))
                this._prefix = prefix;
        }
        #endregion

        #region Public Properties
        public bool IsOutputPathOnly
        {
            get
            {
                return _outputPathOnly;
            }
        }
        #endregion

        #region Public Builder Methods

        public AssertionLineBuilder WithinContext(string aContext, ContextWrapper aWrapper = ContextWrapper.Bracket)
        {
            _context = aContext;
            _contextWrapper = aWrapper;
            return this;
        }

        public AssertionLineBuilder WithinContext(DocumentTemplateElement aContextElement, ContextWrapper aWrapper = ContextWrapper.Bracket)
        {
            var contextBuilder = new ContextBuilder(aContextElement, this._prefix);
            _context = contextBuilder.ToString();
            _contextWrapper = aWrapper;
            return this;
        }

        public AssertionLineBuilder HasParentContext(string aParentContext)
        {
            _parentContext = aParentContext;
            return this;
        }

        public AssertionLineBuilder WithCardinality(Cardinality aCardinality)
        {
            _cardinality = aCardinality;
            return this;
        }

        public AssertionLineBuilder ConformsTo(Conformance aConformance)
        {
            _conformance = aConformance;
            return this;
        }

        public AssertionLineBuilder WithChildElementBuilder(AssertionLineBuilder aAssertionLineBuilder)
        {
            this._childElementAssertionBuilders.Add(aAssertionLineBuilder._element, aAssertionLineBuilder);
            return this;
        }

        public AssertionLineBuilder WithinValueSet(string aValueSetOid, VocabularyOutputType aOutputType = VocabularyOutputType.Default)
        {
            return WithinValueSet(aValueSetOid, "the_voc.xml", aOutputType);
        }

        public AssertionLineBuilder WithinValueSet(string aValueSetOid, string aValueSetFileName, VocabularyOutputType aOutputType = VocabularyOutputType.Default, Boolean aIncludeNullFlavor = false)
        {
            _valueSetOid = aValueSetOid;
            _valueSetFileName = aValueSetFileName;
            _vocabularyOutputType = aOutputType;
            _includeValueSetNullFlavor = aIncludeNullFlavor;
            return this;
        }

        public AssertionLineBuilder ContainsTemplate(string identifier, string primaryContextType = null)
        {
            this.containedTemplates.Add(identifier, primaryContextType);
            return this;
        }

        public AssertionLineBuilder MarkAsBranchRoot()
        {
            _isBranchRoot = true;
            return this;
        }

        public AssertionLineBuilder MarkAsNotBranchRoot()
        {
            _isBranchRoot = false;
            return this;
        }

        public AssertionLineBuilder OutputPathOnly()
        {
            _outputPathOnly = true;
            return this;
        }

        public AssertionLineBuilder HasOptionalParentContext()
        {
            _hasOptionalParentContext = true;
            return this;
        }

        #endregion

        #region Private Utility Functions

        private void BuildCardinalityAndConformance(StringBuilder sb)
        {
            if (_cardinality != null)
            {
                string op = "=";

                if (_cardinality.Right != Cardinality.MANY)
                {
                    var rhs = _cardinality.Right;
                    if ((_cardinality.Left == 0) && (!_cardinality.IsZeroToZero()))
                    {
                        op = " < ";
                        rhs++; //right-hand side (rhs) needs to be increment b/c we are doing a "less than", e.g. less than 2 is 0 and 1, however _cardinality.Right will be 1
                    }
                    else if (_cardinality.IsZeroToZero())
                    {
                        op = "=";
                        rhs = 0;
                    }

                    if ((_attribute != null) && (_attribute.GetNumberOfValuesDefined() <= 1)) //1 to 1, or 0 to 1, on an attribute, just give the name, no need for count. if an attribute has more than one value then it'll be prepended with current()
                    {
                        //cardinality on attribute could only be 0..0, 0..1, or 1..1
                        if (_cardinality.Right == 1) 
                        {
                            if (_cardinality.Left == 0)  //0..1
                            {
                                if (_attribute != null && _hasOptionalParentContext)
                                {
                                    if ((_conformance == Conformance.SHOULD) || (_conformance == Conformance.MAY))
                                        sb.AppendFormat("not({0}{1}{2}{3}{4}) or {0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN,
                                            Sentinels.ATTRIBUTE_TOKEN, Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                                    else
                                        sb.AppendFormat("{0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN,
                                            Sentinels.ATTRIBUTE_TOKEN, Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                                }
                                else
                                {
                                    sb.AppendFormat("not({0}{1}{5}) or {0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN,
                                        Sentinels.ATTRIBUTE_TOKEN, Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN, Sentinels.ATTRIBUTE_IMMUTABLE_TOKEN);
                                }
                            }
                            else //1..1
                            {
                                sb.AppendFormat("{0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, 
                                    Sentinels.ATTRIBUTE_TOKEN, Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                            }
                        }
                        else if (_cardinality.IsZeroToZero()) //0..0
                        {
                            sb.AppendFormat("not({0}{1}{2}{3}{4})", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, 
                                Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                        }
                        else  //unexpected cardinality, but database could have it as a bad value so output valid xpath
                        {
                            sb.AppendFormat("{0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, 
                                Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                        }
                    }
                    else
                    {
                        if (_cardinality.IsOneToOne() || (_cardinality.Left == 0))
                        {
                            if (_outputPathOnly)
                            {
                                sb.AppendFormat("{0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, 
                                    Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                            }
                            else
                            {
                                sb.AppendFormat("count({0}{1}{2}{3}{4}){5}{6}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, 
                                    Sentinels.ATTRIBUTE_TOKEN, Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN, op, rhs);
                            }
                        }
                        else
                        {
                            if (_outputPathOnly)
                            {
                                sb.AppendFormat("{0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, 
                                    Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                            }
                            else
                            {
                                sb.AppendFormat("count({0}{1}{2}{3}{4})[. >= {5}] <= {6}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, 
                                    Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN, _cardinality.Left, _cardinality.Right);
                            }
                        }
                    }
                }
                else
                {
                    if (_cardinality.IsZeroToMany())
                    {
                        if (_isBranchRoot)
                        {
                            sb.AppendFormat("not({0}{1}{2}{3}{4}) or {0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, 
                                Sentinels.ATTRIBUTE_TOKEN, Sentinels.VALUESET_TOKEN, Sentinels.CHILDELEMENT_TOKEN);
                        }
                        else
                        {
                            sb.AppendFormat("not({0}{1}) or {0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, 
                                Sentinels.ATTRIBUTE_TOKEN, Sentinels.VALUESET_TOKEN, Sentinels.CHILDELEMENT_TOKEN);
                        }
                    }
                    else
                    {
                        if (_cardinality.IsOneToMany())
                        {
                            if (_outputPathOnly)
                            {
                                sb.AppendFormat("{0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                            }
                            else
                            {
                                sb.AppendFormat("count({0}{1}{2}{3}{4}) > 0", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                            }
                        }
                        else
                        {
                            if (_outputPathOnly)
                            {
                                sb.AppendFormat("{0}{1}{2}{3}{4}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, 
                                    Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN);
                            }
                            else
                            {
                                sb.AppendFormat("count({0}{1}{2}{3}{4}){5}", Sentinels.CONTEXT_TOKEN, Sentinels.ELEMENT_TOKEN, Sentinels.ATTRIBUTE_TOKEN, 
                                    Sentinels.CHILDELEMENT_TOKEN, Sentinels.VALUESET_TOKEN, op);
                            }
                        }
                    }

                }

                if (((_conformance == Conformance.SHALL_NOT) || 
                    (_conformance == Conformance.SHOULD_NOT) ||
                    (_conformance == Conformance.MAY_NOT)) && (!_cardinality.IsZeroToMany()) && (!_cardinality.IsZeroToZero()))
                {
                    sb.Insert(0, "not(");
                    sb.Append(")");
                }

            }
        }

        private bool IsCodeAttribute()
        {
            return (_attribute != null) && (_attribute.AttributeName.ToLower() == Sentinels.CODE_ATTRIBUTE_NAME);
        }

        private bool IsValueSetCode()
        {
            return !string.IsNullOrEmpty(_valueSetOid);
        }

        private bool ZeroAttributes()
        {
            bool attributeAttachedToElement = (_element != null) && (_element.Attributes.Count == 0);
            return attributeAttachedToElement && (_attribute == null);  
        }

        private string GetValueSetDocumentStringForOutputType()
        {
            switch (_vocabularyOutputType )
            {
                case VocabularyOutputType.Default:
                    return string.Format(@"document('{0}')/voc:systems/voc:system[@valueSetOid='{1}']/voc:code/@value", _valueSetFileName, _valueSetOid);
                case VocabularyOutputType.SVS:
                    return string.Format(@"document('{0}')/svs:RetrieveMultipleValueSetsResponse/svs:DescribedValueSet[@ID='{1}']/svs:ConceptList/svs:Concept/@code", _valueSetFileName, _valueSetOid);
                case VocabularyOutputType.SVS_SingleValueSet:
                    return string.Format(@"document('{0}.xml')/svs:RetrieveValueSetResponse/svs:ValueSet[@id='{0}']/svs:ConceptList/svs:Concept/@code", _valueSetOid);
                default:
                    return string.Format(@"document('{0}')/voc:systems/voc:system[@valueSetOid='{1}']/voc:code/@value", _valueSetFileName, _valueSetOid);
            }
        }

        private string GetValueSetNullFlavor()
        {
            string nullFlavor = "";

            if (_includeValueSetNullFlavor)
            {
                nullFlavor = " or @nullFlavor";
            }

            return nullFlavor;
        }

        private string CreateValueSetForElementString()
        {
            var valueSetString = string.Empty;
            if (!string.IsNullOrEmpty(_valueSetOid))
            {
                valueSetString = "@code=" + GetValueSetDocumentStringForOutputType() + GetValueSetNullFlavor();
                if ((!_cardinality.IsZeroToMany() && !IsCodeAttribute() && !ZeroAttributes())  //not a zero to many cardinality, isn't a code attribute by itself, and has attributes, put in an 'and'
                        || (_cardinality.IsZeroToMany() && IsValueSetCode() && !ZeroAttributes()))  //is a zero to many cardinality, is a value set code, and has attributes, put in an 'and'
                {
                    valueSetString = valueSetString.Insert(0, " and ");
                }
            }
            return valueSetString;
        }

        private void BuildElementContext(StringBuilder sb)
        {
            //add context to string
            var context = this.GetFormattedContext();

            if (!string.IsNullOrEmpty(context) && this.IsComplexContext(context))
            {
                sb.Replace(Sentinels.CONTEXT_TOKEN, context);
                if (!_cardinality.IsZeroToMany())  //no need to put brackets b/c if it's zero to many then the brackets are inserted in the cardinality function
                {
                    var prefixWrapper = "[";
                    var postfixWrapper = "]";
                    if (_contextWrapper == ContextWrapper.Slash)
                    {
                        prefixWrapper = "/";
                        postfixWrapper = string.Empty;
                    }
                    sb.Replace(Sentinels.ELEMENT_TOKEN, string.Format("{0}{1}", prefixWrapper, Sentinels.ELEMENT_TOKEN));
                    sb.Replace(Sentinels.VALUESET_TOKEN, string.Format("{0}{1}", Sentinels.VALUESET_TOKEN, postfixWrapper));
                }
            }
            else
            {
                sb.Replace(Sentinels.CONTEXT_TOKEN, string.Empty);
            }
        }

        private void AddAttributesToElement(StringBuilder sb, ref string valueSetString)
        {
            //examine attributes to add to the string
            if ((_element.Attributes.Count > 0) || (!string.IsNullOrEmpty(_valueSetOid))) //if we have an attribute OR we have a valueset oid (which implies an @code attribute)
            {
                var sbAttribute = new StringBuilder();
                if (_attribute == null)  //attribute is null, so we are not looking for specific attribute test, looking for all attributes on the element
                {
                    foreach (var attr in _element.Attributes)
                    {
                        var addlString = string.Empty;
                        if (!string.IsNullOrEmpty(valueSetString))
                        {
                            addlString = valueSetString;
                            valueSetString = string.Empty; //clear the string b/c we've used it
                        }
                        sbAttribute.Append(attr.GetAssertionStringIdentifier(addlString));
                    }
                }
                else
                {
                    if (!IsValueSetCode()) //if we have only @code and we have a valueset oid then the @code will be handled by the valueset string
                    {
                        //only generate test for a specific attribte on this element
                        sbAttribute.Append(_attribute.GetAssertionStringIdentifier());
                    }
                }
                sb.Replace(Sentinels.ATTRIBUTE_TOKEN, sbAttribute.ToString());
                sb.Replace(Sentinels.ATTRIBUTE_IMMUTABLE_TOKEN, sbAttribute.ToString()); 
            }
            else
            {
                sb.Replace(Sentinels.ATTRIBUTE_TOKEN, string.Empty);
                sb.Replace(Sentinels.ATTRIBUTE_IMMUTABLE_TOKEN, string.Empty);
            }
        }

        private void CreateAndPopulateElementAssertionString(StringBuilder sb)
        {
            //generate the element assertion
            var elementAssert = _element.GetAssertionStringIdentifier();

            if (!string.IsNullOrEmpty(elementAssert))
                elementAssert = this.GetFormattedPrefix() + elementAssert;
            else if (string.IsNullOrEmpty(elementAssert) && this.containedTemplates.Count > 0)
                elementAssert = "*";

            List<string> containedTemplateContexts = new List<string>();

            foreach (var containedTemplate in this.containedTemplates)
            {
                TemplateContextBuilder tcb = new TemplateContextBuilder(this._tdb, this._igType, this._igTypeSchema, this._prefix);
                string containedTemplateContext = tcb.BuildContextString(containedTemplate.Key);
                containedTemplateContexts.Add(containedTemplateContext);
            }

            if (containedTemplateContexts.Count == 1)
                elementAssert += "[" + containedTemplateContexts[0] + "]";
            else if (containedTemplateContexts.Count > 1)
                elementAssert += "[(" + string.Join(") or (", containedTemplateContexts) + ")]";

            sb.Replace(Sentinels.ELEMENT_TOKEN, elementAssert);
        }

        private void AddChildAssertions(StringBuilder sb)
        {
            //do we have child element assertions?
            if (_childElementAssertionBuilders.Count > 0)
            {
                var sbChildElementAssertion = new StringBuilder();

                foreach (var builder in _childElementAssertionBuilders.Values)
                {
                    if (builder.IsOutputPathOnly)
                    {
                        sbChildElementAssertion.Append("/" + builder.ToString() );
                    }
                    else
                    {
                        sbChildElementAssertion.Append("[" + builder.ToString() + "]");
                    }
                }
                sb.Replace(Sentinels.CHILDELEMENT_TOKEN, sbChildElementAssertion.ToString());
            }
            else
            {
                //clear the placeholder, no child element assertions
                sb.Replace(Sentinels.CHILDELEMENT_TOKEN, string.Empty);
            }
        }

        private void AddValueSet(StringBuilder sb, string valueSetString)
        {
            //if we didn't use the valueSetString already and we have a valueSetString then use it now
            if (!string.IsNullOrEmpty(valueSetString))
            {
                if ((_cardinality.IsZeroToMany() && ZeroAttributes()) || (IsValueSetCode() || ZeroAttributes()))   //passed in the @code with a valueset
                {
                    valueSetString = string.Format("[{0}]", valueSetString);
                }

                sb.Replace(Sentinels.VALUESET_TOKEN, valueSetString);
            }
            else
            {
                sb.Replace(Sentinels.VALUESET_TOKEN, string.Empty);
            }            
        }

        private void BuildElement(StringBuilder sb)
        {
            var valueSetString = CreateValueSetForElementString();

            BuildElementContext(sb);
            
            AddAttributesToElement(sb, ref valueSetString);
            
            CreateAndPopulateElementAssertionString(sb);
            
            AddChildAssertions(sb);

            AddValueSet(sb, valueSetString);
        }

        private void BuildAttribute(StringBuilder sb)
        {
            bool forceBrackets = false;
            var valueSetString = string.Empty;
            // do we have a value set?
            if (!string.IsNullOrEmpty(_valueSetOid))
            {
                valueSetString = GetValueSetDocumentStringForOutputType();
                //if zero to many then we have another statement preceding this one to check for the count > 0
                if (!_cardinality.IsZeroToMany())
                {
                    if (_cardinality.IsOneToOne())
                    {
                        valueSetString = valueSetString.Insert(0, string.Format(" and @{0}=", _attribute.AttributeName));
                        forceBrackets = !string.IsNullOrEmpty(_context) || !string.IsNullOrEmpty(_parentContext);
                    }
                    else
                    {
                        valueSetString = valueSetString.Insert(0, string.Format("=", _attribute.AttributeName));
                        if (!string.IsNullOrEmpty(_parentContext))
                        {
                            sb.Insert(0, "[");
                            sb.Append("]");
                        }
                    }
                }
            }
            //we don't use the token, pass it in through the addlString param of the Attribute.GetAssertionStringIdentifier method
            sb.Replace(Sentinels.VALUESET_TOKEN, string.Empty);
            //attributes can't have child elements
            sb.Replace(Sentinels.CHILDELEMENT_TOKEN, string.Empty);
            //only generating attribute, don't need element
            sb.Replace(Sentinels.ELEMENT_TOKEN, string.Empty);
            //replace attribute placeholder with the attribute's assertion
            sb.Replace(Sentinels.ATTRIBUTE_TOKEN, _attribute.GetAssertionStringIdentifier(valueSetString, forceBrackets)); //allow the attribute string to be mutated with valueset
            //do we have a context that is optional? if so then put the context into the attribute immutable space
            sb.Replace(Sentinels.ATTRIBUTE_IMMUTABLE_TOKEN, _attribute.GetAssertionStringIdentifier()); //do not allow the attribute string to be mutated
            //check to see if we only have a single value and a data type
            var currentFunction = string.Empty;
            if (!string.IsNullOrEmpty(_attribute.SingleValue) && !string.IsNullOrEmpty(_attribute.DataType)) //we have a datatype and a single value, we infer it's an element, use current() function of XPATH
            {
                currentFunction = "self::node()"; //we need to be sure that the element is specified using the current function
            }

            //add context to string
            sb.Replace(Sentinels.CONTEXT_TOKEN, _context + currentFunction);
        }

        private string GetFormattedContext()
        {
            if (string.IsNullOrEmpty(_context))
                return string.Empty;

            if (string.IsNullOrEmpty(_prefix))
                return this._context;

            string[] levels = this._context.Split('/');
            List<string> newLevels = new List<string>();

            foreach (string cLevel in levels)
            {
                if (string.IsNullOrEmpty(cLevel))
                    continue;

                if (cLevel.Contains(":"))
                    newLevels.Add(cLevel);
                else
                    newLevels.Add(this.GetFormattedPrefix() + cLevel);
            }

            if (newLevels.Count == 0)
                return string.Empty;

            return string.Join("/", newLevels);
        }

        private string GetFormattedPrefix()
        {
            if (this._element.ElementName.Contains(":")) //we already have prefix in the name
            {
                return "";
            }

            if (string.IsNullOrEmpty(this._prefix))
                return string.Empty;

            if (this._prefix.EndsWith(":"))
                return this._prefix;

            return this._prefix + ":";
        }

        /// <summary>
        /// Determines whether the context passed in is complex (in other words, it has parents or a namespace)
        /// </summary>
        /// <param name="aContext">
        /// Context to evaluate
        /// </param>
        /// <returns>
        /// true if the context is considered to be complex (i.e. has parents or has a namespace), false otherwise.
        /// </returns>
        private bool IsComplexContext(string aContext)
        {
            return aContext.Split('/').Length > 1 || !aContext.EndsWith(":");
        }

        private void AddParentContext(StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(sb.ToString()) && !string.IsNullOrEmpty(_parentContext))
            {
                var prefix = "[";
                var postfix = "]";
                if ((_attribute != null && !string.IsNullOrEmpty(_valueSetOid))
                    || (sb.ToString().StartsWith("[")))
                {
                    prefix = string.Empty;
                    postfix = string.Empty;
                }

                if (_parentContext.Contains(":")) //already has the prefix
                {
                    sb.Insert(0, string.Format("{0}{1}", _parentContext, prefix));
                }
                else
                {
                    sb.Insert(0, string.Format("{0}:{1}{2}", _prefix, _parentContext, prefix));
                }
                sb.Append(postfix);

                /*if ((_hasOptionalParentContext && (_cardinality.Left == 1 || _cardinality.Left == Cardinality.MANY))
                    || (_hasOptionalParentContext && _attribute != null && _cardinality.Left == 0))*/
                if (_hasOptionalParentContext)
                {
                    sb.Insert(0, string.Format("not({0}) or ", _parentContext));
                }
            }
        }

        #endregion

        #region Overrides

        public void OverrideShouldZeroToAnything()
        {
            if ((_conformance == Conformance.SHOULD || _conformance == Conformance.SHOULD_NOT)
                 && (_cardinality.Left == 0 && _cardinality.Right > 0))
            {
                _cardinality.Left = 1;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            //special fix per Rick, 0 to anything should be treated as 1 to anything if it's a SHOULD
            OverrideShouldZeroToAnything();

            BuildCardinalityAndConformance(sb);

            if (_element != null)
            {
                BuildElement(sb);
            }
            else if (_attribute != null)
            {
                BuildAttribute(sb);
            }

            AddParentContext(sb);

            return sb.ToString();
        }
        #endregion

    }
}
