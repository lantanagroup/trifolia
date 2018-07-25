using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Test.Generation.IG
{
    [TestClass]
    public class FormattedConstraintSchemaChoiceTest
    {
        private MockObjectRepository tdb;
        private ImplementationGuide fhirIg;
        private IGSettingsManager fhirIgSettings;
        private IIGTypePlugin fhirIgTypePlugin;

        [TestInitialize]
        public void Setup()
        {
            this.tdb = new MockObjectRepository();
            this.tdb.InitializeCDARepository();
            this.tdb.InitializeFHIR3Repository();

            var fhirIgType = this.tdb.FindImplementationGuideType(Constants.IGTypeNames.FHIR_STU3);
            this.fhirIg = this.tdb.FindOrCreateImplementationGuide(fhirIgType, "Test FHIR IG");
            this.fhirIgSettings = new IGSettingsManager(this.tdb, this.fhirIg.Id);
            this.fhirIgTypePlugin = fhirIgType.GetPlugin();
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void SingleChild()
        {
            var templateType = this.tdb.FindOrCreateTemplateType(this.fhirIg.ImplementationGuideType, "Observation");
            var template = this.tdb.CreateTemplate("http://test.com", templateType, "Test Observation", this.fhirIg);
            var c1 = this.tdb.AddConstraintToTemplate(template, null, null, "effective[x]", "SHALL", "1..1", isChoice: true);
            var c2 = this.tdb.AddConstraintToTemplate(template, c1, null, "effectiveDateTime");

            var c1fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c1);
            var c1Text = c1fc.GetPlainText();

            var c2fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c2);
            var c2Text = c2fc.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] effective[x], where effective[x] is (CONF:2-1)", c1Text);
            Assert.AreEqual("effectiveDateTime (CONF:2-2)", c2Text);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void MultipleChildren()
        {
            var templateType = this.tdb.FindOrCreateTemplateType(this.fhirIg.ImplementationGuideType, "Observation");
            var template = this.tdb.CreateTemplate("http://test.com", templateType, "Test Observation", this.fhirIg);
            var c1 = this.tdb.AddConstraintToTemplate(template, null, null, "effective[x]", "SHALL", "1..1", isChoice: true);
            var c2 = this.tdb.AddConstraintToTemplate(template, c1, null, "effectiveDateTime");
            var c3 = this.tdb.AddConstraintToTemplate(template, c1, null, "effectivePeriod");

            var c1fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c1);
            var c1Text = c1fc.GetPlainText();

            var c2fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c2);
            var c2Text = c2fc.GetPlainText();

            var c3fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c3);
            var c3Text = c3fc.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] effective[x], where effective[x] is one of (CONF:2-1)", c1Text);
            Assert.AreEqual("effectiveDateTime (CONF:2-2)", c2Text);
            Assert.AreEqual("or effectivePeriod (CONF:2-3)", c3Text);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void SingleChildWithValueSetBinding()
        {
            var templateType = this.tdb.FindOrCreateTemplateType(this.fhirIg.ImplementationGuideType, "Extension");
            var template = this.tdb.CreateTemplate("http://test.com", templateType, "Test Extension", this.fhirIg);
            var valueSet = this.tdb.FindOrCreateValueSet("Test Value Set", "urn:oid:1.2.3.4");
            var c1 = this.tdb.AddConstraintToTemplate(template, null, null, "value[x]", "SHALL", "1..1", isChoice: true);
            var c2 = this.tdb.AddConstraintToTemplate(template, c1, null, "valueCodeableConcept", valueSet: valueSet);

            var c1fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c1);
            var c1Text = c1fc.GetPlainText();

            var c2fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c2);
            var c2Text = c2fc.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] value[x], where value[x] is (CONF:2-1)", c1Text);
            Assert.AreEqual("valueCodeableConcept (ValueSet: Test Value Set urn:oid:1.2.3.4) (CONF:2-2)", c2Text);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void SingleChildWithCodeSystemBinding()
        {
            var templateType = this.tdb.FindOrCreateTemplateType(this.fhirIg.ImplementationGuideType, "Extension");
            var template = this.tdb.CreateTemplate("http://test.com", templateType, "Test Extension", this.fhirIg);
            var codeSystem = this.tdb.FindOrCreateCodeSystem("Test Code System", "urn:oid:4.3.2.1");
            var c1 = this.tdb.AddConstraintToTemplate(template, null, null, "value[x]", "SHALL", "1..1", isChoice: true);
            var c2 = this.tdb.AddConstraintToTemplate(template, c1, null, "valueCodeableConcept", codeSystem: codeSystem);

            var c1fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c1);
            var c1Text = c1fc.GetPlainText();

            var c2fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c2);
            var c2Text = c2fc.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] value[x], where value[x] is (CONF:2-1)", c1Text);
            Assert.AreEqual("valueCodeableConcept (CodeSystem: Test Code System urn:oid:4.3.2.1) (CONF:2-2)", c2Text);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void SingleChildWithSingleValueBinding()
        {
            var templateType = this.tdb.FindOrCreateTemplateType(this.fhirIg.ImplementationGuideType, "Extension");
            var template = this.tdb.CreateTemplate("http://test.com", templateType, "Test Extension", this.fhirIg);
            var c1 = this.tdb.AddConstraintToTemplate(template, null, null, "value[x]", "SHALL", "1..1", isChoice: true);
            var c2 = this.tdb.AddConstraintToTemplate(template, c1, null, "valueCodeableConcept", value: "1234-x", displayName: "Test Value");

            var c1fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c1);
            var c1Text = c1fc.GetPlainText();

            var c2fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c2);
            var c2Text = c2fc.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] value[x], where value[x] is (CONF:2-1)", c1Text);
            Assert.AreEqual("valueCodeableConcept=\"1234-x\" Test Value (CONF:2-2)", c2Text);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void SingleChildWithContainedTemplate()
        {
            var templateType = this.tdb.FindOrCreateTemplateType(this.fhirIg.ImplementationGuideType, "Extension");
            var templateType2 = this.tdb.FindOrCreateTemplateType(this.fhirIg.ImplementationGuideType, "Observation");
            var template = this.tdb.CreateTemplate("http://test.com", templateType, "Test Extension", this.fhirIg);
            var template2 = this.tdb.CreateTemplate("http://test2.com", templateType2, "Test Observation", this.fhirIg);
            var c1 = this.tdb.AddConstraintToTemplate(template, null, null, "value[x]", "SHALL", "1..1", isChoice: true);
            var c2 = this.tdb.AddConstraintToTemplate(template, c1, template2, "valueReference");

            var c1fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c1);
            var c1Text = c1fc.GetPlainText();

            var c2fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.fhirIgSettings, this.fhirIgTypePlugin, c2);
            var c2Text = c2fc.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] value[x], where value[x] is (CONF:2-1)", c1Text);
            Assert.AreEqual("valueReference which includes Test Observation (identifier: http://test2.com) (CONF:2-2)", c2Text);
        }
    }
}
