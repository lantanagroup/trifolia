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
using Saxon.Api;

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
        private const string OpenXmlWordProcessingNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const string OpenXmlDrawingMLNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";
        private const string OpenXmlRelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string OpenXmlWordProcessingDrawingNamespace = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";
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

        private static string Transform(string html)
        {
            var processor = new Processor();
            var compiler = processor.NewXsltCompiler();
            var executable = compiler.Compile(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Trifolia.Shared.htmlToOpenXml.xsl"));

            var dest = new DomDestination();
            using (MemoryStream ms = new MemoryStream(ASCIIEncoding.UTF8.GetBytes("<root>" + html + "</root>")))
            {
                var transformer = executable.Load();

                // Parameters to stylesheet
                transformer.SetParameter(new QName("linkStyle"), new XdmAtomicValue(Properties.Resource.LinkStyle));
                transformer.SetParameter(new QName("bulletListStyle"), new XdmAtomicValue("ListBullet"));
                transformer.SetParameter(new QName("orderedListStyle"), new XdmAtomicValue("ListNumber"));

                transformer.SetInputStream(ms, new Uri("https://trifolia.lantanagroup.com/"));
                transformer.Run(dest);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                dest.XmlDocument.Save(ms);
                return ASCIIEncoding.UTF8.GetString(ms.ToArray());
            }
        }

        private string CleanupXml(string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("w", OpenXmlWordProcessingNamespace);
            nsManager.AddNamespace("a", OpenXmlDrawingMLNamespace);
            nsManager.AddNamespace("r", OpenXmlRelationshipsNamespace);
            nsManager.AddNamespace("pic", OpenXmlPictureNamespace);
            nsManager.AddNamespace("wp", OpenXmlWordProcessingDrawingNamespace);

            var hyperlinkNodes = doc.SelectNodes("//w:hyperlink", nsManager);
            var drawingNodes = doc.SelectNodes("//w:drawing", nsManager);

            // Ensure that hyperlinks have a HyperlinkRelationship created for them
            // and the id of the relationship is set on the w:hyperlink element
            foreach (XmlElement hyperlinkNode in hyperlinkNodes)
            {
                XmlComment hrefCommentNode = hyperlinkNode.PreviousSibling as XmlComment;

                if (hrefCommentNode == null)
                    continue;

                string href = hrefCommentNode.Value;

                HyperlinkRelationship rel = this.mainPart.AddHyperlinkRelationship(new Uri(href), true);

                XmlAttribute idAttr = doc.CreateAttribute("id", OpenXmlRelationshipsNamespace);
                idAttr.Value = rel.Id;
                hyperlinkNode.Attributes.Append(idAttr);
            }

            // Ensure that image data has been added to the document, an id has been created
            // for the image, the image's filename has been set, and the width and height are set appropriately
            foreach (XmlElement drawingNode in drawingNodes)
            {
                XmlComment srcCommentNode = drawingNode.PreviousSibling as XmlComment;

                if (srcCommentNode == null)
                    throw new Exception("Expected transformed OpenXml w:drawing to have a comment preceding it representing the src of the image");

                if (this.igImageRegex.IsMatch(srcCommentNode.Value))
                {
                    var match = this.igImageRegex.Match(srcCommentNode.Value);
                    int implementationGuideId = Int32.Parse(match.Groups[1].Value);
                    string fileName = match.Groups[2].Value;

                    var file = this.tdb.ImplementationGuideFiles.SingleOrDefault(y => y.ImplementationGuideId == implementationGuideId && y.FileName.ToLower() == fileName.ToLower());

                    if (file == null)
                        continue;

                    string imageExtension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
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

                    // Set the id of the image to the id of the relationship part
                    XmlAttribute embedAttr = (XmlAttribute)drawingNode.SelectSingleNode("//a:blip/@r:embed", nsManager);
                    embedAttr.Value = this.mainPart.GetIdOfPart(newImagePart);

                    XmlAttribute nameAttr = (XmlAttribute)drawingNode.SelectSingleNode("//pic:nvPicPr/pic:cNvPr/@name", nsManager);
                    nameAttr.Value = file.FileName;

                    XmlAttribute extentWidthAttribute = (XmlAttribute)drawingNode.SelectSingleNode("//wp:inline/wp:extent/@cx", nsManager);
                    extentWidthAttribute.Value = docImageWidth.ToString();

                    XmlAttribute extentHeightAttribute = (XmlAttribute)drawingNode.SelectSingleNode("//wp:inline/wp:extent/@cy", nsManager);
                    extentHeightAttribute.Value = docImageHeight.ToString();
                }
                else
                {
                    // We don't yet support external images
                    continue;
                }
            }

            return doc.OuterXml;
        }

        public static OpenXmlElement HtmlToOpenXml(IObjectRepository tdb, MainDocumentPart mainPart, string html, bool returnInvalid = false)
        {
            HtmlToOpenXmlConverter converter = new HtmlToOpenXmlConverter(tdb, mainPart);
            return converter.Convert(html, returnInvalid);
        }

        internal OpenXmlElement Convert(string html, bool returnInvalid = false)
        {
            string xmlContent = Transform(html);
            xmlContent = this.CleanupXml(xmlContent);

            using (MemoryStream mem = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
                {
                    // Add a main document part. 
                    MainDocumentPart newPart = wordDocument.AddMainDocumentPart();

                    using (StreamWriter sw = new StreamWriter(newPart.GetStream()))
                    {
                        sw.Write(xmlContent);
                    }

                    // Validate the parsed content. If validation fails return the content in plain-text, wrapped in a para
                    OpenXmlValidator validator = new OpenXmlValidator();
                    IEnumerable<ValidationErrorInfo> validationErrors = validator.Validate(newPart.Document.Body);
                    var filteredErrors = validationErrors.Where(y =>
                        y.Description != "The 'http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing:editId' attribute is not declared.");

                    if (filteredErrors.Count() > 0)
                    {
                        Logging.Log.For(this).Error("{0} Validation errors on OpenXml converted from HTML", filteredErrors.Count());

                        if (!returnInvalid)
                        {
                            return new Body(
                                new Paragraph(
                                    new Run(
                                        new Text()
                                        {
                                            Text = html
                                        })));
                        }
                    }

                    return newPart.Document.Body;
                }
            }
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
    }
}
