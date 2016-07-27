using System.Linq;
using Trifolia.DB;
using Trifolia.Generation.Schematron.Model;
using Trifolia.Shared.Plugins;

namespace Trifolia.Generation.Schematron.ConstraintToDocumentElementMap
{
    static internal class ConstraintToDocumentElementHelper
    {

        static public void ParseContextForElementAndAttribute(IConstraint aConstraint, out DocumentTemplateElement aElement, out DocumentTemplateElementAttribute aAttribute)
        {
            var cp = new ContextParser(aConstraint.Context);
            cp.Parse(out aElement, out aAttribute);
        }

        static public DocumentTemplateElement CreateParentElementForAttribute(IConstraint aConstraint, DocumentTemplateElementAttribute aAttribute)
        {
            DocumentTemplateElement parentElement = null;
            // Is there a parent constraint that should be used as a context
            if (aConstraint.Parent != null && !string.IsNullOrEmpty(aConstraint.Parent.Context) && !aConstraint.Parent.IsBranch)
            {
                if ((aAttribute != null) && (aAttribute.Element == null)) //we have an attribute, but no element attached. the parent constraint would then be the element.
                {
                    parentElement = new DocumentTemplateElement(aConstraint.Parent.Context);
                    if (aConstraint.Parent.IsBranch)
                        parentElement.AddAttribute(aAttribute);
                }
                else if ((aAttribute != null) && (aAttribute.Element != null) && (aConstraint.Parent.Context != aAttribute.Element.ElementName)) //we have an attribute, with an element attached, but the element does not match the parent context
                {
                    parentElement = new DocumentTemplateElement(aConstraint.Parent.Context);
                    parentElement.AddElement(aAttribute.Element);
                }
                else if (aAttribute != null && aAttribute.Element != null)
                {
                    parentElement = aAttribute.Element;
                }
            }
            return parentElement;
        }

        static public void AddElementValueAndDataType(string aPrefix, DocumentTemplateElement aElement, IConstraint aTemplateConstraint)
        {
            if (!string.IsNullOrEmpty(aTemplateConstraint.Value))
            {
                if (!string.IsNullOrEmpty(aTemplateConstraint.ValueConformance))
                {
                    var valueConformanceType = ConformanceParser.Parse(aTemplateConstraint.ValueConformance);
                    var conformanceType = ConformanceParser.Parse(aTemplateConstraint.Conformance);
                    if (valueConformanceType == conformanceType)
                    {
                        aElement.Value = !string.IsNullOrEmpty(aTemplateConstraint.Value) ? aTemplateConstraint.Value.Trim() : aTemplateConstraint.Value;
                    }
                } 
                else 
                {
                    aElement.Value = !string.IsNullOrEmpty(aTemplateConstraint.Value) ? aTemplateConstraint.Value.Trim() : aTemplateConstraint.Value;
                }
            }
            // Add the data type attribute if one is present on the constraint
            if (!string.IsNullOrEmpty(aTemplateConstraint.DataType))
            {
                if (aPrefix == "cda")
                {
                    if (aElement.ElementName != "code")
                    {
                        DocumentTemplateElementAttribute dataTypeAttr = new DocumentTemplateElementAttribute("xsi:type");
                        dataTypeAttr.SingleValue = aTemplateConstraint.DataType;
                        aElement.AddAttribute(dataTypeAttr);
                    }
                }
            }
        }

        static public void AddConformance(IConstraint aTemplateConstraint, AssertionLineBuilder aAssertionLineBuilder)
        {
            // Determine if we have a conformance
            if (!string.IsNullOrEmpty(aTemplateConstraint.Conformance))
                aAssertionLineBuilder.ConformsTo(
                    ConformanceParser.Parse(aTemplateConstraint.Conformance));
        }

        static public void AddCardinality(IConstraint aTemplateConstraint, AssertionLineBuilder aAssertionLineBuilder)
        {
            // Determine if we have a cardinality
            if (!string.IsNullOrEmpty(aTemplateConstraint.Cardinality))
                aAssertionLineBuilder.WithCardinality(
                    CardinalityParser.Parse(aTemplateConstraint.Cardinality));
        }


        static public void AddCodeSystemToElement(IObjectRepository aTdb, IIGTypePlugin igTypePlugin, DocumentTemplateElement aElement, IConstraint aTemplateConstraint)
        {
            if (aTemplateConstraint.ValueCodeSystemId.HasValue && (aElement.ElementName == "value" || aElement.ElementName == "code"))
            {
                var codeSystem = aTdb.CodeSystems.Single(y => y.Id == aTemplateConstraint.ValueCodeSystemId);
                if (codeSystem != null)
                {
                    var codeSystemIdentifier = igTypePlugin.ParseIdentifier(codeSystem.Oid);
                    aElement.AddAttribute(new DocumentTemplateElementAttribute("codeSystem", codeSystemIdentifier, true)); //TODO: this is cda specific, need to update it
                }
            }
        }

    }
}
