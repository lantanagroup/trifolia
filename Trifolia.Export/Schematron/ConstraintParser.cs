using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using Trifolia.Export.Schematron.Model;
using Trifolia.Export.Schematron.ConstraintToDocumentElementMap;
using Trifolia.DB;

namespace Trifolia.Export.Schematron
{
    /// <summary>
    /// ConstraintParser takes a constraint and builds an AssertionLineBuilder. It knows how to map
    /// attributes, values, contexts, branches into the AssertionLineBuilder. 
    /// </summary>
    public class ConstraintParser
    {
        private IObjectRepository tdb;
        private IConstraint constraint;
        private string valueSetFile;
        private ValueSet constraintValueSet;
        private CodeSystem constraintCodeSystem;
        private Cardinality constraintCardinalityType = new Cardinality();
        private Conformance constraintConformanceType = new Conformance();
        private Template containedTemplate = null;
        private string prefix;
        private VocabularyOutputType vocabularyOutputType = VocabularyOutputType.Default;
        private IIGTypePlugin igTypePlugin = null;
        private ImplementationGuideType igType = null;

        public ConstraintParser(IObjectRepository tdb, IConstraint constraint, ImplementationGuideType igType, string valueSetFile = "voc.xml", VocabularyOutputType vocabularyOutputType = VocabularyOutputType.Default)
        {
            this.tdb = tdb;
            this.constraint = constraint;
            this.valueSetFile = valueSetFile;
            this.igType = igType;
            this.igTypePlugin = igType.GetPlugin();

            if (this.constraint.ValueSetId != null)
                this.constraintValueSet = this.tdb.ValueSets.Single(y => y.Id == constraint.ValueSetId);

            if (this.constraint.ValueCodeSystemId != null)
                this.constraintCodeSystem = this.tdb.CodeSystems.Single(y => y.Id == constraint.ValueCodeSystemId);

            if (!string.IsNullOrEmpty(this.constraint.Cardinality))
                this.constraintCardinalityType = CardinalityParser.Parse(this.constraint.Cardinality);

            if (!string.IsNullOrEmpty(this.constraint.Conformance))
                this.constraintConformanceType = ConformanceParser.Parse(this.constraint.Conformance);

            if (this.constraint.ContainedTemplateId != null)
                this.containedTemplate = this.tdb.Templates.Single(y => y.Id == this.constraint.ContainedTemplateId.Value);

            this.prefix = igType.SchemaPrefix;

            this.vocabularyOutputType = vocabularyOutputType;
        }

        #region Private Helper Methods

        /// <summary>
        /// Helper function which creates an AssertionLineBuilder for an element given the constraint. This function will examine the constraint's value and datatype
        /// and add those to the builder if necessary. Also if aGenerateContext == true then it will add the context of the element.
        /// </summary>
        /// <param name="aElement">Element to base the AssertionLineBuilder from</param>
        /// <param name="aTemplateConstraint">TemplateConstraint which has the necessary properties (e.g. Element Value, Data Type) to add to the AssertionLineBuilder</param>
        /// <param name="aGenerateContext">Flag to determine whether a context should be added as part of the AssertionLineBuilder</param>
        /// <returns>A new AssertionLineBuilder for the aElement passed in</returns>
        private AssertionLineBuilder CreateAssertionLineBuilderForElement(DocumentTemplateElement aElement, IConstraint aTemplateConstraint, ref string aParentContext, bool aGenerateContext = true)
        {
            //add the value and data type
            ConstraintToDocumentElementHelper.AddElementValueAndDataType(this.prefix, aElement, aTemplateConstraint);

            //create builders
            var builder = new AssertionLineBuilder(aElement, this.igType);

            if (aGenerateContext)
            {
                ContextBuilder contextBuilder = null;

                if (aElement.ParentElement != null) //build the context
                {
                    contextBuilder = new ContextBuilder(aElement.ParentElement, this.prefix); //the context will start from the first parent (or root)
                    builder.WithinContext(string.Format("{0}", contextBuilder.GetFullyQualifiedContextString()));
                }
            }

            if (aTemplateConstraint.ContainedTemplateId != null)
            {
                var containedTemplate = this.tdb.Templates.Single(y => y.Id == aTemplateConstraint.ContainedTemplateId.Value);
                if (containedTemplate != null)
                    builder.ContainsTemplate(containedTemplate.Oid);

                if (aTemplateConstraint.Parent != null && aTemplateConstraint.Parent.IsBranch)
                {
                    builder.WithinContext(aParentContext, ContextWrapper.Slash); //put the parent context into the element context, this is a special case where we want the full context within the template assertion
                    aParentContext = string.Empty; //clear the parent context b/c we have put it within the element's context
                }
            }

            if (aTemplateConstraint.Parent != null)
            {
                Conformance conformance = ConformanceParser.Parse(aTemplateConstraint.Parent.Conformance);
                if (conformance == Conformance.SHOULD)
                    builder.HasOptionalParentContext();
            }

            //TODO: Refactor this out, hardcoding these special cases for QRDA
            if (this.prefix.ToLower().Contains("cda"))
            {
                aElement.ElementToAttributeOverrideMapping.Add("code", "code");
                aElement.ElementToAttributeOverrideMapping.Add("statusCode", "code");
                aElement.ElementToAttributeOverrideMapping.Add("realmCode", "code");
                aElement.ElementToAttributeOverrideMapping.Add("externalDocument", "classCode");
            }

            return builder;
        }

