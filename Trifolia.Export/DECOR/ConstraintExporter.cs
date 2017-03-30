using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Shared.Plugins;

namespace Trifolia.Export.DECOR
{
    public class ConstraintExporter
    {
        private XmlDocument dom;
        private Template template;
        private IGSettingsManager igSettings;
        private IIGTypePlugin igTypePlugin;
        private IObjectRepository tdb;

        public ConstraintExporter(XmlDocument dom, Template template, IGSettingsManager igSettings, IIGTypePlugin igTypePlugin, IObjectRepository tdb)
        {
            this.dom = dom;
            this.template = template;
            this.igSettings = igSettings;
            this.igTypePlugin = igTypePlugin;
            this.tdb = tdb;
        }

        private attribute ExportAttribute(TemplateConstraint constraint)
        {
            IFormattedConstraint formattedConstraint = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.igSettings, this.igTypePlugin, constraint, null, null, false, false, false, false);
            attribute constraintAttribute = new attribute();
            constraintAttribute.name = constraint.Context.Substring(1);
            constraintAttribute.isOptional = constraint.Conformance != "SHALL";

            constraintAttribute.Items = this.ExportConstraints(constraint, true);

            return constraintAttribute;
        }

        private RuleDefinition ExportElement(TemplateConstraint constraint)
        {
            IFormattedConstraint formattedConstraint = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.igSettings, this.igTypePlugin, constraint, null, null, false, false, false, false);
            RuleDefinition constraintRule = new RuleDefinition();
            constraintRule.name = constraint.Context;
            constraintRule.minimumMultiplicity = constraint.CardinalityType.Left.ToString();
            constraintRule.maximumMultiplicity = constraint.CardinalityType.Right == Int32.MaxValue ? "*" : constraint.CardinalityType.Right.ToString();
            constraintRule.isMandatory = constraint.Conformance == "SHALL";
            constraintRule.text = new string[] { formattedConstraint.GetPlainText(false, false, false) };

            var allItems = this.ExportConstraints(constraint);

            // properties and vocabulary
            constraintRule.Items = allItems.Where(y => y.GetType() == typeof(property) || y.GetType() == typeof(vocabulary)).ToArray();

            // attributes
            List<attribute> attributes = new List<attribute>();
            foreach (var attribute in allItems.Where(y => y.GetType() == typeof(attribute)))
            {
                attributes.Add((attribute)attribute);
            }
            constraintRule.attribute = attributes.ToArray();

            // everything else
            constraintRule.Items1 = allItems.Where(y => y.GetType() != typeof(property) && y.GetType() != typeof(vocabulary) && y.GetType() != typeof(attribute)).ToArray();

            return constraintRule;
        }

        private object[] ExportConstraints(TemplateConstraint parentConstraint = null, bool isAttribute = false)
        {
            List<object> constraintRules = new List<object>();

            if (parentConstraint != null)
            {
                if (parentConstraint.ValueSet != null || parentConstraint.CodeSystem != null || !string.IsNullOrEmpty(parentConstraint.Value))
                {
                    vocabulary vocabConstraint = new vocabulary();

                    if (parentConstraint.ValueSet != null)
                    {
                        vocabConstraint.valueSet = parentConstraint.ValueSet.GetIdentifier(this.igTypePlugin);

                        string oid, ext;

                        if (IdentifierHelper.IsIdentifierOID(parentConstraint.ValueSet.GetIdentifier(this.igTypePlugin)))
                        {
                            IdentifierHelper.GetIdentifierOID(parentConstraint.ValueSet.GetIdentifier(this.igTypePlugin), out oid);
                            vocabConstraint.valueSet = oid;
                        }
                        else if (IdentifierHelper.IsIdentifierII(parentConstraint.ValueSet.GetIdentifier(this.igTypePlugin)))
                        {
                            IdentifierHelper.GetIdentifierII(parentConstraint.ValueSet.GetIdentifier(this.igTypePlugin), out oid, out ext);
                            vocabConstraint.valueSet = oid;
                        }
                    }

                    if (parentConstraint.CodeSystem != null)
                    {
                        vocabConstraint.codeSystem = parentConstraint.CodeSystem.Oid;
                        vocabConstraint.codeSystemName = parentConstraint.CodeSystem.Name;

                        string oid, ext;

                        if (IdentifierHelper.IsIdentifierOID(parentConstraint.CodeSystem.Oid))
                        {
                            IdentifierHelper.GetIdentifierOID(parentConstraint.CodeSystem.Oid, out oid);
                            vocabConstraint.codeSystem = oid;
                        }
                        else if (IdentifierHelper.IsIdentifierII(parentConstraint.CodeSystem.Oid))
                        {
                            IdentifierHelper.GetIdentifierII(parentConstraint.CodeSystem.Oid, out oid, out ext);
                            vocabConstraint.codeSystem = oid;
                        }
                    }

                    if (!string.IsNullOrEmpty(parentConstraint.Value))
                        vocabConstraint.code = parentConstraint.Value;

                    if (!string.IsNullOrEmpty(parentConstraint.DisplayName))
                        vocabConstraint.displayName = parentConstraint.DisplayName;

                    if (parentConstraint.IsStatic == true && parentConstraint.ValueSetDate != null)
                        vocabConstraint.flexibility = parentConstraint.ValueSetDate.Value.ToString("yyyy-MM-ddThh:mm:ss");
                    else if (parentConstraint.IsStatic == false)
                        vocabConstraint.flexibility = "dynamic";

                    constraintRules.Add(vocabConstraint);
                }
            }

            foreach (var constraint in this.template.ChildConstraints.Where(y => y.ParentConstraintId == (parentConstraint != null ? parentConstraint.Id : (int?)null)))
            {
                if (!constraint.IsPrimitive)
                {
                    if (isAttribute)
                        continue;       // Can't export child elements/attributes of an attribute constraint

                    if (constraint.Context.StartsWith("@"))
                        constraintRules.Add(this.ExportAttribute(constraint));
                    else
                        constraintRules.Add(this.ExportElement(constraint));
                }
                else
                {
                    IFormattedConstraint formattedConstraint = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.igSettings, this.igTypePlugin, constraint, null, null, false, false, false, false);

                    XmlNode[] anyField = new XmlNode[] { this.dom.CreateTextNode(formattedConstraint.GetPlainText(false, false, false)) };
                    constraintRules.Add(new FreeFormMarkupWithLanguage()
                    {
                        Any = anyField
                    });
                }
            }

            return constraintRules.ToArray();
        }

        public object[] ExportConstraints()
        {
            return this.ExportConstraints(null);
        }
    }
}
