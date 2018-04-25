using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using Trifolia.DB;
using Trifolia.Plugins;
using Helper = Trifolia.Shared.Helper;

namespace Trifolia.Export.MSWord
{
    public class ValueSetsExport
    {
        private int defaultMaxMembers;
        private Dictionary<string, int> valueSetMaximumMembers;
        private bool generateAsAppendix;
        private TableCollection tables;
        private MainDocumentPart mainPart;
        private IIGTypePlugin igTypePlugin;
        private HyperlinkTracker hyperlinkTracker;

        private Dictionary<ValueSet, DateTime> appendixValueSets = new Dictionary<ValueSet, DateTime>();

        public ValueSetsExport(IIGTypePlugin igTypePlugin, MainDocumentPart mainPart, HyperlinkTracker hyperlinkTracker, TableCollection tables, bool generateAsAppendix, int defaultMaxMembers, Dictionary<string, int> valueSetMaximumMembers)
        {
            this.igTypePlugin = igTypePlugin;
            this.mainPart = mainPart;
            this.hyperlinkTracker = hyperlinkTracker;
            this.tables = tables;
            this.generateAsAppendix = generateAsAppendix;
            this.defaultMaxMembers = defaultMaxMembers;
            this.valueSetMaximumMembers = valueSetMaximumMembers;
        }

        public string GetValueSetBookmark(ValueSet valueSet)
        {
            return Helper.GetCleanName(valueSet.Name, 39);
        }

        public void AddValueSet(ValueSet valueSet, DateTime bindingDate)
        {
            if (this.generateAsAppendix && this.appendixValueSets.ContainsKey(valueSet) && this.appendixValueSets[valueSet] < bindingDate)
                this.appendixValueSets.Remove(valueSet);

            if (!this.appendixValueSets.ContainsKey(valueSet))
            {
                if (!this.generateAsAppendix)
                    this.AddValueSetDetailTable(valueSet, bindingDate);

                this.appendixValueSets.Add(valueSet, bindingDate);
            }
        }

