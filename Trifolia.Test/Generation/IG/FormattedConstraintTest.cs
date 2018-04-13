using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Trifolia.DB;
using Trifolia.Export.MSWord;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Plugins;
using Trifolia.Shared;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

namespace Trifolia.Test.Generation.IG
{
    /// <summary>
    ///This is a test class for FormattedConstraintTest and is intended
    ///to contain all FormattedConstraintTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FormattedConstraintTest
    {
        private TestContext testContextInstance;
        private MockObjectRepository mockRepo;
        private IGSettingsManager igSettings;
        private IIGTypePlugin igTypePlugin;
        private bool linkContainedTemplate;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            this.mockRepo = TestDataGenerator.GenerateMockDataset2();

            ImplementationGuide ig = this.mockRepo.ImplementationGuides.SingleOrDefault(y => y.Name == TestDataGenerator.DS2_IG_NAME);

            this.igSettings = new IGSettingsManager(this.mockRepo, ig.Id);
            this.igTypePlugin = ig.ImplementationGuideType.GetPlugin();
            this.linkContainedTemplate = false;
        }

        /// <summary>
        ///A test for GetPlainText
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ContextConformanceTest()
        {
            // Test 1000
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 1);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] value (CONF:1-1).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceValueCodeSystemTest()
        {
            // Test 1001
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 2);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] @classCode=\"OBS\" Observation (CodeSystem: HL7ActStatus 113883.5.14) (CONF:1-2).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceTemplateTest1()
        {
            // Test 1002
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 3);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] templateId/@root=\"22.4.47\" (CONF:1-3).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceDataTypeTest()
        {
            // Test 1003
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 4);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] code with @xsi:type=\"CD\" (CONF:1-4).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceTemplateTest2()
        {
            // Test 1004
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 5);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("MAY contain zero or one [0..1] Test Template 2 (identifier: 1.2.3.4.5.6.5) (CONF:1-5).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceXpathValueSetTest()
        {
            // Test 1005
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 6);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] administrativeGenderCode/@code, which MAY be selected from ValueSet GenderCode 11.1 (CONF:1-6).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceValueSetVersionTest()
        {
            // Test 1005
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 6);
            constraint.Context = "administrativeGenderCode";
            constraint.ValueSetDate = new DateTime(2012, 5, 1);

            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, constraint.Template.OwningImplementationGuideId);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] administrativeGenderCode, which MAY be selected from ValueSet GenderCode 11.1 2012-05-01 (CONF:1-6).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceValueSetVersionStaticTest()
        {
            // Test 1005
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 6);
            constraint.Context = "administrativeGenderCode";
            constraint.ValueSetDate = new DateTime(2012, 5, 1);
            constraint.IsStatic = true;

            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, constraint.Template.OwningImplementationGuideId);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] administrativeGenderCode, which MAY be selected from ValueSet GenderCode 11.1 STATIC 2012-05-01 (CONF:1-6).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceValueCodeSystemTest1()
        {
            // Test 1006
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 7);
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, constraint.Template.OwningImplementationGuideId);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] statusCode=\"completed\" Completed (CodeSystem: HL7ActStatus 113883.5.14) (CONF:1-7).", constraintText);
        }

        public void GetPlainText_ConformanceValueCodeSystemTest2()
        {
            // Test 1007
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 8);
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, constraint.Template.OwningImplementationGuideId);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] code/@code=\"1234-X\" Test Disp, which SHALL be selected from CodeSystem SNOMED CT (6.96) (CONF:1-8).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_ConformanceValueWithConformanceCodeSystemTest()
        {
            // Test 1008
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 9);
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, constraint.Template.OwningImplementationGuideId);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] code=\"1234-X\" Test Disp with @xsi:type=\"CD\", where the code SHALL be selected from CodeSystem SNOMED CT (6.96) (CONF:1-9).", constraintText);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        public void GetPlainText_CategorizedConstraint()
        {
            TemplateConstraint constraint = mockRepo.TemplateConstraints.Single(y => y.Id == 10);

            // Test WITH categories enabled
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, constraint.Template.OwningImplementationGuideId);
            IFormattedConstraint target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, linkIsBookmark: false, includeCategory: true);
            string constraintText = target.GetPlainText();

            Assert.AreEqual("[TestCategory] SHALL contain exactly one [1..1] code (CONF:1-10).", constraintText);

            // Test WITHOUT categories enabled
            target = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, igTypePlugin, constraint, linkContainedTemplate: linkContainedTemplate, includeCategory: false);
            constraintText = target.GetPlainText();

            Assert.AreEqual("SHALL contain exactly one [1..1] code (CONF:1-10).", constraintText);
        }

        /// <summary>
        /// A test for MakePlural
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void MakePluralTest1()
        {
            string noun = "entry";
            string expected = "entries";
            string actual = noun.MakePlural();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for MakePlural
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void MakePluralTest2()
        {
            string noun = "beach";
            string expected = "beaches";
            string actual = noun.MakePlural();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for MakePlural
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void MakePluralTest3()
        {
            string noun = "wish";
            string expected = "wishes";
            string actual = noun.MakePlural();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for MakePlural
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void MakePluralTest4()
        {
            string noun = "templateId";
            string expected = "templateIds";
            string actual = noun.MakePlural();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for MakePlural
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void MakePluralTest5()
        {
            string noun = "potato";
            string expected = "potatoes";
            string actual = noun.MakePlural();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for MakePlural
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void MakePluralTest6()
        {
            string noun = "auto";
            string expected = "autos";
            string actual = noun.MakePlural();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for MakePlural
        ///</summary>
        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void MakePluralTest7()
        {
            string noun = "prefix";
            string expected = "prefixes";
            string actual = noun.MakePlural();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Narrative Generation")]
        [DeploymentItem("Trifolia.Generation.dll")]
        public void PrimitiveTextFormattingTest1()
        {
            var repo = new MockObjectRepository();
            
            TemplateConstraint newConstraint = new TemplateConstraint()
            {
                Number = 1234,
                IsPrimitive = true,
                PrimitiveText = "This entry SHALL contain X which SHALL NOT contain Y"
            };

            IGSettingsManager igSettings = new IGSettingsManager(repo);
            HyperlinkTracker hyperlinkTracker = new HyperlinkTracker();
            var formattedConstraint = FormattedConstraintFactory.NewFormattedConstraint(repo, igSettings, null, newConstraint);

            using (MemoryStream ms = new MemoryStream())
            {
                var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document);
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();

                formattedConstraint.AddToDocParagraph(mainPart, hyperlinkTracker, mainPart.Document, 1, 1, "");
            }
        }
    }
}
