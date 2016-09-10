using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml;

using Ionic.Zip;

using Trifolia.Shared;
using Trifolia.Generation.IG;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.DB;

namespace Trifolia.Test.Generation.IG
{
    /// <summary>
    ///This is a test class for ImplementationGuideGeneratorTest and is intended
    ///to contain all ImplementationGuideGeneratorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ImplementationGuideGeneratorTest
    {
        private TestContext testContextInstance;
        private MockObjectRepository mockRepo;
        private byte[] docBytes;
        private string docContents;
        private XmlNamespaceManager nsMgr;
        private XmlDocument doc;

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
            this.mockRepo = TestDataGenerator.GenerateMockDataset1();
            IEnumerable<int> templateIds = (from t in this.mockRepo.Templates
                                             select t.Id);

            ImplementationGuideGenerator generator = new ImplementationGuideGenerator(this.mockRepo,
                                                                                      1, templateIds);
            ExportSettings lSettings = new ExportSettings();
            lSettings.Use(s =>
            {
                s.GenerateTemplateConstraintTable = true;
                s.GenerateTemplateContextTable = true;
                s.GenerateDocTemplateListTable = true;
                s.GenerateDocContainmentTable = true;
                s.AlphaHierarchicalOrder = false;
                s.DefaultValueSetMaxMembers = 0;
                s.GenerateValueSetAppendix = false;
                s.IncludeXmlSamples = false;
                s.IncludeChangeList = false;
                s.IncludeTemplateStatus = true;
                s.IncludeNotes = true;
            });

            generator.BuildImplementationGuide(lSettings);
            this.docBytes = generator.GetDocument();

            this.docContents = GetWordDocumentContents(docBytes);
            this.doc = ReadWordDocXml(docContents, out this.nsMgr);
        }

        #region Table Tests

        /// <summary>
        /// Tests that containment tables are generated in the document and that the header is present and has the title and type columns.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestContainmentTable()
        {
            // Test the containment table's caption
            XmlElement containmentTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Template Containments')]]", nsMgr) as XmlElement;
            Assert.IsNotNull(containmentTitle);
            Assert.IsNotNull(containmentTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", nsMgr), "Expected to find a style on the containment table's caption");

            // Test the containment table's existance
            XmlElement containmentTable = containmentTitle.NextSibling as XmlElement;
            XmlElement templateTitleHeader = containmentTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Template Title']", nsMgr) as XmlElement;
            XmlElement templateTypeHeader = containmentTable.SelectSingleNode("w:tr[1]/w:tc[w:p/w:pPr/w:pStyle/@w:val='TableHead'][w:p/w:r/w:t/text() = 'Template Type']", nsMgr) as XmlElement;
            Assert.AreEqual("tbl", containmentTable.LocalName, "The node following the caption of the containment table is not a table!");

            // Test the containment table's header
            Assert.IsNotNull(containmentTable.SelectSingleNode("w:tr[1]/w:trPr/w:tblHeader", nsMgr), "The table does not contain the header property");
            Assert.IsNotNull(templateTitleHeader, "Expected to find a 'Template Title' column.");
            Assert.IsNotNull(templateTypeHeader, "Expected to find a 'Template Type' column.");
            Assert.AreEqual(templateTypeHeader.PreviousSibling, templateTitleHeader, "Expected template title to come before template type.");
        }

        /// <summary>
        /// Tests that constraint tables are generated in the document, that the header row is properly formatted, and that the first row matches the template's context
        /// </summary>
        public void TestConstraintTable()
        {
            // TODO
        }

        /// <summary>
        /// Tests the document template list table
        /// </summary>
        public void TestDocTemplateListTable()
        {
            // TODO
        }

        /// <summary>
        /// Tests that the document's containment table is correct
        /// </summary>
        public void TestDocContainmentTable()
        {
            // TODO
        }

        #endregion

        #region Constraint Generation Tests

        /// <summary>
        /// Test a contained template WITHOUT a context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestFormattedConstraint_ContainedTemplate1()
        {
            TemplateConstraint ctConstraint = this.mockRepo.TemplateConstraints.First(y => y.ContainedTemplateId != null);
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, ctConstraint.Template.OwningImplementationGuideId);

            IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, ctConstraint);
            string constraintText = fc.GetPlainText();

