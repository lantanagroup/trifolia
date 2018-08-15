extern alias fhir_latest;

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Export.FHIR.Latest;
using Trifolia.Shared;

namespace Trifolia.Test.Export.FHIR.Latest
{
    [TestClass]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class LatestTests
    {
        private const string RESOURCE_TYPE_OBS = "Observation";
        private const string DESCRIPTION = "This is my test profile";

        private static MockObjectRepository tdb;
        private static ImplementationGuide fhirIg;
        private static SimpleSchema schema;
        private static Template template1;
        private static Template template2;
        private static Template template3;
        private static Template template4;

        [ClassInitialize]
        public static void SetupData(TestContext context)
        {
            LatestTests.tdb = new MockObjectRepository();
            LatestTests.tdb.InitializeFHIRLatestRepository();

            var fhirIgType = LatestTests.tdb.FindImplementationGuideType(Constants.IGTypeNames.FHIR_LATEST);
            LatestTests.fhirIg = LatestTests.tdb.FindOrCreateImplementationGuide(fhirIgType, "Test IG");

            string schemaLocation = Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(fhirIgType);
            schema = SimpleSchema.CreateSimpleSchema(schemaLocation);

            var templateType = tdb.FindTemplateType(Constants.IGTypeNames.FHIR_LATEST, RESOURCE_TYPE_OBS);

            LatestTests.template1 = LatestTests.tdb.CreateTemplate("http://test.com/StructureDefinition/1", templateType, "Test Observation", fhirIg, RESOURCE_TYPE_OBS, RESOURCE_TYPE_OBS, DESCRIPTION);
            var c1_1 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template1, null, null, "extension");
            var c1_2 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template1, c1_1, null, "value[x]", isChoice: true);
            var c1_3 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template1, c1_2, null, "valueCodeableConcept", value: "1234-x", displayName: "test single-value binding");

            LatestTests.template2 = LatestTests.tdb.CreateTemplate("http://test.com/StructureDefinition/2", templateType, "Test Observation", fhirIg, RESOURCE_TYPE_OBS, RESOURCE_TYPE_OBS, DESCRIPTION);
            var c2_1 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template2, null, null, "extension");
            var c2_2 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template2, c2_1, null, "value[x]", isChoice: true, cardinality: "1..1");
            var c2_3 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template2, c2_2, null, "valueCodeableConcept", value: "1234-x", displayName: "test single-value binding");
            var c2_4 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template2, c2_2, null, "valuePeriod");
            var c2_5 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template2, c2_4, null, "start", cardinality: "1..1");

            LatestTests.template3 = LatestTests.tdb.CreateTemplate("http://test.com/StructureDefinition/3", templateType, "Test Observation", fhirIg, RESOURCE_TYPE_OBS, RESOURCE_TYPE_OBS, DESCRIPTION);
            var c3_1 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template3, null, null, "comment", cardinality: "1..1", value: "Test");

            LatestTests.template4 = LatestTests.tdb.CreateTemplate("http://test.com/StructureDefinition/4", templateType, "Test Observation", fhirIg, RESOURCE_TYPE_OBS, RESOURCE_TYPE_OBS, DESCRIPTION);
            var c4_1 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template4, null, null, "referenceRange", cardinality: "1..1", isBranch: true);
            var c4_2 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template4, c4_1, null, "low", cardinality: "1..1");
            var c4_3 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template4, c4_1, null, "appliesTo", cardinality: "1..1", value: "at1", displayName: "appliesTo1", isBranchIdentifier: true);
            var c4_4 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template4, null, null, "referenceRange", cardinality: "1..1", isBranch: true);
            var c4_5 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template4, c4_4, null, "high", cardinality: "1..1");
            var c4_6 = LatestTests.tdb.AddConstraintToTemplate(LatestTests.template4, c4_4, null, "appliesTo", cardinality: "1..1", value: "at2", displayName: "appliesTo2", isBranchIdentifier: true);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void Latest_StructureDefinitionExport()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(LatestTests.template1, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));

            Assert.IsNotNull(structureDefinition);
            Assert.AreEqual("Test_Observation", structureDefinition.Id);
            Assert.AreEqual("Test Observation", structureDefinition.Name);
            Assert.AreEqual(DESCRIPTION, structureDefinition.Description.Value);
            Assert.AreEqual("http://test.com/StructureDefinition/1", structureDefinition.Url);
            Assert.IsNotNull(structureDefinition.Differential);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void Latest_TestChoiceWithOneOption()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(LatestTests.template1, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));
            var xml = fhir_latest.Hl7.Fhir.Serialization.FhirSerializer.SerializeToXml(structureDefinition);

            Assert.IsNotNull(structureDefinition);
            Assert.IsNotNull(structureDefinition.Differential);

            var diff = structureDefinition.Differential;
            Assert.AreEqual(4, diff.Element.Count);
            Assert.AreEqual("Observation", diff.Element[0].ElementId);
            Assert.AreEqual("Observation", diff.Element[0].Path);

            var extension = diff.Element[1];
            Assert.AreEqual("Observation.extension", extension.ElementId);
            Assert.AreEqual("Observation.extension", extension.Path);

            var valueChoice = diff.Element[2];
            Assert.AreEqual("Observation.extension.value[x]", valueChoice.ElementId);
            Assert.AreEqual("Observation.extension.value[x]", valueChoice.Path);
            Assert.IsNotNull(valueChoice.Slicing);
            Assert.IsNotNull(valueChoice.Slicing.Discriminator);
            Assert.AreEqual(1, valueChoice.Slicing.Discriminator.Count);
            Assert.AreEqual(fhir_latest.Hl7.Fhir.Model.ElementDefinition.DiscriminatorType.Type, valueChoice.Slicing.Discriminator[0].Type);
            Assert.AreEqual("$this", valueChoice.Slicing.Discriminator[0].Path);

            var valueCodeableConcept = diff.Element[3];
            Assert.AreEqual("Observation.extension.value[x]:valueCodeableConcept", valueCodeableConcept.ElementId);
            Assert.AreEqual("Observation.extension.valueCodeableConcept", valueCodeableConcept.Path);
            Assert.AreEqual("valueCodeableConcept", valueCodeableConcept.SliceName);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void Latest_TestChoiceWithTwoOptions()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(LatestTests.template2, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));
            var xml = fhir_latest.Hl7.Fhir.Serialization.FhirSerializer.SerializeToXml(structureDefinition);

            Assert.IsNotNull(structureDefinition);
            Assert.IsNotNull(structureDefinition.Differential);

            var diff = structureDefinition.Differential;
            Assert.AreEqual(6, diff.Element.Count);
            Assert.AreEqual("Observation", diff.Element[0].Path);

            var extension = diff.Element[1];
            Assert.AreEqual("Observation.extension", extension.Path);

            var valueChoice = diff.Element[2];
            Assert.AreEqual("Observation.extension.value[x]", valueChoice.ElementId);
            Assert.AreEqual("Observation.extension.value[x]", valueChoice.Path);
            Assert.IsNotNull(valueChoice.Slicing);
            Assert.IsNotNull(valueChoice.Slicing.Discriminator);
            Assert.AreEqual(1, valueChoice.Slicing.Discriminator.Count());
            Assert.AreEqual(1, valueChoice.Min);
            Assert.AreEqual("1", valueChoice.Max);

            var discriminator = valueChoice.Slicing.Discriminator.FirstOrDefault();
            Assert.IsNotNull(discriminator);
            Assert.AreEqual(fhir_latest.Hl7.Fhir.Model.ElementDefinition.DiscriminatorType.Type, discriminator.Type);
            Assert.AreEqual("$this", discriminator.Path);

            var valueCodeableConcept = diff.Element[3];
            Assert.AreEqual("Observation.extension.value[x]:valueCodeableConcept", valueCodeableConcept.ElementId);
            Assert.AreEqual("Observation.extension.valueCodeableConcept", valueCodeableConcept.Path);
            Assert.AreEqual("valueCodeableConcept", valueCodeableConcept.SliceName);

            var valuePeriod = diff.Element[4];
            Assert.AreEqual("Observation.extension.value[x]:valuePeriod", valuePeriod.ElementId);
            Assert.AreEqual("Observation.extension.valuePeriod", valuePeriod.Path);
            Assert.AreEqual("valuePeriod", valuePeriod.SliceName);

            var start = diff.Element[5];
            Assert.AreEqual("Observation.extension.value[x]:valuePeriod.start", start.ElementId);
            Assert.AreEqual("Observation.extension.valuePeriod.start", start.Path);
            Assert.AreEqual("valuePeriod", start.SliceName);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void Latest_TestCommentValue()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(LatestTests.template3, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));
            var xml = fhir_latest.Hl7.Fhir.Serialization.FhirSerializer.SerializeToXml(structureDefinition);

            Assert.IsNotNull(structureDefinition);
            Assert.IsNotNull(structureDefinition.Differential);

            var diff = structureDefinition.Differential;
            Assert.AreEqual(2, diff.Element.Count);
            Assert.AreEqual("Observation", diff.Element[0].Path);

            var comment = diff.Element[1];
            Assert.AreEqual("Observation.comment", comment.Path);
            Assert.AreEqual("Observation.comment", comment.ElementId);
            Assert.AreEqual(1, comment.Min);
            Assert.AreEqual("1", comment.Max);
            Assert.IsNull(comment.SliceName);
            Assert.IsNull(comment.Slicing);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        public void Latest_TestBranch()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(tdb, "http", "localhost");
            var structureDefinition = exporter.Convert(LatestTests.template4, schema.GetSchemaFromContext(RESOURCE_TYPE_OBS));
            var xml = fhir_latest.Hl7.Fhir.Serialization.FhirSerializer.SerializeToXml(structureDefinition);

            Assert.IsNotNull(structureDefinition);
            Assert.IsNotNull(structureDefinition.Differential);

            var diff = structureDefinition.Differential;

            Assert.AreEqual(8, diff.Element.Count);

            var referenceRangeSlice = diff.Element[1];
            Assert.AreEqual("Observation.referenceRange", referenceRangeSlice.ElementId);
            Assert.AreEqual("Observation.referenceRange", referenceRangeSlice.Path);
            Assert.IsNotNull(referenceRangeSlice.Slicing);
            Assert.IsNotNull(referenceRangeSlice.Slicing.Discriminator);
            Assert.AreEqual(1, referenceRangeSlice.Slicing.Discriminator.Count);
            Assert.AreEqual(fhir_latest.Hl7.Fhir.Model.ElementDefinition.DiscriminatorType.Value, referenceRangeSlice.Slicing.Discriminator.First().Type);
            Assert.AreEqual("appliesTo", referenceRangeSlice.Slicing.Discriminator.First().Path);

            var referenceRangeSlice1 = diff.Element[2];
            Assert.AreEqual("Observation.referenceRange:referenceRange1", referenceRangeSlice1.ElementId);
            Assert.AreEqual("Observation.referenceRange", referenceRangeSlice1.Path);
            Assert.AreEqual("referenceRange1", referenceRangeSlice1.SliceName);
            Assert.AreEqual(1, referenceRangeSlice1.Min);
            Assert.AreEqual("1", referenceRangeSlice1.Max);

            var low1 = diff.Element[3];
            Assert.AreEqual("Observation.referenceRange:referenceRange1.low", low1.ElementId);
            Assert.AreEqual("Observation.referenceRange.low", low1.Path);
            Assert.AreEqual("referenceRange1", low1.SliceName);
            Assert.AreEqual(1, low1.Min);
            Assert.AreEqual("1", low1.Max);

            var appliesTo1 = diff.Element[4];
            Assert.AreEqual("Observation.referenceRange:referenceRange1.appliesTo", appliesTo1.ElementId);
            Assert.AreEqual("Observation.referenceRange.appliesTo", appliesTo1.Path);
            Assert.AreEqual("referenceRange1", appliesTo1.SliceName);
            Assert.AreEqual(1, appliesTo1.Min);
            Assert.AreEqual("1", appliesTo1.Max);
            Assert.IsNotNull(appliesTo1.Pattern);

            var referenceRangeSlice2 = diff.Element[5];
            Assert.AreEqual("Observation.referenceRange:referenceRange2", referenceRangeSlice2.ElementId);
            Assert.AreEqual("Observation.referenceRange", referenceRangeSlice2.Path);
            Assert.AreEqual("referenceRange2", referenceRangeSlice2.SliceName);
            Assert.AreEqual(1, referenceRangeSlice2.Min);
            Assert.AreEqual("1", referenceRangeSlice2.Max);

            var high2 = diff.Element[6];
            Assert.AreEqual("Observation.referenceRange:referenceRange2.high", high2.ElementId);
            Assert.AreEqual("Observation.referenceRange.high", high2.Path);
            Assert.AreEqual("referenceRange2", high2.SliceName);
            Assert.AreEqual(1, high2.Min);
            Assert.AreEqual("1", high2.Max);

            var appliesTo2 = diff.Element[7];
            Assert.AreEqual("Observation.referenceRange:referenceRange2.appliesTo", appliesTo2.ElementId);
            Assert.AreEqual("Observation.referenceRange.appliesTo", appliesTo2.Path);
            Assert.AreEqual("referenceRange2", appliesTo2.SliceName);
            Assert.AreEqual(1, appliesTo2.Min);
            Assert.AreEqual("1", appliesTo2.Max);
            Assert.IsNotNull(appliesTo2.Pattern);
        }
    }
}
