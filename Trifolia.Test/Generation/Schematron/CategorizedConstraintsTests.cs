using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

using Trifolia.Export.Schematron;
using Trifolia.DB;

namespace Trifolia.Test.Generation.Schematron
{
    /// <summary>
    /// Summary description for CategorizedConstraintsTests
    /// </summary>
    [TestClass]
    [DeploymentItem("Trifolia.Plugins.dll")]
    public class CategorizedConstraintsTests
    {
        private TestContext testContextInstance;
        private MockObjectRepository tdb = new MockObjectRepository();
        private ImplementationGuide ig;

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

        [TestInitialize]
        public void Setup()
        {
            this.tdb.InitializeCDARepository();

            var cdaIgType = this.tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var docType = this.tdb.FindTemplateType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "Document");
            this.ig = this.tdb.FindOrCreateImplementationGuide(cdaIgType, "Test IG");

            var template = this.tdb.CreateTemplate("urn:oid:1.2.3.4", docType, "Test Template", ig, "ClinicalDocument", "ClinicalDocument");
            this.tdb.AddConstraintToTemplate(template, null, null, "code", "SHALL", "1..1");
            this.tdb.AddConstraintToTemplate(template, null, null, "value", "SHOULD", "0..1", category: "CAT1");
            this.tdb.AddConstraintToTemplate(template, null, null, "value", "SHALL", "1..1", category: "CAT2");

            var b1 = this.tdb.AddConstraintToTemplate(template, null, null, "entryRelationship", "SHALL", "1..1", isBranch: true);
            this.tdb.AddConstraintToTemplate(template, b1, null, "@typeCode", "SHALL", "1..1", value: "REFR", isBranchIdentifier: true);
            this.tdb.AddConstraintToTemplate(template, b1, null, "observation", "SHALL", "1..1");

            var b2 = this.tdb.AddConstraintToTemplate(template, null, null, "entryRelationship", "SHALL", "1..1", isBranch: true, category: "CAT1");
            this.tdb.AddConstraintToTemplate(template, b2, null, "@typeCode", "SHALL", "1..1", value: "SUBJ", isBranchIdentifier: true);
            this.tdb.AddConstraintToTemplate(template, b2, null, "observation", "SHALL", "1..1");

            var b3 = this.tdb.AddConstraintToTemplate(template, null, null, "entryRelationship", "SHALL", "1..1", isBranch: true, category: "CAT2");
            this.tdb.AddConstraintToTemplate(template, b3, null, "@typeCode", "SHALL", "1..1", value: "XXXX", isBranchIdentifier: true);
            this.tdb.AddConstraintToTemplate(template, b3, null, "observation", "SHALL", "1..1");

            // Category 1&2 constraint
            this.tdb.AddConstraintToTemplate(template, null, null, "effectiveTime", "SHALL", "1..1", category: "CAT1,CAT2");
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

        [TestMethod, TestCategory("Schematron")]
        public void ExportWithoutCategoriesSelected()
        {
            SchematronGenerator generator = new SchematronGenerator(this.tdb, this.ig, this.tdb.Templates.ToList(), true);
            XmlNamespaceManager nsManager = null;
            XmlDocument doc = GenerateDocument(generator, out nsManager);
            XmlNode errorPattern = doc.DocumentElement.SelectSingleNode("//sch:pattern[@id='p-urn-oid-1.2.3.4-errors']", nsManager);
            XmlNode warningPattern = doc.DocumentElement.SelectSingleNode("//sch:pattern[@id='p-urn-oid-1.2.3.4-warnings']", nsManager);

            Assert.IsNotNull(errorPattern);
            Assert.IsNotNull(warningPattern);

            // Un-categorized
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-1']", "Did not find assertion for un-categorized constraint");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-1'][text()='SHALL contain exactly one [1..1] code (CONF:1-1).']", "Unexpected narrative for uncategorized constraint");

            // Category 2
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-3']", "Did not find assertion for category 2 constraint");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-3'][text()='[CAT2] SHALL contain exactly one [1..1] value (CONF:1-3).']", "Unexpected narrative for category 2 constraint");

            // Category 1
            AssertXML.XPathExists(warningPattern, nsManager, "sch:rule/sch:assert[@id='a-1-2']", "Did not find assertion for category 2 constraint");
            AssertXML.XPathExists(warningPattern, nsManager, "sch:rule/sch:assert[@id='a-1-2'][text()='[CAT1] SHOULD contain zero or one [0..1] value (CONF:1-2).']", "Unexpected narrative for category 1 constraint");

