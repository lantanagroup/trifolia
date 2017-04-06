using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Shared;

namespace Trifolia.Test.Export.FHIR
{
    [TestClass, DeploymentItem("Schemas\\", "Schemas\\")]
    public class STU3Tests
    {
        private const string RESOURCE_TYPE_OBS = "Observation";
        private const string DESCRIPTION = "This is my test profile";

        private MockObjectRepository tdb;
        private ImplementationGuide fhirIg;
        private SimpleSchema schema;
        private Template template;

        [TestInitialize]
        public void Init()
        {
            this.tdb = new MockObjectRepository();
            this.tdb.InitializeFHIR3Repository();

            var fhirIgType = this.tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_FHIR_STU3_IG_TYPE_NAME);
            this.fhirIg = this.tdb.FindOrCreateImplementationGuide(fhirIgType, "Test IG");

            string schemaLocation = Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(fhirIgType);
            schema = SimpleSchema.CreateSimpleSchema(schemaLocation);

            var templateType = tdb.FindTemplateType(MockObjectRepository.DEFAULT_FHIR_STU3_IG_TYPE_NAME, RESOURCE_TYPE_OBS);
            this.template = tdb.CreateTemplate("http://test.com", templateType, "Test Observation", fhirIg, RESOURCE_TYPE_OBS, RESOURCE_TYPE_OBS, DESCRIPTION);
            var c1 = tdb.AddConstraintToTemplate(this.template, null, null, "extension");
            var c2 = tdb.AddConstraintToTemplate(this.template, c1, null, "value[x]", isChoice: true);
            var c3 = tdb.AddConstraintToTemplate(this.template, c2, null, "valueCodeableConcept", value: "1234-x", displayName: "test single-value binding");
        }

        [TestMethod]
        public void StructureDefinitionExport()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(this.template, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));

            Assert.IsNotNull(structureDefinition);
            Assert.AreEqual("Test_Observation", structureDefinition.Id);
            Assert.AreEqual("Test Observation", structureDefinition.Name);
            Assert.AreEqual(DESCRIPTION, structureDefinition.Description.Value);
            Assert.AreEqual("http://test.com", structureDefinition.Url);
            Assert.IsNotNull(structureDefinition.Differential);
        }

        [TestMethod]
        public void TestChoiceWithOneOption()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(this.template, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));

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
        public void TestChoiceWithTwoOptions()
        {
            var parentConstraint = this.tdb.TemplateConstraints.Single(y => y.Context == "value[x]");
            this.tdb.AddConstraintToTemplate(this.template, parentConstraint, null, "valuePeriod");

            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(template, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));

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
