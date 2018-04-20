using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

using Trifolia.Shared;
using Trifolia.Shared.ImportExport;
using ExportTemplate = Trifolia.Shared.ImportExport.Model.TrifoliaTemplate;
using ExportConformanceTypes = Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance;
using Trifolia.DB;
using Trifolia.Export.Native;

namespace Trifolia.Test.Generation.XML
{
    
    
    /// <summary>
    ///This is a test class for TemplateExportFactoryTest and is intended
    ///to contain all TemplateExportFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TemplateExportTest
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
        public void ExportTemplatesModelTest1()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            Organization org = tdb.FindOrCreateOrganization("LCG Test");
            ImplementationGuide ig = tdb.FindOrCreateImplementationGuide(tdb.FindImplementationGuideType(Constants.IGType.CDA_IG_TYPE), "Test Implementation Guide");
            IGSettingsManager igSettings = new IGSettingsManager(tdb, ig.Id);

            Template template = tdb.CreateTemplate("1.2.3.4", "Document", "Test Template", ig, "observation", "Observation", "Test Description", "Test Notes", org);
            var tc1 = tdb.AddConstraintToTemplate(template, null, null, "entryRelationship", "SHALL", "1..1");
            var tc2 = tdb.AddConstraintToTemplate(template, tc1, null, "observation", "SHOULD", "0..1");
            var tc3 = tdb.AddConstraintToTemplate(template, tc2, null, "value", "SHALL", "1..1", "CD", value: "4321", displayName: "Test");
            tc3.Description = "Test description";
            tc3.Label = "Test Label";
            var tc4 = tdb.AddPrimitiveToTemplate(template, null, "SHALL", "This is a test");

            ExportTemplate export = template.Export(tdb, igSettings);

            Assert.IsNotNull(export);
            Assert.AreEqual(template.Oid, export.identifier);
            Assert.AreEqual(template.Name, export.title);
            Assert.AreEqual(template.TemplateType.Name, export.templateType);
            Assert.AreEqual(template.PrimaryContext, export.context);
            Assert.AreEqual(template.PrimaryContextType, export.contextType);
            Assert.AreEqual(template.Description, export.Description);
            Assert.AreEqual(template.Notes, export.Notes);
            
            // Assert constraints
            Assert.IsNotNull(export.Constraint);
            Assert.AreEqual(2, export.Constraint.Count);

            // tc1
            Assert.AreEqual(tc1.Context, export.Constraint[0].context);
            Assert.AreEqual(ExportConformanceTypes.SHALL, export.Constraint[0].conformance);
            Assert.AreEqual(tc1.Cardinality, export.Constraint[0].cardinality);

            // tc4
            Assert.IsNull(export.Constraint[1].context);
            Assert.AreEqual(true, export.Constraint[1].isPrimitive);
            Assert.AreEqual(tc4.PrimitiveText, export.Constraint[1].NarrativeText);

            // tc2
            Assert.AreEqual(1, export.Constraint[0].Constraint.Count);
            Assert.AreEqual(tc2.Context, export.Constraint[0].Constraint[0].context);
            Assert.AreEqual(ExportConformanceTypes.SHOULD, export.Constraint[0].Constraint[0].conformance);
            Assert.AreEqual(tc2.Cardinality, export.Constraint[0].Constraint[0].cardinality);

