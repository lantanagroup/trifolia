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
using Trifolia.Shared.Plugins;

namespace Trifolia.Test.Generation.IG
{
    /// <summary>
    ///This is a test class for ImplementationGuideGeneratorTest and is intended
    ///to contain all ImplementationGuideGeneratorTest Unit Tests
    ///</summary>
    [DeploymentItem("Schemas\\", "Schemas\\")]
    [TestClass()]
    public class ImplementationGuideVersionTest
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
            ImplementationGuideGenerator generator = new ImplementationGuideGenerator(this.mockRepo, 4, templateIds);
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
                s.IncludeChangeList = true;
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

        [TestMethod]
        public void SomeTest()
        {
            // Should pass :)
        }

        /// <summary>
        /// Generates a word document and stores it on the file system for manual viewing
        /// As the generation code is updated, the sample will have to be regenerated (uncomment the line below to regenerate the sample).
        /// </summary>
        [TestMethod, TestCategory("MSWord")]
        public void GenerateIGFileTest()
        {
            File.WriteAllText(Path.Combine(this.TestContext.TestDir, "sample1_version_word_document.xml"), this.docContents);
            File.WriteAllText(Path.Combine(this.TestContext.TestDir, "sample1_version_word_comments.xml"), this.docComments);

            string docOutputLocation = Path.Combine(this.TestContext.TestDir, "sample_version.docx");
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
