using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Trifolia.DB;

namespace Trifolia.Generation.IG
{
    public class WIKIParser
    {
        private const string RootWrapperFormat =
@"<?xml version='1.0' standalone='yes' ?>
<!DOCTYPE r [
    <!ENTITY nbsp '&#160;'>
]><root>{0}</root>";

        private IObjectRepository tdb;
        private MainDocumentPart mainPart;
        private bool currentIsTableHeader = false;
        private int currentListLevel = 0;
        private string currentListStyle = null;

        public WIKIParser(IObjectRepository tdb, MainDocumentPart mainPart)
            : this(tdb)
        {
            this.mainPart = mainPart;
        }

        public WIKIParser(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public string ParseAsHtml(string wikiContent)
        {
            if (string.IsNullOrEmpty(wikiContent))
                return string.Empty;

            string htmlContent = string.Format(RootWrapperFormat, WikiNetParser.WikiProvider.ConvertToHtml(wikiContent));

            XmlDocument htmlDoc = new XmlDocument();
            htmlDoc.LoadXml(htmlContent);

            foreach (var cHyperlink in htmlDoc.SelectNodes("//a").OfType<XmlElement>())
            {
                string cHyperlinkHref = cHyperlink.Attributes["href"] != null ? cHyperlink.Attributes["href"].Value : string.Empty;

                if (!string.IsNullOrEmpty(cHyperlinkHref) && cHyperlinkHref.StartsWith("#"))
                {
                    Template foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == cHyperlinkHref.Substring(1));

                    if (foundTemplate != null)
                    {
                        cHyperlink.Attributes["href"].Value = foundTemplate.GetViewUrl();
                        cHyperlink.InnerText = string.Format("{0} (identifier: {1})", foundTemplate.Name, foundTemplate.Oid);
                    }
                }
                else
                {
                    if (cHyperlink.Attributes["target"] == null)
                        cHyperlink.Attributes.Append(htmlDoc.CreateAttribute("target"));

                    cHyperlink.Attributes["target"].Value = "_new";
                }
            }

            return htmlDoc.DocumentElement.InnerXml;
        }

        public OpenXmlElement ParseAsOpenXML(string wikiContent)
        {
            if (string.IsNullOrEmpty(wikiContent))
                return null;

            string htmlContent = string.Format(RootWrapperFormat, WikiNetParser.WikiProvider.ConvertToHtml(wikiContent));
            OpenXmlElement current = new Body();

            using (StringReader strReader = new StringReader(htmlContent))
            {
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Parse
                };

                XmlReader xmlReader = XmlReader.Create(strReader, xmlReaderSettings);

                // Skip the DTD definition
                xmlReader.Read();
                xmlReader.Read();
                xmlReader.Read();
                xmlReader.Read();

                while (xmlReader.Read())
                {
                    current = this.Process(xmlReader, current);
                }
            }

            // Validate the parsed content. If validation fails return the content in plain-text, wrapped in a para
            OpenXmlValidator validator = new OpenXmlValidator(FileFormatVersions.Office2007);
            IEnumerable<ValidationErrorInfo> validationErrors = validator.Validate(current);

            if (validationErrors.ToList().Count > 0)
            {
                return new Body(
                    new Paragraph(
                        new Run(
                            new Text()
                            {
                                Text = wikiContent
                            })));
            }

            return current;
        }

        public void ParseAndAppend(string wikiContent, OpenXmlElement destination)
        {
            OpenXmlElement parsedContent = this.ParseAsOpenXML(wikiContent);

            if (parsedContent != null)
            {
                foreach (OpenXmlElement cParsedElement in parsedContent.ChildElements)
                {
                    destination.Append(
                        cParsedElement.CloneNode(true));
                }
            }
        }

