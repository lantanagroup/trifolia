using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.DB;
using Trifolia.Logging;

namespace Trifolia.Generation.IG
{
    public class FigureCollection
    {
        private int figureCount = 0;
        private Body documentBody;

        public FigureCollection(Body documentBody)
        {
            this.documentBody = documentBody;
        }

        public void AddSample(string name, string content)
        {
            Paragraph lHeader = DocHelper.CreateFigureCaption(this.figureCount++, name, caption: "Figure ");
            lHeader.ParagraphProperties.Indentation = new Indentation()
            {
                Left = "130",
                Right = "115"
            };
            this.documentBody.AppendChild(lHeader);
            string lXmlSample = content;

            foreach (string lLine in lXmlSample.Split('\r', '\n'))
            {
                if (string.IsNullOrEmpty(lLine)) continue;
                Paragraph lLineParagraph = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = "Example" },
                        new Indentation()
                        {
                            Left = "130",
                            Right = "115"
                        }));

                lLineParagraph.Append(DocHelper.CreateRun(lLine));
                this.documentBody.AppendChild(lLineParagraph);
            }

            // Add a blank line
            var blankPara = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId()
                    {
                        Val = Properties.Settings.Default.BlankLinkText
                    }));
            this.documentBody.AppendChild(blankPara);
        }
    }
}
