using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Trifolia.DB;
using Trifolia.Plugins;

namespace Trifolia.Test.Generation.Sample
{


    /// <summary>
    ///This is a test class for TemplateSampleGeneratorTest and is intended
    ///to contain all TemplateSampleGeneratorTest Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("Trifolia.Plugins.dll")]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class TemplateSampleGeneratorTest
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


        /// <summary>
        ///A test for GenerateSample
        ///</summary>
        [TestMethod()]
        public void GenerateSampleTest()
        {
            MockObjectRepository lRepository = new MockObjectRepository();

            Template lMockTemplate = TestDataGenerator.GenerateTemplate();

            var plugin = lMockTemplate.ImplementationGuideType.GetPlugin();
            string sampleXml = plugin.GenerateSample(lRepository, lMockTemplate);

            Assert.IsNotNull(sampleXml, "Sample XML was not created");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sampleXml);

            Assert.AreEqual("ClinicalDocument", doc.DocumentElement.Name, "Incorrect name for root element");
            Assert.AreEqual(0, doc.DocumentElement.Attributes.Count, "Root element should not have attributes");
            Assert.AreEqual(12, doc.DocumentElement.ChildNodes.Count, "Expected to find 12 children of the document element");
            Assert.AreEqual("templateId", doc.DocumentElement.ChildNodes[0].Name, "First child node is incorrectly named");
            Assert.AreEqual(1, doc.DocumentElement.ChildNodes[0].Attributes.Count, "First child node has incorrect number of attributes");
            Assert.AreEqual("root", doc.DocumentElement.ChildNodes[0].Attributes[0].Name, "First child node's second attribute is incorrect");
            Assert.AreEqual("1.2.3.4", doc.DocumentElement.ChildNodes[0].Attributes[0].Value, "First child node's first attribute has incorrect value");
        }
    }
}