        public void ParseAndAppend(string wikiContent, Paragraph destination, bool applyKeywordStyles = false)
        {
            // Only allow paragraphs (turned into line-breaks)
            OpenXmlElement parsedPrimitiveText = this.ParseAsOpenXML(wikiContent);
            string[] keywords = new string[] { "SHALL NOT", "SHOULD NOT", "MAY NOT", "SHALL", "SHOULD", "MAY" };

            if (applyKeywordStyles)
            {
                var runs = parsedPrimitiveText.Descendants<Run>().ToList();

                foreach (var cRun in runs)
                {
                    var cText = cRun.GetFirstChild<Text>();
                    Regex regex = new Regex(" SHALL NOT | SHOULD NOT | MAY NOT | SHALL | SHOULD | MAY ");
                    MatchCollection matches = regex.Matches(cText.Text);
                    string[] split = regex.Split(cText.Text);
                    List<OpenXmlElement> newElements = new List<OpenXmlElement>();

                    if (split.Length > 1)
                    {
                        var runParent = cRun.Parent != null ? cRun.Parent : parsedPrimitiveText.FirstChild;

                        for (var i = 0; i < split.Length; i++)
                        {
                            var newOtherRun = new Run(new Text(split[i]) { Space = SpaceProcessingModeValues.Preserve });
                            newElements.Add(newOtherRun);

                            if (i < split.Length - 1)
                            {
                                var newKeywordRun = new Run(
                                    new RunProperties(new RunStyle()
                                        {
                                            Val = Properties.Settings.Default.ConformanceVerbStyle
                                        }),
                                    new Text(matches[i].Value) { Space = SpaceProcessingModeValues.Preserve });
                                newElements.Add(newKeywordRun);
                            }
                        }
                    }

                    for (var i = newElements.Count - 1; i >= 0; i--)
                    {
                        cRun.Parent.InsertAfter(newElements[i], cRun);
                    }

                    if (newElements.Count > 0)
                        cRun.Parent.RemoveChild(cRun);
                }
            }

            if (parsedPrimitiveText != null)
            {
                List<Paragraph> paragraphs = parsedPrimitiveText.ChildElements.OfType<Paragraph>().ToList();

                for (int i = 0; i < paragraphs.Count; i++)
                {
                    paragraphs[i].ChildElements.ToList().ForEach(c =>
                    {
                        destination.Append(
                            c.CloneNode(true));
                    });

                    if (i < paragraphs.Count - 1)
                        destination.Append(new Break());
                }
            }
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
                switch (xmlReader.Name)
                {
                    case "p":
                        if (cPara != null)
                            break;

                        Paragraph newParagraph = new Paragraph(
                            new ParagraphProperties(
                                new ParagraphStyleId()
                                {
                                    Val = Properties.Settings.Default.TemplateDescriptionStyle
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
                            Template foundTemplate = null;

                            if (hrefAttr.StartsWith("#") && hrefAttr.Length > 1)
                                foundTemplate = this.tdb.Templates.SingleOrDefault(y => y.Oid == hrefAttr.Substring(1));

                            if (foundTemplate != null)
                            {
                                newHyperlink.Anchor = foundTemplate.Bookmark;
                                newHyperlink.Append(
                                    DocHelper.CreateRun(
                                        string.Format("{0} ({1})",
                                            foundTemplate.Name,
                                            foundTemplate.Oid)));
                            }
                            else
                            {
                                try
                                {
                                    HyperlinkRelationship rel = mainPart.AddHyperlinkRelationship(new Uri(hrefAttr), true);
                                    newHyperlink.History = true;
                                    newHyperlink.Id = rel.Id;
                                }
                                catch { }
                            }
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
                                new ParagraphStyleId(){ Val = this.currentListStyle }));
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
                    current.Append(DocHelper.CreateRun(text));
                }
                else if (cHyperlink != null)
                {
                    if (!(cHyperlink.LastChild is Run))
                        cHyperlink.Append(DocHelper.CreateRun(text));
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
                            Val = Properties.Settings.Default.LinkStyle
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

        private int GetLastBulletNumber(OpenXmlElement current)
        {
            OpenXmlElement next = current.LastChild;
            int lastLevel;
            int lastNumber;

            if (!OpenXmlElementIsBulleted(next, out lastLevel, out lastNumber))
                return 0;

            if (lastLevel != this.currentListLevel)
            {
                next = next.PreviousSibling();

                while (next != null)
                {
                    if (!OpenXmlElementIsBulleted(next, out lastLevel, out lastNumber))
                        return 0;

                    if (lastLevel == this.currentListLevel)
                        return lastNumber;

                    next = next.PreviousSibling();
                }
            }

            if (lastLevel != this.currentListLevel)
                return 0;

            return lastNumber;
        }

        private bool OpenXmlElementIsBulleted(OpenXmlElement element, out int level, out int number)
        {
            level = number = 0;

            Paragraph para = element as Paragraph;

            if (para == null || para.ParagraphProperties == null || para.ParagraphProperties.NumberingProperties == null)
                return false;

            NumberingProperties numProp = para.ParagraphProperties.NumberingProperties;

            if (numProp.NumberingId == null || numProp.NumberingId.Val == null)
                return false;

            if (numProp.NumberingLevelReference == null || numProp.NumberingLevelReference.Val == null)
                return false;

            level = numProp.NumberingLevelReference.Val.Value;
            number = numProp.NumberingId.Val.Value;

            return true;
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
    }
}
