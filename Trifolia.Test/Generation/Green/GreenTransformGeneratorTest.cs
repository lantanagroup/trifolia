using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Xml;

using Trifolia.Shared;
using Trifolia.Generation.Green.Transform;

namespace Trifolia.Test.Generation.Green
{
    /// <summary>
    ///This is a test class for GreenTransformGeneratorTest and is intended
    ///to contain all GreenTransformGeneratorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GreenTransformGeneratorTest
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

        /// <summary>
        ///A test for BuildTransform
        ///</summary>
        //[TestMethod()]
        public void BuildGreenTransformTest()
        {
            MockObjectRepository mockRepo = TestDataGenerator.GenerateGreenMockDataset1();
            int templateId = 1;
            Dictionary<int, int> greenTemplates = new Dictionary<int, int>();
            greenTemplates.Add(templateId, 1);
            GreenTransformGenerator target = new GreenTransformGenerator(mockRepo, greenTemplates, templateId);
            target.BuildTransform();
            string transformXml = target.GetTransform();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(transformXml);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");
            nsManager.AddNamespace("ig", "urn:hl7-org:v3");

            XmlElement testGreenTemplate1 = doc.SelectSingleNode("/xsl:stylesheet/xsl:template[@name='TestGreenTemplate1']", nsManager) as XmlElement;
            Assert.IsNotNull(testGreenTemplate1);

            XmlTestHelper.AssertXmlSingleNode(testGreenTemplate1, nsManager, "ig:ClinicalDocument", "Expected to find a root clinical document node");
            XmlTestHelper.AssertXmlSingleNode(testGreenTemplate1, nsManager, "ig:ClinicalDocument/ig:templateId[@root='1.2.3.4']", "Expected to find a templateId");
            XmlTestHelper.AssertXmlSingleNode(testGreenTemplate1, nsManager, "ig:ClinicalDocument/xsl:element[@name='setId'][xsl:call-template[@name='dataType_II' and xsl:with-param[@name='instance' and @select='VersionSet']]]", 
                "Expected to find xsl:element for setId");
        }
    }
}
