using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Trifolia.DB;
using Trifolia.Shared.Plugins;
using Trifolia.Generation.IG;
using Trifolia.Shared;
using System.Xml;
using System.IO;
using Ionic.Zip;

namespace Trifolia.Test.Generation.IG
{
    [TestClass]
    public class ImplementationGuideSettingsTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides information about and functionality for the current test run.
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

        #region Utility Word Doc Methods

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

        private static XmlDocument ReadWordDocXml(string docContents, out XmlNamespaceManager nsManager)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(docContents);

            nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

            return doc;
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

        private XmlDocument CreateDocument(ExportSettings settings, string name, out XmlNamespaceManager nsManager)
        {
            var mockRepo = TestDataGenerator.GenerateMockDataset4();
            IEnumerable<int> templateIds = (from t in mockRepo.Templates
                                            select t.Id);

            ImplementationGuide ig = mockRepo.ImplementationGuides.Single(y => y.Name == TestDataGenerator.DS1_IG_NAME);
            IIGTypePlugin igTypePlugin = ig.ImplementationGuideType.GetPlugin();
            ImplementationGuideGenerator generator = new ImplementationGuideGenerator(mockRepo, 1, templateIds);

            generator.BuildImplementationGuide(settings, igTypePlugin);
            var docBytes = generator.GetDocument();

            var docContents = GetWordDocumentContents(docBytes);
            XmlDocument doc = ReadWordDocXml(docContents, out nsManager);

            File.WriteAllText(Path.Combine(this.TestContext.TestDir, name + ".xml"), docContents);

            string outputLocation = Path.Combine(this.TestContext.TestDir, name + ".docx");
            File.WriteAllBytes(outputLocation, docBytes);
            Console.WriteLine("Saved output to " + outputLocation);

            return doc;
        }

