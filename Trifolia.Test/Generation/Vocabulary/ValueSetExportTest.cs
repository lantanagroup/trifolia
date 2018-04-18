using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Xml;
using Trifolia.DB;
using Trifolia.Export.MSWord;

namespace Trifolia.Test.Generation.Vocabulary
{
    [TestClass]
    public class ValueSetExportTest
    {
        private MainDocumentPart mainPart;
        private Body body;
        private MockObjectRepository tdb;
        private ValueSet vs1;
        private ValueSet vs2;
        private ValueSet vs3;

        [TestInitialize]
        public void Setup()
        {
            this.tdb = new MockObjectRepository();

            MemoryStream docStream = new MemoryStream();
            WordprocessingDocument document = WordprocessingDocument.Create(docStream, WordprocessingDocumentType.Document);

            this.body = new Body();
            this.mainPart = document.AddMainDocumentPart();
            this.mainPart.Document = new Document(this.body);

            // Dummy Data
            CodeSystem snomed = this.tdb.FindOrCreateCodeSystem("SNOMED-CT", "1.2.3.4");

            this.vs1 = this.tdb.FindOrCreateValueSet("ValueSet 1", "1.2.3");
            this.tdb.FindOrCreateValueSetMember(this.vs1, snomed, "1", "One");
            this.tdb.FindOrCreateValueSetMember(this.vs1, snomed, "2", "Two");
            this.tdb.FindOrCreateValueSetMember(this.vs1, snomed, "3", "Three");

            this.vs2 = this.tdb.FindOrCreateValueSet("ValueSet 2", "1.2.3.4");
            this.tdb.FindOrCreateValueSetMember(this.vs2, snomed, "1", "One");
            this.tdb.FindOrCreateValueSetMember(this.vs2, snomed, "2", "Two");

            this.vs3 = this.tdb.FindOrCreateValueSet("ValueSet 3", "1.2.3.4.5");
            this.vs3.IsIncomplete = true;
            this.vs3.Source = "http://www.lantanagroup.com";
        }

        [TestMethod]
        public void ValueSetExport_Inline()
        {
            HyperlinkTracker hyperlinkTracker = new HyperlinkTracker();
            TableCollection tables = new TableCollection(this.body);
            ValueSetsExport vse = new ValueSetsExport(null, mainPart, hyperlinkTracker, tables, false, 2, null);

            vse.AddValueSet(this.vs1, DateTime.Now);

            Assert.AreEqual(3, this.body.ChildElements.Count);
            Paragraph p1 = this.body.ChildElements[0] as Paragraph;
            Assert.IsNotNull(p1);

            Table t1 = this.body.ChildElements[1] as Table;
            Assert.IsNotNull(t1);
            Assert.AreEqual(7, t1.ChildElements.Count);

            AssertWordXpath(t1, "w:tr[last()][count(w:tc[w:tcPr/w:gridSpan/@w:val='4'])=1]", "Expected to find one column with a gridspan of 4 in the last row");
            AssertWordXpath(t1, "w:tr[last()]/w:tc/w:p/w:r/w:t[text() = '...']", "Expected to find a text block with ...");

            vse.AddValueSet(this.vs2, DateTime.Now);
            Assert.AreEqual(6, this.body.ChildElements.Count);

            Table t2 = this.body.ChildElements[4] as Table;
            Assert.IsNotNull(t2);
            Assert.AreEqual(6, t2.ChildElements.Count);

            AssertWordXpath(t2, "w:tr[last()][not(w:tc/w:p/w:r/w:t[text() = '...'])]", "Shouldn't have found an elipsis in the last row.");

            vse.AddValueSetsAppendix();

            Assert.AreEqual(10, this.body.ChildElements.Count);
            Table t3 = this.body.ChildElements[8] as Table;
            Assert.IsNotNull(t3);

            AssertWordXpath(t3, "w:tr[1]/w:trPr/w:tblHeader", "Expected first row to be table header in appendix table.");
            AssertWordXpath(t3, "w:tr[2]/w:tc[1]//w:t[text() = 'ValueSet 1']", "Expected to find ValueSet 2 in second row");
            AssertWordXpath(t3, "w:tr[3]/w:tc[1]//w:t[text() = 'ValueSet 2']", "Expected to find ValueSet 3 in third row");
        }

        [TestMethod]
        public void ValueSetExport_Appendix()
        {
            HyperlinkTracker hyperlinkTracker = new HyperlinkTracker();
            TableCollection tables = new TableCollection(this.body);
            ValueSetsExport vse = new ValueSetsExport(null, mainPart, hyperlinkTracker, tables, true, 2, null);

            vse.AddValueSet(this.vs1, DateTime.Now);
            Assert.AreEqual(0, this.body.ChildElements.Count);

            vse.AddValueSet(this.vs2, DateTime.Now);
            Assert.AreEqual(0, this.body.ChildElements.Count);

            vse.AddValueSetsAppendix();
            Assert.AreEqual(7, this.body.ChildElements.Count);

            Table t1 = this.body.ChildElements[2] as Table;
            Assert.IsNotNull(t1);

            Table t2 = this.body.ChildElements[5] as Table;
            Assert.IsNotNull(t2);
        }

        [TestMethod]
        public void ValueSetExport_NoMembers()
        {
            HyperlinkTracker hyperlinkTracker = new HyperlinkTracker();
            TableCollection tables = new TableCollection(this.body);
            ValueSetsExport vse = new ValueSetsExport(null, mainPart, hyperlinkTracker, tables, false, 2, null);

            vse.AddValueSet(this.vs3, DateTime.Now);
            Assert.AreEqual(0, this.body.ChildElements.Count);
        }

        [TestMethod]
        public void ValueSetExport_SameValueSetTwice()
        {
            HyperlinkTracker hyperlinkTracker = new HyperlinkTracker();
            TableCollection tables = new TableCollection(this.body);
            ValueSetsExport vse = new ValueSetsExport(null, mainPart, hyperlinkTracker, tables, false, 2, null);

            vse.AddValueSet(this.vs2, DateTime.Now);
            Assert.AreEqual(3, this.body.ChildElements.Count);

            vse.AddValueSet(this.vs2, DateTime.Now);
            Assert.AreEqual(3, this.body.ChildElements.Count);
        }

        private void AssertWordXpath(OpenXmlElement element, string xpath, string message = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(element.OuterXml);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

            XmlNodeList nodes = doc.DocumentElement.SelectNodes(xpath, nsManager);

            if (message != null)
                Assert.AreNotEqual(0, nodes.Count, message);
            else
                Assert.AreNotEqual(0, nodes.Count);
        }
    }
}
