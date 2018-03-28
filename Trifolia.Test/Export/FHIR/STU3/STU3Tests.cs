using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Shared;

namespace Trifolia.Test.Export.FHIR.STU3
{
    [TestClass]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class STU3Tests
    {
        private const string RESOURCE_TYPE_OBS = "Observation";
        private const string DESCRIPTION = "This is my test profile";

        private static MockObjectRepository tdb;
        private static ImplementationGuide fhirIg;
        private static SimpleSchema schema;
        private static Template template1;
        private static Template template2;

        [ClassInitialize]
        public static void SetupData(TestContext context)
        {
            STU3Tests.tdb = new MockObjectRepository();
            STU3Tests.tdb.InitializeFHIR3Repository();

            var fhirIgType = STU3Tests.tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_FHIR_STU3_IG_TYPE_NAME);
            STU3Tests.fhirIg = STU3Tests.tdb.FindOrCreateImplementationGuide(fhirIgType, "Test IG");

            string schemaLocation = Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(fhirIgType);
            schema = SimpleSchema.CreateSimpleSchema(schemaLocation);

            var templateType = STU3Tests.tdb.FindTemplateType(MockObjectRepository.DEFAULT_FHIR_STU3_IG_TYPE_NAME, RESOURCE_TYPE_OBS);

            STU3Tests.template1 = STU3Tests.tdb.CreateTemplate("http://test.com/StructureDefinition/1", templateType, "Test Observation", fhirIg, RESOURCE_TYPE_OBS, RESOURCE_TYPE_OBS, DESCRIPTION);
            var c1_1 = STU3Tests.tdb.AddConstraintToTemplate(STU3Tests.template1, null, null, "extension");
            var c1_2 = STU3Tests.tdb.AddConstraintToTemplate(STU3Tests.template1, c1_1, null, "value[x]", isChoice: true);
            var c1_3 = STU3Tests.tdb.AddConstraintToTemplate(STU3Tests.template1, c1_2, null, "valueCodeableConcept", value: "1234-x", displayName: "test single-value binding");

            STU3Tests.template2 = STU3Tests.tdb.CreateTemplate("http://test.com/StructureDefinition/2", templateType, "Test Observation", fhirIg, RESOURCE_TYPE_OBS, RESOURCE_TYPE_OBS, DESCRIPTION);
            var c2_1 = STU3Tests.tdb.AddConstraintToTemplate(STU3Tests.template2, null, null, "extension");
            var c2_2 = STU3Tests.tdb.AddConstraintToTemplate(STU3Tests.template2, c2_1, null, "value[x]", isChoice: true);
            var c2_3 = STU3Tests.tdb.AddConstraintToTemplate(STU3Tests.template2, c2_2, null, "valueCodeableConcept", value: "1234-x", displayName: "test single-value binding");
            var c2_4 = STU3Tests.tdb.AddConstraintToTemplate(STU3Tests.template2, c2_2, null, "valuePeriod");
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void STU3_StructureDefinitionExport()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(STU3Tests.template1, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));

            Assert.IsNotNull(structureDefinition);
            Assert.AreEqual("Test_Observation", structureDefinition.Id);
            Assert.AreEqual("Test Observation", structureDefinition.Name);
            Assert.AreEqual(DESCRIPTION, structureDefinition.Description.Value);
            Assert.AreEqual("http://test.com/StructureDefinition/1", structureDefinition.Url);
            Assert.IsNotNull(structureDefinition.Differential);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void STU3_TestChoiceWithOneOption()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(STU3Tests.template1, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));

            Assert.IsNotNull(structureDefinition);
            Assert.IsNotNull(structureDefinition.Differential);

            var diff = structureDefinition.Differential;
            Assert.AreEqual(4, diff.Element.Count);
            Assert.AreEqual("Observation", diff.Element[0].Path);
            Assert.AreEqual("Observation.extension", diff.Element[1].Path);
            Assert.AreEqual("Observation.extension.value[x]", diff.Element[2].Path);
            Assert.AreEqual("Observation.extension.valueCodeableConcept", diff.Element[3].Path);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void STU3_TestChoiceWithTwoOptions()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(STU3Tests.template2, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));

            Assert.IsNotNull(structureDefinition);
            Assert.IsNotNull(structureDefinition.Differential);

            var diff = structureDefinition.Differential;
            Assert.AreEqual(5, diff.Element.Count);
            Assert.AreEqual("Observation", diff.Element[0].Path);
            Assert.AreEqual("Observation.extension", diff.Element[1].Path);
            Assert.AreEqual("Observation.extension.value[x]", diff.Element[2].Path);
            Assert.AreEqual("Observation.extension.valueCodeableConcept", diff.Element[3].Path);
            Assert.AreEqual("Observation.extension.valuePeriod", diff.Element[4].Path);

            // Ensure that slicing is set on the value[x] choice
            Assert.IsNotNull(diff.Element[2].Slicing);
            Assert.IsNotNull(diff.Element[2].Slicing.Discriminator);
            Assert.AreEqual(1, diff.Element[2].Slicing.Discriminator.Count());

            var discriminator = diff.Element[2].Slicing.Discriminator.FirstOrDefault();
            Assert.IsNotNull(discriminator);
            Assert.AreEqual("@type", discriminator.Path);
        }
    }
}