        /// <summary>
        /// Test that the Template Status is generated when the Export Settings Template/Profile Status "Include" option is checked
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestTemplateStatusExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.IncludeTemplateStatus = true;

            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "template_status_exists", nsManager: out nsManager);

            XmlElement templateStatus = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Draft as part of Test Implementation Guide')]]", nsManager) as XmlElement;
            Assert.IsNotNull(templateStatus, "Could not find Template Status in Document");
        }

        /// <summary>
        /// Test that an Template Status is not generated when the Export Settings Template/Profile Status "Include" option is unchecked
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestTemplateStatusNotExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.IncludeTemplateStatus = false;

            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "template_status_not_exists", nsManager: out nsManager);

            XmlElement xmlSampleTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Draft as part of Test Implementation Guide')]]", nsManager) as XmlElement;
            Assert.IsNull(xmlSampleTitle, "Found Template Status in Document");
        }

        // Test the constraints overview table is generated with the "Template/Profile Tables" setting set to "Constraints Overview"
        [TestMethod, TestCategory("MSWord")]
        public void TestConstraintTableExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.GenerateTemplateConstraintTable = true;
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "constraint_table_exists", out nsManager);

            // Test the constraint table's caption exists
            XmlElement constraintTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), ': Test Constraint Description Template Constraints Overview')]]", nsManager) as XmlElement;
            Assert.IsNotNull(constraintTitle);
            Assert.IsNotNull(constraintTitle.SelectSingleNode("w:pPr/w:pStyle[@w:val='Caption']", nsManager), "Expected to find a style on the constraint table's caption");

        }
        /// <summary>
        /// Test the constraint overview table does not get generated with the "Template/Profile Tables" setting set to "None"
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestConstraintTableNotExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.GenerateTemplateConstraintTable = false;
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "constraint_table_not_exists", nsManager: out nsManager);

            // Test the constraint table's caption does not exist
            XmlElement constraintTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), ': Test Constraint Description Template Constraints Overview')]]", nsManager) as XmlElement;
            Assert.IsNull(constraintTitle);
        }


        /// <summary>
        /// Test the Containment and List tables are not generated when the Export Settings Document Tables option is set to "None"
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestContainmentAndListTableNotExist()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.GenerateDocContainmentTable = false;
                s.GenerateDocTemplateListTable = false;
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "template_containment_and_list_table_does_not_exist", nsManager: out nsManager);

            // Test the containment table's caption does not exist
            XmlElement containmentTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Template Containments')]]", nsManager) as XmlElement;
            Assert.IsNull(containmentTitle);

            // Test the template list table's caption does not exist
            XmlElement templateListTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), ': Template List')]]", nsManager) as XmlElement;
            Assert.IsNull(templateListTitle);
        }

        /// <summary>
        /// Test that only containment tables are generated when the Export Settings Document Tables option is set to "Containment"
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestContainmentTableExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.GenerateDocContainmentTable = true;
                s.GenerateDocTemplateListTable = false;
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "template_containment_table_exists", nsManager: out nsManager);

            // Test the containment table's caption exists
            XmlElement containmentTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Template Containments')]]", nsManager) as XmlElement;
            Assert.IsNotNull(containmentTitle, "Could not find Containment Table");

            // Test the template list table's caption does not exist
            XmlElement templateListTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), ': Template List')]]", nsManager) as XmlElement;
            Assert.IsNull(templateListTitle, "Found Template List Table");
        }

        /// <summary>
        /// Test that only List tables are generated when the Export Settings Document Tables option is set to "List"
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestListTableExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.GenerateDocContainmentTable = false;
                s.GenerateDocTemplateListTable = true;
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "template_list_table_exists", nsManager: out nsManager);

            // Test the containment table's caption does not exists
            XmlElement containmentTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Template Containments')]]", nsManager) as XmlElement;
            Assert.IsNull(containmentTitle, "Found Containment Table");

            // Test the template list table's caption does exist
            XmlElement templateListTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), ': Template List')]]", nsManager) as XmlElement;
            Assert.IsNotNull(templateListTitle, "Could Not Find Template List Table");
        }


        /// <summary>
        /// Test that the XML Sample Title is generated when the Export Settings XML Samples "Include" option is checked
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestXMLSampleExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.IncludeXmlSamples = true;
                
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "XML_Sample_exists", nsManager: out nsManager);

            XmlElement xmlSampleTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), ': Test_Template_1_Example')]]", nsManager) as XmlElement;
            Assert.IsNotNull(xmlSampleTitle, "Could not find XML Sample Title in Document");
        }

        /// <summary>
        /// Test that an XML Sample Title is not generated when the Export Settings XML Samples "Include" option is unchecked
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestXMLSampleNotExists()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.IncludeXmlSamples = false;

            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "XML_Sample_not_exists", nsManager: out nsManager);

            XmlElement xmlSampleTitle = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), ': Test_Template_1_Example')]]", nsManager) as XmlElement;
            Assert.IsNull(xmlSampleTitle, "Found XML Sample Title in Document");
        }
        /// <summary>
        /// Test Value Set Table does not exist in the Appendix when the Export Settings "false" is uncheckchecked, and maxmimum members set to 0.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestValueSetTableNotExist()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.DefaultValueSetMaxMembers = 0;
                s.GenerateValueSetAppendix = false;

            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "ValueSet_Table_Not_Exists", nsManager: out nsManager);

            // Test the Value Sets In This Guide table does not appear in the document
            XmlElement valueSetTableNotExist = doc.DocumentElement.SelectSingleNode("//w:p[w:r/w:t[contains(text(), 'Value Sets In This Guide')]]", nsManager) as XmlElement;
            Assert.IsNull(valueSetTableNotExist, "Did not find Value Set in Document");
        }

        /// Test Value Set Table does exist in the Appendix when the Export Settings "true" is uncheckchecked, and maxmimum members set to 10.
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestValueSetMemberAll()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.DefaultValueSetMaxMembers = 10;
                s.GenerateValueSetAppendix = true;
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "ValueSet_In_Appendix_All_Members", nsManager: out nsManager);

            // Test the Value Sets In This Guide table does appear in the document Appendix and all members appear

            XmlElement ValueSetMembersAll = doc.DocumentElement.SelectSingleNode("//w:tbl/w:tr/w:tc/w:p[w:r/w:t[contains(text(), 'Cancelled')]]", nsManager) as XmlElement;
            Assert.IsNotNull(ValueSetMembersAll, "Last value set member does not appear in Value sets table in Appendix");
        }

        /// Test that an Value Set tables are not generated in the Appendix when the Export Settings "Include as Appendix" is unchecked
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestValueSetLinkInAppendix()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.DefaultValueSetMaxMembers = 10;
                s.GenerateValueSetAppendix = false;

            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "ValueSet_Link_In_Appendix", nsManager: out nsManager);

            // Test the Value Sets in this Guide table only contains the link and not value set details.

            XmlElement valueSetLinkInAppendix = doc.DocumentElement.SelectSingleNode("//w:tbl[2]/w:tr[2]/w:tc[1]/w:p[1]/w:hyperlink[1][@w:anchor='Treatment_status']", nsManager) as XmlElement;
            Assert.IsNotNull(valueSetLinkInAppendix, "Missing Value Set Appendix Link");
        }
      
        /// 
        /// Test only 2 Value Sets members appear in the Appendix when the Export Settings "Include as Appendix" is checked, and maximum members is set to 2
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void TestValueSetMembersTwo()
        {
            ExportSettings settings = new ExportSettings();
            settings.Use(s =>
            {
                s.DefaultValueSetMaxMembers = 2;
                s.GenerateValueSetAppendix = true;
            });

            XmlNamespaceManager nsManager;
            XmlDocument doc = this.CreateDocument(settings, "ValueSet_In_Appendix_Two_Members", nsManager: out nsManager);

            // Test the containment table's caption does not exists
            XmlElement ValueSetMembersTwo = doc.DocumentElement.SelectSingleNode("//w:tbl/w:tr/w:tc/w:p[w:r/w:t[contains(text(), 'ThirdTreatment')]]", nsManager) as XmlElement;
            Assert.IsNull(ValueSetMembersTwo, "Found More than 2 Value Set Members in Appendix");
        }
    }
}
