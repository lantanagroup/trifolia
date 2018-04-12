using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Trifolia.Shared
{
    public class HtmlToOpenXmlConverter
    {
        private bool currentIsTableHeader = false;
        private int currentListLevel = 0;
        private string currentListStyle = null;
        private MainDocumentPart mainPart;

        internal HtmlToOpenXmlConverter(MainDocumentPart mainPart)
        {
            this.mainPart = mainPart;
        }

        public static OpenXmlElement HtmlToOpenXml(MainDocumentPart mainPart, string html)
        {
            HtmlToOpenXmlConverter converter = new HtmlToOpenXmlConverter(mainPart);
            return converter.Convert(html);
        }

        internal OpenXmlElement Convert(string html)
        {
            OpenXmlElement current = new Body();

            using (StringReader strReader = new StringReader("<root>" + html + "</root>"))
            {
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Parse
                };

                XmlReader xmlReader = XmlReader.Create(strReader, xmlReaderSettings);

                while (xmlReader.Read())
                {
                    current = this.Process(xmlReader, current);
                }
            }

            // Validate the parsed content. If validation fails return the content in plain-text, wrapped in a para
            OpenXmlValidator validator = new OpenXmlValidator();
            IEnumerable<ValidationErrorInfo> validationErrors = validator.Validate(current);

            if (validationErrors.ToList().Count > 0)
            {
                return new Body(
                    new Paragraph(
                        new Run(
                            new Text()
                            {
                                Text = html
                            })));
            }

            return current;
        }

        private OpenXmlElement Process(XmlReader xmlReader, OpenXmlElement current)
        {
            Paragraph cPara = current as Paragraph;
            Run cRun = current as Run;
            Hyperlink cHyperlink = current as Hyperlink;
            Table cTable = current as Table;
            TableRow cTableRow = current as TableRow;
            TableCell cTableCell = current as TableCell;

            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                // Do something for elements
                switch (xmlReader.Name.ToLower())
                {
                    case "p":
                        if (cPara != null)
                            break;
                        Paragraph newParagraph = new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Resource.TemplateDescriptionStyle
                                }));
                        return NewChild(current, newParagraph);
                    case "b":
                        if (cPara != null)
                        {
                            Run newRun = new Run();
                            AddBoldToRun(newRun);
                            return NewChild(current, newRun);
                        }
                        else if (cRun != null)
                        {
                            AddBoldToRun(cRun);
                            return cRun;
                        }
                        break;
                    case "i":
                        if (cPara != null)
                        {
                            Run newRun = new Run();
                            AddItalicsToRun(newRun);
                            return NewChild(current, newRun);
                        }
                        else if (cRun != null)
                        {
                            AddItalicsToRun(cRun);
                            return cRun;
                        }
                        break;
                    case "a":
                        string hrefAttr = xmlReader.GetAttribute("href");
                        Hyperlink newHyperlink = new Hyperlink(
                            new ProofError() { Type = ProofingErrorValues.GrammarStart });

                        if (!string.IsNullOrEmpty(hrefAttr))
                        {
                            try
                            {
                                HyperlinkRelationship rel = this.mainPart.AddHyperlinkRelationship(new Uri(hrefAttr), true);
                                newHyperlink.History = true;
                                newHyperlink.Id = rel.Id;
                            }
                            catch { }
                        }

                        if (cPara != null)
                        {
                            return NewChild(current, newHyperlink);
                        }

                        break;
                    case "ul":
                        this.currentListLevel++;
                        this.currentListStyle = "ListBullet";
                        if (current is Paragraph)
                            current = current.Parent;
                        break;
                    case "ol":
                        this.currentListLevel++;
                        this.currentListStyle = "ListNumber";
                        if (current is Paragraph)
                            current = current.Parent;
                        break;
                    case "li":
                        Paragraph bulletPara = new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId() { Val = this.currentListStyle }));
                        return NewChild(current, bulletPara);
                    case "table":
                        Table newTable = new Table(
                            new TableProperties(),
                            new TableGrid());
                        return NewChild(current, newTable);
                    case "thead":
                        this.currentIsTableHeader = true;
                        break;
                    case "tr":
                        if (cTable != null)
                        {
                            TableRow newTableRow = new TableRow();
                            if (this.currentIsTableHeader)
                            {
                                newTableRow.Append(
                                    new TableRowProperties(
                                        new CantSplit(),
                                        new TableHeader()));
                            }
                            return NewChild(current, newTableRow);
                        }
                        break;
                    case "td":
                        if (cTableRow != null)
                        {
                            TableCell newCell = new TableCell();
                            if (this.currentIsTableHeader)
                            {
                                newCell.Append(
                                    new TableCellProperties()
                                    {
                                        Shading = new Shading()
                                        {
                                            Val = new EnumValue<ShadingPatternValues>(ShadingPatternValues.Clear),
                                            Color = new StringValue("auto"),
                                            Fill = new StringValue("E6E6E6")
                                        }
                                    });
                            }
                            // Cells' contents should be within a paragraph
                            Paragraph newPara = new Paragraph();
                            newCell.AppendChild(newPara);

                            current.Append(newCell);
                            return newPara;
                        }
                        break;
                    case "span":
                        if (cPara != null)
                        {
                            Run newRun = new Run();
                            return NewChild(current, newRun);
                        }
                        break;
                    case "root":
                    case "tbody":
                        break;
                    case "strong":
                        var run = cRun;

                        if (run == null)
                            run = (Run) NewChild(current, new Run());

                        this.AddBoldToRun(run);

                        return run;
                    default:
                        throw new Exception("Unsupported wiki syntax");
                }
            }
            else if (xmlReader.NodeType == XmlNodeType.Text)
            {
                string text = xmlReader.Value
                    .Replace("&nbsp;", " ");

                if (current is Paragraph || current is TableCell)
                {
                    current.Append(CreateRun(text));
                }
                else if (cHyperlink != null)
                {
                    if (!(cHyperlink.LastChild is Run))
                        cHyperlink.Append(CreateRun(text));
                }
                else if (cRun != null)
                {
                    cRun.Append(new Text(text));
                }
            }
            else if (xmlReader.NodeType == XmlNodeType.EndElement)
            {
                if (xmlReader.Name == "thead")
                {
                    this.currentIsTableHeader = false;
                    return current;
                }
                else if (xmlReader.Name == "tbody")
                {
                    return current;
                }
                else if (xmlReader.Name == "td")
                {
                    // Expect that we are in a paragraph within a table cell, and when TD ends, we need to return two levels higher
                    if (cPara != null && cPara.Parent is TableCell)
                        return current.Parent.Parent;
                }
                else if (xmlReader.Name == "a")
                {
                    // Make sure all runs within a hyperlink have the correct style
                    foreach (var cChildRun in current.ChildElements.OfType<Run>())
                    {
                        cChildRun.RunProperties.RunStyle = new RunStyle()
                        {
                            Val = Properties.Resource.LinkStyle
                        };
                    }
                }
                else if (xmlReader.Name == "ul")
                {
                    this.currentListLevel--;
                }

                if (current.Parent == null)
                    return current;

                return current.Parent;
            }

            return current;
        }

        private void AddBoldToRun(Run run)
        {
            if (run.RunProperties == null)
                run.RunProperties = new RunProperties();

            if (run.RunProperties.Bold == null)
                run.RunProperties.Bold = new Bold();
        }

        private void AddItalicsToRun(Run run)
        {
            if (run.RunProperties == null)
                run.RunProperties = new RunProperties();

            if (run.RunProperties.Italic == null)
                run.RunProperties.Italic = new Italic();
        }

        private OpenXmlElement NewChild(OpenXmlElement current, OpenXmlElement newChild)
        {
            current.Append(newChild);
            return newChild;
        }

        private static Run CreateRun(string text, string anchorName = null, bool bold = false, bool italic = false, int? size = null, string style = null, string font = null)
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
    }
}