        private AssertionLineBuilder CreateBranchedRootAssertionLineBuilderFromConstraint(IConstraint aConstraint)
        {
            AssertionLineBuilder asb = null;
            DocumentTemplateElement element = null;
            DocumentTemplateElementAttribute attribute = null;
            //parse the context
            ContextParser contextParser = new ContextParser(aConstraint.Context);
            contextParser.Parse(out element, out attribute);

            if (element != null)
            {
                asb = new AssertionLineBuilder(element, this.igType);
                if (aConstraint.ContainedTemplateId != null)
                {
                    var containedTemplate = this.tdb.Templates.Single(y => y.Id == aConstraint.ContainedTemplateId.Value);
                    if (containedTemplate != null)
                        asb.ContainsTemplate(containedTemplate.Oid, containedTemplate.PrimaryContext);
                }
            }
            else if (attribute != null)
            {
                if (!string.IsNullOrEmpty(aConstraint.Value))
                {
                    attribute.SingleValue = aConstraint.Value;
                }
                asb = new AssertionLineBuilder(attribute, this.igType);
            }
            else
            {
                throw new Exception();
            }
            ConstraintToDocumentElementHelper.AddCardinality(aConstraint, asb);
            ConstraintToDocumentElementHelper.AddConformance(aConstraint, asb);

            foreach (var child in aConstraint.Children)
            {
                DocumentTemplateElement childElement = null;
                DocumentTemplateElementAttribute childAttribute = null;
                ContextParser childContextParser = new ContextParser(child.Context);
                childContextParser.Parse(out childElement, out childAttribute);

                if (child.IsBranchIdentifier)
                {
                    if (childElement != null)
                    {
                        asb.WithChildElementBuilder(CreateBranchedRootAssertionLineBuilderFromConstraint(child));
                    }
                    else if (childAttribute != null)
                    {
                        if (!string.IsNullOrEmpty(child.Value))
                        {
                            childAttribute.SingleValue = child.Value;
                        }
                        element.AddAttribute(childAttribute);
                    }
                }
            }

            if (aConstraint.IsBranch)
                asb.MarkAsBranchRoot();

            return asb;
        }

        private string CreateParentContextForElement(DocumentTemplateElement aParentElement, DocumentTemplateElement aElement, IConstraint aConstraint)
        {
            string parentContext = string.Empty;
            if (aConstraint.Parent != null && !aConstraint.Parent.IsBranch)
            {
                parentContext = ConstraintToDocumentElementMapper.CreateFullParentContext(this.prefix, aConstraint);
            }
            return parentContext;
        }

        private void DecorateAttributeFromConstraint(string aParentContext, DocumentTemplateElementAttribute aAttribute, IConstraint aConstraint, ValueSet aConstraintValueSet)
        {
            // Set the attribute's value if the constraint indicates one
            if (!string.IsNullOrEmpty(aConstraint.Value))
                aAttribute.SingleValue = !string.IsNullOrEmpty(aConstraint.Value) ? aConstraint.Value.Trim() : string.Empty;

            // Set the data-type for the attribute if one is present in the constraint
            if (!string.IsNullOrEmpty(aConstraint.DataType))
            {
                aAttribute.DataType = aConstraint.DataType;
            }

            // Do we have a valueset on this attribute?
            if (aConstraintValueSet != null)
                aAttribute.ValueSet = this.igTypePlugin.ParseIdentifier(this.constraintValueSet.Oid);

            if (this.constraintCodeSystem != null)
            {
                //TODO: CDA specific logic, need to refactor this out to make more dynamic
                if ((aAttribute.AttributeName == "code" || aAttribute.AttributeName == "value") && (aParentContext.EndsWith(this.prefix + ":code") || aParentContext.EndsWith(this.prefix + ":value")))
                    aAttribute.CodeSystemOid = this.igTypePlugin.ParseIdentifier(this.constraintCodeSystem.Oid);
            }
        }

        private AssertionLineBuilder CreateAssertionLineBuilderForAttribute(DocumentTemplateElement aParentElement, DocumentTemplateElementAttribute aAttribute, ref string aParentContext, ref IConstraint aConstraint)
        {
            AssertionLineBuilder asb = null;
            if ((aParentElement != null) && (aConstraint.Parent.IsBranch))
            {
                asb = CreateAssertionLineBuilderForElement(aParentElement, aConstraint.Parent, ref aParentContext);
                aConstraint = aConstraint.Parent;
            }
            else
            {
                asb = new AssertionLineBuilder(aAttribute, this.igType);

                if (aConstraint.Parent != null)
                {
                    Cardinality cardinality = CardinalityParser.Parse(aConstraint.Parent.Cardinality);
                    if (cardinality.Left == 0)
                        asb.HasOptionalParentContext();
                }
            }

            return asb;
        }

