using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using Trifolia.Export.MSWord.Models;

namespace Trifolia.Export.MSWord
{
    public enum CaptionTypes
    {
        Table,
        Figure
    }

    public class DocHelper
    {
        #region Private Constants
        
        /// <summary>
        /// Represents 1 inch in DXA units
        /// </summary>
        private const double DOCUMENT_INCH_IN_DXA = 1440;

        #endregion

        public static Paragraph CreateCaption(int count, string title, string caption, CaptionTypes captionType, string bookmarkId = null)
        {
            FieldCode fieldCode = null;

            if (captionType == CaptionTypes.Table)
                fieldCode = new FieldCode(" SEQ Table \\* ARABIC ");
            else if (captionType == CaptionTypes.Figure)
                fieldCode = new FieldCode(" SEQ Figure \\* ARABIC ");

            Paragraph p3 = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = Properties.Settings.Default.TableCaptionStyle }),
                DocHelper.CreateRun(caption),
                new Run(
                    new FieldChar()
                    {
                        FieldCharType = new EnumValue<FieldCharValues>(FieldCharValues.Begin)
                    }),
                new Run(fieldCode),
                new Run(
                    new FieldChar()
                    {
                        FieldCharType = new EnumValue<FieldCharValues>(FieldCharValues.Separate)
                    }),
                DocHelper.CreateRun(count.ToString(), bookmarkId),
                new Run(
                    new FieldChar()
                    {
                        FieldCharType = new EnumValue<FieldCharValues>(FieldCharValues.End)
                    }),
                DocHelper.CreateRun(": " + title));

            return p3;
        }

        public static Paragraph CreateTableCaption(int tableCount, string title, string bookmarkId=null, string caption = "Table ")
        {
            return CreateCaption(++tableCount, title, caption, CaptionTypes.Table, bookmarkId);
        }

        public static Paragraph CreateFigureCaption(int figureCount, string title, string bookmarkId = null, string caption = "Figure ")
        {
            return CreateCaption(++figureCount, title, caption, CaptionTypes.Figure, bookmarkId);
        }

        public static Table CreateTable(params OpenXmlElement[] children)
        {
            return CreateTable("10080", 500, children);
        }

        internal static Table CreateTable(params HeaderDescriptor[] headers)
        {
            Table table = CreateTable(new OpenXmlElement[] { });
            TableProperties lProperties = table.FirstChild as TableProperties;

            if (lProperties != null)
            {
                lProperties.TableStyle = new TableStyle() { Val = new StringValue("TableGrid") };
            }

            TableGrid lGrid = new TableGrid();
            table.Append(lGrid);

            foreach (HeaderDescriptor lDescriptor in headers)
            {
                GridColumn lColumn = new GridColumn() { Width = new StringValue() { Value = lDescriptor.ColumnWidth } };
                lGrid.Append(lColumn);
            }

            TableRow lRow = new TableRow();
            table.Append(lRow);

            foreach (HeaderDescriptor lDescriptor in headers)
            {
                TableCellProperties lCellProperties = new TableCellProperties()
                {
                    // Apply 10% gray to table headers
                    Shading = new Shading()
                    {
                        Val = new EnumValue<ShadingPatternValues>(ShadingPatternValues.Clear),
                        Color = new StringValue("auto"),
                        Fill = new StringValue("E6E6E6")
                    }
                };

                if (!lDescriptor.AutoWrap) lCellProperties.NoWrap = new NoWrap();

                if (lDescriptor.AutoResize)
                {
                    TableCellWidth lWidth = new TableCellWidth() { Type = TableWidthUnitValues.Auto };
                    lCellProperties.TableCellWidth = lWidth;
                }
                else
                {
                    TableCellWidth lWidth = new TableCellWidth() 
                    { 
                        Type = TableWidthUnitValues.Dxa, 
                        Width = new StringValue((lDescriptor.CellWidth * DOCUMENT_INCH_IN_DXA).ToString()) 
                    };
                    lCellProperties.TableCellWidth = lWidth;
                }

                TableCell lCell = new TableCell() { TableCellProperties = lCellProperties };

                Paragraph lHeaderParagraph = new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableHeadingStyle
                                }),
                            new Run(
                                new RunProperties() { Bold = new Bold() },
                                new Text(lDescriptor.HeaderName) { Space = SpaceProcessingModeValues.Preserve }));
                lCell.Append(lHeaderParagraph);
                lRow.Append(lCell);
            }

            return table;
        }

        public static Table CreateTable(string aTableWidth, int aTableIndentation, params OpenXmlElement[] children)
        {
            Table table = new Table(
                new TableProperties()
                {
                    TableLayout = new TableLayout()
                    {
                        Type = TableLayoutValues.Fixed  
                    },
                    TableJustification = new TableJustification(){
                        Val = TableRowAlignmentValues.Center
                    },
                    TableWidth = new TableWidth()
                    {
                        Width = new StringValue(aTableWidth),
                        Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Dxa)
                    },
                    TableIndentation = new TableIndentation()
                    {
                        Width = new Int32Value(aTableIndentation),
                        Type = new EnumValue<TableWidthUnitValues>(TableWidthUnitValues.Dxa)
                    },
                    TableBorders = DocHelper.CreateTableBorder(),
                    TableLook = new TableLook()
                    {
                        FirstRow = new OnOffValue(true),
                        LastRow = new OnOffValue(false),
                        FirstColumn = new OnOffValue(true),
                        LastColumn = new OnOffValue(false),
                        NoVerticalBand = new OnOffValue(false)
                    }
                });

            table.Append(children);

            return table;
        }

        public static TableRow CreateTableHeader(params string[] headerColumns)
        {
            TableRow row = new TableRow(
                new TableRowProperties(
                    new CantSplit(),
                    new TableHeader()));

            foreach (string cHeaderText in headerColumns)
            {
                row.Append(
                    new TableCell(
                        new TableCellProperties()
                        {
                            Shading = new Shading()
                            {
                                Val = new EnumValue<ShadingPatternValues>(ShadingPatternValues.Clear),
                                Color = new StringValue("auto"),
                                Fill = new StringValue("E6E6E6")
                            }
                        },
                        new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TableHeadingStyle
                                }),
                            new Run(
                                new RunProperties() { Bold = new Bold() },
                                new Text(cHeaderText) { Space = SpaceProcessingModeValues.Preserve }))));
            }

            return row;
        }

        public static Hyperlink CreateUrlHyperlink(MainDocumentPart mainPart, string text, string url, string style)
        {
            RunProperties rp = new RunProperties(
                new RunStyle()
                {
                    Val = style
                });

            HyperlinkRelationship rel = mainPart.AddHyperlinkRelationship(new Uri(url), true);

            return new Hyperlink(
                new ProofError() { Type = ProofingErrorValues.GrammarStart },
                new Run(
                    rp,
                    new Text(text)))
                {
                    History = true,
                    Id = rel.Id
                };
        }

        public static Hyperlink CreateAnchorHyperlink(string text, string anchor, string style)
        {
            return CreateAnchorHyperlink(text, anchor, style, -1);
        }

        public static Hyperlink CreateAnchorHyperlink(string text, string anchor, string style, int size)
        {
            RunProperties rp = new RunProperties(
                new RunStyle()
                {
                    Val = style,
                });

            if (size != -1)
                rp.Append(
                    new FontSize()
                    {
                        Val = new StringValue((2 * size).ToString())
                    });

            return new Hyperlink(
                new ProofError() { Type = ProofingErrorValues.GrammarStart },
                new Run(
                    rp,
                    new Text(text))) { Anchor = anchor };
        }

        public static Run CreateRun(string text, string anchorName=null, bool bold=false, bool italic=false, int? size=null, string style=null, string font=null)
        {
            Run newRun = new Run();
            RunProperties newRunProperties = new RunProperties();

            if (!string.IsNullOrEmpty(anchorName))
            {
                newRun.Append(
                    new BookmarkStart() { Id = anchorName, Name = anchorName },
                    new BookmarkEnd() { Id = anchorName });
            }

            if (bold)
                newRunProperties.Append(new Bold());

            if (italic)
                newRunProperties.Append(new Italic());

            if (!string.IsNullOrEmpty(font))
                newRunProperties.Append(
                    new RunProperties()
                    {
                        RunFonts = new RunFonts()
                        {
                            Ascii = font,
                            HighAnsi = new StringValue(font)
                        }
                    });

            if (size != null && size != -1)
                newRunProperties.Append(
                    new FontSize()
                    {
                        Val = new StringValue((2 * size).ToString())
                    });

            if (!string.IsNullOrEmpty(style))
                newRunProperties.Append(
                    new RunStyle()
                    {
                        Val = style
                    });

            newRun.Append(newRunProperties);

            if (text != null)
            {
                string[] textSplit = text.Split('\n');

                for (int i = 0; i < textSplit.Length; i++)
                {
                    newRun.Append(
                        new Text(textSplit[i])
                        {
                            Space = SpaceProcessingModeValues.Preserve
                        });

                    if (i < textSplit.Length - 1)
                        newRun.Append(
                            new Break());
                }
            }

            return newRun;
        }

        public static TableBorders CreateTableBorder()
        {
            TableBorders borders = new TableBorders(
                new LeftBorder()
                {
                    Val = BorderValues.Single,
                    Size = new UInt32Value((uint)4),
                    Space = new UInt32Value((uint)0),
                    Color = new StringValue("auto")
                },
                new RightBorder()
                {
                    Val = BorderValues.Single,
                    Size = new UInt32Value((uint)4),
                    Space = new UInt32Value((uint)0),
                    Color = new StringValue("auto")
                },
                new BottomBorder()
                {
                    Val = BorderValues.Single,
                    Size = new UInt32Value((uint)4),
                    Space = new UInt32Value((uint)0),
                    Color = new StringValue("auto")
                },
                new TopBorder()
                {
                    Val = BorderValues.Single,
                    Size = new UInt32Value((uint)4),
                    Space = new UInt32Value((uint)0),
                    Color = new StringValue("auto")
                },
                new InsideHorizontalBorder()
                {
                    Val = BorderValues.Single,
                    Size = new UInt32Value((uint)4),
                    Space = new UInt32Value((uint)0),
                    Color = new StringValue("auto")
                },
                new InsideVerticalBorder()
                {
                    Val = BorderValues.Single,
                    Size = new UInt32Value((uint)4),
                    Space = new UInt32Value((uint)0),
                    Color = new StringValue("auto")
                });

            return borders;
        }
    }
}