            // tc3
            Assert.AreEqual(1, export.Constraint[0].Constraint[0].Constraint.Count);
            Assert.AreEqual(tc3.Context, export.Constraint[0].Constraint[0].Constraint[0].context);
            Assert.AreEqual(ExportConformanceTypes.SHALL, export.Constraint[0].Constraint[0].Constraint[0].conformance);
            Assert.AreEqual(tc3.Cardinality, export.Constraint[0].Constraint[0].Constraint[0].cardinality);
            Assert.AreEqual(tc3.Description, export.Constraint[0].Constraint[0].Constraint[0].Description);
            Assert.AreEqual(tc3.Label, export.Constraint[0].Constraint[0].Constraint[0].Label);
            Assert.IsFalse(string.IsNullOrEmpty(export.Constraint[0].Constraint[0].Constraint[0].Description));
        }

        [TestMethod]
        public void ExportTemplatesModelTest2()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            Organization org = tdb.FindOrCreateOrganization("LCG Test");
            ImplementationGuide ig = tdb.FindOrCreateImplementationGuide(tdb.FindImplementationGuideType(Constants.IGType.CDA_IG_TYPE), "Test Implementation Guide");
            IGSettingsManager igSettings = new IGSettingsManager(tdb, ig.Id);

            Template template = tdb.CreateTemplate("1.2.3.4", "Document", "Test Template", ig, "observation", "Observation", "Test Description", "Test Notes");

            ExportTemplate export = template.Export(tdb, igSettings);

            Assert.IsNotNull(export);
            Assert.AreEqual(template.Oid, export.identifier);
            Assert.AreEqual(template.Name, export.title);
            Assert.AreEqual(template.TemplateType.Name, export.templateType);
            Assert.AreEqual(template.PrimaryContext, export.context);
            Assert.AreEqual(template.PrimaryContextType, export.contextType);
            Assert.AreEqual(template.Description, export.Description);
            Assert.AreEqual(template.Notes, export.Notes);
            Assert.AreEqual(template.OwningImplementationGuide.Name, export.ImplementationGuide.name);
        }

        /// <summary>
        /// Generates mock dataset 1 and exports the templates from the dataset into XML format.
        /// Tests that the resulting XML contains the correct number templates, that the attributes on the first template
        /// are set properly, and lastly tests the number of constraints output for the first exported template.
        ///</summary>
        [TestMethod()]
        public void ExportTemplatesTest()
        {
            MockObjectRepository tdb = TestDataGenerator.GenerateMockDataset1();
            List<Template> templates = tdb.Templates.ToList();
            IGSettingsManager igSettings = new IGSettingsManager(tdb);

            TemplateExporter exporter = new TemplateExporter(tdb, templates, igSettings);
            string actual = exporter.GenerateXMLExport();

            Assert.IsNotNull(actual, "Export should have produced content.");
            Assert.AreNotEqual(string.Empty, actual, "Export should have produced content.");

            XmlDocument exportDoc = new XmlDocument();
            exportDoc.LoadXml(actual);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(exportDoc.NameTable);
            nsManager.AddNamespace("lcg", "http://www.lantanagroup.com");

            XmlNodeList templateNodes = exportDoc.DocumentElement.SelectNodes("lcg:Template", nsManager);

            Assert.IsNotNull(templateNodes, "Did not find any templates in export.");
            Assert.AreEqual(4, templateNodes.Count, "Export should have produced three (4) Template elements.");

            XmlAttribute identifierAttribute = templateNodes[0].Attributes["identifier"];
            XmlAttribute implementationGuideTypeAttribute = templateNodes[0].Attributes["implementationGuideType"];
            XmlAttribute templateTypeAttribute = templateNodes[0].Attributes["templateType"];
            XmlAttribute titleAttribute = templateNodes[0].Attributes["title"];
            XmlAttribute bookmarkAttribute = templateNodes[0].Attributes["bookmark"];

            Assert.IsNotNull(identifierAttribute, "Couldn't find identifier attribute on Template.");
            Assert.AreEqual("1.2.3.4.5", identifierAttribute.Value, "Template's identifier has an incorrect value.");

            Assert.IsNotNull(implementationGuideTypeAttribute, "Couldn't find implementationGuideType attribute on Template.");
            Assert.AreEqual(Constants.IGType.CDA_IG_TYPE, implementationGuideTypeAttribute.Value, "Template's implementationGuideType has an incorrect value.");

            Assert.IsNotNull(templateTypeAttribute, "Couldn't find templateType attribute on Template.");
            Assert.AreEqual("Document", templateTypeAttribute.Value, "Template's templateType has an incorrect value.");

            Assert.IsNotNull(titleAttribute, "Couldn't find title attribute on Template.");
            Assert.AreEqual("Test Template 1", titleAttribute.Value, "Template's title has an incorrect value.");

            Assert.IsNotNull(bookmarkAttribute, "Couldn't find bookmark attribute on Template.");
            Assert.AreEqual("D_Test_Template_1", bookmarkAttribute.Value, "Template's bookmark has an incorrect value.");

            XmlNodeList constraintNodes = templateNodes[0].SelectNodes("lcg:Constraint", nsManager);

            Assert.IsNotNull(constraintNodes, "Did not find any constraints in the first template.");
            Assert.AreEqual(3, constraintNodes.Count, "Did not find the correct number of root-level constraints in the first exported template.");

            XmlNodeList childConstraintNodes = constraintNodes[1].SelectNodes("lcg:Constraint", nsManager);

            Assert.IsNotNull(childConstraintNodes, "Did not find any grand-child constraints in the first template.");
            Assert.AreEqual(1, childConstraintNodes.Count, "Did not find the correct number of grand-child constraints in the first exported template.");
        }
    }
}
