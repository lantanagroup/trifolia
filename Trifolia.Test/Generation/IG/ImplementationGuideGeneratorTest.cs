using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Trifolia.DB;
using Trifolia.Export.MSWord;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Test.Generation.IG
{
    /// <summary>
    ///This is a test class for ImplementationGuideGeneratorTest and is intended
    ///to contain all ImplementationGuideGeneratorTest Unit Tests
    ///</summary>
    [DeploymentItem("Schemas\\", "Schemas\\")]
    [TestClass()]
    public class ImplementationGuideGeneratorTest
    {
        private TestContext testContextInstance;
        private MockObjectRepository mockRepo;
        private byte[] docBytes;
        private string docContents;
        private string docComments;
        private XmlNamespaceManager docNsMgr;
        private XmlNamespaceManager commentsNsMgr;
        private XmlDocument doc;
        private XmlDocument comments;

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
        public void Setup()
        {
            this.mockRepo = TestDataGenerator.GenerateMockDataset4();
            IEnumerable<int> templateIds = (from t in this.mockRepo.Templates
                                             select t.Id);

            ImplementationGuide ig = this.mockRepo.ImplementationGuides.Single(y => y.Name == TestDataGenerator.DS1_IG_NAME);
            IIGTypePlugin igTypePlugin = ig.ImplementationGuideType.GetPlugin();
            ImplementationGuideGenerator generator = new ImplementationGuideGenerator(this.mockRepo, 1, templateIds);
            ExportSettings lSettings = new ExportSettings();
            lSettings.Use(s =>
            {
                s.GenerateTemplateConstraintTable = true;
                s.GenerateTemplateContextTable = true;
                s.GenerateDocTemplateListTable = true;
                s.GenerateDocContainmentTable = true;
                s.AlphaHierarchicalOrder = false;
                s.DefaultValueSetMaxMembers = 10;
                s.GenerateValueSetAppendix = true;
                s.IncludeXmlSamples = true;
                s.IncludeChangeList = false;
                s.IncludeTemplateStatus = true;
                s.IncludeNotes = true;
            });

            generator.BuildImplementationGuide(lSettings, igTypePlugin);
            this.docBytes = generator.GetDocument();

            this.docContents = GetWordDocumentContents(docBytes);
            this.docComments = GetWordCommentContents(docBytes);
            this.doc = ReadWordDocXml(this.docContents, out this.docNsMgr);
            this.comments = ReadWordDocXml(this.docComments, out this.commentsNsMgr);

            GenerateIGFileTest();
        }

        #region Table Tests

        /// <summary>
        /// Tests that the document's context table is correct, that the header row is properly formatted, and that the first row matches the template's context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestContextTable()
        {
            // Test the template context table's caption
            XmlElement templateContextTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[text() = 'Test Template 2 Contexts']]", docNsMgr) as XmlElement;
            Assert.IsNotNull(templateContextTitle);
            Assert.IsNotNull(templateContextTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", docNsMgr), "Expected to find a style on the template list table's caption");

            // Test the template context table's existance
            XmlElement templateContextTable = templateContextTitle.NextSibling as XmlElement;
            XmlElement templateTitleHeader = templateContextTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Contained By:']", docNsMgr) as XmlElement;
            XmlElement templateTypeHeader = templateContextTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Contains:']", docNsMgr) as XmlElement;
            Assert.AreEqual("tbl", actual: templateContextTable.LocalName, message: "The node following the caption of the template context table!");

            // Test the template context table's header
            Assert.IsNotNull(templateContextTable.SelectSingleNode("w:tr[1]/w:trPr/w:tblHeader", docNsMgr), "The table does not contain the header property");
            Assert.IsNotNull(templateTitleHeader, "Expected to find a 'Contained By:' column.");
            Assert.IsNotNull(templateTypeHeader, "Expected to find a 'Contains:' column.");

            // Test the template context table's contents
            XmlNode contextContent = doc.SelectSingleNode("/w:document/w:body/w:tbl[3]", docNsMgr);
            Assert.IsNotNull(contextContent.SelectSingleNode("w:tr[2]/w:tc[1][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:hyperlink/w:r/w:t='Test Template 3']", docNsMgr), "Missing Contained Template Link");
            Assert.IsNotNull(contextContent.SelectSingleNode("w:tr[2]/w:tc[1][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t=' (required)']", docNsMgr), "Missing Contained Table Link optionality");
        }

        /// <summary>
        /// Tests that constraint overview tables are generated in the document, that the header row is properly formatted, and that the first row matches the template's context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestConstraintOverviewTable()
        {
            // Test the constraint table's caption
            XmlElement constraintTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[text() = 'Test Constraint Description Template Constraints Overview']]", docNsMgr) as XmlElement;
            Assert.IsNotNull(constraintTitle);
            Assert.IsNotNull(constraintTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", docNsMgr), "Expected to find a style on the constraint table's caption");

            // Test the constraint table's existance
            XmlElement constraintTable = constraintTitle.NextSibling as XmlElement;
            XmlElement xPathHeader = constraintTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'XPath']", docNsMgr) as XmlElement;
            XmlElement cardHeader = constraintTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Card.']", docNsMgr) as XmlElement;
            XmlElement verbHeader = constraintTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Verb']", docNsMgr) as XmlElement;
            XmlElement dataTypeHeader = constraintTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Data Type']", docNsMgr) as XmlElement;
            XmlElement confHeader = constraintTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'CONF#']", docNsMgr) as XmlElement;
            XmlElement valueHeader = constraintTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Value']", docNsMgr) as XmlElement;
            Assert.AreEqual("tbl", actual: constraintTable.LocalName, message: "The node following the caption of the template list table!");

            // Test the constraint table's header
            Assert.IsNotNull(constraintTable.SelectSingleNode("/w:document/w:body/w:tbl[1]/w:tblGrid", docNsMgr), "The table does not contain the grid property");
            Assert.IsNotNull(xPathHeader, "Expected to find a 'xPath' column.");
            Assert.IsNotNull(cardHeader, "Expected to find a 'Cardinality' column.");
            Assert.IsNotNull(verbHeader, "Expected to find a 'Verb' column.");
            Assert.IsNotNull(dataTypeHeader, "Expected to find a 'Data Type' column.");
            Assert.IsNotNull(confHeader, "Expected to find a 'Conformance Number' column.");
            Assert.IsNotNull(valueHeader, "Expected to find a 'Value' column.");

            // Test the constraint table's contents
            XmlNode constraintContent = doc.SelectSingleNode("/w:document/w:body/w:tbl[2]", docNsMgr);
            Assert.IsNotNull(constraintContent.SelectSingleNode("w:tr[2]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='ClinicalDocument (identifier: 1.2.3.4.5)']", docNsMgr), "Missing Template ID");
            Assert.IsNotNull(constraintContent.SelectSingleNode("w:tr[3]/w:tc[1][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='	code']", docNsMgr), "Missing Xpath");
            Assert.IsNotNull(constraintContent.SelectSingleNode("w:tr[3]/w:tc[2][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='1..1']", docNsMgr), "Missing Cardinality");
            Assert.IsNotNull(constraintContent.SelectSingleNode("w:tr[3]/w:tc[3][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='SHALL']", docNsMgr), "Missing Verb");
            Assert.IsNotNull(constraintContent.SelectSingleNode("w:tr[3]/w:tc[4][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='CD']", docNsMgr), "Missing Data Type");
            Assert.IsNotNull(constraintContent.SelectSingleNode("w:tr[3]/w:tc[5][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:hyperlink/w:r/w:t='1-1']", docNsMgr), "Missing Conformance Number");
            Assert.IsNotNull(constraintContent.SelectSingleNode("w:tr[3]/w:tc[6][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='urn:oid:2.16.840.1.113883.6.1 (LOINC) = 51897-7']", docNsMgr), "Missing Value");
        }

        /// <summary>
        /// Tests the document template list table is generated and that the header is present and has the title and type columns.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestDocTemplateListTable()
        {
            // Test the template list table's caption
            XmlElement templateListTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Template List')]]", docNsMgr) as XmlElement;
            Assert.IsNotNull(templateListTitle);
            Assert.IsNotNull(templateListTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", docNsMgr), "Expected to find a style on the template list table's caption");

            // Test the template list table's existance
            XmlElement templateListTable = templateListTitle.NextSibling as XmlElement;
            XmlElement templateTitleHeader = templateListTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Template Title']", docNsMgr) as XmlElement;
            XmlElement templateTypeHeader = templateListTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Template Type']", docNsMgr) as XmlElement;
            XmlElement templateIdHeader = templateListTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'templateId']", docNsMgr) as XmlElement;
            Assert.AreEqual("tbl", actual: templateListTable.LocalName, message: "The node following the caption of the template list table is not a table!");

            // Test the template list table's header
            Assert.IsNotNull(templateListTable.SelectSingleNode("w:tr[1]/w:trPr/w:tblHeader", docNsMgr), "The table does not contain the header property");
            Assert.IsNotNull(templateTitleHeader, "Expected to find a 'Template Title' column.");
            Assert.IsNotNull(templateTypeHeader, "Expected to find a 'Template Type' column.");
            Assert.IsNotNull(templateIdHeader, "Expected to find a 'templateId' column.");

            // Test the template list table's contents
            XmlNode templateListContent = doc.SelectSingleNode("/w:document/w:body/w:tbl[9]", docNsMgr);
            Assert.IsNotNull(templateListContent.SelectSingleNode("w:tr[2]/w:tc[1][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:hyperlink/w:r/w:t='Test Constraint Description Template']", docNsMgr), "Missing Template Title");
            Assert.IsNotNull(templateListContent.SelectSingleNode("w:tr[2]/w:tc[2][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='Document']", docNsMgr), "Missing Template Type");
            Assert.IsNotNull(templateListContent.SelectSingleNode("w:tr[2]/w:tc[3][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='8.2234.19.234.11']", docNsMgr), "Missing Template ID");
        }

        /// <summary>
        /// Tests that the document's containment table is correct, that the header row is properly formatted, and that the first row matches the template's context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestDocContainmentTable()
        {
            // Test the template containment table's caption
            XmlElement templateContainmentTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[text() = 'Template Containments']]", docNsMgr) as XmlElement;
            Assert.IsNotNull(templateContainmentTitle);
            Assert.IsNotNull(templateContainmentTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", docNsMgr), "Expected to find a style on the template list table's caption");

            // Test the template containment table's existance
            XmlElement templateContainmentTable = templateContainmentTitle.NextSibling as XmlElement;
            XmlElement templateTitleHeader = templateContainmentTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Template Title']", docNsMgr) as XmlElement;
            XmlElement templateTypeHeader = templateContainmentTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Template Type']", docNsMgr) as XmlElement;
            XmlElement templateIdHeader = templateContainmentTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'templateId']", docNsMgr) as XmlElement;
            Assert.AreEqual("tbl", actual: templateContainmentTable.LocalName, message: "The node following the caption of the template containment table!");

            // Test the template containment table's header
            Assert.IsNotNull(templateContainmentTable.SelectSingleNode("w:tr[1]/w:trPr/w:tblHeader", docNsMgr), "The table does not contain the header property");
            Assert.IsNotNull(templateTitleHeader, "Expected to find a 'Template Title' column.");
            Assert.IsNotNull(templateTypeHeader, "Expected to find a 'Template Type' column.");
            Assert.IsNotNull(templateIdHeader, "Expected to find a 'templateId' column.");

            // Test the template containment table's contents
            XmlNode containmentContent = doc.SelectSingleNode("/w:document/w:body/w:tbl[10]", docNsMgr);
            Assert.IsNotNull(containmentContent.SelectSingleNode("w:tr[2]/w:tc[1][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:hyperlink/w:r/w:t='Test Template 1']", docNsMgr), "Missing Template Title");
            Assert.IsNotNull(containmentContent.SelectSingleNode("w:tr[2]/w:tc[2][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='Document']", docNsMgr), "Missing Template Type");
            Assert.IsNotNull(containmentContent.SelectSingleNode("w:tr[2]/w:tc[3][w:p/w:pPr/w:pStyle/@w:val='TableText'][w:p/w:r/w:t='1.2.3.4.5']", docNsMgr), "Missing Template ID");
        }

        /// <summary>
        /// Tests that "Value Sets in this Guide" table is correct, that the header row is properly formatted, and that the first row matches the template's context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestValueSetsTable()
        {
            // Test the Value Sets table's caption
            XmlElement tableValueSetTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[text() = 'Treatment status']]", docNsMgr) as XmlElement;
            Assert.IsNotNull(tableValueSetTitle);
            Assert.IsNotNull(tableValueSetTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", docNsMgr), "Expected to find a style on the value sets table's heading");

            // Test the Value Sets  table's existance
            XmlElement tableValueSet = tableValueSetTitle.NextSibling as XmlElement;
            XmlElement valueSetCode = tableValueSet.SelectSingleNode("w:tr[2]/w:tc[1][w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Code']", docNsMgr) as XmlElement;
            XmlElement vscodeSystemName = tableValueSet.SelectSingleNode("w:tr[2]/w:tc[2][w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Code System']", docNsMgr) as XmlElement;
            XmlElement vscodeSystemOID = tableValueSet.SelectSingleNode("w:tr[2]/w:tc[3][w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Code System OID']", docNsMgr) as XmlElement;
            XmlElement valueSetValueName = tableValueSet.SelectSingleNode("w:tr[2]/w:tc[4][w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Print Name']", docNsMgr) as XmlElement;
            Assert.AreEqual("tbl", actual: tableValueSet.LocalName, message: "The node following the Heading of the value set table is not a table");

            // Test the Value Sets table's header
            Assert.IsNotNull(tableValueSet.SelectSingleNode("w:tr[2]/w:trPr/w:tblHeader", docNsMgr), "The value set table does not contain the header property");
            Assert.IsNotNull(valueSetCode, "Expected to find a 'Value Set Code' column.");
            Assert.IsNotNull(vscodeSystemName, "Expected to find a 'Value Set Code System Name' column.");
            Assert.IsNotNull(vscodeSystemOID, "Expected to find a 'Value Set Code System OID' column.");
            Assert.IsNotNull(valueSetValueName, "Expected to find a 'Value Set Code System OID' column.");

            // Test the Value Sets table's contents
            XmlNode valueSetContent = doc.SelectSingleNode("/w:document/w:body/w:tbl[11]", docNsMgr);
            Assert.IsNotNull(valueSetContent.SelectSingleNode("w:tr[3]/w:tc[1][w:p/w:r/w:t='5561003']", docNsMgr), "Missing Code System Name");
            Assert.IsNotNull(valueSetContent.SelectSingleNode("w:tr[3]/w:tc[2][w:p/w:r/w:t='SNOMED CT']", docNsMgr), "Missing Code System OID");
            Assert.IsNotNull(valueSetContent.SelectSingleNode("w:tr[3]/w:tc[3][w:p/w:r/w:t='urn:oid:2.16.840.1.113883.6.96']", docNsMgr), "Missing Code System OID");
            Assert.IsNotNull(valueSetContent.SelectSingleNode("w:tr[3]/w:tc[4][w:p/w:r/w:t='Active']", docNsMgr), "Missing Code System OID");
        }
        /// <summary>
        /// Tests that "Code Systems in this Guide" table is correct, that the header row is properly formatted, and that the first row matches the template's context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestCodeSystemsTable()
        {
            // Test the code system table's caption
            XmlElement tableCodeSystemTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[text() = 'Code Systems']]", docNsMgr) as XmlElement;
            Assert.IsNotNull(tableCodeSystemTitle);
            Assert.IsNotNull(tableCodeSystemTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", docNsMgr), "Expected to find a style on the code system table's caption");

            // Test the code system table's existance
            XmlElement tableCodeSystem = tableCodeSystemTitle.NextSibling as XmlElement;
            XmlElement codeSystemName = tableCodeSystem.SelectSingleNode("w:tr[1]/w:tc[1][w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Name']", docNsMgr) as XmlElement;
            XmlElement codeSystemOID = tableCodeSystem.SelectSingleNode("w:tr[1]/w:tc[2][w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'OID']", docNsMgr) as XmlElement;
            Assert.AreEqual("tbl", actual: tableCodeSystem.LocalName, message: "The node following the Heading of the code systems table is not a table");

            // Test the code system table's header
            Assert.IsNotNull(tableCodeSystem.SelectSingleNode("w:tr[1]/w:trPr/w:tblHeader", docNsMgr), "The code systems table does not contain the header property");
            Assert.IsNotNull(codeSystemName, "Expected to find a 'Code System Name' column.");
            Assert.IsNotNull(codeSystemOID, "Expected to find a 'Code System OID' column.");

            // Test the code system table's contents
            XmlNode codeSystemContent = doc.SelectSingleNode("/w:document/w:body/w:tbl[12]", docNsMgr);
            Assert.IsNotNull(codeSystemContent.SelectSingleNode("w:tr[2]/w:tc[1][w:p/w:r/w:t='HL7ParticipationType']", docNsMgr), "Missing Code System Name");
            Assert.IsNotNull(codeSystemContent.SelectSingleNode("w:tr[2]/w:tc[2][w:p/w:r/w:t='urn:oid:2.16.840.1.113883.5.90']", docNsMgr), "Missing Code System OID");
        }

        #endregion

        #region Template Generation Tests

        /// <summary>
        /// Tests that the Template Status is generated correctly.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestTemplateStatus()
        {
            XmlNode templateStatus = doc.SelectSingleNode("/w:document/w:body/w:p[13]", docNsMgr) as XmlElement;
            Assert.IsNotNull(templateStatus, "Could not find template status for the first template in the document ");

            Assert.IsNotNull(templateStatus.SelectSingleNode("w:pPr/w:pStyle[@w:val='BracketData']", docNsMgr), "Expected to find a style for the Template Status");
            Assert.IsNotNull(templateStatus.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='Draft as part of Test Implementation Guide']", docNsMgr), "Did not find Template Status Text");
        }


        #endregion 

        #region Constraint Generation Tests


        /// <summary>
        /// Test a contained template WITHOUT a context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestFormattedConstraint_ContainedTemplate1()
        {
            TemplateConstraint ctConstraint = this.mockRepo.TemplateConstraints.First(y => y.References.Any(x => x.ReferenceType == ConstraintReferenceTypes.Template));
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, ctConstraint.Template.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = ctConstraint.Template.OwningImplementationGuide.ImplementationGuideType.GetPlugin();

            IFormattedConstraint fc = new FormattedConstraint(mockRepo, igSettings, igTypePlugin, ctConstraint);
            string constraintText = fc.GetPlainText();

            Assert.IsNotNull(constraintText);
            Assert.AreEqual("SHALL contain exactly one [1..1] Test Template 2 (identifier: 1.2.3.4.5.6) (CONF:1-11).", constraintText);
        }

        /// <summary>
        /// Test a contained template WITH a context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestFormattedConstraint_ContainedTemplate2()
        {
            TemplateConstraint ctConstraint = this.mockRepo.TemplateConstraints.Last(y => y.References.Any(x => x.ReferenceType == ConstraintReferenceTypes.Template));
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, ctConstraint.Template.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = ctConstraint.Template.OwningImplementationGuide.ImplementationGuideType.GetPlugin();

            IFormattedConstraint fc = new FormattedConstraint(mockRepo, igSettings, igTypePlugin, ctConstraint);
            string constraintText = fc.GetPlainText();

            Assert.IsNotNull(constraintText);
            Assert.AreEqual("This entry SHALL contain exactly one [1..1] Test Template 2 (identifier: 1.2.3.4.5.6) (CONF:1-13).", constraintText);
        }

        /// <summary>
        /// Tests that the contained templates are properly linked and formatted in constraints.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestContainedTemplate()
        {
            XmlNode containedPara = doc.SelectSingleNode("/w:document/w:body/w:p[54][w:pPr/w:numPr[w:ilvl/@w:val=0][w:numId/@w:val=503]]", docNsMgr);
            Assert.IsNotNull(containedPara, "Could not find 'Contained' paragraph.");

            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[1][w:rPr/w:rStyle/@w:val='keyword'][w:t='SHALL']", docNsMgr), "Missing conformance");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[2][not(w:rPr/w:rStyle)][w:t=' contain ']", docNsMgr), "Missing ' contain '");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[3][not(w:rPr/w:rStyle)][w:t='exactly one [1..1] ']", docNsMgr), "Missing cardinality");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:hyperlink[@w:anchor='D_Test_Template_2'][w:r[w:rPr/w:rStyle/@w:val='HyperlinkCourierBold'][w:t='Test Template 2']]", docNsMgr), "Missing hyperlink to contained template");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[5][w:rPr/w:rStyle/@w:val='XMLname'][w:t=' (identifier: 1.2.3.4.5.6)']", docNsMgr), "Missing contained template's oid");
            var bookmarkStart = containedPara.SelectSingleNode("w:bookmarkStart[@w:name='C_1-11'][following-sibling::w:r]", docNsMgr);
            var run = containedPara.SelectSingleNode("w:r[not(w:rPr/w:rStyle)][w:t[text()=' (CONF:1-11)']]", docNsMgr);
            var bookmarkEnd = containedPara.SelectSingleNode("w:bookmarkEnd[preceding-sibling::w:r]", docNsMgr);
            Assert.IsNotNull(bookmarkStart);
            Assert.IsNotNull(run);
            Assert.IsNotNull(bookmarkEnd);
            //Assert.IsNotNull(containedPara.SelectSingleNode("w:r[5][w:bookmarkStart/@w:name='C_1-11'][not(w:rPr/w:rStyle)][w:t=' (CONF:1-11)']", docNsMgr), "Missing CONF #");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[7][not(w:rPr/w:rStyle)][w:t='.']", docNsMgr), "Missing ending period");
        }

        /// <summary>
        /// Tests that an implied template outputs the "Conforms to" entry in the constraints list of the template.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestImpliedTemplate()
        {
            XmlNode conformsPara = doc.SelectSingleNode("/w:document/w:body/w:p[44][w:pPr/w:numPr[w:ilvl/@w:val=0][w:numId/@w:val=502]]", docNsMgr);
            Assert.IsNotNull(conformsPara, "Could not find the 'Conforms to' paragraph");

            Assert.IsNotNull(conformsPara.SelectSingleNode("w:r[1][not(w:rPr/w:pStyle)][w:t='Conforms to ']", docNsMgr), "Expect to find 'Conforms to' as first run");
            Assert.IsNotNull(conformsPara.SelectSingleNode("w:hyperlink[@w:anchor='D_Test_Template_1'][w:r[w:rPr/w:rStyle/@w:val='HyperlinkCourierBold']/w:t='Test Template 1']", docNsMgr), "Hyperlink is incorrect.");
            Assert.IsNotNull(conformsPara.SelectSingleNode("w:r[2][not(w:rPr/w:rStyle)][w:t=' template ']", docNsMgr), "Missing ' template ' run.");
            Assert.IsNotNull(conformsPara.SelectSingleNode("w:r[3][w:rPr/w:rStyle/@w:val='XMLname'][w:t='(identifier: 1.2.3.4.5)']", docNsMgr), "Missing template oid run.");
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestConstraintDescription()
        {
            XmlNode constraintPara = doc.SelectSingleNode("/w:document/w:body/w:p[16]", docNsMgr);

            Assert.IsNotNull(constraintPara, "Could not find constraint in document");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:pPr/w:pStyle[@w:val='BodyText']", docNsMgr), "Constraint's paragraph is not styled as 'BodyText'");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:pPr/w:spacing[@w:before='120']", docNsMgr), "Constraint's paragraph is not spaced correctly");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r/w:t[text() = 'Test constraint description']", docNsMgr), "Constraint does not contain the correct description.");
        }

        /// <summary>
        /// Tests that a basic constraint (the first) is output correctly.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestBasicConstraint()
        {
            // Test parts of constraint #1: "SHALL contain exactly one [1..1] templateId (CONF:1-1)."
            XmlNode constraint1Para = doc.SelectSingleNode("//w:document/w:body/w:p[28][w:pPr/w:numPr[w:numId/@w:val='501'][w:ilvl/@w:val='0']]", docNsMgr) as XmlNode;
            Assert.IsNotNull(constraint1Para, "The first constraint paragraph could not be found, possibly due to the numbering of the list");

            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", docNsMgr), "Conformance verb incorrect for first constraint.");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[not(w:rPr/w:rStyle)]/w:t[text()=' contain ']", docNsMgr), "Plain text following conformance incorrect for first constraint.");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[not(w:rPr/w:rStyle)]/w:t[text()='exactly one [1..1] ']", docNsMgr), "Cardinality statement incorrect for first constraint.");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='templateId']", docNsMgr), "Context incorrect for first constraint");
            var bookmarkStart = constraint1Para.SelectSingleNode("w:bookmarkStart[@w:name='C_1-5'][following-sibling::w:r]", docNsMgr);
            var run = constraint1Para.SelectSingleNode("w:r[w:t[text()=' (CONF:1-5)']]", docNsMgr);
            var bookmarkEnd = constraint1Para.SelectSingleNode("w:bookmarkEnd[preceding-sibling::w:r]", docNsMgr);
            Assert.IsNotNull(bookmarkStart);
            Assert.IsNotNull(run);
            Assert.IsNotNull(bookmarkEnd);
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[not(w:rPr/w:pStyle)]/w:t[text()='.']", docNsMgr), "Missing period at end of first constraint");
        }

        /// <summary>
        /// Tests that the child constraint is output correctly with a single value and display name
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestChildConstraint()
        {
            // Test parts of constraint #1: "SHALL contain exactly one [1..1] @code (CONF:1-3)."
            XmlNode constraintPara = doc.SelectSingleNode("//w:document/w:body/w:p[30][w:pPr/w:numPr[w:numId/@w:val='501'][w:ilvl/@w:val='1']]", docNsMgr) as XmlNode;
            Assert.IsNotNull(constraintPara, "The third constraint paragraph could not be found, possibly due to the numbering of the list");

            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='This code ']", docNsMgr), "First part is not valid.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[2][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", docNsMgr), "Conformance verb incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[3][not(w:rPr/w:rStyle)]/w:t[text()=' contain ']", docNsMgr), "Plain text following conformance incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[4][not(w:rPr/w:rStyle)]/w:t[text()='exactly one [1..1] ']", docNsMgr), "Cardinality statement incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[5][w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='@value']", docNsMgr), "Context incorrect for");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[6][not(w:rPr/w:rStyle)]/w:t[text()='=']", docNsMgr), "Missing equals sign for static value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[7][w:rPr/w:rStyle/@w:val='XMLname']/w:t[text()='\"236435004\"']", docNsMgr), "Missing value for statically defined value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[8][not(w:rPr/w:rStyle)]/w:t[text()=' Test Static Value']", docNsMgr), "Missing display name from statically defined value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[9][not(w:rPr/w:rStyle)]/w:t[text()=' (CodeSystem: ']", docNsMgr), "Missing Code System text from statically defined value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[10][w:rPr/w:rStyle/@w:val='XMLname']/w:t[text()='SNOMED CT urn:oid:2.16.840.1.113883.6.96']", docNsMgr), "Missing Code System Name & OID for statically defined value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[11][not(w:rPr/w:rStyle)]/w:t[text()=')']", docNsMgr), "Missing closing bracket from statically defined value");
            var bookmarkStart = constraintPara.SelectSingleNode("w:bookmarkStart[@w:name='C_1-7']", docNsMgr);
            var confRun = constraintPara.SelectSingleNode("w:r[12][w:t[text()=' (CONF:1-7)']]", docNsMgr);
            var bookmarkEnd = constraintPara.SelectSingleNode("w:bookmarkEnd", docNsMgr);
            Assert.IsNotNull(bookmarkStart, "Missing bookmarkStart");
            Assert.IsNotNull(confRun, "CONF # or bookmark incorrect");
            Assert.AreEqual(bookmarkStart.NextSibling, confRun, "bookmarkStart is supposed to preceed the run");
            Assert.AreEqual(confRun.NextSibling, bookmarkEnd, "bookmarkEnd is supposed to follow the run");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[13][not(w:rPr/w:pStyle)]/w:t[text()='.']", docNsMgr), "Missing period at end");
        }

        /// <summary>
        /// Tests that STATIC valueset is properly output in constraint
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestStaticValuesetConstraint()
        {
            XmlNode constraintPara = doc.SelectSingleNode("/w:document/w:body/w:p[32][w:pPr/w:numPr[w:ilvl/@w:val=1][w:numId/@w:val=501]]", docNsMgr);
            Assert.IsNotNull(constraintPara, "Could not find constraint paragraph");

            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='This code ']", docNsMgr), "Did not find 'This code '.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[2][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", docNsMgr), "Conformance verb incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[3][not(w:rPr/w:rStyle)]/w:t[text()=' contain ']", docNsMgr), "Plain text following conformance incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[4][not(w:rPr/w:rStyle)]/w:t[text()='exactly one [1..1] ']", docNsMgr), "Cardinality statement incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[5][w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='@code']", docNsMgr), "Context incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[6][not(w:rPr/w:rStyle)]/w:t[text()='=']", docNsMgr), "Missing equals sign for static value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[7][w:rPr/w:rStyle/@w:val='XMLname']/w:t[text()='\"55561003\"']", docNsMgr), "Missing value for statically defined value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[8][not(w:rPr/w:rStyle)][w:t=' Active']", docNsMgr), "Missing display name from statically defined value'");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[9][not(w:rPr/w:rStyle)][w:t=', which ']", docNsMgr), "Could not find ', which '");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[10][w:rPr/w:rStyle/@w:val='keyword'][w:t='SHALL']", docNsMgr), "Value Set conformance incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[11][not(w:rPr/w:rStyle)][w:t=' be selected from ValueSet ']", docNsMgr), "Valueset narrative incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[12][w:rPr/w:rStyle/@w:val='XMLname'][w:t=' urn:oid:2.16.840.1.114222.4.11.3203']", docNsMgr), "Missing valueset name and OID");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[13][w:rPr/w:rStyle/@w:val='keyword'][w:t=' STATIC']", docNsMgr), "Missing 'STATIC'");

            var bookmarkStart = constraintPara.SelectSingleNode("w:bookmarkStart[@w:name='C_1-9']", docNsMgr);
            var confRun = constraintPara.SelectSingleNode("w:r[14][w:t[text()=' (CONF:1-9)']]", docNsMgr);
            var bookmarkEnd = constraintPara.SelectSingleNode("w:bookmarkEnd", docNsMgr);
            Assert.IsNotNull(bookmarkStart, "Missing bookmarkStart");
            Assert.IsNotNull(confRun, "CONF # or bookmark incorrect");
            Assert.AreEqual(bookmarkStart.NextSibling, confRun, "bookmarkStart is supposed to preceed the run");
            Assert.AreEqual(confRun.NextSibling, bookmarkEnd, "bookmarkEnd is supposed to follow the run");
            
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[15][not(w:rPr/w:rStyle)][w:t='.']", docNsMgr), "Missing period at end of constraint.'");
        }


        /// <summary>
        /// Test that DYNAMIC code system is properly output in constraint
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestDynamicCodeSystemConstraint()
        {
            XmlNode constraintPara = doc.SelectSingleNode("/w:document/w:body/w:p[24][w:pPr/w:numPr[w:ilvl/@w:val=0][w:numId/@w:val=501]]", docNsMgr);
            Assert.IsNotNull(constraintPara, "Could not find constraint paragraph");

            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[1][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", docNsMgr), "Conformance verb incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[2][not(w:rPr/w:rStyle)]/w:t[text()=' contain ']", docNsMgr), "Plain text following conformance incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[3][not(w:rPr/w:rStyle)]/w:t[text()='exactly one [1..1] ']", docNsMgr), "Cardinality statement incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[4][w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='code']", docNsMgr), "Context incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[5][not(w:rPr/w:rStyle)][w:t='=']", docNsMgr), "Could not find '='");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[6][w:rPr/w:rStyle/@w:val='XMLname'][w:t='\"51897-7\"']", docNsMgr), "Code incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[7][not(w:rPr/w:rStyle)]/w:t[text()=' Test Disp']", docNsMgr), "Code System narrative incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[8][not(w:rPr/w:rStyle)]/w:t[text()=' with @xsi:type=\"CD\"']", docNsMgr), "Code System type incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[9][not(w:rPr/w:rStyle)]/w:t[text()=', where the code ']", docNsMgr), "Code System text incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[10][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", docNsMgr), "Code System Conformance verb incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[11][not(w:rPr/w:rStyle)]/w:t[text()=' be selected from CodeSystem ']", docNsMgr), "Code System selection text incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[12][w:rPr/w:rStyle/@w:val='XMLname'][w:t='LOINC (urn:oid:2.16.840.1.113883.6.1)']", docNsMgr), "Missing valueset definition");
            var bookmarkStart = constraintPara.SelectSingleNode("w:bookmarkStart[@w:name='C_1-1'][following-sibling::w:r]", docNsMgr);
            var run = constraintPara.SelectSingleNode("w:r[13][w:t=' (CONF:1-1)']", docNsMgr);
            var bookmarkEnd = constraintPara.SelectSingleNode("w:bookmarkEnd[preceding-sibling::w:r]", docNsMgr);
            Assert.IsNotNull(bookmarkStart, "bookmarkStart is not valid, or does not precede a run");
            Assert.IsNotNull(run, "Run does not exist with conformance number formatted correctly");
            Assert.IsNotNull(bookmarkEnd, "bookmarkEnd is not valid, or does not follow a run");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[14][not(w:rPr/w:rStyle)][w:t='.']", docNsMgr), "Missing period at end of constraint.'");
        }

        /// <summary>
        /// Test that narrative in a primitive constraint is properly output
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestPrimitiveConstraint()
        {
            XmlNode constraintPrimitive = doc.SelectSingleNode("/w:document/w:body/w:p[27][w:pPr/w:numPr[w:ilvl/@w:val = 0][w:numId/@w:val = 501]]", docNsMgr);
            Assert.IsNotNull(constraintPrimitive, "Could not find primitive constraint in document");

            Assert.IsNotNull(constraintPrimitive.SelectSingleNode("w:r[1]/w:t[text() = 'A templateId element']", docNsMgr), "Constraint does not contain the correct text");
            Assert.IsNotNull(constraintPrimitive.SelectSingleNode("w:r[2][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()=' SHALL ']", docNsMgr), "Conformance verb incorrect.");
            Assert.IsNotNull(constraintPrimitive.SelectSingleNode("w:r[3]/w:t[text() = 'be present representing conformance to this release of the Implementation Guide (CONF:1-4).']", docNsMgr), "Constraint does not contain the correct text");

        }

        /// <summary>
        /// Test that branched constraints output properly
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestBranchConstraint()
        {
            XmlNode constraintBranched = doc.SelectSingleNode("/w:document/w:body/w:p[25][w:pPr/w:numPr[w:ilvl/@w:val = 0][w:numId/@w:val = 501]]", docNsMgr);
            Assert.IsNotNull(constraintBranched, "Could not find branched constraint in document");

            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:r[1][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", docNsMgr), "Conformance verb incorrect.");
            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:r[2]/w:t[text() = ' contain ']", docNsMgr), "Constraint does not contain the correct containment text");
            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:r[3]/w:t[text() = 'exactly one [1..1] ']", docNsMgr), "Constraint does not contain the correct conformance text");
            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:r[4][w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='participant']", docNsMgr), "Constraint name incorrect.");
            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:bookmarkStart[@w:name='C_1-2'][following-sibling::w:r]", docNsMgr), "Missing bookmarkStart");
            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:r[5][w:t=' (CONF:1-2)']", docNsMgr), "Missing CONF#");
            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:bookmarkEnd[preceding-sibling::w:r]", docNsMgr), "Missing bookmarkEnd");
            Assert.IsNotNull(constraintBranched.SelectSingleNode("w:r[6]/w:t[text() = ' such that it']", docNsMgr), "Constraint Code branching text incorrect");
        }


        /// <summary>
        /// Test that XML Sample Header Information is output properly
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestXMLSampleHeader()
        {
            XmlElement XMLSampleTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/wrPr/w:t[text() = 'Test_Template_1_Example']]", docNsMgr) as XmlElement;
            XmlNode xmlSampleHeader = doc.SelectSingleNode("/w:document/w:body/w:p[33]", docNsMgr);
            Assert.IsNotNull(xmlSampleHeader, "Could not find XML Sample"); 

            Assert.IsNotNull(xmlSampleHeader.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", docNsMgr), "Expected to find a style for the XML Sample caption");
            Assert.IsNotNull(xmlSampleHeader.SelectSingleNode("w:pPr/w:ind[@w:left='130']", docNsMgr), "Expected to find a Left Indent for the XML Sample");
            Assert.IsNotNull(xmlSampleHeader.SelectSingleNode("w:pPr/w:ind[@w:right='115']", docNsMgr), "Expected to find a Right Indent for the XML Sample");
            Assert.IsNotNull(xmlSampleHeader.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='Figure ']", docNsMgr), "Did not find XML Sample Label");        
            Assert.IsNotNull(xmlSampleHeader.SelectSingleNode("w:r[5][not(w:rPr/w:rStyle)]/w:t[text()='1: ']", docNsMgr), "Figure Number missing for XML Sample");
            Assert.IsNotNull(xmlSampleHeader.SelectSingleNode("w:r[7][not(w:rPr/w:rStyle)]/w:t[text()='Test_Template_1_Example']", docNsMgr), "Title missing for XML Sample");
        }

        /// <summary>
        /// Test that XML Sample Content is output properly
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestXMLSampleBody()
        {
            XmlNode xmlSampleBody = doc.SelectSingleNode("/w:document/w:body/w:p[34]", docNsMgr);
            Assert.IsNotNull(xmlSampleBody, "Could not find XML Sample");

            Assert.IsNotNull(xmlSampleBody.SelectSingleNode("w:pPr/w:pStyle[@w:val='Example']", docNsMgr), "Expected to find a style for the XML Sample Body");
            Assert.IsNotNull(xmlSampleBody.SelectSingleNode("w:pPr/w:ind[@w:left='130']", docNsMgr), "Expected to find a Left Indent for the XML Sample Body");
            Assert.IsNotNull(xmlSampleBody.SelectSingleNode("w:pPr/w:ind[@w:right='115']", docNsMgr), "Expected to find a Right Indent for the XML Sample Body");
            Assert.IsNotNull(xmlSampleBody.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='<observation/>']", docNsMgr), "Did not find XML Sample Contents");
        }

        #endregion

        #region Template Notes Tests

        /// <summary>
        /// Test that Template Notes are output properly
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestTemplateNotes()
        {
            // Confirm Template Level Notes exist
            XmlNode templateNotes = comments.SelectSingleNode("/w:comments/w:comment[82]/w:p[1]", commentsNsMgr);
            Assert.IsNotNull(templateNotes.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='Template Level Note for Test Template 1']", docNsMgr), "Did not find Template 1 Note");

            // Confirm Constraint Level Notes exist
            XmlNode constraintNotes = comments.SelectSingleNode("/w:comments/w:comment[83]/w:p[1]", commentsNsMgr);
            Assert.IsNotNull(constraintNotes.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='Constraint *TemplateId* Note for Test Template 1']", docNsMgr), "Did not find Template 1 Constraint Note");
        }

        #endregion

        /// <summary>
        /// Generates a word document and stores it on the file system for manual viewing
        /// As the generation code is updated, the sample will have to be regenerated (uncomment the line below to regenerate the sample).
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void GenerateIGFileTest()
        {
            File.WriteAllText(Path.Combine(this.TestContext.TestDir, "sample1_word_document.xml"), this.docContents);
            File.WriteAllText(Path.Combine(this.TestContext.TestDir, "sample1_word_comments.xml"), this.docComments);

            string docOutputLocation = Path.Combine(this.TestContext.TestDir, "sample.docx");
            File.WriteAllBytes(docOutputLocation, docBytes);
            Console.WriteLine("Saved output to " + docOutputLocation);
        }

        #region Utility Word Doc Methods

        private static XmlDocument ReadWordDocXml(string docContents, out XmlNamespaceManager nsManager)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(docContents);

            nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

            return doc;
        }

        private static string GetWordDocumentContents(byte[] generatedContents)
        {
            using (MemoryStream ms = new MemoryStream(generatedContents))
            {
                using (ZipFile docZip = ZipFile.Read(ms))
                {
                    ZipEntry docEntry = docZip["word\\document.xml"];
                    return GetZipEntryContents(docEntry);
                }
            }
        }

        private static string GetWordCommentContents(byte[] generatedContents)
        {
            using (MemoryStream ms = new MemoryStream(generatedContents))
            {
                using (ZipFile docZip = ZipFile.Read(ms))
                {
                    ZipEntry docEntry = docZip["word\\comments.xml"];
                    return GetZipEntryContents(docEntry);
                }
            }
        }

        private static string GetZipEntryContents(ZipEntry entry)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                entry.Extract(ms);

                ms.Seek(0, SeekOrigin.Begin);

                StreamReader reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        #endregion
    }
}
