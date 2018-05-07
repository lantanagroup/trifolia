using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Trifolia.DB;

namespace Trifolia.Shared
{
    /// <summary>
    /// Converts HTML into OpenXML.
    /// </summary>
    /// <remarks>
    /// Recommend updating "markdown" extension in knockout (in the
    /// knockout.extensions.js file) when adding/removing support 
    /// for additional HTML tags/elements.
    /// </remarks>
    public class HtmlToOpenXmlConverter
    {
        private const string OpenXmlPictureNamespace = "http://schemas.openxmlformats.org/drawingml/2006/picture";

        private bool currentIsTableHeader = false;
        private int currentListLevel = 0;
        private string currentListStyle = null;
        private MainDocumentPart mainPart;
        private IObjectRepository tdb;

        private Regex igImageRegex = new Regex(@"\/api\/ImplementationGuide\/(\d+)\/Image/(.+?)$", RegexOptions.Multiline);

        internal HtmlToOpenXmlConverter(IObjectRepository tdb, MainDocumentPart mainPart)
        {
            this.tdb = tdb;
            this.mainPart = mainPart;
        }

        public static OpenXmlElement HtmlToOpenXml(IObjectRepository tdb, MainDocumentPart mainPart, string html)
        {
            HtmlToOpenXmlConverter converter = new HtmlToOpenXmlConverter(tdb, mainPart);
            return converter.Convert(html);
        }

        internal OpenXmlElement Convert(string html)
        {
            Body body = new Body();
            OpenXmlElement current = body;

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
            var filteredErrors = validationErrors.Where(y => 
                y.Description != "The 'http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing:editId' attribute is not declared.");

            if (filteredErrors.Count() > 0)
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

        private ImagePartType GetImagePartType(string extension)
        {
            switch (extension)
            {
                case "jpg":
                case "jpeg":
                    return ImagePartType.Jpeg;
                case "bmp":
                    return ImagePartType.Bmp;
                case "gif":
                case "giff":
                    return ImagePartType.Gif;
                case "tif":
                case "tiff":
                    return ImagePartType.Tiff;
                case "png":
                    return ImagePartType.Png;
                default:
                    throw new NotSupportedException("Image extension " + extension + " is not supported");
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
                    case "em":
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
                        // TODO: handle headers separately

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
                    case "th":
                    case "td":
                        if (cTableRow != null)
                        {
                            TableCell newCell = new TableCell();

                            // If we are in the header, shade the cell appropriately
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

                            current.Append(newCell);
                            return newCell;
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
                        var strongRun = cRun;

                        if (strongRun == null)
                            strongRun = (Run) NewChild(current, new Run());

                        this.AddBoldToRun(strongRun);

                        return strongRun;
                    case "cite":
                        var citeRun = cRun;

                        if (citeRun == null)
                            citeRun = (Run)NewChild(current, new Run());

                        citeRun.Append(new Text("\"" + xmlReader.ReadElementContentAsString() + "\""));

                        return current;
                    case "img":
                        string imageSource = xmlReader.GetAttribute("src");
                        string imageAlt = xmlReader.GetAttribute("alt");
                        string imageTitle = xmlReader.GetAttribute("title");
                        string imageExtension = imageSource.Substring(imageSource.LastIndexOf(".") + 1);

                        if (this.igImageRegex.IsMatch(imageSource))
                        {
                            var match = this.igImageRegex.Match(imageSource);
                            int implementationGuideId = Int32.Parse(match.Groups[1].Value);
                            string fileName = match.Groups[2].Value;

                            var file = this.tdb.ImplementationGuideFiles.SingleOrDefault(y => y.ImplementationGuideId == implementationGuideId && y.FileName.ToLower() == fileName.ToLower());

                            if (file == null)
                                return current;

                            var latestVersion = file.GetLatestData();
                            ImagePart newImagePart = this.mainPart.AddImagePart(this.GetImagePartType(imageExtension));
                            int docImageWidth = 0;
                            int docImageHeight = 0;

                            using (MemoryStream ms = new MemoryStream(latestVersion.Data))
                            {
                                newImagePart.FeedData(ms);
                            }

                            using (MemoryStream ms = new MemoryStream(latestVersion.Data))
                            {
                                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);
                                docImageWidth = (int)Math.Round((decimal)bitmap.Width * 9525);
                                docImageHeight = (int)Math.Round((decimal)bitmap.Height * 9525);
                            }

                            string description = file.Description;

                            if (!string.IsNullOrEmpty(imageAlt))
                                description = imageAlt;
                            else if (!string.IsNullOrEmpty(imageTitle))
                                description = imageTitle;

                            return AddImage(
                                current,
                                this.mainPart.GetIdOfPart(newImagePart),
                                file.FileName,
                                description,
                                docImageWidth,
                                docImageHeight);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    default:
                        Logging.Log.For(this).Error("Unsupported wiki syntax/element " + xmlReader.Name);
                        return current;
                }
            }
            else if (xmlReader.NodeType == XmlNodeType.Text)
            {
                string text = xmlReader.Value
                    .Replace("&nbsp;", " ");

                if (current is TableCell)
                {
                    Paragraph tcPara = new Paragraph(CreateRun(text));
                    current.Append(tcPara);
                }
                if (current is Paragraph)
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

        private static Drawing AddImage(OpenXmlElement parent, string relationshipId, string fileName, string description, int width, int height)
        {
            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         new DW.Extent() { Cx = width, Cy = height },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = description
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = fileName
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState = A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents() { Cx = width, Cy = height }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = OpenXmlPictureNamespace })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });

            var para = parent as Paragraph;

            if (para == null)
            {
                para = new Paragraph();
                parent.AppendChild(para);
            }

            var paraProp = para.ChildElements.OfType<ParagraphProperties>().FirstOrDefault();

            if (paraProp == null)
            {
                paraProp = new ParagraphProperties();
                para.AppendChild(paraProp);
            }

            paraProp.ParagraphStyleId = new ParagraphStyleId() { Val = "BodyImage" };

            para.AppendChild(new Run(element));

            return element;
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
