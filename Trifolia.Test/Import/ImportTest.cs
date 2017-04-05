using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

using Trifolia.Shared;
using Trifolia.Shared.ImportExport;
using Trifolia.Shared.ImportExport.Model;
using ExportModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using ExportTemplate = Trifolia.Shared.ImportExport.Model.TrifoliaTemplate;
using ExportConformanceTypes = Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance;
using Trifolia.DB;
using Trifolia.Import.Native;
using Trifolia.Web.Controllers.API;

namespace Trifolia.Test.Import
{
    /// <summary>
    ///This is a test class for TemplateExportFactoryTest and is intended
    ///to contain all TemplateExportFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ImportTest
    {
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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod]
        public void TestImportController()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();
            tdb.InitializeLCG();
            
            string importXml = Helper.GetSampleContents("Trifolia.Test.DocSamples.ccda1-native.xml");
            ExportModel model = ExportModel.Deserialize(importXml);

            Helper.AuthLogin(tdb, MockObjectRepository.DEFAULT_USERNAME, MockObjectRepository.DEFAULT_ORGANIZATION);

            ImportController controller = new ImportController(tdb);
            var importResponse = controller.ImportTrifoliaModel(model);

            Assert.IsTrue(importResponse.Success, "Expected import to succeed");

            importResponse = controller.ImportTrifoliaModel(model);

            Assert.IsTrue(importResponse.Success, "Expected import to succeed for a second time");
        }

        /// <summary>
        ///A test for ImportTemplates
        ///</summary>
        [TestMethod()]
        public void ImportTemplatesTest()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var implementationGuide = tdb.FindOrCreateImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "Test IG");

            string importXml =
                "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                "<Trifolia xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.lantanagroup.com\">" +
                "  <Template identifier=\"urn:oid:1.2.3.4.5\" implementationGuideType=\"CDA\" templateType=\"Document\" title=\"Test Template 1\" bookmark=\"D_Test_Template_1\" context=\"ClinicalDocument\">" +
                "    <ImplementationGuide name=\"Test IG\" />" + 
                "    <Description>Test Description 2</Description>" +
                "    <Notes>Test Note 1</Notes>" +
                "    <Constraint isVerbose=\"false\" number=\"1000\" context=\"templateId\" conformance=\"SHALL\" cardinality=\"1..1\" />" +
                "    <Constraint isVerbose=\"false\" number=\"1001\" context=\"code\" conformance=\"SHALL\" cardinality=\"1..1\">" +
                "      <Constraint isVerbose=\"false\" number=\"1002\" context=\"@code\" conformance=\"SHALL\" cardinality=\"1..1\">" +
                "        <SingleValueCode code=\"12345X\" displayName=\"Test Static Value\" />" +
                "      </Constraint>" +
                "    </Constraint>" +
                "    <Constraint isVerbose=\"false\" number=\"1003\" context=\"code\" conformance=\"SHALL\" cardinality=\"1..1\">" +
                "      <Constraint isVerbose=\"false\" number=\"1004\" context=\"@code\" conformance=\"SHALL\" cardinality=\"1..1\" dataType=\"CE\">" +
                "        <ValueSet identifier=\"urn:oid:9.8.7.6.5.4.3.2.1\" isStatic=\"true\" />" +
                "      </Constraint>" +
                "    </Constraint>" +
                "  </Template>" +
                "  <Template identifier=\"urn:oid:1.2.3.4.5.6\" implementationGuideType=\"CDA\" templateType=\"Document\" title=\"Test Template 2\" bookmark=\"D_Test_Template_2\" impliedTemplateOid=\"urn:oid:1.2.3.4.5\" context=\"Document\">" +
                "    <ImplementationGuide name=\"Test IG\" />" + 
                "    <Description>Test Description 1</Description>" +
                "    <Notes>Test Note 2</Notes>" +
                "    <Constraint isVerbose=\"false\" number=\"1005\" context=\"code\" conformance=\"SHALL\" cardinality=\"1..1\" />" +
                "  </Template>" +
                "  <Template identifier=\"urn:oid:1.2.3.4.5.6.7\" implementationGuideType=\"CDA\" templateType=\"Document\" title=\"Test Template 3\" bookmark=\"D_Test_Template_3\" context=\"Document\">" +
                "    <ImplementationGuide name=\"Test IG\" />" + 
                "    <Description>Test Description 3</Description>" +
                "    <Notes>Test Note 3</Notes>" +
                "    <Constraint isVerbose=\"false\" number=\"1006\" conformance=\"SHALL\" cardinality=\"1..1\" containedTemplateOid=\"urn:oid:1.2.3.4.5.6\" />" +
                "    <Constraint isVerbose=\"false\" number=\"1007\" context=\"entry\" conformance=\"SHALL\" cardinality=\"1..1\">" +
                "      <Constraint isVerbose=\"false\" number=\"1008\" context=\"observation\" conformance=\"SHALL\" cardinality=\"1..1\" containedTemplateOid=\"urn:oid:1.2.3.4.5.6\" />" +
                "    </Constraint>" +
                "  </Template>" +
                "</Trifolia>";
            TemplateImporter importer = new TemplateImporter(tdb);
            List<Template> templates = importer.Import(importXml);

            Assert.AreEqual(0, importer.Errors.Count, "Should not have errors");

            Assert.IsNotNull(templates, "Did not expect import to return null list of templates");
            Assert.AreEqual(3, templates.Count, "Expected to find 3 imported templates");

            Template firstTemplate = templates[0];

            Assert.AreEqual("urn:oid:1.2.3.4.5", firstTemplate.Oid, "First template's oid was not correct.");
            Assert.AreEqual("Test Template 1", firstTemplate.Name, "First template's title was not set correctly.");
            Assert.AreEqual("D_Test_Template_1", firstTemplate.Bookmark, "First template's bookmark was not set correctly.");
            
            Assert.IsNotNull(firstTemplate.ImplementationGuideType, "First template's implementation guide type was not set.");
            Assert.AreEqual(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, firstTemplate.ImplementationGuideType.Name, "First template's implementation guide was not correct.");

            Assert.IsNotNull(firstTemplate.TemplateType, "First template's type was not set.");
            Assert.AreEqual("Document", firstTemplate.TemplateType.Name, "First template's type was not correct.");

            Assert.IsNotNull(firstTemplate.PrimaryContext, "First template's context was not set.");
            Assert.AreEqual("ClinicalDocument", firstTemplate.PrimaryContext, "First template's context was not correct.");

            Assert.AreEqual(5, firstTemplate.ChildConstraints.Count, "Expected to find 5 constraints in the first template.");

            TemplateConstraint constraint = firstTemplate.ChildConstraints.ToList()[0];

            Assert.IsNotNull(constraint.Context, "First constraint of first template does not have a context.");
            Assert.AreEqual(constraint.Context, "templateId", "First constraint of first template does not have the correct context.");

            Assert.AreEqual(constraint.Cardinality, "1..1", "First constraint of first template does not have the correct cardinality.");

            Assert.IsNotNull(constraint.Conformance, "First constraint of first template does does not have a conformance.");
            Assert.AreEqual(constraint.Conformance, "SHALL", "First constraint of first template does does not have the correct conformance.");

            TemplateConstraint secondConstraint = firstTemplate.ChildConstraints.ToList()[2];

            Assert.IsNotNull(secondConstraint.ParentConstraint, "Third constraint of the first template does not have a parent constraint.");
        }
    }
}
