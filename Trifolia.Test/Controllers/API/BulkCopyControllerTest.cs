using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Web.Models.BulkCopy;
using Trifolia.Web.Controllers.API;

namespace Trifolia.Test.Controllers.API
{
    [TestClass]
    public class BulkCopyControllerTest
    {
        private byte[] exampleContent;

        [TestInitialize()]
        public void Setup()
        {
            this.exampleContent = Helper.GetSampleContentBytes("Trifolia.Test.DocSamples.ExampleBulkCopy.xlsx");
        }

        [TestMethod]
        public void TestParseColumnsNoHeader()
        {
            MockObjectRepository repo = new MockObjectRepository();
            BulkCopyController controller = new BulkCopyController(repo);
            ExcelUpload uploadModel = new ExcelUpload()
            {
                ExcelFile = this.exampleContent
            };

            List<ExcelSheet> sheets = controller.Parse(uploadModel);

            Assert.IsNotNull(sheets);
            Assert.AreEqual(2, sheets.Count);

            var firstSheet = sheets[0];
            var secondSheet = sheets[1];

            Assert.IsNotNull(firstSheet.Columns);
            Assert.AreEqual(4, firstSheet.Columns.Count);
            Assert.AreEqual("A", firstSheet.Columns[0].Letter);
            Assert.AreEqual("A", firstSheet.Columns[0].Name);

            Assert.IsNotNull(secondSheet.Columns);
            Assert.AreEqual(15, secondSheet.Columns.Count);
            Assert.AreEqual("A", secondSheet.Columns[0].Letter);
            Assert.AreEqual("A", secondSheet.Columns[0].Name);
        }

        [TestMethod]
        public void TestParseColumnsHeaders()
        {
            MockObjectRepository repo = new MockObjectRepository();
            BulkCopyController controller = new BulkCopyController(repo);
            ExcelUpload uploadModel = new ExcelUpload()
            {
                ExcelFile = this.exampleContent,
                FirstRowIsHeader = true
            };

            List<ExcelSheet> sheets = controller.Parse(uploadModel);

            Assert.IsNotNull(sheets);
            Assert.AreEqual(2, sheets.Count);

            var firstSheet = sheets[0];
            var secondSheet = sheets[1];

            Assert.IsNotNull(firstSheet.Columns);
            Assert.AreEqual(4, firstSheet.Columns.Count);
            Assert.AreEqual("A", firstSheet.Columns[0].Letter);
            Assert.AreEqual("Name", firstSheet.Columns[0].Name);
            Assert.AreEqual("B", firstSheet.Columns[1].Letter);
            Assert.AreEqual("OID", firstSheet.Columns[1].Name);

            Assert.IsNotNull(secondSheet.Columns);
            Assert.AreEqual(15, secondSheet.Columns.Count);
            Assert.AreEqual("A", secondSheet.Columns[0].Letter);
            Assert.AreEqual("Template", secondSheet.Columns[0].Name);
            Assert.AreEqual("B", secondSheet.Columns[1].Letter);
            Assert.AreEqual("Number", secondSheet.Columns[1].Name);
        }

        [TestMethod]
        public void TestParseRowsWithHeaders()
        {
            MockObjectRepository repo = new MockObjectRepository();
            BulkCopyController controller = new BulkCopyController(repo);
            ExcelUpload uploadModel = new ExcelUpload()
            {
                ExcelFile = this.exampleContent,
                FirstRowIsHeader = true
            };

            List<ExcelSheet> sheets = controller.Parse(uploadModel);

            Assert.IsNotNull(sheets);
            Assert.AreEqual(2, sheets.Count);

            var firstSheet = sheets[0];

            Assert.IsNotNull(firstSheet.Rows);
            Assert.AreEqual(3, firstSheet.Rows.Count);

            var firstSheetFirstRow = firstSheet.Rows[0];
            Assert.IsNotNull(firstSheetFirstRow.Cells);
            Assert.AreEqual(4, firstSheetFirstRow.Cells.Count);

            Assert.AreEqual("A", firstSheetFirstRow.Cells[0].Letter);
            Assert.AreEqual("Template 1", firstSheetFirstRow.Cells[0].Value);

            Assert.AreEqual("B", firstSheetFirstRow.Cells[1].Letter);
            Assert.AreEqual("1.2.3.4", firstSheetFirstRow.Cells[1].Value);

            Assert.AreEqual("B", firstSheetFirstRow.Cells[1].Letter);
            Assert.AreEqual("1.2.3.4", firstSheetFirstRow.Cells[1].Value);

            var secondSheet = sheets[1];
        }
    }
}
