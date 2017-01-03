using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Test.Reporting
{
    [TestClass]
    public class TemplateValidationTest
    {
        private MockObjectRepository tdb = new MockObjectRepository();
        private ImplementationGuide ig = null;
        private SimpleSchema mainSchema;

        [TestInitialize]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void Setup()
        {
            this.tdb.InitializeCDARepository();

            this.ig = this.tdb.FindOrAddImplementationGuide(this.tdb.FindImplementationGuideType("CDA"), "Test IG");
            this.mainSchema = this.tdb.FindImplementationGuideType("CDA").GetSimpleSchema();
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationSuccess()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "section", "Section");
            var tc1 = tdb.AddConstraintToTemplate(newTemplate, null, null, "code", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, tc1, null, "@code", "SHALL", "1..1", value: "TEST", displayName: "TESTDISPLAY");
            tdb.AddConstraintToTemplate(newTemplate, null, null, "title", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, null, null, "text", "SHALL", "1..1");
            var tc2 = tdb.AddConstraintToTemplate(newTemplate, null, null, "entry", "SHALL", "1..1");

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var tc2Choice = tdb.AddConstraintToTemplate(newTemplate, tc2, null, "choice", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, tc2Choice, null, "observation", "SHALL", "1..1");
            */
            
            tdb.AddConstraintToTemplate(newTemplate, tc2, null, "observation", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(0, errors.Count, "Shouldn't have found any errors.");
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationSuccessWithCustomDataType()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.AddConstraintToTemplate(newTemplate, context: "effectiveTime", conformance: "SHALL", cardinality: "1..1", dataType: "IVL_TS");

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var tc1Choice = tc1.AddChildConstraintToTemplate(tdb, newTemplate, context: "choice");   // effectiveTime.choice

            tc1Choice
                .AddChildConstraintToTemplate(tdb, newTemplate, null, "low", "SHALL", "1..1");       // effectiveTime.choice.low
            tc1Choice
                .AddChildConstraintToTemplate(tdb, newTemplate, null, "high", "SHALL", "1..1");      // effectiveTime.choice.high
                */

            tc1
                .AddChildConstraintToTemplate(tdb, newTemplate, null, "low", "SHALL", "1..1");       // effectiveTime.low
            tc1
                .AddChildConstraintToTemplate(tdb, newTemplate, null, "high", "SHALL", "1..1");      // effectiveTime.high

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(0, errors.Count, "Shouldn't have found any errors.");
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationFailureBadSchematron()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.AddConstraintToTemplate(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            tc1.Schematron = "not(thisisbad";

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Custom schematron is not valid: 'not(thisisbad' has an invalid token.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Error, errors[0].Level);
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationFailureInvalidConstraint()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.AddConstraintToTemplate(newTemplate, null, null, "badelement", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Constraint has context of \"badelement\" which is not found in the schema.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Error, errors[0].Level);
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationFailurePrimitiveWithoutNarrative()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.GeneratePrimitive(newTemplate, null, "SHALL", string.Empty);

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Primitive does not have any narrative text.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Error, errors[0].Level);
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationSuccessContainedTemplate()
        {
            Template containedTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            tdb.AddConstraintToTemplate(containedTemplate, null, null, "effectiveTime", "SHALL", "1..1");

            Template containingTemplate = tdb.GenerateTemplate("4.3.2.1", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var tc1 = tdb
                .AddConstraintToTemplate(containingTemplate, context: "entryRelationship", conformance: "SHALL", cardinality: "1..1")   // entryRelationship
                .AddChildConstraintToTemplate(tdb, containingTemplate, context: "choice", conformance: "SHALL", cardinality: "1..1")         // entryRelationship.choice
                .AddChildConstraintToTemplate(tdb, containingTemplate, containedTemplate, "observation", "SHALL", "1..1");                   // entryRelationship.choice.observation
                */

            var tc1 = tdb
                .AddConstraintToTemplate(containingTemplate, context: "entryRelationship", conformance: "SHALL", cardinality: "1..1")   // entryRelationship
                .AddChildConstraintToTemplate(tdb, containingTemplate, containedTemplate, "observation", "SHALL", "1..1");                   // entryRelationship.observation

            List<TemplateValidationResult> errors = containingTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(0, errors.Count, "Shouldn't have found any errors.");
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationFailureInvalidContainedTemplate()
        {
            Template containedTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            tdb.AddConstraintToTemplate(containedTemplate, null, null, "effectiveTime", "SHALL", "1..1");

            Template containingTemplate = tdb.GenerateTemplate("4.3.2.1", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var tc1 = tdb.AddConstraintToTemplate(containingTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            var tc1Choice = tdb.AddConstraintToTemplate(containingTemplate, tc1, null, "choice", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(containingTemplate, tc1Choice, containedTemplate, "substanceAdministration", "SHALL", "1..1");
            */

            var tc1 = tdb.AddConstraintToTemplate(containingTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(containingTemplate, tc1, containedTemplate, "substanceAdministration", "SHALL", "1..1");

            List<TemplateValidationResult> errors = containingTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find one error.");
            Assert.AreEqual("Contained template \"1.2.3.4\" has a type of \"Observation\" which does not match the containing element \"SubstanceAdministration\"", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Error, errors[0].Level);
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationFailureUnbranchedMultipleCardinality()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            newTemplate.IsOpen = true;

            var tc1 = tdb.AddConstraintToTemplate(newTemplate, null, null, "templateId", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, tc1, null, "@root", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Schema allows multiple for \"templateId\" but the constraint is not branched. Consider branching this constraint.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Warning, errors[0].Level);
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationFailureBranchWithoutIdentifier()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            newTemplate.IsOpen = true;

            var tc1 = tdb.AddConstraintToTemplate(newTemplate, null, null, "templateId", "SHALL", "1..1", isBranch: true);
            tdb.AddConstraintToTemplate(newTemplate, tc1, null, "@root", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Branched constraint \"templateId\" does not have any identifiers associated with it.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Warning, errors[0].Level);
        }

        [TestMethod, TestCategory("Validation")]
        public void TemplateValidationFailureCodeAndEntryRelationshipUnbranched()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4.5.6.7", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            newTemplate.IsOpen = true;

            var tc1 = tdb.AddConstraintToTemplate(newTemplate, null, null, "value", "SHALL", "1..1", dataType: "CD");
            tdb.AddConstraintToTemplate(newTemplate, tc1, null, "@code", "SHALL", "1..1");

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var tc2 = tdb.AddConstraintToTemplate(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            var tc2Choice = tdb.AddConstraintToTemplate(newTemplate, tc2, null, "choice", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, tc2Choice, null, "observation", "SHALL", "1..1");

            var tc3 = tdb.AddConstraintToTemplate(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            var tc3Choice = tdb.AddConstraintToTemplate(newTemplate, tc3, null, "choice", "SHALL", "1..1");
            var tc4 = tdb.AddConstraintToTemplate(newTemplate, tc3Choice, null, "observation", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, tc4, null, "value", "SHALL", "1..1");
            */

            var tc2 = tdb.AddConstraintToTemplate(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, tc2, null, "observation", "SHALL", "1..1");

            var tc3 = tdb.AddConstraintToTemplate(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            var tc4 = tdb.AddConstraintToTemplate(newTemplate, tc3, null, "observation", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(newTemplate, tc4, null, "value", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(4, errors.Count, "Expected to find three errors.");
        }
    }
}
