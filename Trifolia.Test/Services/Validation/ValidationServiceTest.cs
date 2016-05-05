using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

using Trifolia.Shared;
using Trifolia.ValidationService;
using Trifolia.DB;

namespace Trifolia.Test.Services.Validation
{
    
    
    /// <summary>
    ///This is a test class for ValidationServiceTest and is intended
    ///to contain all ValidationServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ValidationServiceTest
    {
        private MockObjectRepository tdb;
        private TestContext testContextInstance;
        private long testSchFileId;

        #region Context

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

        #endregion

        #region Setup

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            tdb = new MockObjectRepository();

            // IG Type
            ImplementationGuideType cdaType = tdb.FindOrCreateImplementationGuideType("CDA", "cda.xsd", "cda", "urn:hl7-org:v3");

            // Template Type
            TemplateType docType = tdb.FindOrCreateTemplateType(cdaType, "document", "ClinicalDocument", "ClinicalDocument", 1);
            TemplateType secType = tdb.FindOrCreateTemplateType(cdaType, "section", "section", "Section", 2);
            TemplateType entType = tdb.FindOrCreateTemplateType(cdaType, "entry", "entry", "Entry", 3);

            // Code System
            CodeSystem cs1 = tdb.FindOrCreateCodeSystem("SNOMED CT", "6.36");

            // Value Set
            ValueSet vs1 = tdb.FindOrCreateValueSet("Test Value Set 1", "1.2.3.4");
            tdb.FindOrCreateValueSetMember(vs1, cs1, "1234", "Test Member 1");
            tdb.FindOrCreateValueSetMember(vs1, cs1, "4321", "Test Member 2");

            // Implementation Guide
            ImplementationGuide ig1 = tdb.FindOrAddImplementationGuide(cdaType, "Test Implementation Guide 1", publishDate: new DateTime(2012, 1, 1));

            // IG Files
            byte[] testSchBytes = Helper.GetSampleContentBytes("Trifolia.Test.DocSamples.test.sch");

            ImplementationGuideFile testSchFile = tdb.GenerateImplementationGuideFile(ig1, "test.sch", ImplementationGuideFile.ContentTypeSchematron, "text/xml", content:testSchBytes);
            this.testSchFileId = testSchFile.Id;

            tdb.GenerateImplementationGuideFile(ig1, "voc.xml", ImplementationGuideFile.ContentTypeVocabulary, "text/xml", content: testSchBytes);
            tdb.GenerateImplementationGuideFile(ig1, "test1_template1.ent", ImplementationGuideFile.ContentTypeSchematronHelper, "text/xml", content: testSchBytes);

            ImplementationGuide ig2 = tdb.FindOrAddImplementationGuide(cdaType, "Test Implementation Guide 2");

            // Template 1
            Template t1 = tdb.GenerateTemplate("urn:oid:1.2.3.4", docType, "Test Template", ig1, null, null, null);
            tdb.GenerateConstraint(t1, null, null, "title", "SHALL", "1..1");

            // Template 2
            Template t2 = tdb.GenerateTemplate("urn:oid:1.2.3.4.1", docType, "Test Template", ig1, null, null, null);
            TemplateConstraint t2_tc1 = tdb.GenerateConstraint(t2, null, null, "title", "SHALL", "1..1");
            t2_tc1.Schematron = "count(cda:title)";

            TemplateConstraint t2_tc2 = tdb.GeneratePrimitive(t2, null, "SHALL", "This is a test primitive");

            tdb.GeneratePrimitive(t2, null, "SHALL", "This is test primitive #2", "count(cda:title) &gt; 0");

            // Template 3
            Template t3 = tdb.GenerateTemplate("urn:oid:1.2.3.4.2", docType, "Test Template", ig2, null, null, null);
            tdb.GenerateConstraint(t1, null, null, "title", "SHOULD", "1..1");

            // Template 4
            Template t4 = tdb.GenerateTemplate("urn:oid:1.2.3.4.3", docType, "Test Template", ig2, null, null, null);
            TemplateConstraint t4_p1 = tdb.GenerateConstraint(t4, null, null, "entryRelationship", "SHALL", "1..1", null, null, null, null, null, null, null, true);
            TemplateConstraint t4_p2 = tdb.GenerateConstraint(t4, t4_p1, null, "@typeCode", "SHALL", "1..1", null, null, "DRIV");
            TemplateConstraint t4_p3 = tdb.GenerateConstraint(t4, t4_p1, null, "observation", "SHALL", "1..1", null, null, "DRIV", null, null, null, null, true);
        }

        #endregion

        /// <summary>
        ///A test for GetValidationPackage
        ///</summary>
        [TestMethod()]
        public void GetValidationPackageStoredTest()
        {
            ValidationService.ValidationService target = new ValidationService.ValidationService();
            List<ValidationDocument> actual = target.GetValidationPackage(this.tdb, 1, GenerationOptions.Generate, null);

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Count);
        }

        /// <summary>
        ///A test for GetValidationPackage
        ///</summary>
        [TestMethod()]
        public void GetValidationPackageGeneratedTest()
        {
            ValidationService.ValidationService target = new ValidationService.ValidationService();

            List<ValidationProfile> profiles = target.GetValidationProfiles(this.tdb);
            ValidationProfile firstGeneratedProfile = profiles.FirstOrDefault(y => y.Id < 0);
            ValidationProfile lastGeneratedProfile = profiles.LastOrDefault(y => y.Id < 0);

            List<ValidationDocument> actual = target.GetValidationPackage(this.tdb, 2, GenerationOptions.Generate, null);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Count);

            actual = target.GetValidationPackage(this.tdb, 1, GenerationOptions.Generate, null);

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Count);
        }

        /// <summary>
        ///A test for GetValidationProfiles
        ///</summary>
        [TestMethod()]
        public void GetValidationProfilesTest()
        {
            ValidationService.ValidationService target = new ValidationService.ValidationService();
            List<ValidationProfile> actual = target.GetValidationProfiles(this.tdb);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Count);
        }
    }
}
