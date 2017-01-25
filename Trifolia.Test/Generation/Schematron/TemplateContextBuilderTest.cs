using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Export.Schematron;
using Trifolia.DB;

namespace Trifolia.Test.Generation.Schematron
{
    /// <summary>
    /// Summary description for TemplateContextBuilder
    /// </summary>
    [TestClass]
    public class TemplateContextBuilderTest
    {
        public TemplateContextBuilderTest()
        {
        }

        private TestContext testContextInstance;

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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        /// <summary>
        /// Tests that build the rule xpath (context) for a CDA "Document" template, with a plain OID identifier, produces the
        /// correct rule context xpath (specifically that @root is specified).
        /// </summary>
        [TestMethod]
        public void TestBuildContextStringForDocument()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var docTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrAddImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(igType);

            Template template = tdb.GenerateTemplate("urn:oid:1.2.3.4", docTemplateType, "Test Template", ig);
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='1.2.3.4']]", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template with a versioned identifier (urn:hl7ii:) produces 
        /// a rule context that includes @root and @extension
        /// </summary>
        [TestMethod]
        public void TestBuildContextStringForVersionIdentifier()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var docTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrAddImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(igType);

            Template template = tdb.GenerateTemplate("urn:hl7ii:1.2.3.4:1234", docTemplateType, "Test Template", ig);
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='1.2.3.4' and @extension = '1234']]", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template with an HTTP identifier produces the correct results.
        /// </summary>
        /// <remarks>
        /// Unlikely that HTTP identifier would be used for a CDA template, but possible.
        /// </remarks>
        [TestMethod]
        public void TestBuildContextStringForHTTPIdentifier()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var docTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrAddImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(igType);

            Template template = tdb.GenerateTemplate("http://test.com/doc/test", docTemplateType, "Test Template", ig);
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='http://test.com/doc/test']]", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template representing an addr[AD] produces the correct results.
        /// </summary>
        [TestMethod]
        public void TestBuildContextStringForAddress()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var unspecifiedTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_UNSPECIFIED_TYPE);
            var ig = tdb.FindOrAddImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(igType);

            Template template = tdb.GenerateTemplate("urn:oid:1.2.3.4", unspecifiedTemplateType, "Test Template", ig, "addr", "AD");
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:addr[cda:templateId[@root='1.2.3.4']]", contextString);
        }
    }
}
