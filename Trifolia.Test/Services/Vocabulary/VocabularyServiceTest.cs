using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Trifolia.DB;
using Trifolia.Export.Terminology;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Test.Services.Vocabulary
{
    [TestClass]
    public class VocabularyServiceTest
    {
        private MockObjectRepository tdb;
        private ImplementationGuide ig;
        private ValueSet vs1;
        private ValueSet vs2;
        private ValueSet vs3;

        [TestInitialize]
        public void TestSetup()
        {
            this.tdb = new MockObjectRepository();
            this.tdb.InitializeCDARepository();

            CodeSystem cs = tdb.FindOrCreateCodeSystem("LOINC", "1.2.3.4.5");

            this.vs1 = tdb.FindOrCreateValueSet("Test Valueset 1", "1.2.3.4");
            this.tdb.FindOrCreateValueSetMember(this.vs1, cs, "1", "One", "active", "1/1/2000");
            this.tdb.FindOrCreateValueSetMember(this.vs1, cs, "2", "Two", "active", "1/1/2000");
            this.tdb.FindOrCreateValueSetMember(this.vs1, cs, "1", "One", "inactive", "1/1/2001");
            this.tdb.FindOrCreateValueSetMember(this.vs1, cs, "3", "Three", "active", "1/1/2001");
            this.tdb.FindOrCreateValueSetMember(this.vs1, cs, "1", "One", "active", "1/1/2002");

            this.vs2 = tdb.FindOrCreateValueSet("Test Valueset 2", "4.3.2.1");
            this.tdb.FindOrCreateValueSetMember(this.vs2, cs, "1", "One");
            this.tdb.FindOrCreateValueSetMember(this.vs2, cs, "2", "Two");
            this.tdb.FindOrCreateValueSetMember(this.vs2, cs, "3", "Three", "inactive", "1/1/2000");
            this.tdb.FindOrCreateValueSetMember(this.vs2, cs, "4", "Four", "active", "1/1/2000");

            this.vs3 = tdb.FindOrCreateValueSet("Test Valueset 3", "1.4.2.3");
            this.tdb.FindOrCreateValueSetMember(this.vs3, cs, "1", "One");
            this.tdb.FindOrCreateValueSetMember(this.vs3, cs, "2", "Two");
            this.tdb.FindOrCreateValueSetMember(this.vs3, cs, "3", "Three", valueSetStatus: "inactive", dateOfValueSetStatus: "08/05/2013");
            this.tdb.FindOrCreateValueSetMember(this.vs3, cs, "4", "Four", valueSetStatus: "active");
            this.tdb.FindOrCreateValueSetMember(this.vs3, cs, "5", "Five");

            this.ig = this.tdb.FindOrCreateImplementationGuide("CDA", "Test IG");
        }

        /// <summary>
        /// Tests that calling the VocabularyService's GetImplementationGuideVocabularySpreadsheet returns a valid OpenXML Spreadsheet
        /// that contains two sheets (one for the "Affected Value Sets" and one for the "Value Set Members"), and that each sheet contains
        /// column width definitions, header rows, and the correct number of rows
        /// </summary>
        [TestMethod, TestCategory("Terminology")]
        public void GetImplementationGuideVocabularySpreadsheet()
        {
            Template t = this.tdb.CreateTemplate("1.2.3.4.5", "Document", "Test Template 1", this.ig);
            this.tdb.AddConstraintToTemplate(t, null, null, "code", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs1);
            this.tdb.AddConstraintToTemplate(t, null, null, "value", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs2);

            ExcelExporter exporter = new ExcelExporter(this.tdb);
            byte[] spreadsheetBytes = exporter.GetSpreadsheet(this.ig.Id, 100);

            Assert.IsNotNull(spreadsheetBytes);

            using (MemoryStream ms = new MemoryStream(spreadsheetBytes))
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(ms, false);

                Sheets sheets = doc.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                Assert.IsNotNull(sheets);

                // Affected Value Sets
                Sheet summarySheet = sheets.FirstChild as Sheet;
                Assert.IsNotNull(summarySheet);
                Assert.AreEqual("Affected Value Sets", summarySheet.Name.Value);

                WorksheetPart summarySheetPart = doc.WorkbookPart.WorksheetParts.FirstOrDefault();
                Assert.IsNotNull(summarySheetPart, "Expected to find two worksheets, but didn't find any.");

                Columns summarySheetCols = summarySheetPart.Worksheet.GetFirstChild<Columns>();
                Assert.IsNotNull(summarySheetCols);

                SheetData summarySheetData = summarySheetPart.Worksheet.GetFirstChild<SheetData>();
                Assert.IsNotNull(summarySheetData);

                Assert.AreEqual(3, summarySheetData.ChildElements.Count);
                Row summaryHeaderRow = summarySheetData.ChildElements[0] as Row;
                Assert.IsNotNull(summaryHeaderRow);
                Assert.AreEqual((uint)1, summaryHeaderRow.RowIndex.Value);
                Assert.AreEqual(2, summaryHeaderRow.ChildElements.Count);
                
                Cell summaryHeaderCell1 = summaryHeaderRow.Elements<Cell>().Single(y => y.CellReference == "A1");
                Cell summaryHeaderCell2 = summaryHeaderRow.Elements<Cell>().Single(y => y.CellReference == "B1");
                Assert.AreEqual("Value Set Name", summaryHeaderCell1.CellValue.Text);
                Assert.AreEqual("Value Set OID", summaryHeaderCell2.CellValue.Text);

                Row valueSet1Row = summarySheetData.ChildElements[1] as Row;
                Assert.IsNotNull(valueSet1Row);

                Cell valueSet1Cell1 = valueSet1Row.Elements<Cell>().Single(y => y.CellReference == "A2");
                Cell valueSet1Cell2 = valueSet1Row.Elements<Cell>().Single(y => y.CellReference == "B2");
                Assert.AreEqual(this.vs1.Name, valueSet1Cell1.CellValue.Text);
                Assert.AreEqual(this.vs1.GetIdentifier(), valueSet1Cell2.CellValue.Text);

                Row valueSet2Row = summarySheetData.ChildElements[2] as Row;
                Assert.IsNotNull(valueSet1Row);

                Cell valueSet2Cell1 = valueSet2Row.Elements<Cell>().Single(y => y.CellReference == "A3");
                Cell valueSet2Cell2 = valueSet2Row.Elements<Cell>().Single(y => y.CellReference == "B3");
                Assert.AreEqual(this.vs2.Name, valueSet2Cell1.CellValue.Text);
                Assert.AreEqual(this.vs2.GetIdentifier(), valueSet2Cell2.CellValue.Text);

                // Value Set Members
                Sheet membersSheet = sheets.LastChild as Sheet;
                Assert.IsNotNull(membersSheet);
                Assert.AreEqual("Value Set Members", membersSheet.Name.Value);

                WorksheetPart membersSheetPart = doc.WorkbookPart.WorksheetParts.LastOrDefault();
                Assert.IsNotNull(membersSheetPart, "Expected to find two worksheets, but didn't find any.");

                Columns membersSheetCols = membersSheetPart.Worksheet.GetFirstChild<Columns>();
                Assert.IsNotNull(membersSheetCols);

                SheetData membersSheetData = membersSheetPart.Worksheet.GetFirstChild<SheetData>();
                Assert.IsNotNull(membersSheetData);

                Assert.AreEqual(7, membersSheetData.ChildElements.Count);
                Row membersHeaderRow = membersSheetData.ChildElements[0] as Row;
                Assert.IsNotNull(membersHeaderRow);
                Assert.AreEqual((uint)1, membersHeaderRow.RowIndex.Value);
                Assert.AreEqual(5, membersHeaderRow.ChildElements.Count);

                Cell membersHeaderCell1 = membersHeaderRow.Elements<Cell>().Single(y => y.CellReference == "A1");
                Cell membersHeaderCell2 = membersHeaderRow.Elements<Cell>().Single(y => y.CellReference == "B1");
                Cell membersHeaderCell3 = membersHeaderRow.Elements<Cell>().Single(y => y.CellReference == "C1");
                Cell membersHeaderCell4 = membersHeaderRow.Elements<Cell>().Single(y => y.CellReference == "D1");
                Cell membersHeaderCell5 = membersHeaderRow.Elements<Cell>().Single(y => y.CellReference == "E1");
                Assert.AreEqual("Value Set OID", membersHeaderCell1.CellValue.Text);
                Assert.AreEqual("Value Set Name", membersHeaderCell2.CellValue.Text);
                Assert.AreEqual("Code", membersHeaderCell3.CellValue.Text);
                Assert.AreEqual("Display Name", membersHeaderCell4.CellValue.Text);
                Assert.AreEqual("Code System Name", membersHeaderCell5.CellValue.Text);
            }
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetImplementationGuideVocabulary_NoStaticValuesets()
        {
            Template t = this.tdb.CreateTemplate("1.2.3.4.5", "Document", "Test Template 1", this.ig);
            TemplateConstraint tc = this.tdb.AddConstraintToTemplate(t, null, null, "code", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs1);
            tc.IsStatic = false;

            NativeTerminologyExporter exporter = new NativeTerminologyExporter(this.tdb);
            byte[] export = exporter.GetExport(ig.Id, 0, Encoding.UTF8);
            string vocXml = Encoding.UTF8.GetString(export);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(vocXml);

            Assert.AreEqual("systems", doc.DocumentElement.LocalName);
            Assert.AreEqual(0, doc.DocumentElement.ChildNodes.Count);
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetImplementationGuideVocabulary_StaticValuesetNoDate()
        {
            Template t = this.tdb.CreateTemplate("1.2.3.4.5", "Document", "Test Template 1", this.ig);
            TemplateConstraint tc = this.tdb.AddConstraintToTemplate(t, null, null, "code", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs1);
            tc.IsStatic = true;

            NativeTerminologyExporter exporter = new NativeTerminologyExporter(this.tdb);
            byte[] export = exporter.GetExport(ig.Id, 0, Encoding.UTF8);
            string vocXml = Encoding.UTF8.GetString(export);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(vocXml);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("voc", "http://www.lantanagroup.com/voc");

            Assert.AreEqual("systems", doc.DocumentElement.LocalName);
            Assert.AreEqual(1, doc.DocumentElement.ChildNodes.Count);

            XmlElement exportValueSet = doc.DocumentElement.ChildNodes[0] as XmlElement;
            Assert.AreEqual(3, exportValueSet.ChildNodes.Count);
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='1']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='2']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='3']", nsManager));
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetImplementationGuideVocabulary_StaticValuesetWithDate1()
        {
            Template t = this.tdb.CreateTemplate("1.2.3.4.5", "Document", "Test Template 1", this.ig);
            TemplateConstraint tc = this.tdb.AddConstraintToTemplate(t, null, null, "code", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs1);
            tc.IsStatic = true;
            tc.ValueSetDate = DateTime.Parse("1/1/2000");

            NativeTerminologyExporter exporter = new NativeTerminologyExporter(this.tdb);
            byte[] export = exporter.GetExport(ig.Id, 0, Encoding.UTF8);
            string vocXml = Encoding.UTF8.GetString(export);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(vocXml);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("voc", "http://www.lantanagroup.com/voc");

            Assert.AreEqual("systems", doc.DocumentElement.LocalName);
            Assert.AreEqual(1, doc.DocumentElement.ChildNodes.Count);

            XmlElement exportValueSet = doc.DocumentElement.ChildNodes[0] as XmlElement;
            Assert.AreEqual(2, exportValueSet.ChildNodes.Count);
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='1']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='2']", nsManager));
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetImplementationGuideVocabulary_StaticValuesetWithDate2()
        {
            Template t = this.tdb.CreateTemplate("1.2.3.4.5", "Document", "Test Template 1", this.ig);
            TemplateConstraint tc = this.tdb.AddConstraintToTemplate(t, null, null, "code", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs1);
            tc.IsStatic = true;
            tc.ValueSetDate = DateTime.Parse("1/1/2001");

            NativeTerminologyExporter exporter = new NativeTerminologyExporter(this.tdb);
            byte[] export = exporter.GetExport(ig.Id, 0, Encoding.UTF8);
            string vocXml = Encoding.UTF8.GetString(export);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(vocXml);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("voc", "http://www.lantanagroup.com/voc");

            Assert.AreEqual("systems", doc.DocumentElement.LocalName);
            Assert.AreEqual(1, doc.DocumentElement.ChildNodes.Count);

            XmlElement exportValueSet = doc.DocumentElement.ChildNodes[0] as XmlElement;
            Assert.AreEqual(2, exportValueSet.ChildNodes.Count);
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='2']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='3']", nsManager));
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetImplementationGuideVocabulary_StaticValuesetWithDate3()
        {
            Template t = this.tdb.CreateTemplate("1.2.3.4.5", "Document", "Test Template 1", this.ig);
            TemplateConstraint tc = this.tdb.AddConstraintToTemplate(t, null, null, "code", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs1);
            tc.IsStatic = true;
            tc.ValueSetDate = DateTime.Parse("1/1/2002");

            NativeTerminologyExporter exporter = new NativeTerminologyExporter(this.tdb);
            byte[] export = exporter.GetExport(ig.Id, 0, Encoding.UTF8);
            string vocXml = Encoding.UTF8.GetString(export);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(vocXml);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("voc", "http://www.lantanagroup.com/voc");

            Assert.AreEqual("systems", doc.DocumentElement.LocalName);
            Assert.AreEqual(1, doc.DocumentElement.ChildNodes.Count);

            XmlElement exportValueSet = doc.DocumentElement.ChildNodes[0] as XmlElement;
            Assert.AreEqual(3, exportValueSet.ChildNodes.Count);
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='1']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='2']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='3']", nsManager));
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetImplementationGuideVocabulary_Valueset3()
        {
            Template t = this.tdb.CreateTemplate("1.2.3.4.5", "Document", "Test Template 1", this.ig);
            TemplateConstraint tc = this.tdb.AddConstraintToTemplate(t, null, null, "code", "SHALL", "1..1", valueConformance: "SHALL", valueSet: this.vs3);
            tc.IsStatic = true;

            NativeTerminologyExporter exporter = new NativeTerminologyExporter(this.tdb);
            byte[] export = exporter.GetExport(ig.Id, 0, Encoding.UTF8);
            string vocXml = Encoding.UTF8.GetString(export);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(vocXml);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("voc", "http://www.lantanagroup.com/voc");

            Assert.AreEqual("systems", doc.DocumentElement.LocalName);
            Assert.AreEqual(1, doc.DocumentElement.ChildNodes.Count);

            XmlElement exportValueSet = doc.DocumentElement.ChildNodes[0] as XmlElement;
            Assert.AreEqual(4, exportValueSet.ChildNodes.Count);
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='1']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='2']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='4']", nsManager));
            Assert.IsNotNull(exportValueSet.SelectSingleNode("voc:code[@value='5']", nsManager));
        }
    }
}