        private void DecorateParentOptionality(AssertionLineBuilder aAssertionLineBuilder, IConstraint aConstraint)
        {
            Cardinality cardinality = CardinalityParser.Parse(aConstraint.Cardinality);
            if (aConstraint.Parent != null)
            {
                var current = aConstraint.Parent;
                while (current != null)
                {
                    cardinality = CardinalityParser.Parse(current.Cardinality);

                    if (cardinality.Left == 0)
                    {
                        aAssertionLineBuilder.HasOptionalParentContext();
                        break;
                    }

                    current = current.Parent;
                }
            }
        }
        #endregion

        /// <summary>
        /// This is the main public interface for constraint parser. Takes the given constraint and builds an AssertionLineBuilder, applying the values, attributes, context, etc as necessary.
        /// </summary>
        /// <returns>
        /// AssertionLineBuilder representing the values from the constraint given to the parser.
        /// </returns>
        public AssertionLineBuilder CreateAssertionLineBuilder()
        {
            IConstraint currentConstraint = this.constraint; //set current constraint, setting this as a variable allows us to move the current constraint to the parent when dealing with branches
            if (string.IsNullOrEmpty(currentConstraint.Context) 
                && (currentConstraint.ContainedTemplateId == null)) //we can have empty context but a contained template  
                return null;

            DocumentTemplateElement element = null;
            DocumentTemplateElementAttribute attribute = null;

            ConstraintToDocumentElementHelper.ParseContextForElementAndAttribute(currentConstraint, out element, out attribute);
            DocumentTemplateElement parentElement =
                ConstraintToDocumentElementHelper.CreateParentElementForAttribute(currentConstraint, attribute);
            
            string parentContext = 
                CreateParentContextForElement(parentElement, element, currentConstraint);

            AssertionLineBuilder asb = null;
            AssertionLineBuilder branchedRootAsb = null;

            // Determine if we should create the AssertionLineBuilder starting with an attribute or an element.
            if (attribute != null)
            {
                DecorateAttributeFromConstraint(parentContext, attribute, currentConstraint, this.constraintValueSet);
                if (currentConstraint.Parent != null && currentConstraint.IsBranch)
                {
                    branchedRootAsb = CreateBranchedRootAssertionLineBuilderFromConstraint(currentConstraint);                    
                }
                else
                {
                    asb = CreateAssertionLineBuilderForAttribute(parentElement, attribute, ref parentContext, ref currentConstraint);
                }
            }
            else //this is an element
            {
                ConstraintToDocumentElementHelper.AddCodeSystemToElement(this.tdb, this.igTypePlugin, element, currentConstraint);

                if (currentConstraint.IsBranch)
                {
                    branchedRootAsb = CreateBranchedRootAssertionLineBuilderFromConstraint(currentConstraint);
                    branchedRootAsb.HasParentContext(parentContext);
                }
                else
                {
                    //if we have a context already then we will append it at end so pass in false, else go ahead and generate (pass in true)
                    asb = CreateAssertionLineBuilderForElement(element, this.constraint, ref parentContext, string.IsNullOrEmpty(parentContext)); 
                }
            }

            if (branchedRootAsb == null) //if this is a branched root then a separate builder was constructed
            {
                ConstraintToDocumentElementHelper.AddConformance(currentConstraint, asb);
                ConstraintToDocumentElementHelper.AddCardinality(currentConstraint, asb);
                // Determine if we have a valueset
                if (this.constraintValueSet != null && currentConstraint.IsValueSetStatic)
                {
                    var requireValueset = true;

                    if (!string.IsNullOrEmpty(currentConstraint.ValueConformance))
                    {
                        if (ConformanceParser.Parse(currentConstraint.Conformance) != ConformanceParser.Parse(currentConstraint.ValueConformance))
                        {
                            requireValueset = false;
                        }
                    }

                    if (requireValueset)
                    {
                        //TODO: Move into CDA specific library
                        //are we bound directly to a code or value element?
                        bool includeNullFlavor = false;
                        if (element != null && attribute == null && (element.ElementName == "value" || element.ElementName == "code"))
                        {
                            includeNullFlavor = true;
                        }

                        string valueSetIdentifier = this.igTypePlugin.ParseIdentifier(this.constraintValueSet.Oid);
                        asb.WithinValueSet(valueSetIdentifier, this.valueSetFile, this.vocabularyOutputType, includeNullFlavor);
                    }
                }

                //determine if we have a parent context
                if (!string.IsNullOrEmpty(parentContext))
                {
                    asb.HasParentContext(parentContext);
                    DecorateParentOptionality(asb, currentConstraint);
                }
            }
            else //branched root, use that one instead
            {
                //determine if we have a parent context
                if (!string.IsNullOrEmpty(parentContext))
                {
                    branchedRootAsb.HasParentContext(parentContext);
                    DecorateParentOptionality(branchedRootAsb, currentConstraint);
                }
            }

            return (branchedRootAsb == null) ? asb : branchedRootAsb;
        }
    }
}
