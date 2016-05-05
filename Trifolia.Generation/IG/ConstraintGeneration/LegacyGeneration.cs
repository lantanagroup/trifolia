using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Generation.IG.ConstraintGeneration
{
    public class LegacyGeneration : IConstraintGenerator
    {
        private int templateConstraintCount = 1;

        #region IConstraintGenerator Properties

        private IGSettingsManager igSettings;
        private Body documentBody;
        private IObjectRepository dataSource;
        private List<TemplateConstraint> rootConstraints;
        private List<TemplateConstraint> allConstraints;
        private Template currentTemplate;
        private List<Template> allTemplates;
        private string constraintHeadingStyle;

        public CommentManager CommentManager { get; set; }

        public string ConstraintHeadingStyle
        {
            get { return constraintHeadingStyle; }
            set { constraintHeadingStyle = value; }
        }

        public WIKIParser WikiParser
        {
            get { return null; }
            set { }
        }

        public FigureCollection Figures
        {
            get { return null; }
            set { }
        }

        public bool IncludeSamples
        {
            get { return false; }
            set { }
        }

        public IGSettingsManager IGSettings
        {
            get { return igSettings; }
            set { igSettings = value; }
        }

        public Body DocumentBody
        {
            get { return documentBody; }
            set { documentBody = value; }
        }

        public IObjectRepository DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }

        public List<TemplateConstraint> RootConstraints
        {
            get { return rootConstraints; }
            set { rootConstraints = value; }
        }

        public List<TemplateConstraint> AllConstraints
        {
            get { return allConstraints; }
            set { allConstraints = value; }
        }

        public Template CurrentTemplate
        {
            get { return currentTemplate; }
            set { currentTemplate = value; }
        }

        public List<Template> AllTemplates
        {
            get { return allTemplates; }
            set { allTemplates = value; }
        }

        public bool IncludeCategory { get; set; }
        public List<string> SelectedCategories { get; set; }

        #endregion

        public void GenerateConstraints(bool aCreateHyperlinksForValueSetNames = false, bool includeNotes = false)
        {
            // Output the constraints
            foreach (TemplateConstraint cConstraint in rootConstraints)
            {
                this.AddTemplateConstraint(cConstraint, 1);
            }
        }

        private void AddTemplateConstraint(TemplateConstraint constraint, int level)
        {
            // TODO: May be able to make this more efficient
            List<TemplateConstraint> childConstraints = allConstraints
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
                this.documentBody.Append(pDescription);
            }

            Paragraph pConstraint = new Paragraph(
                new ParagraphProperties(
                    new NumberingProperties(
                        new NumberingLevelReference() { Val = level - 1 },
                        new NumberingId() { Val = GenerationConstants.BASE_TEMPLATE_INDEX + (int)currentTemplate.Id })));
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
                            DocHelper.CreateRun(this.igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne) + " "));
                        break;
                    case "0..1":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne) + " "));
                        break;
                    case "1..*":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne) + " "));
                        break;
                    case "0..*":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore) + " "));
                        break;
                    case "0..0":
                        pConstraint.Append(
                            DocHelper.CreateRun(this.igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZero) + " "));
                        break;
                    default:
                        pConstraint.Append(
                            DocHelper.CreateRun("[" + constraint.Cardinality + "] "));
                        break;
                }

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
                else if (constraint.ContainedTemplate != null)
                {
                    Template containedTemplate = this.dataSource.Templates.Single(y => y.Id == constraint.ContainedTemplateId.Value);

                    if (this.allTemplates.Exists(y => y.Id == containedTemplate.Id))
                    {
                        pConstraint.Append(
                            DocHelper.CreateAnchorHyperlink(containedTemplate.Name, containedTemplate.Bookmark, Properties.Settings.Default.LinkStyle),
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
                            DocHelper.CreateRun(valueConformance, Properties.Settings.Default.ConformanceVerbStyle),
                            DocHelper.CreateRun(" be selected from ValueSet "),
                            DocHelper.CreateRun(constraint.ValueSet.Name + " " + constraint.ValueSet.Oid, 
                            anchorName:lValueSetAnchor,
                            style:Properties.Settings.Default.VocabularyConstraintStyle));
                    }
                    else if (constraint.CodeSystem != null)
                    {
                        pConstraint.Append(
                            DocHelper.CreateRun(", which "),
                            DocHelper.CreateRun(valueConformance, Properties.Settings.Default.ConformanceVerbStyle),
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
                            DocHelper.CreateRun(constraint.ValueSet.Name + " " + constraint.ValueSet.Oid, style:Properties.Settings.Default.TemplateOidStyle),
                            DocHelper.CreateRun(")"));
                    }
                }

                pConstraint.Append(
                    DocHelper.CreateRun(string.Format(" (CONF:{0})", constraint.Id), "C_" + constraint.Id.ToString()));

                if (constraint.IsBranch == true && childConstraints.Count > 0)
                {
                    pConstraint.Append(
                        DocHelper.CreateRun(" such that it"));
                }
            }

            // Add the constraint as a bullet to the document
            this.documentBody.Append(pConstraint);

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
