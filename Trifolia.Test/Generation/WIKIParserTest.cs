using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Generation.IG;
using Trifolia.DB;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using Trifolia.Shared;

namespace Trifolia.Test.Generation
{
    [TestClass]
    public class WIKIParserTest
    {
        private MainDocumentPart mainPart;
        private MockObjectRepository tdb;
        private Template template1;
        private Template template2;
        private WordprocessingDocument doc;

        [TestInitialize]
        public void Setup()
        {
            this.tdb = new MockObjectRepository();

            ImplementationGuideType cdaType = this.tdb.FindOrCreateImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "cda.xsd", "cda", "urn:hl7-org:v3");
            TemplateType docType = this.tdb.FindOrCreateTemplateType(cdaType, "Document Templates", "ClinicalDocument", "ClinicalDocument", 1);
            ImplementationGuide ig = this.tdb.FindOrCreateImplementationGuide(cdaType, "Test IG");

            this.template1 = this.tdb.CreateTemplate("1.2.3.4.5", docType, "Test Template 1", ig);
            this.template1.Bookmark = "TEST_TEMPLATE1";
            this.template2 = this.tdb.CreateTemplate("5.4.3.2.1", docType, "Test Template 2", ig);
            this.template2.Bookmark = "TEST_TEMPLATE2";