            Assert.IsNotNull(constraintText);
            Assert.AreEqual("SHALL contain exactly one [1..1] Test Template 2 (identifier: 1.2.3.4.5.6) (CONF:1-7).", constraintText);
        }

        /// <summary>
        /// Test a contained template WITH a context
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestFormattedConstraint_ContainedTemplate2()
        {
            TemplateConstraint ctConstraint = this.mockRepo.TemplateConstraints.Last(y => y.ContainedTemplateId != null);
            IGSettingsManager igSettings = new IGSettingsManager(this.mockRepo, ctConstraint.Template.OwningImplementationGuideId);

            IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(mockRepo, igSettings, ctConstraint);
            string constraintText = fc.GetPlainText();

            Assert.IsNotNull(constraintText);
            Assert.AreEqual("This entry SHALL contain exactly one [1..1] Test Template 2 (identifier: 1.2.3.4.5.6) (CONF:1-9).", constraintText);
        }

        /// <summary>
        /// Tests that the contained templates are properly linked and formatted in constraints.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestContainedTemplate()
        {
            XmlNode containedPara = doc.SelectSingleNode("/w:document/w:body/w:p[47][w:pPr/w:numPr[w:ilvl/@w:val=0][w:numId/@w:val=503]]", nsMgr);
            Assert.IsNotNull(containedPara, "Could not find 'Contained' paragraph.");

            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[1][w:rPr/w:rStyle/@w:val='keyword'][w:t='SHALL']", nsMgr), "Missing conformance");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[2][not(w:rPr/w:rStyle)][w:t=' contain ']", nsMgr), "Missing ' contain '");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[3][not(w:rPr/w:rStyle)][w:t='exactly one [1..1] ']", nsMgr), "Missing cardinality");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:hyperlink[@w:anchor='D_Test_Template_2'][w:r[w:rPr/w:rStyle/@w:val='HyperlinkCourierBold'][w:t='Test Template 2']]", nsMgr), "Missing hyperlink to contained template");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[4][w:rPr/w:rStyle/@w:val='XMLname'][w:t=' (identifier: 1.2.3.4.5.6)']", nsMgr), "Missing contained template's oid");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[5][w:bookmarkStart/@w:name='C_1-7'][not(w:rPr/w:rStyle)][w:t=' (CONF:1-7)']", nsMgr), "Missing CONF #");
            Assert.IsNotNull(containedPara.SelectSingleNode("w:r[6][not(w:rPr/w:rStyle)][w:t='.']", nsMgr), "Missing ending period");
        }

        /// <summary>
        /// Tests that an implied template outputs the "Conforms to" entry in the constraints list of the template.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestImpliedTemplate()
        {
            XmlNode conformsPara = doc.SelectSingleNode("/w:document/w:body/w:p[37][w:pPr/w:numPr[w:ilvl/@w:val=0][w:numId/@w:val=502]]", nsMgr);
            Assert.IsNotNull(conformsPara, "Could not find the 'Conforms to' paragraph");

            Assert.IsNotNull(conformsPara.SelectSingleNode("w:r[1][not(w:rPr/w:pStyle)][w:t='Conforms to ']", nsMgr), "Expect to find 'Conforms to' as first run");
            Assert.IsNotNull(conformsPara.SelectSingleNode("w:hyperlink[@w:anchor='D_Test_Template_1'][w:r[w:rPr/w:rStyle/@w:val='HyperlinkCourierBold']/w:t='Test Template 1']", nsMgr), "Hyperlink is incorrect.");
            Assert.IsNotNull(conformsPara.SelectSingleNode("w:r[2][not(w:rPr/w:rStyle)][w:t=' template ']", nsMgr), "Missing ' template ' run.");
            Assert.IsNotNull(conformsPara.SelectSingleNode("w:r[3][w:rPr/w:rStyle/@w:val='XMLname'][w:t='(identifier: 1.2.3.4.5)']", nsMgr), "Missing template oid run.");
        }

        /// <summary>
        /// Tests that a basic constraint (the first) is output correctly.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestBasicConstraint()
        {
            // Test parts of constraint #1: "SHALL contain exactly one [1..1] templateId (CONF:1-1)."
            XmlNode constraint1Para = doc.SelectSingleNode("//w:document/w:body/w:p[24][w:pPr/w:numPr[w:numId/@w:val='501'][w:ilvl/@w:val='0']]", nsMgr) as XmlNode;
            Assert.IsNotNull(constraint1Para, "The first constraint paragraph could not be found, possibly due to the numbering of the list");

            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", nsMgr), "Conformance verb incorrect for first constraint.");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[not(w:rPr/w:rStyle)]/w:t[text()=' contain ']", nsMgr), "Plain text following conformance incorrect for first constraint.");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[not(w:rPr/w:rStyle)]/w:t[text()='exactly one [1..1] ']", nsMgr), "Cardinality statement incorrect for first constraint.");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='templateId']", nsMgr), "Context incorrect for first constraint");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[w:bookmarkStart/@w:name='C_1-1']/w:t[text()=' (CONF:1-1)']", nsMgr), "CONF # or bookmark incorrect for first constraint");
            Assert.IsNotNull(constraint1Para.SelectSingleNode("w:r[not(w:rPr/w:pStyle)]/w:t[text()='.']", nsMgr), "Missing period at end of first constraint");
        }

        /// <summary>
        /// Tests that the child constraint is output correctly with a single value and display name
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestChildConstraint()
        {
            // Test parts of constraint #1: "SHALL contain exactly one [1..1] @code (CONF:1-3)."
            XmlNode constraintPara = doc.SelectSingleNode("//w:document/w:body/w:p[26][w:pPr/w:numPr[w:numId/@w:val='501'][w:ilvl/@w:val='1']]", nsMgr) as XmlNode;
            Assert.IsNotNull(constraintPara, "The third constraint paragraph could not be found, possibly due to the numbering of the list");

            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='This code ']", nsMgr), "First part is not valid.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[2][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", nsMgr), "Conformance verb incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[3][not(w:rPr/w:rStyle)]/w:t[text()=' contain ']", nsMgr), "Plain text following conformance incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[4][not(w:rPr/w:rStyle)]/w:t[text()='exactly one [1..1] ']", nsMgr), "Cardinality statement incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[5][w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='@code']", nsMgr), "Context incorrect for");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[6][not(w:rPr/w:rStyle)]/w:t[text()='=']", nsMgr), "Missing equals size for static value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[7][w:rPr/w:rStyle/@w:val='XMLname']/w:t[text()='\"12345X\"']", nsMgr), "Missing value for statically defined value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[8][not(w:rPr/w:rStyle)]/w:t[text()=' Test Static Value']", nsMgr), "Missing display name from statically defined value");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[9][w:bookmarkStart/@w:name='C_1-3']/w:t[text()=' (CONF:1-3)']", nsMgr), "CONF # or bookmark incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[10][not(w:rPr/w:pStyle)]/w:t[text()='.']", nsMgr), "Missing period at end");
        }

        /// <summary>
        /// Tests that STATIC valueset is properly output in constraint
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestStaticValuesetConstraint()
        {
            XmlNode constraintPara = doc.SelectSingleNode("/w:document/w:body/w:p[28][w:pPr/w:numPr[w:ilvl/@w:val=1][w:numId/@w:val=501]]", nsMgr);
            Assert.IsNotNull(constraintPara, "Could not find constraint paragraph");

            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[1][not(w:rPr/w:rStyle)]/w:t[text()='This code ']", nsMgr), "Did not find 'This code '.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[2][w:rPr/w:rStyle/@w:val='keyword']/w:t[text()='SHALL']", nsMgr), "Conformance verb incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[3][not(w:rPr/w:rStyle)]/w:t[text()=' contain ']", nsMgr), "Plain text following conformance incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[4][not(w:rPr/w:rStyle)]/w:t[text()='exactly one [1..1] ']", nsMgr), "Cardinality statement incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[5][w:rPr/w:rStyle/@w:val='XMLnameBold']/w:t[text()='@code']", nsMgr), "Context incorrect.");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[6][not(w:rPr/w:rStyle)][w:t=', which ']", nsMgr), "Could not find ', which '");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[7][w:rPr/w:rStyle/@w:val='keyword'][w:t='SHALL']", nsMgr), "Conformance incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[8][not(w:rPr/w:rStyle)][w:t=' be selected from ValueSet ']", nsMgr), "Valueset narrative incorrect");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[9][w:rPr/w:rStyle/@w:val='XMLname'][w:t='Test Value Set 9.8.7.6.5.4.3.2.1']", nsMgr), "Missing valueset definition");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[10][w:rPr/w:rStyle/@w:val='keyword'][w:t=' STATIC']", nsMgr), "Missing 'STATIC'");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[11][w:bookmarkStart/@w:name='C_1-5'][w:t=' (CONF:1-5)']", nsMgr), "Missing CONF# or Bookmark'");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r[12][not(w:rPr/w:rStyle)][w:t='.']", nsMgr), "Missing period at end of constraint.'");
        }

        [TestMethod, TestCategory("MSWord")]
        public void TestConstraintDescription()
        {
            XmlNode constraintPara = doc.SelectSingleNode("/w:document/w:body/w:p[16]", nsMgr);

            Assert.IsNotNull(constraintPara, "Could not find constraint in document");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:pPr/w:pStyle[@w:val='BodyText']", nsMgr), "Constraint's paragraph is not styled as 'BodyText'");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:pPr/w:spacing[@w:before='120']", nsMgr), "Constraint's paragraph is not spaced correctly");
            Assert.IsNotNull(constraintPara.SelectSingleNode("w:r/w:t[text() = 'Test constraint description']", nsMgr), "Constraint does not contain the correct description.");
        }

        /// <summary>
        /// Test that DYNAMIC code system is properly output in constraint
        /// </summary>
        public void TestDynamicCodeSystemConstraint()
        {
            // TODO
        }

        /// <summary>
        /// Test that narrative in a primitive constraint is properly output
        /// </summary>
        public void TestPrimitiveConstraint()
        {
            // TODO
        }

        /// <summary>
        /// Test that branched constraints output properly
        /// </summary>
        public void TestBranchConstraint()
        {
            // TODO
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

            string outputLocation = Path.Combine(this.TestContext.TestDir, "sample.docx");
            File.WriteAllBytes(outputLocation, docBytes);
            Console.WriteLine("Saved output to " + outputLocation);
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
