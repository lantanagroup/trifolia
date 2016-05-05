using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Web.Controllers.API;
using Trifolia.Web.Models.TerminologyManagement;

namespace Trifolia.Test.Controllers.API
{
    [TestClass]
    public class TerminologyControllerTest
    {
        [TestMethod]
        public void TestImportExcel()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            var data = Helper.GetSampleContentBytes("Trifolia.Test.DocSamples.ExampleTerminologyImport.xlsx");
            TerminologyController controller = new TerminologyController(tdb);
            ImportCheckRequest request = new ImportCheckRequest()
            {
                Data = data,
                FirstRowIsHeader = true
            };

            var response = controller.CheckExcelImport(request);
            Assert.AreEqual(response.Errors.Count, 1, "Expected an error related to code system not being found");

            // Code system is found, first valueset is not found, to be added
            var codesystem = tdb.FindOrCreateCodeSystem("Test Code System", "4.3.2.1");
            response = controller.CheckExcelImport(request);
            Assert.AreEqual(response.Errors.Count, 0);
            Assert.AreEqual(response.ValueSets.Count, 2);
            Assert.IsNull(response.ValueSets[0].Id);
            Assert.AreEqual(response.ValueSets[0].ChangeType, ImportValueSetChange.ChangeTypes.Add);

            // First valueset is found, name to be updated
            var valueset = tdb.FindOrCreateValueSet("Value Set", "1.2.3.4");
            response = controller.CheckExcelImport(request);
            Assert.AreEqual(response.Errors.Count, 0);
            Assert.AreEqual(response.ValueSets.Count, 2);
            Assert.IsNotNull(response.ValueSets[0].Id);
            Assert.AreEqual(response.ValueSets[0].ChangeType, ImportValueSetChange.ChangeTypes.Update);

            // First valueset is found, nothing to update
            valueset.Name = "Test Valueset 1";
            response = controller.CheckExcelImport(request);
            Assert.AreEqual(response.Errors.Count, 0);
            Assert.AreEqual(response.ValueSets.Count, 2);
            Assert.IsNotNull(response.ValueSets[0].Id);
            Assert.AreEqual(response.ValueSets[0].ChangeType, ImportValueSetChange.ChangeTypes.None);

            // Concepts to be added
            Assert.AreEqual(response.ValueSets[0].Concepts.Count, 1);
            Assert.IsNull(response.ValueSets[0].Concepts[0].Id);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].ChangeType, ImportValueSetChange.ChangeTypes.Add);
            
            // Found exact concept, but nothing to update
            var concept = tdb.FindOrCreateValueSetMember(valueset, codesystem, "asdf", "TEST", "active", "5/15/2014");
            response = controller.CheckExcelImport(request);
            Assert.AreEqual(response.Errors.Count, 0);
            Assert.IsNotNull(response.ValueSets[0].Concepts[0].Id);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].ChangeType, ImportValueSetChange.ChangeTypes.None);

            // Found exact concept, but update the display
            concept.DisplayName = "invalid display";
            response = controller.CheckExcelImport(request);
            Assert.AreEqual(response.Errors.Count, 0);
            Assert.IsNotNull(response.ValueSets[0].Concepts[0].Id);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].ChangeType, ImportValueSetChange.ChangeTypes.Update);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].Code, "asdf");
            Assert.AreEqual(response.ValueSets[0].Concepts[0].DisplayName, "TEST");

            concept.Status = null;
            concept.StatusDate = null;
            response = controller.CheckExcelImport(request);
            Assert.AreEqual(response.Errors.Count, 0);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].ChangeType, ImportValueSetChange.ChangeTypes.Add);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].Code, "asdf");
            Assert.AreEqual(response.ValueSets[0].Concepts[0].DisplayName, "TEST");
            Assert.AreEqual(response.ValueSets[0].Concepts[0].CodeSystemOid, codesystem.Oid);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].CodeSystemName, codesystem.Name);
            Assert.AreEqual(response.ValueSets[0].Concepts[0].Status, "active");
            Assert.AreEqual(response.ValueSets[0].Concepts[0].StatusDate, DateTime.Parse("5/15/2014"));
        }
    }
}
