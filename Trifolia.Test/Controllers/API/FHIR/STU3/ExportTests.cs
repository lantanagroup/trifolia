extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Serialization;
using fhir_stu3.Hl7.Fhir.Model;
using LantanaGroup.Schematron;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Import.Native;
using Trifolia.Shared;
using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Test.Controllers.API.FHIR.STU3
{
    [TestClass]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class ExportTests
    {
        private const string STRUC_DEF_SCH = "Trifolia.Test.Schemas.FHIR_STU3.structuredefinition.sch";
        private const string IMPL_GUIDE_SCH = "Trifolia.Test.Schemas.FHIR_STU3.implementationguide.sch";
        private const string IMPORT_XML = "Trifolia.Test.DocSamples.ccda-on-fhir-native.xml";

        private static MockObjectRepository tdb;
        private static string exportedXml;
        private static Bundle exportedBundle;

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
            ExportTests.exportedBundle = exporter.GetImplementationGuides(implementationGuideId: ig.Id, include: "ImplementationGuide:resource");

            ExportTests.exportedXml = FhirSerializer.SerializeResourceToXml(ExportTests.exportedBundle);

            Assert.IsNotNull(ExportTests.exportedXml);
        }

        /// <summary>
        /// Tests aspects of structure definition exports that are not covered by schematron from the FHIR build
        /// </summary>
        [TestMethod, TestCategory("FHIR3")]
        public void TestStructureDefinition()
        {
            var structureDefinitions = ExportTests.exportedBundle.Entry
                .Where(y => y.Resource is StructureDefinition)
                .Select(y => y.Resource as StructureDefinition);

            foreach (var structureDefinition in structureDefinitions)
            {
                Assert.IsNotNull(structureDefinition.Derivation, "Expected all structure definitions to have a derivation property");
                Assert.AreEqual(StructureDefinition.TypeDerivationRule.Constraint, structureDefinition.Derivation, "Expected derivation to be constraint");
            }
        }
    }
}
