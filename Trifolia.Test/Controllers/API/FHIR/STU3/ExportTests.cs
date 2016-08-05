extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Serialization;
using LantanaGroup.Schematron;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Import.Native;
using Trifolia.Shared;
using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;

namespace Trifolia.Test.Controllers.API.FHIR.STU3
{
    [TestClass]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class ExportTests
    {
        private const string STRUC_DEF_SCH = "Trifolia.Test.Schemas.FHIR_Latest.structuredefinition.sch";
        private const string IMPL_GUIDE_SCH = "Trifolia.Test.Schemas.FHIR_Latest.implementationguide.sch";
        private const string IMPORT_XML = "Trifolia.Test.DocSamples.ccda-on-fhir-native.xml";

        private static MockObjectRepository tdb;
        private static string exportedXml;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            ExportTests.tdb = new MockObjectRepository();
            TrifoliaImporter importer = new TrifoliaImporter(ExportTests.tdb);
            ExportTests.tdb.InitializeFHIR3Repository();
            ExportTests.tdb.InitializeLCGAndLogin();

            string importContent = Helper.GetSampleContents(IMPORT_XML);
            var importModel = ImportModel.Deserialize(importContent);

            var importStatus = importer.Import(importModel);

            Assert.IsTrue(importStatus.Success, "Expected import to succeed");
            Assert.AreNotEqual(importStatus.ImplementationGuides.Count, 0, "Expected import to include implementation guides");

            ImplementationGuide ig = ExportTests.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == importStatus.ImplementationGuides.First().InternalId);
            var schema = ig.ImplementationGuideType.GetSimpleSchema();
            ImplementationGuideExporter exporter = new ImplementationGuideExporter(ExportTests.tdb, schema, "localhost", "http");
            var exportedBundle = exporter.GetImplementationGuides(implementationGuideId: ig.Id, include: "ImplementationGuide:resource");

            ExportTests.exportedXml = FhirSerializer.SerializeResourceToXml(exportedBundle);

            Assert.IsNotNull(ExportTests.exportedXml);
        }

        [TestMethod]
        public void ValidateStructureDefinition()
        {
            string strucDefSch = Helper.GetSampleContents(STRUC_DEF_SCH);
            var validator = SchematronValidationFactory.NewValidator(strucDefSch);

            var results = validator.Validate(ExportTests.exportedXml, LantanaGroup.ValidationUtility.Model.ValidationPhases.All);

            Assert.AreEqual(0, results.Messages.Count, "Expected 0 validation messages");
        }

        [TestMethod]
        public void ValidateImplementationGuide()
        {
            string implGuidSche = Helper.GetSampleContents(IMPL_GUIDE_SCH);
            var validator = SchematronValidationFactory.NewValidator(implGuidSche);

            var results = validator.Validate(ExportTests.exportedXml, LantanaGroup.ValidationUtility.Model.ValidationPhases.All);

            Assert.AreEqual(0, results.Messages.Count, "Expected 0 validation messages");
        }
    }
}
