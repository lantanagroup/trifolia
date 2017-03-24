using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Generation.IG.ConstraintGeneration
{
    public class FormattedConstraint20140415 : IFormattedConstraint
    {
        public FormattedConstraint20140415()
        {
            this.ContainedTemplateId = -1;
        }

        private List<ConstraintPart> parts = new List<ConstraintPart>();
        private IGSettingsManager igSettings;
        private IObjectRepository tdb;
        private Template containedTemplate;

        #region Properties

        public IGSettingsManager IgSettings
        {
            get
            {
                return this.igSettings;
            }
            set
            {
                this.igSettings = value;
            }
        }

        public IObjectRepository Tdb
        {
            get
            {
                return this.tdb;
            }
            set
            {
                this.tdb = value;
            }
        }

        public bool IncludeCategory { get; set; }
        public bool LinkContainedTemplate { get; set; }
        public bool LinkIsBookmark { get; set; }
        public bool CreateLinkForValueSets { get; set; }

        public string Category { get; set; }            // Nothing is done with this property in this version
        public string Number { get; set; }
        public string Context { get; set; }
        public bool IsPrimitive { get; set; }
        public bool IsBranch { get; set; }
        public bool HasChildren { get; set; }
        public string Narrative { get; set; }
        public bool ParentIsBranch { get; set; }
        public string ParentContext { get; set; }
        public string ParentCardinality { get; set; }
        public string Conformance { get; set; }
        public string Cardinality { get; set; }
        public string DataType { get; set; }
        public int ContainedTemplateId { get; set; }
        public string ContainedTemplateTitle { get; set; }
        public string ContainedTemplateLink { get; set; }
        public string ContainedTemplateOid { get; set; }
        public string ValueConformance { get; set; }
        public string StaticDynamic { get; set; }
        public string ValueSetName { get; set; }
        public string ValueSetOid { get; set; }
        public DateTime? ValueSetVersion { get; set; }
        public string CodeSystemName { get; set; }
        public string CodeSystemOid { get; set; }
        public string Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Label { get; set; }
        public bool IsHeading { get; set; }
        public string HeadingDescription { get; set; }
        public string TemplateLinkBase { get; set; }
        public string ValueSetLinkBase { get; set; }

        #endregion

        public void ParseConstraint(IConstraint constraint, Template containedTemplate = null, ValueSet valueSet = null, CodeSystem codeSystem = null)
        {
            this.Number = string.Format("{0}-{1}",
                constraint.Template != null ? constraint.Template.OwningImplementationGuideId.ToString() : "X",
                constraint.Number.HasValue ? constraint.Number.ToString() : "X");
            this.Context = constraint.Context;
            this.IsPrimitive = constraint.IsPrimitive == true;
            this.IsBranch = constraint.IsBranch == true;
            this.HasChildren = constraint.Children != null && constraint.Children.Count() > 0;
            this.Narrative = constraint.PrimitiveText;
            this.Description = constraint.Description;
            this.Label = constraint.Label;
            this.IsHeading = constraint.IsHeading;
            this.HeadingDescription = constraint.HeadingDescription;

            if (constraint.Parent != null)
            {
                this.ParentIsBranch = constraint.Parent.IsBranch == true;
                this.ParentContext = constraint.Parent.Context;
                this.ParentCardinality = constraint.Parent.Cardinality;
            }

            this.Conformance = constraint.Conformance;
            this.Cardinality = constraint.Cardinality;
            this.DataType = constraint.DataType;

            if (constraint.ContainedTemplateId != null)
            {
                this.containedTemplate = this.tdb.Templates.Single(y => y.Id == constraint.ContainedTemplateId);

                this.ContainedTemplateId = this.containedTemplate.Id;
                this.ContainedTemplateTitle = this.containedTemplate.Name;
                this.ContainedTemplateOid = this.containedTemplate.Oid;
            }

            this.ValueConformance = constraint.ValueConformance;

            if (constraint.IsStatic == true)
                this.StaticDynamic = "STATIC";
            else if (constraint.IsStatic == false)
                this.StaticDynamic = "DYNAMIC";

            if (constraint.ValueSetId != null)
            {
                // If the caller didn't pass in the ValueSet, get it from the db
                if (valueSet == null || valueSet.Id != constraint.ValueSetId)
                    valueSet = this.tdb.ValueSets.Single(y => y.Id == constraint.ValueSetId);

                this.ValueSetName = valueSet.Name;
                this.ValueSetOid = valueSet.Oid;
                this.ValueSetVersion = constraint.ValueSetDate;
            }

            if (constraint.ValueCodeSystemId != null)
            {
                // If the caller didn't pass in the CodeSystem, get it from the db
                if (codeSystem == null || codeSystem.Id != constraint.ValueCodeSystemId)
                    codeSystem = this.tdb.CodeSystems.Single(y => y.Id == constraint.ValueCodeSystemId);

                this.CodeSystemName = codeSystem.Name;
                this.CodeSystemOid = codeSystem.Oid;
            }

            this.Value = constraint.Value;
            this.DisplayName = constraint.ValueDisplayName;
        }

        public void ParseFormattedConstraint()
        {
            this.parts = new List<ConstraintPart>();

            // Make sure we don't process contained template constraints as 
            // primitives simply because a context is not specified
            if (this.ContainedTemplateId != -1)
                this.IsPrimitive = false;

            // Build the constraint text
            if (this.IsPrimitive)
            {
                string narrativeText = this.Narrative != null ? this.Narrative.Trim() : string.Empty;

                if (string.IsNullOrEmpty(narrativeText))
                    narrativeText = "MISSING NARRATIVE FOR PRIMITIVE ";

                if (narrativeText.EndsWith("."))
                    narrativeText = narrativeText.Substring(0, narrativeText.Length - 1);

                narrativeText = narrativeText.Substring(0, 1).ToUpper() + narrativeText.Substring(1);

                this.parts.Add(
                    new ConstraintPart(ConstraintPart.PartTypes.PrimitiveText, narrativeText + " (CONF:" + this.Number + ")."));
            }
            else
            {
                // If we have defined a contained template, then ignore the context.
                if (this.ContainedTemplateId != -1)
                {
                    this.Context = null;

                    if (this.LinkIsBookmark)
                        this.ContainedTemplateLink = string.Format("{0}{1}", this.TemplateLinkBase, this.containedTemplate.Bookmark);
                    else
                        this.ContainedTemplateLink = this.containedTemplate.GetViewUrl(this.TemplateLinkBase);
                }

                if (!this.ParentIsBranch && !string.IsNullOrEmpty(this.ParentContext) && !string.IsNullOrEmpty(this.ParentCardinality))
                {
                    if (this.ParentCardinality == "1..1")
                    {
                        this.parts.Add(new ConstraintPart("This " + this.ParentContext + " "));
                    }
                    else if (this.ParentCardinality.StartsWith("0.."))
                    {
                        this.parts.Add(new ConstraintPart("The " + this.ParentContext + ", if present, "));
                    }
                    else if (this.ParentCardinality.EndsWith("..*"))
                    {
                        this.parts.Add(new ConstraintPart("Such " + MakePlural(this.ParentContext) + " "));
                    }
                }

                // SHOULD | SHALL | MAY | ETC.
                if (!string.IsNullOrEmpty(this.Conformance))
                {
                    this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, this.Conformance));
                    this.parts.Add(new ConstraintPart(" contain "));
                }
                else
                {
                    this.parts.Add(new ConstraintPart("Contains "));
                }

                // exactly one, zero or one, at least one, etc.
                switch (this.Cardinality)
                {
                    case "1..1":
                        if (igSettings != null)
                            this.parts.Add(new ConstraintPart(igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne) + " "));
                        else
                            this.parts.Add(new ConstraintPart(this.Cardinality + " "));
                        break;
                    case "0..1":
                        if (igSettings != null)
                            this.parts.Add(new ConstraintPart(igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne) + " "));
                        else
                            this.parts.Add(new ConstraintPart(this.Cardinality + " "));
                        break;
                    case "1..*":
                        if (igSettings != null)
                            this.parts.Add(new ConstraintPart(igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne) + " "));
                        else
                            this.parts.Add(new ConstraintPart(this.Cardinality + " "));
                        break;
                    case "0..*":
                        if (igSettings != null)
                            this.parts.Add(new ConstraintPart(igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore) + " "));
                        else
                            this.parts.Add(new ConstraintPart(this.Cardinality + " "));
                        break;
                    case "0..0":
                        if (igSettings != null)
                            this.parts.Add(new ConstraintPart(igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZero) + " "));
                        else
                            this.parts.Add(new ConstraintPart(this.Cardinality + " "));
                        break;
                    case "1..2":
                    case "1..3":
                    case "1..4":
                    case "1..5":
                    case "1..6":
                    case "1..7":
                    case "1..8":
                    case "1..9":
                    case "1..10":
                        this.parts.Add(new ConstraintPart("at least one and not more than " + this.Cardinality.Substring(3) + " "));
                        break;
                    default:
                        this.parts.Add(new ConstraintPart("[" + this.Cardinality + "] "));
                        break;
                }

                // If we have a context, add the context
                if (!string.IsNullOrEmpty(this.Context))
                {
                    this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Context, this.Context));

                    if (!string.IsNullOrEmpty(this.Value))
                    {
                        if (!string.IsNullOrEmpty(this.Context) && this.Context.Contains("@xsi:type"))
                            this.parts.Add(new ConstraintPart(", where the @code"));

                        this.parts.Add(new ConstraintPart("="));
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Vocabulary, "\"" + this.Value + "\""));

                        if (!string.IsNullOrEmpty(this.DisplayName))
                            this.parts.Add(new ConstraintPart(" " + this.DisplayName));
                    }

                    // For value context's, if a data-type is present, output with @xsi:type
                    if ((this.Context.ToLower() == "code" || this.Context.ToLower() == "value") && !string.IsNullOrEmpty(this.DataType))
                        this.parts.Add(new ConstraintPart(string.Format(" with @xsi:type=\"{0}\"", this.DataType)));
                }

                if (!string.IsNullOrEmpty(this.ValueConformance))
                {
                    string selectionText = ", which ";

                    if (!string.IsNullOrEmpty(this.Context) && !this.Context.Contains("@") && !string.IsNullOrEmpty(this.DataType) && !this.Context.ToLower().Contains("@code"))
                        selectionText = ", where the code ";

                    if (!string.IsNullOrEmpty(this.ValueSetOid) && !string.IsNullOrEmpty(this.ValueSetName))
                    {
                        this.parts.Add(new ConstraintPart(selectionText));
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, this.ValueConformance));
                        this.parts.Add(new ConstraintPart(" be selected from ValueSet "));

                        if (this.CreateLinkForValueSets)
                        {
                            this.parts.Add(new ConstraintPart(
                                ConstraintPart.PartTypes.Link, this.ValueSetName)
                                {
                                    LinkDestination = string.Format("{0}{1}", this.ValueSetLinkBase, Helper.GetCleanName(this.ValueSetName, 39))
                                });
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Vocabulary, " " + this.ValueSetOid));
                        }
                        else
                        {
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Vocabulary, this.ValueSetName + " " + this.ValueSetOid));
                        }

                        if (!string.IsNullOrEmpty(this.StaticDynamic))
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, " " + this.StaticDynamic));

                        if (this.ValueSetVersion != null)
                            this.parts.Add(new ConstraintPart(" " + this.ValueSetVersion.Value.ToString("yyyy-MM-dd")));
                    }
                    else if (!string.IsNullOrEmpty(this.CodeSystemOid) && !string.IsNullOrEmpty(this.CodeSystemName))
                    {
                        this.parts.Add(new ConstraintPart(selectionText));
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, this.ValueConformance));
                        this.parts.Add(new ConstraintPart(" be selected from CodeSystem "));
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Vocabulary, this.CodeSystemName + " (" + this.CodeSystemOid + ")"));

                        if (!string.IsNullOrEmpty(this.StaticDynamic))
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, " " + this.StaticDynamic));
                    }
                }

                if (string.IsNullOrEmpty(this.ValueConformance))
                {
                    if (!string.IsNullOrEmpty(this.CodeSystemOid) && !string.IsNullOrEmpty(this.CodeSystemName))
                    {
                        this.parts.Add(new ConstraintPart(" (CodeSystem: "));
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Template, this.CodeSystemName + " " + this.CodeSystemOid));

                        if (!string.IsNullOrEmpty(this.StaticDynamic))
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, " " + this.StaticDynamic));

                        this.parts.Add(new ConstraintPart(")"));
                    }

                    if (!string.IsNullOrEmpty(this.ValueSetOid) && !string.IsNullOrEmpty(this.ValueSetName))
                    {
                        this.parts.Add(new ConstraintPart(" (ValueSet: "));

                        if (this.CreateLinkForValueSets)
                        {
                            this.parts.Add(new ConstraintPart(
                                ConstraintPart.PartTypes.Link, this.ValueSetName) 
                                {
                                    LinkDestination = string.Format("{0}{1}", this.ValueSetLinkBase, Helper.GetCleanName(this.ValueSetName, 39))
                                });
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Vocabulary, " " + this.ValueSetOid));
                        }
                        else
                        {
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Vocabulary, this.ValueSetName + " " + this.ValueSetOid));
                        }

                        if (!string.IsNullOrEmpty(this.StaticDynamic))
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, " " + this.StaticDynamic));

                        if (this.ValueSetVersion != null)
                            this.parts.Add(new ConstraintPart(" " + this.ValueSetVersion.Value.ToString("yyyy-MM-dd")));

                        this.parts.Add(new ConstraintPart(")"));
                    }
                }

                // Add the contained template if specified
                if (this.ContainedTemplateId >= 0)
                {
                    if (!string.IsNullOrEmpty(this.Context))
                    {
                        this.parts.Add(new ConstraintPart(" which includes "));
                    }

                    if (this.LinkContainedTemplate)
                    {
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Link, this.ContainedTemplateTitle)
                        {
                            LinkDestination = this.ContainedTemplateLink
                        });
                    }
                    else
                    {
                        this.parts.Add(new ConstraintPart(this.ContainedTemplateTitle));
                    }

                    this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Template, " (identifier: " + this.ContainedTemplateOid + ")"));
                }

                this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Constraint, string.Format(" (CONF:{0})", this.Number))
                {
                    IsAnchor = true
                });

                if (this.IsBranch && this.HasChildren)
                {
                    this.parts.Add(new ConstraintPart(" such that it"));
                }
                else
                {
                    this.parts.Add(new ConstraintPart("."));// End the constraint with a period
                }
            }
        }

        private void ApplyStyleToElement(OpenXmlElement element)
        {
            Paragraph cPara = element as Paragraph;

            if (cPara != null)
            {
                if (cPara.ParagraphProperties == null)
                    cPara.ParagraphProperties = new ParagraphProperties();

                cPara.ParagraphProperties.ParagraphStyleId = new ParagraphStyleId()
                {
                    Val = Properties.Settings.Default.TemplateDescriptionStyle
                };

                cPara.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines()
                {
                    Before = new StringValue("120")
                };
            }
        }

        public Paragraph AddToDocParagraph(WIKIParser wikiParser, OpenXmlElement parent, int level, int id, string headingStyle)
        {
            // Add the heading
            if (this.IsHeading && !string.IsNullOrEmpty(this.Context))
            {
                Paragraph pHeading = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId()
                        {
                            Val = headingStyle
                        }),
                    DocHelper.CreateRun(this.Context));
                parent.Append(pHeading);

                if (!string.IsNullOrEmpty(this.HeadingDescription))
                {
                    OpenXmlElement parsedHeadingDescription = wikiParser.ParseAsOpenXML(this.HeadingDescription);

                    if (parsedHeadingDescription != null)
                    {
                        foreach (OpenXmlElement cParsedChild in parsedHeadingDescription.ChildElements)
                        {
                            OpenXmlElement cClonedParsedChild = cParsedChild.CloneNode(true);
                            this.ApplyStyleToElement(cClonedParsedChild);
                            parent.Append(cClonedParsedChild);
                        }
                    }
                }
            }

            // Add the description above the constraint definition
            if (!string.IsNullOrEmpty(this.Description))
            {
                OpenXmlElement parsedDescription = wikiParser.ParseAsOpenXML(this.Description);

                if (parsedDescription != null)
                {
                    foreach (OpenXmlElement cParsedChild in parsedDescription.ChildElements)
                    {
                        OpenXmlElement cClonedParsedChild = cParsedChild.CloneNode(true);
                        this.ApplyStyleToElement(cClonedParsedChild);
                        parent.Append(cClonedParsedChild);
                    }
                }
            }

            // Add the constraint definition
            Paragraph para = new Paragraph(
                new ParagraphProperties(
                    new NumberingProperties(
                        new NumberingLevelReference() { Val = level },
                        new NumberingId() { Val = id })));

            foreach (ConstraintPart cPart in this.parts)
            {
                switch (cPart.PartType)
                {
                    case ConstraintPart.PartTypes.Keyword:
                        para.Append(
                            DocHelper.CreateRun(cPart.Text, style: Properties.Settings.Default.ConformanceVerbStyle));
                        break;
                    case ConstraintPart.PartTypes.Context:
                        para.Append(
                            DocHelper.CreateRun(cPart.Text, style: Properties.Settings.Default.ConstraintContextStyle));
                        break;
                    case ConstraintPart.PartTypes.Template:
                        para.Append(
                            DocHelper.CreateRun(cPart.Text, style: Properties.Settings.Default.TemplateOidStyle));
                        break;
                    case ConstraintPart.PartTypes.Vocabulary:
                        para.Append(
                            DocHelper.CreateRun(cPart.Text, style: Properties.Settings.Default.VocabularyConstraintStyle));
                        break;
                    case ConstraintPart.PartTypes.Link:
                        para.Append(
                            DocHelper.CreateAnchorHyperlink(cPart.Text, cPart.LinkDestination, Properties.Settings.Default.LinkStyle));
                        break;
                    case ConstraintPart.PartTypes.Constraint:
                        para.Append(
                            DocHelper.CreateRun(cPart.Text, (cPart.IsAnchor ? "C_" + this.Number : string.Empty)));
                        break;
                    case ConstraintPart.PartTypes.PrimitiveText:
                        wikiParser.ParseAndAppend(cPart.Text, para, true);
                        break;
                    default:
                        para.Append(
                            DocHelper.CreateRun(cPart.Text));
                        break;
                }
            }

            // Add the label after a line break on the run to the paragraph
            if (!string.IsNullOrEmpty(this.Label))
            {
                string additionalLabel = string.Format("Note: {0}", this.Label);
                para.AppendChild(new Break());
                para.AppendChild(
                    DocHelper.CreateRun(additionalLabel));
            }

            parent.Append(para);

            return para;
        }

        public string GetPlainText(bool includeHeading = true, bool includeDescription = true, bool includeLabel = true)
        {
            StringBuilder sb = new StringBuilder();

            if (includeHeading && this.IsHeading && !string.IsNullOrEmpty(this.Context))
            {
                sb.Append("Heading: " + this.Context);

                if (!string.IsNullOrEmpty(this.HeadingDescription))
                    sb.Append(" (" + this.HeadingDescription + ")");

                sb.Append("\r\n");
            }

            if (includeDescription && !string.IsNullOrEmpty(this.Description))
            {
                sb.Append(this.Description);
                sb.Append("\r\n");
            }

            foreach (ConstraintPart cPart in this.parts)
                sb.Append(cPart.Text);

            if (includeLabel && !string.IsNullOrEmpty(this.Label))
            {
                string additionalLabel = string.Format("\r\nNote: {0}", this.Label);
                sb.Append(additionalLabel);
            }

            return sb.ToString();
        }

        public string GetHtml(WIKIParser parser, string linkBase, int constraintCount, bool includeLabel)
        {
            StringBuilder sb = new StringBuilder();

            foreach (ConstraintPart cPart in this.parts)
            {
                switch (cPart.PartType)
                {
                    case ConstraintPart.PartTypes.Keyword:
                    case ConstraintPart.PartTypes.Context:
                    case ConstraintPart.PartTypes.Template:
                    case ConstraintPart.PartTypes.Vocabulary:
                    case ConstraintPart.PartTypes.Constraint:
                        sb.Append("<b>" + cPart.Text + "</b>");
                        break;
                    case ConstraintPart.PartTypes.Link:
                        sb.Append(string.Format("<a href=\"{0}{1}\">{2}</a>", linkBase, cPart.LinkDestination, cPart.Text));
                        break;
                    case ConstraintPart.PartTypes.PrimitiveText:
                        sb.Append(parser.ParseAsHtml(cPart.Text));
                        break;
                    default:
                        sb.Append(cPart.Text);
                        break;
                }
            }

            if (includeLabel && !string.IsNullOrEmpty(this.Label))
            {
                string additionalLabel = string.Format("<br/>Note: {0}", this.Label);
                sb.Append(additionalLabel);
            }

            return sb.ToString();
        }

        internal static string MakePlural(string noun)
        {
            string[] oNounsES = new string[] { "echo", "hero", "potato", "veto" };
            string[] oNounS = new string[] { "auto", "memo", "pimento", "pro" };

            if (noun.EndsWith("y"))
            {
                return noun.Substring(0, noun.Length - 1) + "ies";
            }
            else if (noun.EndsWith("s") || noun.EndsWith("z") || noun.EndsWith("ch") || noun.EndsWith("sh") || noun.EndsWith("x"))
            {
                return noun + "es";
            }

            foreach (string cONounES in oNounsES)
            {
                if (noun.EndsWith(cONounES))
                    return noun + "es";
            }

            foreach (string cONounS in oNounS)
            {
                if (noun.EndsWith(cONounS))
                    return noun + "s";
            }

            return noun + "s";
        }

        internal static string HtmlFormatDescriptiveText(WIKIParser parser, string text)
        {
            return parser.ParseAsHtml(text);
        }

        public class ConstraintPart
        {
            public enum PartTypes
            {
                PrimitiveText,
                Context,
                Constraint,
                Link,
                Template,
                Vocabulary,
                Keyword,
                General
            }

            public bool IsAnchor { get; set; }
            public string LinkDestination { get; set; }
            public string Text { get; set; }
            public PartTypes PartType { get; set; }

            public ConstraintPart(PartTypes type, string text)
            {
                this.PartType = type;
                this.Text = text;
            }

            public ConstraintPart(string text)
            {
                this.PartType = PartTypes.General;
                this.Text = text;
            }
        }
    }
}
