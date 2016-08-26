extern alias fhir_dstu1;
using fhir_dstu1.Hl7.Fhir.Model;
using fhir_dstu1.Hl7.Fhir.Serialization;
using System;
using System.Linq;
using System.Collections.Generic;
using LantanaGroup.Schematron;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Import.Native;
using Trifolia.Export.FHIR.DSTU1;
using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Test.Controllers.API.FHIR.DSTU1
{
    [TestClass]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class FHIRDSTU1ExportTest
    {
        private const string PROFILE_SCH = "Trifolia.Test.Schemas.FHIR_DSTU1.profile.sch";
        private const string ATOM_SCH = "Trifolia.Test.Schemas.FHIR_DSTU1.fhir-atom.sch";
        private const string IMPORT_XML = "Trifolia.Test.DocSamples.Camara-FHIR-IG.xml";

        private static MockObjectRepository tdb;
        private static String exportedXml;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            FHIRDSTU1ExportTest.tdb = new MockObjectRepository();
            TrifoliaImporter importer = new TrifoliaImporter(FHIRDSTU1ExportTest.tdb);
            FHIRDSTU1ExportTest.tdb.InitializeFHIRRepository();
            FHIRDSTU1ExportTest.tdb.InitializeLCGAndLogin();    

            string importContent = Helper.GetSampleContents(IMPORT_XML);
            var importModel = ImportModel.Deserialize(importContent);

            var importStatus = importer.Import(importModel);    

            Assert.IsTrue(importStatus.Success, "Expected import to succeed");
            Assert.AreNotEqual(importStatus.ImplementationGuides.Count, 0, "Expected import to include implementation guides");

            IGSettingsManager igManager = new IGSettingsManager(tdb, importStatus.ImplementationGuides.First().InternalId);

            TemplateImporter templateImporter = new TemplateImporter(tdb, shouldUpdate: true);

            FHIRDSTU1ExportTest.exportedXml = FHIRExporter.GenerateExport(tdb, templateImporter.Import(importModel.Template), igManager);

            Assert.IsNotNull(FHIRDSTU1ExportTest.exportedXml);
        }

        [TestMethod]
        public void ValidateProfileFHIRDSTU1()
        {
            string profileSch = Helper.GetSampleContents(PROFILE_SCH);

            var validator = SchematronValidationFactory.NewValidator(profileSch);

            var results = validator.Validate(FHIRDSTU1ExportTest.exportedXml, LantanaGroup.ValidationUtility.Model.ValidationPhases.All);

            Assert.AreEqual(0, results.Messages.Count, "Expected 0 validation messages");
        }
    }
}
