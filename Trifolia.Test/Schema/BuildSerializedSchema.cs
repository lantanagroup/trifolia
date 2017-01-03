using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Test.Schema
{
    [TestClass]
    public class BuildSerializedSchema
    {
        [TestMethod, TestCategory("Schema")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void TestSimplifiedSchema_CDA()
        {
            SimpleSchema cdaSchema = SimpleSchema.CreateSimpleSchema(
                Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(
                    new ImplementationGuideType()
                        {
                            Name = "CDA",
                            SchemaLocation = "CDA.xsd"
                        }));

            Assert.IsNotNull(cdaSchema.ComplexTypes);
            Assert.AreEqual(219, cdaSchema.ComplexTypes.Count);
            Assert.IsNotNull(cdaSchema.Children);
            Assert.AreEqual(1, cdaSchema.Children.Count);

            // Test root document level
            SimpleSchema.SchemaObject clinicalDocument = cdaSchema.Children.SingleOrDefault(y => y.Name == "ClinicalDocument");
            Assert.IsNotNull(clinicalDocument);
            Assert.AreEqual("ClinicalDocument", clinicalDocument.DataType);
            Assert.AreEqual(false, clinicalDocument.Mixed);
            Assert.AreEqual(30, clinicalDocument.Children.Count);
            var clinicalDocumentInvalidChildNames = clinicalDocument.Children.Where(y => string.IsNullOrEmpty(y.Name) || y.Name.Length <= 1);
            Assert.IsTrue(clinicalDocumentInvalidChildNames.Count() == 0);
            Assert.AreEqual(SimpleSchema.ObjectTypes.Element, clinicalDocument.Type);

            // Test datatype
            SimpleSchema.SchemaObject typeId = clinicalDocument.Children.SingleOrDefault(y => y.Name == "typeId");
            Assert.IsNotNull(typeId);
            Assert.AreEqual("typeId[typeId]", typeId.ToString());
            Assert.AreEqual("typeId", typeId.DataType);
            Assert.AreEqual("1..1", typeId.Cardinality);
            Assert.AreEqual("SHALL", typeId.Conformance);
            Assert.AreEqual(false, typeId.Mixed);
            Assert.AreEqual(5, typeId.Children.Count);
            var typeIdInvalidNames = typeId.Children.Where(y => string.IsNullOrEmpty(y.Name) || y.Name.Length <= 1);
            Assert.IsTrue(typeIdInvalidNames.Count() == 0);
            Assert.AreEqual(SimpleSchema.ObjectTypes.Element, typeId.Type);

            // Test attribute
            SimpleSchema.SchemaObject typeIdRoot = typeId.Children.SingleOrDefault(y => y.Name == "root");
            Assert.IsNotNull(typeId);
            Assert.AreEqual("uid", typeIdRoot.DataType);
            Assert.AreEqual("1..1", typeIdRoot.Cardinality);
            Assert.AreEqual("SHALL", typeIdRoot.Conformance);
            Assert.AreEqual(false, typeIdRoot.Mixed);
            Assert.AreEqual(0, typeIdRoot.Children.Count);
            Assert.AreEqual(SimpleSchema.ObjectTypes.Attribute, typeIdRoot.Type);
            
            // Test many cardinality
            SimpleSchema.SchemaObject recordTarget = clinicalDocument.Children.SingleOrDefault(y => y.Name == "recordTarget");
            Assert.IsNotNull(recordTarget);
            Assert.AreEqual("RecordTarget", recordTarget.DataType);
            Assert.AreEqual("1..*", recordTarget.Cardinality);
            Assert.AreEqual("SHALL", recordTarget.Conformance);
            Assert.AreEqual(false, recordTarget.Mixed);
            Assert.AreEqual(7, recordTarget.Children.Count);
            var recordTargetInvalidNames = recordTarget.Children.Where(y => string.IsNullOrEmpty(y.Name) || y.Name.Length <= 1);
            Assert.IsTrue(recordTargetInvalidNames.Count() == 0);
            Assert.AreEqual(SimpleSchema.ObjectTypes.Element, recordTarget.Type);

            // Test patientRole
            SimpleSchema.SchemaObject patientRole = recordTarget.Children.SingleOrDefault(y => y.Name == "patientRole");
            Assert.IsNotNull(patientRole);
            Assert.AreEqual("PatientRole", patientRole.DataType);
            Assert.AreEqual("1..1", patientRole.Cardinality);
            Assert.AreEqual("SHALL", patientRole.Conformance);
            Assert.AreEqual(false, patientRole.Mixed);
            Assert.AreEqual(10, patientRole.Children.Count);
            Assert.AreEqual(SimpleSchema.ObjectTypes.Element, patientRole.Type);

            // Test conformance
            SimpleSchema.SchemaObject patient = patientRole.Children.SingleOrDefault(y => y.Name == "patient");
            Assert.IsNotNull(patient);
            Assert.AreEqual("Patient", patient.DataType);
            Assert.AreEqual("0..1", patient.Cardinality);
            Assert.AreEqual("MAY", patient.Conformance);
            Assert.AreEqual(false, patient.Mixed);
            Assert.AreEqual(18, patient.Children.Count);
            Assert.IsNotNull(patient.Children.SingleOrDefault(y => y.Name == "sdtc:raceCode"));
            var patientInvalidNames = patient.Children.Where(y => string.IsNullOrEmpty(y.Name) || y.Name.Length <= 1);
            Assert.IsTrue(patientInvalidNames.Count() == 0);
            Assert.AreEqual(SimpleSchema.ObjectTypes.Element, patientRole.Type);

            SimpleSchema.SchemaObject sdtcRaceCode = patient.Children.Single(y => y.Name == "sdtc:raceCode");
            var sdtcRaceCodeInvalidNames = sdtcRaceCode.Children.Where(y => string.IsNullOrEmpty(y.Name) || y.Name.Length <= 1);
            Assert.IsTrue(sdtcRaceCodeInvalidNames.Count() == 0);
            
            // Test mixed
            SimpleSchema.SchemaObject patientName = patient.Children.SingleOrDefault(y => y.Name == "name");
            Assert.IsNotNull(patientName);
            Assert.AreEqual("PN", patientName.DataType);
            Assert.AreEqual("0..*", patientName.Cardinality);
            Assert.AreEqual("MAY", patientName.Conformance);
            Assert.AreEqual(true, patientName.Mixed);
            Assert.AreEqual(8, patientName.Children.Count, "Expected patient to have 8 children");       // TODO: Schema Choice support temporarily removed from non-FHIR schemas
            Assert.IsTrue(patientName.Children.Count(y => string.IsNullOrEmpty(y.Name) || y.Name.Length <= 1) == 0);
            Assert.AreEqual(SimpleSchema.ObjectTypes.Element, patientRole.Type);
        }

        [TestMethod, TestCategory("Schema")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void TestSimplifiedSchema_Observation()
        {
            SimpleSchema cdaSchema = SimpleSchema.CreateSimpleSchema(
                Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(
                    new ImplementationGuideType()
                    {
                        Name = "CDA",
                        SchemaLocation = "CDA.xsd"
                    }));

            cdaSchema = cdaSchema.GetSchemaFromContext("Observation");

            var foundClassCodeAttr = cdaSchema.Children.SingleOrDefault(y => y.Name == "classCode" && y.IsAttribute);
            Assert.IsNotNull(foundClassCodeAttr);

            var foundCode = cdaSchema.Children.SingleOrDefault(y => y.Name == "code" && !y.IsAttribute);
            Assert.IsNotNull(foundCode);

            var foundValue = cdaSchema.Children.SingleOrDefault(y => y.Name == "value" && !y.IsAttribute);
            Assert.IsNotNull(foundValue);

            var foundEntryRelationship = cdaSchema.Children.SingleOrDefault(y => y.Name == "entryRelationship" && !y.IsAttribute);
            Assert.IsNotNull(foundEntryRelationship);

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var foundChoice = foundEntryRelationship.Children.SingleOrDefault(y => y.Name == "choice" && !y.IsAttribute);
            Assert.IsNotNull(foundChoice);

            var foundObservation = foundChoice.Children.SingleOrDefault(y => y.Name == "observation" && !y.IsAttribute);
            Assert.IsNotNull(foundObservation);
            */

            var foundObservation = foundEntryRelationship.Children.SingleOrDefault(y => y.Name == "observation" && !y.IsAttribute);
            Assert.IsNotNull(foundObservation);

            Assert.IsNotNull(cdaSchema);
        }

        [TestMethod, TestCategory("Schema")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void TestSimplifiedSchema_eMeasure()
        {
            SimpleSchema cdaSchema = SimpleSchema.CreateSimpleSchema(
                Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(
                    new ImplementationGuideType()
                    {
                        Name = "CDA",
                        SchemaLocation = "CDA.xsd"
                    }));

            cdaSchema = cdaSchema.GetSchemaFromContext("Observation");

            var foundClassCodeAttr = cdaSchema.Children.SingleOrDefault(y => y.Name == "classCode" && y.IsAttribute);
            Assert.IsNotNull(foundClassCodeAttr);

            var foundCode = cdaSchema.Children.SingleOrDefault(y => y.Name == "code" && !y.IsAttribute);
            Assert.IsNotNull(foundCode);

            var foundValue = cdaSchema.Children.SingleOrDefault(y => y.Name == "value" && !y.IsAttribute);
            Assert.IsNotNull(foundValue);

            var foundEntryRelationship = cdaSchema.Children.SingleOrDefault(y => y.Name == "entryRelationship" && !y.IsAttribute);
            Assert.IsNotNull(foundEntryRelationship);

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var foundChoice = foundEntryRelationship.Children.SingleOrDefault(y => y.Name == "choice" && !y.IsAttribute);
            Assert.IsNotNull(foundChoice);

            var foundObservation = foundChoice.Children.SingleOrDefault(y => y.Name == "observation" && !y.IsAttribute);
            Assert.IsNotNull(foundObservation);
            */

            var foundObservation = foundEntryRelationship.Children.SingleOrDefault(y => y.Name == "observation" && !y.IsAttribute);
            Assert.IsNotNull(foundObservation);

            Assert.IsNotNull(cdaSchema);
        }
    }
}