            // Category 1 & 2
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-13']", "Did not find assertion for category 1&2 constraint");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-13'][text()='[CAT1,CAT2] SHALL contain exactly one [1..1] effectiveTime (CONF:1-13).']", "Unexpected narrative in assertion for category 1&2 constraint");
            
            // First branch (REFR)
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-4']", "Did not find assertion for branch 1 root in main template pattern");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-4-branch-4-errors']/sch:assert[@id='a-1-6-branch-4']", "Did not find child constraint's assertion for branch 1");

            // Second branch (SUBJ)
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-7']", "Did not find assertion for branch 2 root in main template pattern");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-7-branch-7-errors']/sch:assert[@id='a-1-9-branch-7']", "Did not find child constraint's assertion for branch 2");

            // Third branch (XXXX)
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-10']", "Did not find assertion for branch 3 root in main template pattern");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-10-branch-10-errors']/sch:assert[@id='a-1-12-branch-10']", "Did not find child constraint's assertion for branch 3");
        }

        [TestMethod, TestCategory("Schematron")]
        public void ExportCategory1()
        {
            List<string> categories = new List<string>();
            categories.Add("CAT1");

            SchematronGenerator generator = new SchematronGenerator(this.tdb, this.ig, this.tdb.Templates.ToList(), true, categories: categories);
            XmlNamespaceManager nsManager = null;
            XmlDocument doc = GenerateDocument(generator, out nsManager);
            XmlNode errorPattern = doc.DocumentElement.SelectSingleNode("//sch:pattern[@id='p-urn-oid-1.2.3.4-errors']", nsManager);
            XmlNode warningPattern = doc.DocumentElement.SelectSingleNode("//sch:pattern[@id='p-urn-oid-1.2.3.4-warnings']", nsManager);

            Assert.IsNotNull(errorPattern);
            Assert.IsNotNull(warningPattern);

            // Un-categorized
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-1']", "Did not find assertion for un-categorized constraint");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-1'][text()='SHALL contain exactly one [1..1] code (CONF:1-1).']", "Unexpected narrative for uncategorized constraint");

            // Category 2
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-3']", "Did not find assertion for category 2 constraint");
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-3'][text()='[CAT2] SHALL contain exactly one [1..1] value (CONF:1-3).']", "Unexpected narrative for category 2 constraint");

            // Category 1
            AssertXML.XPathExists(warningPattern, nsManager, "sch:rule/sch:assert[@id='a-1-2']", "Did not find assertion for category 2 constraint");
            AssertXML.XPathExists(warningPattern, nsManager, "sch:rule/sch:assert[@id='a-1-2'][text()='[CAT1] SHOULD contain zero or one [0..1] value (CONF:1-2).']", "Unexpected narrative for category 1 constraint");

            // Category 1 & 2
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-13']", "Did not find assertion for category 1&2 constraint");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-13'][text()='[CAT1,CAT2] SHALL contain exactly one [1..1] effectiveTime (CONF:1-13).']", "Unexpected narrative in assertion for category 1&2 constraint");

            // First branch (REFR)
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-4']", "Did not find assertion for branch 1 root in main template pattern");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-4-branch-4-errors']/sch:assert[@id='a-1-6-branch-4']", "Did not find child constraint's assertion for branch 1");

            // Second branch (SUBJ)
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-7']", "Did not find assertion for branch 2 root in main template pattern");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-7-branch-7-errors']/sch:assert[@id='a-1-9-branch-7']", "Did not find child constraint's assertion for branch 2");

            // Third branch (XXXX)
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-10']", "Did not find assertion for branch 3 root in main template pattern");
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-10-branch-10-errors']/sch:assert[@id='a-1-12-branch-10']", "Did not find child constraint's assertion for branch 3");
        }

        [TestMethod, TestCategory("Schematron")]
        public void ExportCategory3()
        {
            List<string> categories = new List<string>();
            categories.Add("CAT3");

            SchematronGenerator generator = new SchematronGenerator(this.tdb, this.ig, this.tdb.Templates.ToList(), true, categories: categories);
            XmlNamespaceManager nsManager = null;
            XmlDocument doc = GenerateDocument(generator, out nsManager);
            XmlNode errorPattern = doc.DocumentElement.SelectSingleNode("//sch:pattern[@id='p-urn-oid-1.2.3.4-errors']", nsManager);
            XmlNode warningPattern = doc.DocumentElement.SelectSingleNode("//sch:pattern[@id='p-urn-oid-1.2.3.4-warnings']", nsManager);

            Assert.IsNotNull(errorPattern);
            Assert.IsNotNull(warningPattern);

            // Un-categorized
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-1']", "Did not find assertion for un-categorized constraint");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-1'][text()='SHALL contain exactly one [1..1] code (CONF:1-1).']", "Unexpected narrative for uncategorized constraint");

            // Category 2
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-3']", "Did not find assertion for category 2 constraint");

            // Category 1
            AssertXML.XPathNotExists(warningPattern, nsManager, "sch:rule/sch:assert[@id='a-1-2']", "Did not find assertion for category 2 constraint");

            // Category 1 & 2
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule/sch:assert[@id='a-1-13']", "Did not find assertion for category 1&2 constraint");

            // First branch (REFR)
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-4']", "Did not find assertion for branch 1 root in main template pattern");
            AssertXML.XPathExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-4-branch-4-errors']/sch:assert[@id='a-1-6-branch-4']", "Did not find child constraint's assertion for branch 1");

            // Second branch (SUBJ)
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-7']", "Did not find assertion for branch 2 root in main template pattern");

            // Third branch (XXXX)
            AssertXML.XPathNotExists(errorPattern, nsManager, "sch:rule[@id='r-urn-oid-1.2.3.4-errors']/sch:assert[@id='a-1-10']", "Did not find assertion for branch 3 root in main template pattern");
        }

        private static XmlDocument GenerateDocument(SchematronGenerator generator, out XmlNamespaceManager nsManager)
        {
            string schContent = generator.Generate();         
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(schContent);

            nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("sch", "http://purl.oclc.org/dsdl/schematron");

            return doc;
        }
    }
}
