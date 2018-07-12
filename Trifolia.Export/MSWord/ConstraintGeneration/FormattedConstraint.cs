using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trifolia.DB;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Export.MSWord.ConstraintGeneration
{
    public class FormattedConstraint : IFormattedConstraint
    {
        public FormattedConstraint(
            IObjectRepository tdb,
            IGSettingsManager igSettings,
            IIGTypePlugin igTypePlugin,
            TemplateConstraint constraint,
            List<ConstraintReference> references = null,
            string templateLinkBase = null,
            string valueSetLinkBase = null,
            bool linkContainedTemplate = false,
            bool linkIsBookmark = false,
            bool createLinksForValueSets = false,
            bool includeCategory = true)
        {
            if (references == null)
            {
                references = (from tcr in tdb.TemplateConstraintReferences
                              join t in tdb.Templates on tcr.ReferenceIdentifier equals t.Oid
                              where tcr.TemplateConstraintId == constraint.Id
                              select new ConstraintReference()
                              {
                                  Bookmark = t.Bookmark,
                                  Identifier = t.Oid,
                                  Name = t.Name,
                                  TemplateConstraintId = tcr.TemplateConstraintId
                              }).ToList();
            }
            else
            {
                references = references.Where(y => y.TemplateConstraintId == constraint.Id).ToList();
            }

            this.Tdb = tdb;
            this.IgSettings = igSettings;
            this.IncludeCategory = includeCategory;
            this.TemplateLinkBase = templateLinkBase;
            this.ValueSetLinkBase = valueSetLinkBase;
            this.LinkContainedTemplate = linkContainedTemplate;
            this.LinkIsBookmark = linkIsBookmark;
            this.CreateLinkForValueSets = createLinksForValueSets;
            this.ConstraintReferences = references;

            // Set the properties in the FormattedConstraint based on the IConstraint
            ParseConstraint(igTypePlugin, constraint, constraint.ValueSet, constraint.CodeSystem);

            // Pre-process the constraint so that calls to GetHtml(), GetPlainText(), etc. returns something
            ParseFormattedConstraint();

            this.parts = new List<ConstraintPart>();
            this.ConstraintReferences = new List<ConstraintReference>();
        }

        private List<ConstraintPart> parts;
        private IGSettingsManager igSettings;
        private IObjectRepository tdb;
        private int indexInSiblings;
        private int childCount;

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

        public string Category { get; set; }
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
        public bool IsChoice { get; set; }
        public bool ParentIsChoice { get; set; }
        public List<ConstraintReference> ConstraintReferences { get; set; }

        #endregion

        public void ParseConstraint(IIGTypePlugin igTypePlugin, IConstraint constraint, ValueSet valueSet = null, CodeSystem codeSystem = null)
        {
            this.childCount = constraint.Children != null ? constraint.Children.Count() : 0;

            this.Category = constraint.Category;
            this.Number = constraint.GetFormattedNumber(this.igSettings == null ? null : this.igSettings.PublishDate);
            this.Context = constraint.Context;
            this.IsPrimitive = constraint.IsPrimitive == true;
            this.IsBranch = constraint.IsBranch == true;
            this.HasChildren = constraint.Children != null && constraint.Children.Count() > 0;
            this.Narrative = constraint.PrimitiveText;
            this.Description = constraint.Description;
            this.Label = constraint.Label;
            this.IsHeading = constraint.IsHeading;
            this.HeadingDescription = constraint.HeadingDescription;
            this.IsChoice = constraint.IsChoice;
            this.Conformance = constraint.Conformance;
            this.Cardinality = constraint.Cardinality;
            this.DataType = constraint.DataType;

            if (constraint.Parent != null)
            {
                var parent = constraint.Parent;

                // Ignore the fact that the parent is a choice if this is the only constraint within the parent...
                // The parent is ignored in this case, and this constraint should be treated as a normal constraint
                if (parent.IsChoice && parent.Children.Count() == 1)
                {
                    this.Conformance = parent.Conformance;
                    this.Cardinality = parent.Cardinality;

                    if (parent.Parent != null)
                        parent = constraint.Parent.Parent;
                }

                this.ParentIsBranch = parent.IsBranch == true;
                this.ParentContext = parent.Context;
                this.ParentCardinality = parent.Cardinality;
                this.ParentIsChoice = parent.IsChoice;

                this.indexInSiblings = parent.Children.OrderBy(y => y.Order).ToList().IndexOf(constraint);
            }

            this.ValueConformance = constraint.ValueConformance;

            if (constraint.IsStatic == true)
                this.StaticDynamic = "STATIC";
            else if (constraint.IsStatic == false)
                this.StaticDynamic = "DYNAMIC";

            if (constraint.ValueSetId != null)
            {
                // If the caller didn't pass in the ValueSet, get it from the db
                if (valueSet == null && constraint is TemplateConstraint)
                    valueSet = ((TemplateConstraint)constraint).ValueSet;
                else if (valueSet == null || valueSet.Id != constraint.ValueSetId)
                    valueSet = this.tdb.ValueSets.Single(y => y.Id == constraint.ValueSetId);

                this.ValueSetName = valueSet.Name;
                this.ValueSetOid = valueSet.GetIdentifier(igTypePlugin);
                this.ValueSetVersion = constraint.ValueSetDate;
            }

            if (constraint.ValueCodeSystemId != null)
            {
                // If the caller didn't pass in the CodeSystem, get it from the db
                if (codeSystem == null && constraint is TemplateConstraint)
                    codeSystem = ((TemplateConstraint)constraint).CodeSystem;
                else if (codeSystem == null || codeSystem.Id != constraint.ValueCodeSystemId)
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

            if (this.IncludeCategory && !string.IsNullOrEmpty(this.Category))
            {
                this.parts.Add(
                    new ConstraintPart(ConstraintPart.PartTypes.Keyword, "[" + this.Category + "] "));
            }

            // Make sure we don't process contained template constraints as 
            // primitives simply because a context is not specified
            if (this.ConstraintReferences != null && this.ConstraintReferences.Count > 0)
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
                if (this.ParentIsChoice)
                {
                    if (this.indexInSiblings > 0)
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.General, "or "));

                    this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Keyword, this.Context));

                    if (this.IsBranch && this.HasChildren)
                    {
                        this.parts.Add(new ConstraintPart(" such that it"));
                    }
                }
                else
                {

                    // If we have defined a contained template, then ignore the context.
                    if (this.ConstraintReferences != null && this.ConstraintReferences.Count > 0)
                        this.Context = null;

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
                            this.parts.Add(new ConstraintPart("Such " + this.ParentContext.MakePlural() + " "));
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
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Context, this.Context));
                }


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
                if (!string.IsNullOrEmpty(this.Context) && (this.Context.ToLower() == "code" || this.Context.ToLower() == "value") && !string.IsNullOrEmpty(this.DataType))
                    this.parts.Add(new ConstraintPart(string.Format(" with @xsi:type=\"{0}\"", this.DataType)));

                if (this.IsChoice)
                {
                    this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.General, ", where "));
                    this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Context, this.Context));
                    this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.General, " is"));

                    if (this.childCount > 1)
                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.General, " one of"));
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
                            { LinkDestination = Helper.GetCleanName(this.ValueSetName, 39) });
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
                            { LinkDestination = Helper.GetCleanName(this.ValueSetName, 39) });
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

                // Add the contained template(s) if specified
                if (this.ConstraintReferences != null && this.ConstraintReferences.Count > 0)
                {
                    if (!string.IsNullOrEmpty(this.Context))
                        this.parts.Add(new ConstraintPart(" which includes "));

                    for (var i = 0; i < this.ConstraintReferences.Count; i++)
                    {
                        var constraintReference = this.ConstraintReferences[i];

                        if (i > 0)
                            this.parts.Add(new ConstraintPart(" or "));

                        if (this.LinkContainedTemplate)
                        {
                            this.parts.Add(new ConstraintPart(" "));
                            this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Link, constraintReference.Name)
                            {
                                LinkDestination = constraintReference.GetLink(this.LinkIsBookmark, this.TemplateLinkBase)
                            });
                        }
                        else
                        {
                            this.parts.Add(new ConstraintPart(constraintReference.Name));
                        }

                        this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Template, " (identifier: " + constraintReference.Identifier + ")"));
                    }
                }

                this.parts.Add(new ConstraintPart(ConstraintPart.PartTypes.Constraint, string.Format(" (CONF:{0})", this.Number))
                {
                    IsAnchor = true
                });

                if (this.IsBranch && this.HasChildren)
                {
                    this.parts.Add(new ConstraintPart(" such that it"));
                }
                else if (!this.IsChoice && !this.ParentIsChoice)
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

        public Paragraph AddToDocParagraph(MainDocumentPart mainPart, HyperlinkTracker hyperlinkTracker, OpenXmlElement parent, int level, int id, string headingStyle)
        {
            // Add the heading
            if (this.IsHeading && !string.IsNullOrEmpty(this.Context))
            {
                string headingTitle = this.Context;

                if (!string.IsNullOrEmpty(this.Category))
                    headingTitle += " for " + this.Category;

                Paragraph pHeading = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId()
                        {
                            Val = headingStyle
                        }),
                    DocHelper.CreateRun(headingTitle));
                parent.Append(pHeading);

                if (!string.IsNullOrEmpty(this.HeadingDescription))
                {
                    OpenXmlElement parsedHeadingDescription = this.HeadingDescription.MarkdownToOpenXml(this.tdb, mainPart);

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
                OpenXmlElement parsedDescription = this.Description.MarkdownToOpenXml(this.tdb, mainPart);

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
                        hyperlinkTracker.AddHyperlink(para, cPart.Text, cPart.LinkDestination, Properties.Settings.Default.LinkStyle);
                        break;
                    case ConstraintPart.PartTypes.Constraint:
                        var newRun = DocHelper.CreateRun(cPart.Text);

                        if (cPart.IsAnchor)
                            hyperlinkTracker.AddAnchorAround(para, "C_" + this.Number, newRun);
                        else
                            para.Append(newRun);

                        break;
                    case ConstraintPart.PartTypes.PrimitiveText:
                        var element = cPart.Text.MarkdownToOpenXml(this.tdb, mainPart, styleKeywords: true);
                        OpenXmlHelper.Append(element, para);
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
                para.AppendChild(
                    new Run(
                        new Break()));
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

        public string GetHtml(string linkBase, int constraintCount, bool includeLabel)
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
                        sb.Append(cPart.Text.MarkdownToHtml());
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