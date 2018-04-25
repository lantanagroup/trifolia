using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Trifolia.DB;
using Trifolia.Export.Versioning;
using Trifolia.Logging;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Export.MSWord
{
    public class ImplementationGuideGenerator : IDisposable
    {
        private const string CommentsStyleResource = "Trifolia.Export.MSWord.Styles.comments.xml";
        private const string EndNotesStyleResource = "Trifolia.Export.MSWord.Styles.endnotes.xml";
        private const string FontsTableStyleResource = "Trifolia.Export.MSWord.Styles.fontTable.xml";
        private const string FootNotesStyleResource = "Trifolia.Export.MSWord.Styles.footnotes.xml";
        private const string HeaderStyleResource = "Trifolia.Export.MSWord.Styles.header1.xml";
        private const string SettingsStyleResource = "Trifolia.Export.MSWord.Styles.settings.xml";
        private const string StylesStyleResource = "Trifolia.Export.MSWord.Styles.styles.xml";
        private const string StylesWithEffectsStyleResource = "Trifolia.Export.MSWord.Styles.stylesWithEffects.xml";
        private const string WebSettingsStyleResource = "Trifolia.Export.MSWord.Styles.webSettings.xml";
        private const string ThemeStyleResource = "Trifolia.Export.MSWord.Styles.theme1.xml";
        private const string NumberingStyleResource = "Trifolia.Export.MSWord.Styles.numbering.xml";
        #region Private Fields

        private ImplementationGuide implementationGuide;
        private IGSettingsManager igSettings = null;
        private IIGTypePlugin igTypePlugin = null;
        private WordprocessingDocument document = null;
        private CommentManager commentManager = null;
        private MemoryStream docStream = null;
        private List<Template> templates = null;
        private int templateCount = 0;
        private readonly IObjectRepository _tdb;
        private ValueSetsExport valueSetsExport = null;
        private TableCollection tables = null;
        private FigureCollection figures = null;
        private CodeSystemTable codeSystemTable = null;
        private TemplateConstraintTable constraintTableGenerator;
        private PublishStatus retiredStatus = null;
        private ExportSettings exportSettings = null;
        private List<ViewTemplateRelationship> templateRelationships = null;
        private List<ConstraintReference> constraintReferences = null;
        private SimpleSchema schema = null;
        private HyperlinkTracker hyperlinkTracker = null;

        #endregion

        #region Ctor

        public ImplementationGuideGenerator(IObjectRepository tdb, int implementationGuideId, IEnumerable<int> templateIds)
        {
            this._tdb = tdb;
            this.implementationGuide = tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            this.schema = this.implementationGuide.ImplementationGuideType.GetSimpleSchema();

            this.igSettings = new IGSettingsManager(this._tdb, implementationGuideId);

            this.templates = (from tid in templateIds
                               join t in tdb.Templates on tid equals t.Id
                               select t).Distinct().ToList();
            this.templateRelationships = (from tr in this._tdb.ViewTemplateRelationships
                                          join tid in templateIds on tr.ParentTemplateId equals tid
                                          where templateIds.Contains(tr.ChildTemplateId)
                                          select tr)
                                          .Union(from tr in this._tdb.ViewTemplateRelationships
                                                 join tid in templateIds on tr.ChildTemplateId equals tid
                                                 where templateIds.Contains(tr.ParentTemplateId)
                                                 select tr)
                                                 .ToList();
            this.constraintReferences = (from t in this.templates
                                         join tc in this._tdb.TemplateConstraints on t.Id equals tc.TemplateId
                                         join tcr in this._tdb.TemplateConstraintReferences on tc.Id equals tcr.TemplateConstraintId
                                         join rt in this._tdb.Templates on tcr.ReferenceIdentifier equals rt.Oid
                                         select new ConstraintReference()
                                         {
                                             TemplateConstraintId = tc.Id,
                                             Name = rt.Name,
                                             Identifier = rt.Oid,
                                             Bookmark = rt.Bookmark,
                                             IncludedInIG = this.templates.Contains(rt)
                                         }).ToList();

            this.retiredStatus = PublishStatus.GetRetiredStatus(this._tdb);
        }

        #endregion

        #region Public Methods

        public void BuildImplementationGuide(ExportSettings aModel, IIGTypePlugin igTypePlugin)
        {
            this.exportSettings = aModel;
            this.igTypePlugin = igTypePlugin;
            
            this.docStream = new MemoryStream();
            this.document = WordprocessingDocument.Create(this.docStream, WordprocessingDocumentType.Document);
            this.document.AddMainDocumentPart();

            this.SetupStyles();

            this.document.MainDocumentPart.Document =
                new Document(
                    new Body());

            this.hyperlinkTracker = new HyperlinkTracker();
            this.tables = new TableCollection(this.document.MainDocumentPart.Document.Body);
            this.constraintTableGenerator = new TemplateConstraintTable(this._tdb, this.constraintReferences, this.igSettings, igTypePlugin, this.templates, this.tables, exportSettings.SelectedCategories, this.hyperlinkTracker);
            this.figures = new FigureCollection(this.document.MainDocumentPart.Document.Body);
            this.valueSetsExport 
                = new ValueSetsExport(
                    igTypePlugin,
                    this.document.MainDocumentPart, 
                    this.hyperlinkTracker,
                    this.tables, 
                    exportSettings.GenerateValueSetAppendix, 
                    exportSettings.DefaultValueSetMaxMembers,
                    exportSettings.ValueSetMaxMembers);
            this.codeSystemTable = new CodeSystemTable(this._tdb, this.document.MainDocumentPart.Document.Body, this.templates, this.tables);

            this.AddTitlePage();

            this.AddTableOfContents();

            this.AddTemplateTypeSections();

            if (exportSettings.GenerateDocTemplateListTable || exportSettings.GenerateDocContainmentTable)
            {
                Paragraph entryLevelHeading = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = Properties.Settings.Default.TemplateTypeHeadingStyle }),
                    new Run(
                        new Text("Template Ids in This Guide")));
                this.document.MainDocumentPart.Document.Body.AppendChild(entryLevelHeading);

                if (exportSettings.GenerateDocTemplateListTable)
                {
                    // Add used template table
                    this.AddDocumentTemplateListTable();
                }

                if (exportSettings.GenerateDocContainmentTable)
                {
                    TemplateContainmentGenerator.AddTable(this._tdb, this.document, this.templateRelationships, this.templates, this.tables, this.hyperlinkTracker);
                }
            }

            this.valueSetsExport.AddValueSetsAppendix();

            this.codeSystemTable.AddCodeSystemAppendix();

            this.AddRetiredTemplatesAppendix();

            if (exportSettings.IncludeChangeList)
                this.LoadChangesAppendix();
        }

        private void AddRetiredTemplatesAppendix()
        {
            var retiredTemplates = this.templates.Where(y => y.Status == retiredStatus);

            if (retiredTemplates.Count() == 0)
                return;

            // Create the heading for the appendix
            Paragraph heading = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = Properties.Settings.Default.TemplateTypeHeadingStyle }),
                new Run(
                    new Text("Retired Templates")));
            this.document.MainDocumentPart.Document.Body.AppendChild(heading);

            // Create the table for the appendix
            string[] headers = new string[] { "Name", "OID", "Description" };
            Table t = this.tables.AddTable("Retired Templates", headers);

            foreach (var retiredTemplate in retiredTemplates)
            {
                TableRow newRow = new TableRow(
                    new TableCell(
                        new Paragraph(
                            DocHelper.CreateRun(retiredTemplate.Name))),
                    new TableCell(
                        new Paragraph(
                            DocHelper.CreateRun(retiredTemplate.Oid))),
                    new TableCell(
                        new Paragraph(
                            DocHelper.CreateRun(retiredTemplate.Description))));

                t.Append(newRow);
            }
        }

        public byte[] GetDocument()
        {
            FooterPart footerPart = this.document.MainDocumentPart.GetPartsOfType<FooterPart>().First();
            string footerPartId = this.document.MainDocumentPart.GetIdOfPart(footerPart);

            var sectionProperties = new SectionProperties(      // 1440 = 1 inch, 1728 = 1.2 inch
                    new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId },
                    new FooterReference() { Type = HeaderFooterValues.Even, Id = footerPartId },
                    new FooterReference() { Type = HeaderFooterValues.First, Id = footerPartId },
                    new PageSize() { Width = 12240, Height = 15840 },
                    new PageMargin() { Left = 1080, Right = 1080, Top = 1440, Bottom = 1728, Gutter = 0, Header = 720, Footer = 720 },
                    new Columns() { Space = "720" },
                    new DocGrid() { LinePitch = 360 });
            this.document.MainDocumentPart.Document.Body.Append(sectionProperties);

            /*
            OpenXmlValidator validator = new OpenXmlValidator(FileFormatVersions.Office2010);
            List<ValidationErrorInfo> errors = validator.Validate(this.document).ToList();
            */

            this.document.Close();

            // Get the data back from the reader now that the doc is saved/closed
            this.docStream.Position = 0;
            byte[] buffer = new byte[this.docStream.Length];
            this.docStream.Read(buffer, 0, (int)this.docStream.Length);

            return buffer;
        }
        
        #endregion

        #region Private Methods

        private void AddTitlePage()
        {
            // Title
            string title = this.implementationGuide.GetDisplayName();
            Paragraph pTitle = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId()
                    {
                        Val = "Title"
                    },
                    new OutlineLevel()
                    {
                        Val = 0
                    }),
                    new Run(
                        new Text(title)));

            this.document.MainDocumentPart.Document.Body.Append(pTitle);

            // Date
            DateTime titleDate = this.implementationGuide.PublishDate != null ? this.implementationGuide.PublishDate.Value : DateTime.Now;
            string titleDateString = titleDate.ToString("m");
            Paragraph pDate = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId()
                    {
                        Val = "SubTitle"
                    },
                    new OutlineLevel()
                    {
                        Val = 0
                    }),
                    new Run(
                        new Text(titleDateString)));
            this.document.MainDocumentPart.Document.Body.Append(pDate);

            // Page break
            Paragraph pBreak = new Paragraph(
                new Run(
                    new Break()
                    {
                        Type = BreakValues.Page
                    }));
            this.document.MainDocumentPart.Document.Body.Append(pBreak);
        }

        private void AddTableOfContents()
        {
            // Table of Contents
            this.AddTableOfContents(" TOC \\o \"1-3\" ", "TOC1", "Table of Contents", "Update this field to generate TOC");

            // Table of Figures
            this.AddTableOfContents(" TOC \\c \"Figure\" ", "TableOfFigures", "Table of Figures", "Update this field to generate TOF");

            // Table of Tables
            this.AddTableOfContents(" TOC \\c \"Table\" ", "TableOfFigures", "Table of Tables", "Update this field to generate TOT");
        }

        private void AddTableOfContents(string fieldCode, string style, string title, string displayText)
        {
            Paragraph pHeading = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId()
                    {
                        Val = "TOCTitle"
                    }),
                DocHelper.CreateRun(title));
            this.document.MainDocumentPart.Document.Body.Append(pHeading);

            Paragraph pToc = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId()
                    {
                        Val = style
                    }),
                new Run(new FieldChar()
                {
                    FieldCharType = FieldCharValues.Begin
                }),
                new Run(
                    new RunProperties(
                        new NoProof()),
                    new FieldCode(fieldCode)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    }),
                new Run(new FieldChar()
                {
                    FieldCharType = FieldCharValues.Separate
                }),
                new Run(
                    new Text(displayText)),
                new Run(new FieldChar()
                {
                    FieldCharType = FieldCharValues.End
                }));
            this.document.MainDocumentPart.Document.Body.Append(pToc);
        }

        /// <summary>
        /// If this IG is a new version of a previous IG, scan the templates for new versions and add them as an appendix
        /// </summary>
        private void LoadChangesAppendix()
        {
            List<IGDifferenceViewModel> lDifferences = new List<IGDifferenceViewModel>();

            if (this.implementationGuide.PreviousVersionImplementationGuideId.HasValue)
            {
                VersionComparer lComparer = VersionComparer.CreateComparer(_tdb, this.igTypePlugin, this.igSettings);
                var versionedTemplates = this.templates.Where(y => y.OwningImplementationGuideId == this.implementationGuide.Id);

                foreach (Template lChildTemplate in versionedTemplates)
                {
                    IGDifferenceViewModel lModel = new IGDifferenceViewModel(lChildTemplate);

                    if (lChildTemplate.PreviousVersionTemplateId.HasValue)
                    {
                        Template lPreviousTemplate = _tdb.Templates.Single(t => t.Id == lChildTemplate.PreviousVersionTemplateId);
                        lModel.Difference = lComparer.Compare(lPreviousTemplate, lChildTemplate);
                        lDifferences.Add(lModel);
                    }
                }

                this.LoadDifferencesAsAppendix(this.implementationGuide.Name, lDifferences);
            }
        }

        private void LoadDifferencesAsAppendix(string aIgName, List<IGDifferenceViewModel> aDifferences)
        {
            // Create the heading for the appendix
            Paragraph heading = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = Properties.Settings.Default.TemplateTypeHeadingStyle }),
                new Run(
                    new Text("Changes from Previous Version")));
            this.document.MainDocumentPart.Document.Body.AppendChild(heading);

            foreach (IGDifferenceViewModel lDifference in aDifferences)
            {
                Paragraph changeHeadingPara = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = Properties.Settings.Default.TemplateHeaderStyle }),
                        DocHelper.CreateRun(lDifference.TemplateName));
                this.document.MainDocumentPart.Document.Body.AppendChild(changeHeadingPara);

                OpenXmlElement templateTitleElement = DocHelper.CreateRun(string.Format("{0} ({1})", lDifference.TemplateName, lDifference.TemplateOid));

                if (lDifference.ShouldLink)
                    templateTitleElement = this.hyperlinkTracker.CreateHyperlink(
                        string.Format("{0} ({1})", lDifference.TemplateName, lDifference.TemplateOid), 
                        lDifference.TemplateBookmark, 
                        Properties.Settings.Default.LinkStyle);

                Paragraph changeLinkPara = new Paragraph(
                    new ParagraphProperties(new KeepNext()),
                    templateTitleElement,
                    new Run(
                        new Break()));
                this.document.MainDocumentPart.Document.Body.AppendChild(changeLinkPara);

                Table t = this.tables.AddTable(null, new string[] { "Change", "Old", "New" });

                // Show template field changes first
                foreach (ComparisonFieldResult lResult in lDifference.Difference.ChangedFields)
                {
                    TableRow memberRow = new TableRow(
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                DocHelper.CreateRun(lResult.Name))),
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                DocHelper.CreateRun(lResult.Old))),
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                DocHelper.CreateRun(lResult.New)))
                        );

                    t.Append(memberRow);
                }

                // Show constraint changes second
                foreach (ComparisonConstraintResult lConstraintChange in lDifference.Difference.ChangedConstraints)
                {
                    if (lConstraintChange.Type == CompareStatuses.Unchanged)
                        continue;

                    TableRow memberRow = new TableRow(
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                DocHelper.CreateRun(string.Format("CONF #: {0} {1}", lConstraintChange.Number, lConstraintChange.Type)))),
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                DocHelper.CreateRun(lConstraintChange.OldNarrative))),
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                DocHelper.CreateRun(lConstraintChange.NewNarrative)))
                        );

                    t.Append(memberRow);
                }
            }
        }

        /// <summary>
        /// Adds all templates for the current implementation guide by section.
        /// Loops through each template type, sorting by the OutputOrder specified on the templateType within the 
        /// ImplementationGuideType. For each template type, all templates associated with that type are output
        /// in the order specified by the generation options.
        /// </summary>
        private void AddTemplateTypeSections()
        {
            Log.For(this).Debug("Adding {0} templates for IG '{1}'", templates.Count(), this.implementationGuide.Id);

            var templateTypes = this.igSettings.TemplateTypes.OrderBy(y => y.OutputOrder).ThenBy(y => y.Name);

            // For each template type defined for the IG or the IG Type create a new section
            foreach (IGSettingsManager.IGTemplateType templateType in templateTypes)
            {
                List<Template> cTemplates = this.GetTemplatesForTemplateType(templateType.TemplateTypeId);

                if (cTemplates.Count > 0)
                    this.AddTemplateTypeSection(templateType.Name, templateType.DetailsText, cTemplates);
            }
        }

        /// <summary>
        /// Outputs a primary heading for the template type followed by each of the templates within 
        /// the template type (defined by the templates parameter).
        /// </summary>
        /// <param name="typeName">The template type's name</param>
        /// <param name="detailsText">The details text describing the template type.</param>
        /// <param name="templates">The templates contained within the template type for this ig.</param>
        private void AddTemplateTypeSection(string typeName, string detailsText, List<Template> templates)
        {
            var notRetiredTemplates = templates.Where(y => y.Status != this.retiredStatus);

            if (notRetiredTemplates.Count() > 0)
            {
                // Add a Entry Level Templates heading
                Paragraph entryLevelHeading = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = Properties.Settings.Default.TemplateTypeHeadingStyle }),
                    new Run(
                        new Text(typeName)));
                this.document.MainDocumentPart.Document.Body.AppendChild(entryLevelHeading);

                if (!string.IsNullOrEmpty(detailsText))
                {
                    OpenXmlElement element = detailsText.MarkdownToOpenXml(this.document.MainDocumentPart);
                    OpenXmlHelper.Append(element, this.document.MainDocumentPart.Document.Body);
                }

                foreach (Template cTemplate in notRetiredTemplates)
                {
                    this.AddTemplate(cTemplate);
                }
            }
        }

        private List<Template> GetTemplatesForTemplateType(long templateTypeId)
        {
            if (!exportSettings.AlphaHierarchicalOrder)
            {
                return templates
                    .Where(y => y.TemplateTypeId == templateTypeId)
                    .OrderBy(y => y.Name)
                    .ToList();
            }

            List<Template> orderedTemplates = new List<Template>();
            List<Template> rootTemplates = this.templates
                .Where(y => y.TemplateTypeId == templateTypeId)
                .Where(y => y.ImpliedTemplateId == null || !this.templates.Exists(z => z.Id == y.ImpliedTemplateId))
                .OrderBy(y => y.Name)
                .ToList();

            rootTemplates.ForEach(y => LoadChildTemplates(orderedTemplates, y));

            return orderedTemplates;
        }

        private void LoadChildTemplates(List<Template> orderedTemplates, Template parentTemplate)
        {
            orderedTemplates.Add(parentTemplate);

            List<Template> childTemplates = this.templates
                .Where(y => y.TemplateTypeId == parentTemplate.TemplateTypeId)
                .Where(y => y.ImpliedTemplateId == parentTemplate.Id)
                .OrderBy(y => y.Name)
                .ToList();

            childTemplates.ForEach(y =>
            {
                if (y.ImpliedTemplateId != parentTemplate.ImpliedTemplateId)
                    LoadChildTemplates(orderedTemplates, y);
            });
        }

        /// <summary>
        /// Adds a single template to the implementation guide document.
        /// </summary>
        /// <param name="template">The template to add to the document</param>
        private void AddTemplate(Template template)
        {
            Log.For(this).Trace("BEGIN: Adding template '{0}'.", template.Oid);

            List<TemplateConstraint> templateConstraints = (from tc in this._tdb.TemplateConstraints
                                                            where tc.TemplateId == template.Id
                                                            select tc).ToList();
            List<TemplateConstraint> rootConstraints = templateConstraints
                .Where(y => y.ParentConstraintId == null)
                .OrderBy(y => y.Order)
                .ToList();
            GreenTemplate greenTemplate = template.GreenTemplates.FirstOrDefault();
            string bookmarkId = template.Bookmark;
            string templateIdentifier = string.Format("identifier: {0}", template.Oid);

            if (!string.IsNullOrEmpty(template.PrimaryContext))
                templateIdentifier = string.Format("{0} (identifier: {1})", template.PrimaryContext, template.Oid);

            this.templateCount++;

            string headingLevel = Properties.Settings.Default.TemplateHeaderStyle;

            if (exportSettings.AlphaHierarchicalOrder && template.ImpliedTemplateId != null && this.templates.Exists(y => y.Id == template.ImpliedTemplateId))
                headingLevel = Properties.Settings.Default.TemplateHeaderSecondLevelStyle;

            StringBuilder lTitleBuilder = new StringBuilder(string.Format("{0}", template.Name.Substring(1)));

            bool lDirectlyOwnedTemplate = template.OwningImplementationGuideId == this.implementationGuide.Id;
            bool lStatusMatches = template.StatusId == template.OwningImplementationGuide.PublishStatusId;

            string lTemplateTitle = lTitleBuilder.ToString();

            // Output the title of the template
            Paragraph pHeading = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = headingLevel }));

            this.hyperlinkTracker.AddAnchorAround(pHeading, bookmarkId, 
                new Run(
                    new Text(template.Name.Substring(0, 1))),
                new Run(
                    new Text(lTemplateTitle)));

            if (!string.IsNullOrEmpty(template.Notes) && this.exportSettings.IncludeNotes)
                this.commentManager.AddCommentRange(pHeading, template.Notes);

            this.document.MainDocumentPart.Document.Body.AppendChild(pHeading);

            // Output the "bracket data" for the template
            string detailsText = string.Format("identifier: {0} ({1})", template.Oid, template.IsOpen == true ? "open" : "closed");

            if (!string.IsNullOrEmpty(template.PrimaryContext))
                detailsText = string.Format("[{0}: identifier {1} ({2})]",
                 template.PrimaryContext,
                 template.Oid,
                 template.IsOpen == true ? "open" : "closed");

            Paragraph pDetails = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId()
                    {
                        Val = Properties.Settings.Default.TemplateLocationStyle
                    }),
                DocHelper.CreateRun(detailsText));
            this.document.MainDocumentPart.Document.Body.AppendChild(pDetails);

            //Output IG publish/draft info with "bracket data" format
            if (exportSettings.IncludeTemplateStatus)
            {
                string status = template.Status != null ? template.Status.Status : "Draft";
                string igText = string.Format("{0} as part of {1}", status, template.OwningImplementationGuide.GetDisplayName());

                Paragraph igDetails = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId()
                        {
                            Val = Properties.Settings.Default.TemplateLocationStyle
                        }),
                    DocHelper.CreateRun(igText));
                this.document.MainDocumentPart.Document.Body.AppendChild(igDetails);
            }

            // If we were told to generate context tables for the template...
            if (exportSettings.GenerateTemplateContextTable)
                TemplateContextTable.AddTable(this._tdb, this.templateRelationships, this.tables, this.document.MainDocumentPart.Document.Body, template, this.templates, this.hyperlinkTracker);

            // Output the template's descriptionz
            if (!string.IsNullOrEmpty(template.Description))
            {
                OpenXmlElement element = template.Description.MarkdownToOpenXml(this.document.MainDocumentPart);
                OpenXmlHelper.Append(element, this.document.MainDocumentPart.Document.Body);
            }

            // If we were told to generate tables for the template...
            if (exportSettings.GenerateTemplateConstraintTable)
                this.constraintTableGenerator.AddTemplateConstraintTable(this.schema, template, this.document.MainDocumentPart.Document.Body, templateIdentifier);

            if (templateConstraints.Count(y => y.IsHeading) > 0)
            {
                Paragraph propertiesHeading = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = Properties.Settings.Default.PropertiesHeadingStyle }),
                    DocHelper.CreateRun("Properties"));
                this.document.MainDocumentPart.Document.Body.AppendChild(propertiesHeading);
            }

            // Output the implied template conformance line
            if (template.ImpliedTemplate != null)
            {
                OpenXmlElement templateReference = !this.templates.Contains(template.ImpliedTemplate) ? 
                    (OpenXmlElement) DocHelper.CreateRun(template.ImpliedTemplate.Name) :
                    (OpenXmlElement) this.hyperlinkTracker.CreateHyperlink(template.ImpliedTemplate.Name, template.ImpliedTemplate.Bookmark, Properties.Settings.Default.LinkStyle);

                Paragraph impliedConstraint = new Paragraph(
                    new ParagraphProperties(
                        new NumberingProperties(
                            new NumberingLevelReference() { Val = 0 },
                            new NumberingId() { Val = GenerationConstants.BASE_TEMPLATE_INDEX + (int)template.Id })),
                    DocHelper.CreateRun("Conforms to "),
                    templateReference,
                    DocHelper.CreateRun(" template "),
                    DocHelper.CreateRun("(identifier: " + template.ImpliedTemplate.Oid + ")", style: Properties.Settings.Default.TemplateOidStyle),
                    DocHelper.CreateRun("."));
                this.document.MainDocumentPart.Document.Body.Append(impliedConstraint);
            }

            bool lCreateValueSetTables = exportSettings.DefaultValueSetMaxMembers > 0;

            IConstraintGenerator constraintGenerator = ConstraintGenerationFactory.NewConstraintGenerator(
                this.igSettings,
                this.document.MainDocumentPart.Document.Body,
                this.commentManager,
                this.figures,
                exportSettings.IncludeXmlSamples,
                _tdb,
                rootConstraints,
                templateConstraints,
                template,
                this.templates,
                Properties.Settings.Default.ConstraintHeadingStyle,
                exportSettings.SelectedCategories,
                this.hyperlinkTracker);
            constraintGenerator.GenerateConstraints(lCreateValueSetTables, this.exportSettings.IncludeNotes);

            // Add value-set tables
            if (lCreateValueSetTables)
            {
                var constraintValueSets = (from c in templateConstraints
                                           where c.ValueSet != null
                                           select new { ValueSet = c.ValueSet, ValueSetDate = c.ValueSetDate })
                                          .Distinct();

                foreach (var cConstraintValueSet in constraintValueSets)
                {
                    DateTime? bindingDate = cConstraintValueSet.ValueSetDate != null ? cConstraintValueSet.ValueSetDate : this.implementationGuide.PublishDate;

                    if (bindingDate == null)
                        bindingDate = DateTime.Now;

                    this.valueSetsExport.AddValueSet(cConstraintValueSet.ValueSet, bindingDate.Value);
                }
            }

            if (exportSettings.IncludeXmlSamples)
            {
                foreach (var lSample in template.TemplateSamples.OrderBy(y => y.Id))
                {
                    this.figures.AddSample(lSample.Name, lSample.XmlSample);
                }
            }

            Log.For(this).Trace("END: Adding template '{0}' with {1} constraints.", template.Oid, templateConstraints.Count);
        }

        private void AddDocumentTemplateListTable()
        {
            string[] headers = new string[] { GenerationConstants.USED_TEMPLATE_TABLE_TITLE, GenerationConstants.USED_TEMPLATE_TABLE_TYPE, GenerationConstants.USED_TEMPLATE_TABLE_ID };
            Table t = this.tables.AddTable("Template List", headers);

            List<Template> sortedTemplates = this.templates
                .OrderBy(y => y.TemplateTypeId)
                .ThenBy(y => y.Name)
                .ToList();

            foreach (Template cTemplate in sortedTemplates)
            {
                OpenXmlElement titleElement = DocHelper.CreateRun(cTemplate.Name);

                if (cTemplate.Status != this.retiredStatus)
                    titleElement = this.hyperlinkTracker.CreateHyperlink(cTemplate.Name, cTemplate.Bookmark, Properties.Settings.Default.TableLinkStyle);

                t.AppendChild(
                    new TableRow(
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                titleElement)),
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                new Run(
                                    new Text(cTemplate.TemplateType.Name) { Space = SpaceProcessingModeValues.Preserve }))),
                        new TableCell(
                            new Paragraph(
                                new ParagraphProperties(
                                    new ParagraphStyleId()
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                                new Run(
                                    new Text(cTemplate.Oid) { Space = SpaceProcessingModeValues.Preserve })))));

            }
        }

        private void SetupStyles()
        {
            WordprocessingCommentsPart commentsPart = AddTemplatePart<WordprocessingCommentsPart>(this.document, CommentsStyleResource);
            EndnotesPart endNotesPart = AddTemplatePart<EndnotesPart>(this.document, EndNotesStyleResource);
            FontTablePart fontTablePart = AddTemplatePart<FontTablePart>(this.document, FontsTableStyleResource);
            FootnotesPart footnotesPart = AddTemplatePart<FootnotesPart>(this.document, FootNotesStyleResource);
            HeaderPart headerPart = AddTemplatePart<HeaderPart>(this.document, HeaderStyleResource);
            DocumentSettingsPart settingsPart = AddTemplatePart<DocumentSettingsPart>(this.document, SettingsStyleResource);
            StyleDefinitionsPart styles = AddTemplatePart<StyleDefinitionsPart>(this.document, StylesStyleResource);
            StylesWithEffectsPart stylesWithEffectsPart = AddTemplatePart<StylesWithEffectsPart>(this.document, StylesWithEffectsStyleResource);
            WebSettingsPart webSettingsPart = AddTemplatePart<WebSettingsPart>(this.document, WebSettingsStyleResource);
            ThemePart themePart = AddTemplatePart<ThemePart>(this.document, ThemeStyleResource);
            NumberingDefinitionsPart numberingPart = AddTemplatePart<NumberingDefinitionsPart>(this.document, NumberingStyleResource);

            // Initialize the comments manager with the comments part
            this.commentManager = new CommentManager(commentsPart.Comments);

            // Initialize the footer
            string footerTitle = this.implementationGuide.GetDisplayName();
            DateTime footerDate = this.implementationGuide.PublishDate != null ? this.implementationGuide.PublishDate.Value : DateTime.Now;
            string footerDateString = footerDate.ToString("m");
            FooterPart newFooterPart = this.document.MainDocumentPart.AddNewPart<FooterPart>();
            Footer newFooter = new Footer();
            Paragraph pFooter = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId()
                    {
                        Val = "Footer"
                    }));
            pFooter.Append(
                new Run(
                    new Text(footerTitle)),
                new Run(
                    new TabChar(),
                    new Text(footerDateString)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    }),
                new Run(
                    new TabChar(),
                    new Text("Page ")
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    }),
                new Run(
                    new FieldChar()
                    {
                        FieldCharType = FieldCharValues.Begin
                    }),
                new Run(
                    new FieldCode(" PAGE ")
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    }),
                new Run(
                    new FieldChar()
                    {
                        FieldCharType = FieldCharValues.Separate
                    }),
                new Run(
                    new RunProperties(
                        new NoProof()),
                    new Text("54")),
                new Run(
                    new FieldChar()
                    {
                        FieldCharType = FieldCharValues.End
                    }));
            newFooter.Append(pFooter);
            newFooterPart.Footer = newFooter;

            // Add numbering for templates
            foreach (Template cTemplate in this.templates)
            {
                NumberingInstance ni = new NumberingInstance(
                    new AbstractNumId()
                    {
                        Val = 3
                    })
                {
                    NumberID = GenerationConstants.BASE_TEMPLATE_INDEX + (int)cTemplate.Id
                };

                for (int i = 0; i < 9; i++)
                {
                    ni.Append(new LevelOverride(
                        new StartOverrideNumberingValue()
                        {
                            Val = 1
                        })
                    {
                        LevelIndex = i
                    });
                }

                numberingPart.Numbering.Append(ni);
            }
        }

        private static string GetEmbeddedContent(string name)
        {
            Stream styleStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name);

            using (StreamReader reader = new StreamReader(styleStream))
            {
                return reader.ReadToEnd();
            }
        }

        private static T AddTemplatePart<T>(WordprocessingDocument document, string resource) where T : OpenXmlPart, IFixedContentTypePart
        {
            T part = document.MainDocumentPart.AddNewPart<T>();

            string content = GetEmbeddedContent(resource);

            using (StreamWriter sw = new StreamWriter(part.GetStream()))
            {
                sw.Write(content);
            }

            return part;
        }

        public void Dispose()
        {
            if (this.docStream != null)
            {
                this.docStream.Close();
                this.docStream.Dispose();
            }
        }

        #endregion
    }
}
