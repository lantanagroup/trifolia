using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Shared;

namespace Trifolia.Test.Shared
{
    [TestClass]
    public class HtmlToOpenXmlConverterTests
    {
        private static MockObjectRepository tdb;
        private static WordprocessingDocument doc;
        private static MainDocumentPart mdp;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            tdb = new MockObjectRepository();

            using (MemoryStream ms = new MemoryStream())
            {
                doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document);
                mdp = doc.AddMainDocumentPart();
            }
        }

        private static void ValidateOpenXml(OpenXmlElement element)
        {
            OpenXmlValidator validator = new OpenXmlValidator(FileFormatVersions.Office2010);
            IEnumerable<ValidationErrorInfo> validationErrors = validator.Validate(element);
            var filteredErrors = validationErrors.Where(y =>
                y.Description != "The 'http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing:editId' attribute is not declared.");
            
            foreach (var error in filteredErrors)
            {
                Console.WriteLine("{0} {1}", error.Path.XPath, error.Description);
            }

            Assert.AreEqual(0, filteredErrors.Count());
        }

        private void TestEmbeddedHtml(int number)
        {
            string html = Helper.GetSampleContents("Trifolia.Test.DocSamples.HTML.html" + number + ".html");
            OpenXmlElement element = HtmlToOpenXmlConverter.HtmlToOpenXml(tdb, mdp, html, true);
            ValidateOpenXml(element);
        }

        [TestMethod]
        public void TestWhiteSpacePreservation()
        {
            string html = "<span>This is <bold>a test</bold> of bold text</span>";
            OpenXmlElement element = HtmlToOpenXmlConverter.HtmlToOpenXml(tdb, mdp, html, true);

            Paragraph para = element.FirstChild as Paragraph;
            Assert.IsNotNull(para);

            Run run = para.OfType<Run>().FirstOrDefault();
            Assert.IsNotNull(run);

            Text text = run.OfType<Text>().FirstOrDefault();
            Assert.IsNotNull(text);
            Assert.IsNotNull(text.Space);
            Assert.AreEqual(SpaceProcessingModeValues.Preserve, text.Space.Value);
            Assert.AreEqual("This is ", text.Text);
        }

        [TestMethod]
        public void TestHtml1()
        {
            TestEmbeddedHtml(1);
        }

        [TestMethod]
        public void TestHtml2()
        {
            TestEmbeddedHtml(2);
        }

        [TestMethod]
        public void TestHtml3()
        {
            TestEmbeddedHtml(3);
        }

        [TestMethod]
        public void TestHtml4()
        {
            TestEmbeddedHtml(4);
        }

        [TestMethod]
        public void TestHtml5()
        {
            TestEmbeddedHtml(5);
        }

        [TestMethod]
        public void TestHtml6()
        {
            TestEmbeddedHtml(6);
        }

        [TestMethod]
        public void TestHtml7()
        {
            TestEmbeddedHtml(7);
        }

        [TestMethod]
        public void TestHtml8()
        {
            TestEmbeddedHtml(8);
        }

        [TestMethod]
        public void TestHtml9()
        {
            TestEmbeddedHtml(9);
        }

        [TestMethod]
        public void TestHtml10()
        {
            TestEmbeddedHtml(10);
        }

        [TestMethod]
        public void TestHtml11()
        {
            TestEmbeddedHtml(11);
        }

        [TestMethod]
        public void TestHtml12()
        {
            TestEmbeddedHtml(12);
        }

        [TestMethod]
        public void TestHtml13()
        {
            TestEmbeddedHtml(13);
        }

        [TestMethod]
        public void TestHtml14()
        {
            TestEmbeddedHtml(14);
        }

        [TestMethod]
        public void TestHtml15()
        {
            TestEmbeddedHtml(15);
        }

        [TestMethod]
        public void TestHtml16()
        {
            TestEmbeddedHtml(16);
        }

        [TestMethod]
        public void TestHtml17()
        {
            TestEmbeddedHtml(17);
        }

        [TestMethod]
        public void TestHtml18()
        {
            TestEmbeddedHtml(18);
        }

        [TestMethod]
        public void TestHtml19()
        {
            TestEmbeddedHtml(19);
        }

        [TestMethod]
        public void TestHtml20()
        {
            TestEmbeddedHtml(20);
        }

        [TestMethod]
        public void TestHtml21()
        {
            TestEmbeddedHtml(21);
        }

        [TestMethod]
        public void TestHtml22()
        {
            TestEmbeddedHtml(22);
        }

        [TestMethod]
        public void TestHtml23()
        {
            TestEmbeddedHtml(23);
        }

        [TestMethod]
        public void TestHtml24()
        {
            TestEmbeddedHtml(24);
        }

        [TestMethod]
        public void TestHtml25()
        {
            TestEmbeddedHtml(25);
        }

        /*
        [TestMethod]
        public void CreateHtml()
        {
            string folder = @"C:\Users\sean.mcilvenna\Lantana Consulting Group\Trifolia - HAI IG Markdown\3311";
            string destination = @"C:\Users\sean.mcilvenna\Code\Trifolia-OS\Trifolia.Test\DocSamples\HTML";
            string[] files = System.IO.Directory.GetFiles(folder);

            for (var i = 0; i < files.Length; i++)
            {
                string content = File.ReadAllText(files[i]);
                string html = content.MarkdownToHtml();
                File.WriteAllText(Path.Combine(destination, "html" + (i + 1) + ".html"), html);
            }
        }
        */
    }
}