            MemoryStream ms = new MemoryStream();
            this.doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document);
            this.mainPart = this.doc.AddMainDocumentPart();
            this.mainPart.Document =
                new Document(
                    new Body());
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXML_Bold_then_Italic()
        {
            string testWikiContent = "This *is a bold* test with _italic text as well_";

            OpenXmlElement body = testWikiContent.MarkdownToOpenXml(this.mainPart);

            Assert.AreEqual(1, body.ChildElements.Count);
            Paragraph para = body.ChildElements[0] as Paragraph;
            Assert.IsNotNull(para);

            Assert.AreEqual(5, para.ChildElements.Count);

            Run boldRun = para.ChildElements[2] as Run;
            Assert.IsNotNull(boldRun);
            Assert.IsNotNull(boldRun.RunProperties);
            Assert.IsNotNull(boldRun.RunProperties.Bold);

            Run italicRun = para.ChildElements[4] as Run;
            Assert.IsNotNull(italicRun);
            Assert.IsNotNull(italicRun.RunProperties);
            Assert.IsNotNull(italicRun.RunProperties.Italic);
            AssertOpenXmlValid(body);
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXML_Bullets()
        {
            string testWikiContent = @"* Test bullet 1
* Test bullet 2
** Test sub-bullet 1
* Test bullet 3

This is a non-bulleted test

* Test a new bullet list
* Test a new bullet list ";
            
            OpenXmlElement table = testWikiContent.MarkdownToOpenXml(this.mainPart);
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXML_Tables()
        {
            string testWikiContent = @"|| Test Header1 || Test Header2
| TestRowCell1 | TestRowCell2
| TestRowCell3 | TestRowCell4";
            
            OpenXmlElement table = testWikiContent.MarkdownToOpenXml(this.mainPart);

            this.mainPart.Document.Body.Append(table);

            AssertOpenXmlValid(table);
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXML_LinkToTemplate()
        {
            string testWikiContent = @"This is a test of a [URL:#" + this.template1.Oid + "]";

            OpenXmlElement body = testWikiContent.MarkdownToOpenXml(this.mainPart);

            Assert.AreEqual(1, body.ChildElements.Count);
            Paragraph para = body.ChildElements[0] as Paragraph;
            Assert.IsNotNull(para);

            Run run = para.ChildElements[1] as Run;
            Assert.IsNotNull(run);
            AssertRunText(run, "This is a test of a ");

            Hyperlink hyperlink = para.ChildElements[2] as Hyperlink;
            Assert.IsNotNull(hyperlink);
            Run hyperlinkRun = hyperlink.LastChild as Run;
            Assert.IsNotNull(hyperlinkRun);
            AssertRunText(hyperlinkRun, "Test Template 1 (1.2.3.4.5)");
            Assert.IsNotNull(hyperlink.Anchor);
            Assert.AreEqual(this.template1.Bookmark, hyperlink.Anchor.Value);

            AssertOpenXmlValid(body);
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXML_LinkToURLWithoutName()
        {
            string testWikiContent = @"This is a test of a [URL:http://www.awesome.com]";
            
            OpenXmlElement body = testWikiContent.MarkdownToOpenXml(this.mainPart);

            Assert.AreEqual(1, body.ChildElements.Count);
            Paragraph para = body.ChildElements[0] as Paragraph;
            Assert.IsNotNull(para);

            Run run = para.ChildElements[1] as Run;
            Assert.IsNotNull(run);
            AssertRunText(run, "This is a test of a ");

            Hyperlink hyperlink = para.ChildElements[2] as Hyperlink;
            Assert.IsNotNull(hyperlink);
            Run hyperlinkRun = hyperlink.LastChild as Run;
            Assert.IsNotNull(hyperlinkRun);
            AssertRunText(hyperlinkRun, "http://www.awesome.com");

            AssertOpenXmlValid(body);
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXML_LinkToURLWithName()
        {
            string testWikiContent = @"This is a test of a [URL:Awesome|http://www.awesome.com]";
            
            OpenXmlElement body = testWikiContent.MarkdownToOpenXml(this.mainPart);

            Assert.AreEqual(1, body.ChildElements.Count);
            Paragraph para = body.ChildElements[0] as Paragraph;
            Assert.IsNotNull(para);

            Run run = para.ChildElements[1] as Run;
            Assert.IsNotNull(run);
            AssertRunText(run, "This is a test of a ");

            Hyperlink hyperlink = para.ChildElements[2] as Hyperlink;
            Assert.IsNotNull(hyperlink);
            Run hyperlinkRun = hyperlink.LastChild as Run;
            Assert.IsNotNull(hyperlinkRun);
            AssertRunText(hyperlinkRun, "Awesome");

            AssertOpenXmlValid(body);
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXml_MultipleRoots()
        {
            string testWikiContent = @"The US Realm Patient Generated Document header template must conform to the Universal Realm Patient Generated Document header template. This template is designed to be used in conjunction with the US C-CDA General Header. It only includes additional conformances which further constrain the US C-CDA General Header.

|| TableHead 1 || Table Head 2
| Table Cell 1 | Table Cell 2
| Table Cell 3 | Table Cell 4

There is more information here: [URL:#2.16.840.1.113883.10.20.29.1] and [URL:here|http://www.lantanagroup.com] and [URL:http://www.lantanagroup.com]";

            OpenXmlElement body = testWikiContent.MarkdownToOpenXml(this.mainPart);

            Assert.AreEqual(3, body.ChildElements.Count);
            Assert.IsInstanceOfType(body.ChildElements[0], typeof(Paragraph));
            Assert.IsInstanceOfType(body.ChildElements[1], typeof(Table));
            Assert.IsInstanceOfType(body.ChildElements[2], typeof(Paragraph));

            AssertOpenXmlValid(body);
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestParseAsOpenXml_LineBreaks()
        {
            string testWikiContent = @"This is a 
test of 
a line break and a url [URL:http://www.seanmcilvenna.com] with 
another line break";
            
            OpenXmlElement body = testWikiContent.MarkdownToOpenXml(this.mainPart);

            Assert.AreEqual(4, body.ChildElements.Count);

            AssertOpenXmlValid(body);
        }

        #region Helpers

        private void AssertOpenXmlValid(OpenXmlElement part)
        {
            OpenXmlValidator validator = new OpenXmlValidator(FileFormatVersions.Office2007);
            IEnumerable<ValidationErrorInfo> validationErrors = validator.Validate(part);

            foreach (var cError in validationErrors)
            {
                Assert.Fail("Validating the openxml results failed: " + cError.Description);
            }
        }

        private void AssertRunText(Run run, string text)
        {
            Text runText = run.LastChild as Text;
            Assert.IsNotNull(runText);
            Assert.AreEqual(text, runText.Text);
        }

        #endregion
    }
}