        public void AddValueSetsAppendix()
        {
            if (appendixValueSets.Count == 0)
                return;

            // Create the heading for the appendix
            Paragraph heading = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = Properties.Settings.Default.TemplateTypeHeadingStyle }),
                new Run(
                    new Text("Value Sets In This Guide")));
            this.mainPart.Document.Body.AppendChild(heading);

            if (this.generateAsAppendix)
            {
                foreach (ValueSet cValueSet in this.appendixValueSets.Keys)
                {
                    this.AddValueSetDetailTable(cValueSet, this.appendixValueSets[cValueSet]);
                }
            }
            else
            {
                this.AddValuesetListTable();
            }
        }

        private void AddValuesetListTable()
        {
            string[] headers = new string[] { "Name", "OID", "URL" };
            Table t = this.tables.AddTable("Value Sets", headers);

            foreach (ValueSet cValueSet in this.appendixValueSets.Keys.OrderBy(y => y.Name))
            {
                string valueSetIdentifier = cValueSet.GetIdentifier(this.igTypePlugin);
                string cAnchor = Helper.GetCleanName(cValueSet.Name, 39);
                OpenXmlElement urlRun = DocHelper.CreateRun("N/A");

                if (!string.IsNullOrEmpty(cValueSet.Source))
                    urlRun = this.hyperlinkTracker.CreateUrlHyperlink(this.mainPart, cValueSet.Source, cValueSet.Source, Properties.Settings.Default.TableLinkStyle);

                TableRow newRow = new TableRow(
                    new TableCell(
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }),
                            this.hyperlinkTracker.CreateHyperlink(cValueSet.Name, cAnchor, Properties.Settings.Default.TableLinkStyle))),
                    new TableCell(
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }),
                            DocHelper.CreateRun(valueSetIdentifier))),
                    new TableCell(
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }),
                            urlRun)));
                t.Append(newRow);
            }
        }

        private void AddValueSetDetailTable(ValueSet valueSet, DateTime bindingDate)
        {
            string valueSetIdentifier = valueSet.GetIdentifier(this.igTypePlugin);
            string bookmarkId = this.GetValueSetBookmark(valueSet);
            List<ValueSetMember> members = valueSet.GetActiveMembers(bindingDate);

            if (members == null || members.Count == 0)
                return;

            TableCell headingCell = new TableCell(
                new TableCellProperties()
                {
                    GridSpan = new GridSpan()
                    {
                        Val = 4
                    }
                },
                new Paragraph(
                    new ParagraphProperties(new ParagraphStyleId() { Val = Properties.Settings.Default.TableContentStyle }),
                    DocHelper.CreateRun(
                    string.Format("Value Set: {0} {1}", valueSet.Name, valueSet.GetIdentifier(igTypePlugin)))));

            if (!string.IsNullOrEmpty(valueSet.Description))
                headingCell.Append(
                    new Paragraph(
                        new ParagraphProperties(new ParagraphStyleId() { Val = Properties.Settings.Default.TableContentStyle }),
                        DocHelper.CreateRun(valueSet.Description)));

            if (!string.IsNullOrEmpty(valueSet.Source))
                headingCell.Append(
                    new Paragraph(
                        new ParagraphProperties(new ParagraphStyleId() { Val = Properties.Settings.Default.TableContentStyle }),
                        DocHelper.CreateRun("Value Set Source: "),
                        this.hyperlinkTracker.CreateUrlHyperlink(this.mainPart, valueSet.Source, valueSet.Source, Properties.Settings.Default.LinkStyle)));

            TableRow headerRow = DocHelper.CreateTableHeader("Code", "Code System", "Code System OID", "Print Name");
            List<TableCell> headerCells = headerRow.ChildElements.OfType<TableCell>().ToList();
            headerCells[0].ChildElements.OfType<TableCellProperties>().First().TableCellWidth = new TableCellWidth() { Width = "1170" };
            headerCells[1].ChildElements.OfType<TableCellProperties>().First().TableCellWidth = new TableCellWidth() { Width = "3195" };
            headerCells[2].ChildElements.OfType<TableCellProperties>().First().TableCellWidth = new TableCellWidth() { Width = "3195" };
            headerCells[3].ChildElements.OfType<TableCellProperties>().First().TableCellWidth = new TableCellWidth() { Width = "2520" };
            Table t = DocHelper.CreateTable(
                new TableRow(headingCell),
                headerRow);

            int maximumMembers = this.defaultMaxMembers;

            if (this.valueSetMaximumMembers != null && this.valueSetMaximumMembers.ContainsKey(valueSetIdentifier))
                maximumMembers = this.valueSetMaximumMembers[valueSetIdentifier];

            int count = 0;
            foreach (ValueSetMember currentMember in members)
            {
                if (count >= maximumMembers)
                    break;

                TableRow memberRow = new TableRow(
                    new TableCell(
                        new TableCellProperties()
                        {
                            TableCellWidth = new TableCellWidth() { Width = "1170" }
                        },
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId() 
                                    {
                                        Val = Properties.Settings.Default.TableContentStyle
                                    }),
                            DocHelper.CreateRun(currentMember.Code))),
                    new TableCell(
                        new TableCellProperties()
                        {
                            TableCellWidth = new TableCellWidth() { Width = "3195" }
                        },
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }),
                            DocHelper.CreateRun(currentMember.CodeSystem.Name))),
                    new TableCell(
                        new TableCellProperties()
                        {
                            TableCellWidth = new TableCellWidth() { Width = "3195" }
                        },
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }),
                            DocHelper.CreateRun(currentMember.CodeSystem.Oid))),
                    new TableCell(
                        new TableCellProperties()
                        {
                            TableCellWidth = new TableCellWidth() { Width = "2520" }
                        },
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }),
                            DocHelper.CreateRun(currentMember.DisplayName)))
                    );

                t.Append(memberRow);
                count++;
            }

            if (count <= members.Count - 1 || valueSet.IsIncomplete)
            {
                TableRow moreMembersRow = new TableRow(
                    new TableCell(
                        new TableCellProperties()
                        {
                            GridSpan = new GridSpan()
                            {
                                Val = 4
                            }
                        },
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableContentStyle
                                }),
                            DocHelper.CreateRun("..."))));
                t.Append(moreMembersRow);
            }

            this.tables.AddTable(valueSet.Name, t, bookmarkId);
        }
    }
}
