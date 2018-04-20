using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trifolia.DB;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Export.MSWord.ConstraintGeneration
{
    public class LegacyGeneration : IConstraintGenerator
    {
        private int templateConstraintCount = 1;

        #region IConstraintGenerator Properties

        public IIGTypePlugin IGTypePlugin { get; set; }

        public CommentManager CommentManager { get; set; }

        public List<ConstraintReference> ConstraintReferences { get; set; }

        public string ConstraintHeadingStyle { get; set; }

        public FigureCollection Figures { get; set; }

        public bool IncludeSamples { get; set; }

        public IGSettingsManager IGSettings { get; set; }

        public MainDocumentPart MainPart { get; set; }

        public Body DocumentBody { get; set; }

        public IObjectRepository DataSource { get; set; }

        public List<TemplateConstraint> RootConstraints { get; set; }

        public List<TemplateConstraint> AllConstraints { get; set; }

        public Template CurrentTemplate { get; set; }

        public List<Template> AllTemplates { get; set; }

        public bool IncludeCategory { get; set; }

        public List<string> SelectedCategories { get; set; }
        public HyperlinkTracker HyperlinkTracker { get; set; }

        #endregion

        public void GenerateConstraints(bool aCreateHyperlinksForValueSetNames = false, bool includeNotes = false)
        {
            // Output the constraints
            foreach (TemplateConstraint cConstraint in this.RootConstraints)
            {
                this.AddTemplateConstraint(cConstraint, 1);
            }
        }

        private void AddTemplateConstraint(TemplateConstraint constraint, int level)
        {
            // TODO: May be able to make this more efficient
            List<TemplateConstraint> childConstraints = this.AllConstraints
                .Where(y => y.ParentConstraintId == constraint.Id)
                .OrderBy(y => y.Order)
                .ToList();

            this.templateConstraintCount++;

            // Add the description before the constraint
            if (!string.IsNullOrEmpty(constraint.Description))
            {
                Paragraph pDescription = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId()
                        {
                            Val = Properties.Settings.Default.TemplateDescriptionStyle
                        })
                    {
                        SpacingBetweenLines = new SpacingBetweenLines()
                        {
                            Before = new StringValue("80")
                        }
                    },
                    DocHelper.CreateRun(constraint.Description, style:"Normal,Normal Not Indented"));
                this.DocumentBody.Append(pDescription);
            }

            Paragraph pConstraint = new Paragraph(
                new ParagraphProperties(
                    new NumberingProperties(
                        new NumberingLevelReference() { Val = level - 1 },
                        new NumberingId() { Val = GenerationConstants.BASE_TEMPLATE_INDEX + (int)this.CurrentTemplate.Id })));
            string context = constraint.Context;

            // Build the constraint text
            if (constraint.IsPrimitive == true)
            {
                string narrativeText = !string.IsNullOrEmpty(constraint.PrimitiveText) ? constraint.PrimitiveText : "MISSING NARRATIVE";
                this.AddNarrative(pConstraint, narrativeText);
                pConstraint.Append(
                    DocHelper.CreateRun(" (CONF:" + constraint.Id.ToString() + ")"));
            }
            else
            {
                // SHOULD | SHALL | MAY | ETC.
                if (!string.IsNullOrEmpty(constraint.Conformance))
                {
                    pConstraint.Append(
                        DocHelper.CreateRun(constraint.Conformance, style:Properties.Settings.Default.ConformanceVerbStyle));
                    pConstraint.Append(
                        DocHelper.CreateRun(" contain "));
                }
                else
                {
                    pConstraint.Append(
                        DocHelper.CreateRun("Contains "));
                }

                // exactly one, zero or one, at least one, etc.
                switch (constraint.Cardinality)
                {
                    case "1..1":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.IGSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne) + " "));
                        break;
                    case "0..1":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.IGSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne) + " "));
                        break;
                    case "1..*":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.IGSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne) + " "));
                        break;
                    case "0..*":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.IGSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore) + " "));
                        break;
                    case "0..0":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.IGSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZero) + " "));
                        break;
                    default:
                        pConstraint.Append(
                            DocHelper.CreateRun("[" + constraint.Cardinality + "] "));
                        break;
                }

                var containedTemplate = (from tcr in constraint.References
                                         join t in this.DataSource.Templates on tcr.ReferenceIdentifier equals t.Oid
                                         where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                         select t).FirstOrDefault();

                if (!string.IsNullOrEmpty(constraint.Context))
                {
                    string dataType = constraint.DataType != null ? constraint.DataType.ToLower() : string.Empty;

                    if ((context.ToLower() == "value" || context.ToLower() == "code" || context.ToLower() == "statuscode") && (dataType == "cd" || dataType == "cs" || dataType == "cv" || dataType == "ce"))
                    {
                        if (!string.IsNullOrEmpty(constraint.Value) || constraint.ValueSet != null || constraint.CodeSystem != null)
                        {
                            context = context + "/@code";
                        }
                    }
                    else if (context.ToLower() == "value" && !string.IsNullOrEmpty(constraint.DataType))
                    {
                        context = string.Format("value with @xsi:type=\"{0}\"", constraint.DataType);
                    }

                    pConstraint.Append(
                        DocHelper.CreateRun(context, style:Properties.Settings.Default.ConstraintContextStyle));
                }
                else if (containedTemplate != null)
                {
                    if (this.AllTemplates.Exists(y => y.Id == containedTemplate.Id))
                    {
                        pConstraint.Append(
                            this.HyperlinkTracker.CreateHyperlink(containedTemplate.Name, containedTemplate.Bookmark, Properties.Settings.Default.LinkStyle),
                            DocHelper.CreateRun(" (" + containedTemplate.Oid + ")", style:Properties.Settings.Default.TemplateOidStyle));
                    }
                    else
                    {
                        pConstraint.Append(
                            DocHelper.CreateRun(containedTemplate.Name),
                            DocHelper.CreateRun(" (" + containedTemplate.Oid + ")", style:Properties.Settings.Default.TemplateOidStyle));
                    }
                }

                if (constraint.ValueConformance != null)
                {
                    string valueConformance = constraint.Conformance;
                    string staticDynamic = string.Empty;

                    if (constraint.IsStatic == true)
                        staticDynamic = "STATIC";
                    else if (constraint.IsStatic == false)
                        staticDynamic = "DYNAMIC";

                    if (constraint.ValueSet != null)
                    {
                        string lValueSetAnchor = constraint.ValueSet.Name.Replace(" ", "_");

                        pConstraint.Append(
                            DocHelper.CreateRun(", which "),
                            DocHelper.CreateRun(valueConformance, style: Properties.Settings.Default.ConformanceVerbStyle),
                            DocHelper.CreateRun(" be selected from ValueSet "),
                            new BookmarkStart() { Id = lValueSetAnchor, Name = lValueSetAnchor },
                            DocHelper.CreateRun(
                                constraint.ValueSet.Name + " " + constraint.ValueSet.GetIdentifier(this.IGTypePlugin),
                                style:Properties.Settings.Default.VocabularyConstraintStyle),
                            new BookmarkEnd() { Id = lValueSetAnchor });
                    }
                    else if (constraint.CodeSystem != null)
                    {
                        pConstraint.Append(
                            DocHelper.CreateRun(", which "),
                            DocHelper.CreateRun(valueConformance, style: Properties.Settings.Default.ConformanceVerbStyle),
                            DocHelper.CreateRun(" be selected from CodeSystem "),
                            DocHelper.CreateRun(constraint.CodeSystem.Name + " (" + constraint.CodeSystem.Oid + ")", style:Properties.Settings.Default.VocabularyConstraintStyle));
                    }

                    pConstraint.Append(
                        DocHelper.CreateRun(" " + staticDynamic, style:Properties.Settings.Default.ConformanceVerbStyle));
                }

                if (!string.IsNullOrEmpty(constraint.Value))
                {
                    if (context.Contains("@xsi:type"))
                        pConstraint.Append(
                            DocHelper.CreateRun(", where the @code"));

                    pConstraint.Append(
                        DocHelper.CreateRun("="),
                        DocHelper.CreateRun("\"" + constraint.Value + "\"", style:Properties.Settings.Default.VocabularyConstraintStyle));

                    if (!string.IsNullOrEmpty(constraint.DisplayName))
                    {
                        pConstraint.Append(
                            DocHelper.CreateRun(" " + constraint.DisplayName));
                    }

                    if (constraint.CodeSystem != null)
                    {
                        pConstraint.Append(
                            DocHelper.CreateRun(" (CodeSystem: "),
                            DocHelper.CreateRun(constraint.CodeSystem.Name + " " + constraint.CodeSystem.Oid, style:Properties.Settings.Default.TemplateOidStyle),
                            DocHelper.CreateRun(")"));
                    }

                    if (constraint.ValueSet != null)
                    {
                        pConstraint.Append(
                            DocHelper.CreateRun(" (ValueSet: "),
                            DocHelper.CreateRun(constraint.ValueSet.Name + " " + constraint.ValueSet.GetIdentifier(this.IGTypePlugin), style:Properties.Settings.Default.TemplateOidStyle),
                            DocHelper.CreateRun(")"));
                    }
                }
                
                pConstraint.Append(
                    new BookmarkStart() { Id = "C_" + constraint.Id.ToString(), Name = "C_" + constraint.Id.ToString() },
                    DocHelper.CreateRun(string.Format(" (CONF:{0})", constraint.Id)),
                    new BookmarkEnd() { Id = "C_" + constraint.Id.ToString() });

                if (constraint.IsBranch == true && childConstraints.Count > 0)
                {
                    pConstraint.Append(
                        DocHelper.CreateRun(" such that it"));
                }
            }

            // Add the constraint as a bullet to the document
            this.DocumentBody.Append(pConstraint);

            // Add child constraints
            foreach (TemplateConstraint cConstraint in childConstraints)
            {
                this.AddTemplateConstraint(cConstraint, level + 1);
            }
        }

        private void AddNarrative(Paragraph para, string narrativeText)
        {
            MatchCollection matches = Regex.Matches(narrativeText, @"(SHOULD NOT|SHALL NOT|SHALL|SHOULD|MAY|STATIC|DYNAMIC)");

            if (matches.Count == 0)
            {
                para.Append(
                    DocHelper.CreateRun(narrativeText));
                return;
            }

            for (int i = 0; i < matches.Count; i++)
            {
                Match cMatch = matches[i];
                Capture cCapture = cMatch.Captures[0];

                int lastEnd = i > 0 ? matches[i - 1].Captures[0].Index + matches[i - 1].Captures[0].Length : -1;
                int nextStart = i < matches.Count - 1 ? matches[i + 1].Index : -1;

                int afterStart = cCapture.Index + cCapture.Length;
                int afterLength = nextStart != -1 ? nextStart - afterStart : narrativeText.Length - afterStart;

                string beforeText = cCapture.Index != 0 ? narrativeText.Substring(0, cCapture.Index) : string.Empty;
                string afterText = afterStart != narrativeText.Length ? narrativeText.Substring(afterStart, afterLength) : string.Empty;

                if (i == 0 && !string.IsNullOrEmpty(beforeText))
                {
                    para.Append(
                        DocHelper.CreateRun(beforeText));
                }

                para.Append(
                    DocHelper.CreateRun(cCapture.Value, style:Properties.Settings.Default.ConformanceVerbStyle));

                if (!string.IsNullOrEmpty(afterText))
                {
                    para.Append(
                        DocHelper.CreateRun(afterText));
                }
            }
        }
    }
}
