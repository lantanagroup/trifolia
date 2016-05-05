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

        [TestMethod]
        public void TemplateValidationSuccess()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "section", "Section");
            var tc1 = tdb.GenerateConstraint(newTemplate, null, null, "code", "SHALL", "1..1");
            tdb.GenerateConstraint(newTemplate, tc1, null, "@code", "SHALL", "1..1", value: "TEST", displayName: "TESTDISPLAY");
            tdb.GenerateConstraint(newTemplate, null, null, "title", "SHALL", "1..1");
            tdb.GenerateConstraint(newTemplate, null, null, "text", "SHALL", "1..1");
            var tc2 = tdb.GenerateConstraint(newTemplate, null, null, "entry", "SHALL", "1..1");
            tdb.GenerateConstraint(newTemplate, tc2, null, "observation", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(0, errors.Count, "Shouldn't have found any errors.");
        }

        [TestMethod]
        public void TemplateValidationSuccessWithCustomDataType()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.GenerateConstraint(newTemplate, null, null, "effectiveTime", "SHALL", "1..1", "IVL_TS");
            tdb.GenerateConstraint(newTemplate, tc1, null, "low", "SHALL", "1..1");
            tdb.GenerateConstraint(newTemplate, tc1, null, "high", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(0, errors.Count, "Shouldn't have found any errors.");
        }

        [TestMethod]
        public void TemplateValidationFailureBadSchematron()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.GenerateConstraint(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            tc1.Schematron = "not(thisisbad";

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Custom schematron is not valid: 'not(thisisbad' has an invalid token.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Error, errors[0].Level);
        }

        [TestMethod]
        public void TemplateValidationFailureInvalidConstraint()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.GenerateConstraint(newTemplate, null, null, "badelement", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Constraint has context of \"badelement\" which is not found in the schema.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Error, errors[0].Level);
        }

        [TestMethod]
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

        [TestMethod]
        public void TemplateValidationSuccessContainedTemplate()
        {
            Template containedTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            tdb.GenerateConstraint(containedTemplate, null, null, "effectiveTime", "SHALL", "1..1");

            Template containingTemplate = tdb.GenerateTemplate("4.3.2.1", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.GenerateConstraint(containingTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            tdb.GenerateConstraint(containingTemplate, tc1, containedTemplate, "observation", "SHALL", "1..1");

            List<TemplateValidationResult> errors = containingTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(0, errors.Count, "Shouldn't have found any errors.");
        }

        [TestMethod]
        public void TemplateValidationFailureInvalidContainedTemplate()
        {
            Template containedTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            tdb.GenerateConstraint(containedTemplate, null, null, "effectiveTime", "SHALL", "1..1");

            Template containingTemplate = tdb.GenerateTemplate("4.3.2.1", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            var tc1 = tdb.GenerateConstraint(containingTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            tdb.GenerateConstraint(containingTemplate, tc1, containedTemplate, "substanceAdministration", "SHALL", "1..1");

            List<TemplateValidationResult> errors = containingTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find one error.");
            Assert.AreEqual("Contained template \"1.2.3.4\" has a type of \"Observation\" which does not match the containing element \"SubstanceAdministration\"", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Error, errors[0].Level);
        }

        [TestMethod]
        public void TemplateValidationFailureUnbranchedMultipleCardinality()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            newTemplate.IsOpen = true;

            var tc1 = tdb.GenerateConstraint(newTemplate, null, null, "templateId", "SHALL", "1..1");
            tdb.GenerateConstraint(newTemplate, tc1, null, "@root", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Schema allows multiple for \"templateId\" but the constraint is not branched. Consider branching this constraint.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Warning, errors[0].Level);
        }

        [TestMethod]
        public void TemplateValidationFailureBranchWithoutIdentifier()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            newTemplate.IsOpen = true;

            var tc1 = tdb.GenerateConstraint(newTemplate, null, null, "templateId", "SHALL", "1..1", isBranch: true);
            tdb.GenerateConstraint(newTemplate, tc1, null, "@root", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(1, errors.Count, "Expected to find only one error.");
            Assert.AreEqual("Branched constraint \"templateId\" does not have any identifiers associated with it.", errors[0].Message);
            Assert.AreEqual(ValidationLevels.Warning, errors[0].Level);
        }

        [TestMethod]
        public void TemplateValidationFailureCodeAndEntryRelationshipUnbranched()
        {
            Template newTemplate = tdb.GenerateTemplate("1.2.3.4.5.6.7", tdb.FindTemplateType("CDA", "Document"), "Test 1", this.ig, "observation", "Observation");
            newTemplate.IsOpen = true;

            var tc1 = tdb.GenerateConstraint(newTemplate, null, null, "value", "SHALL", "1..1", dataType: "CD");
            tdb.GenerateConstraint(newTemplate, tc1, null, "@code", "SHALL", "1..1");

            var tc2 = tdb.GenerateConstraint(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            tdb.GenerateConstraint(newTemplate, tc2, null, "observation", "SHALL", "1..1");

            var tc3 = tdb.GenerateConstraint(newTemplate, null, null, "entryRelationship", "SHALL", "1..1");
            var tc4 = tdb.GenerateConstraint(newTemplate, tc3, null, "observation", "SHALL", "1..1");
            tdb.GenerateConstraint(newTemplate, tc4, null, "value", "SHALL", "1..1");

            List<TemplateValidationResult> errors = newTemplate.ValidateTemplate();

            Assert.IsNotNull(errors, "Errors list should not be null.");
            Assert.AreEqual(4, errors.Count, "Expected to find three errors.");
        }
    }
}
