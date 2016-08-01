using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.ImportExport;
using Trifolia.Generation.Green;
using Trifolia.Import.Native;

namespace Trifolia.Test.Generation.Green
{
    /// <summary>
    ///This is a test class for GreenSchemaGeneratorTest and is intended
    ///to contain all GreenSchemaGeneratorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GreenSchemaGeneratorTest
    {
        private TestContext testContextInstance;
        private MockObjectRepository mockRepo;

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

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            this.mockRepo = new MockObjectRepository();
            this.mockRepo.InitializeCDARepository();
            this.mockRepo.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "Dev Test IG");

            string importXml = Helper.GetSampleContents("Trifolia.Test.DocSamples.sample2.xml");
            TemplateImporter importer = new TemplateImporter(this.mockRepo);
            importer.Import(importXml);
        }

        private GreenTemplate CreateGreenTemplate(Template template, string name)
        {
            GreenTemplate newGreenTemplate = new GreenTemplate()
            {
                Template = template,
                TemplateId = template.Id,
                Name = name
            };

            template.GreenTemplates.Add(newGreenTemplate);
            this.mockRepo.GreenTemplates.AddObject(newGreenTemplate);

            return newGreenTemplate;
        }

        private GreenConstraint CreateGreenConstraint(GreenTemplate greenTemplate, TemplateConstraint constraint, GreenConstraint parent, string businessName = null, string elementName = null, string xpath = null, ImplementationGuideTypeDataType dataType = null)
        {
            if (string.IsNullOrEmpty(constraint.Context))
                throw new ArgumentException(constraint.Context);

            if (string.IsNullOrEmpty(elementName))
                elementName = constraint.Context;

            if (string.IsNullOrEmpty(businessName))
                businessName = constraint.Context;

            GreenConstraint newGreenConstraint = new GreenConstraint()
            {
                GreenTemplate = greenTemplate,
                GreenTemplateId = greenTemplate.Id,
                TemplateConstraint = constraint,
                TemplateConstraintId = constraint.Id,
                Description = businessName,
                Name = elementName,
                RootXpath = xpath,
                ImplementationGuideTypeDataType = dataType,
                ImplementationGuideTypeDataTypeId = dataType != null ? (int?)dataType.Id : null
            };

            if (parent != null)
            {
                newGreenConstraint.ParentGreenConstraint = parent;
                newGreenConstraint.ParentGreenConstraintId = parent.Id;
                parent.ChildGreenConstraints.Add(newGreenConstraint);
            }

            constraint.GreenConstraints.Add(newGreenConstraint);
            greenTemplate.ChildGreenConstraints.Add(newGreenConstraint);

            this.mockRepo.GreenConstraints.AddObject(newGreenConstraint);

            return newGreenConstraint;
        }

        #endregion

        /// <summary>
        /// Tests that collapsing a set of constraints in the green model produces the correct schema generation
        /// results.
        /// </summary>
        /// <remarks>
        /// Collapses the recordTarget and patientRole elements, so that "patient" is the top-level element.
        /// </remarks>
        [TestMethod()]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void BuildGreenSchemaTest_CollapsedConstraint1()
        {
            ImplementationGuide ig = this.mockRepo.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "Testing");
            Template docTemplate = this.mockRepo.GenerateTemplate("5.4.3.2.1", "Document", "Testing Collapsed", ig, "ClinicalDocument", "ClinicalDocument");
            var recordTarget = this.mockRepo.GenerateConstraint(docTemplate, null, null, "recordTarget", "SHALL", "1..*");
            var patientRole = this.mockRepo.GenerateConstraint(docTemplate, recordTarget, null, "patientRole", "SHALL", "1..1");
            var patient = this.mockRepo.GenerateConstraint(docTemplate, patientRole, null, "patient", "SHALL", "1..1");
            var patientGender = this.mockRepo.GenerateConstraint(docTemplate, patient, null, "administrativeGenderCode", "SHALL", "1..1");
            var patientName = this.mockRepo.GenerateConstraint(docTemplate, patient, null, "name", "SHALL", "1..1");
            var patientFirstName = this.mockRepo.GenerateConstraint(docTemplate, patientName, null, "given", "SHALL", "1..1");
            var patientLastName = this.mockRepo.GenerateConstraint(docTemplate, patientName, null, "family", "SHALL", "1..1");

            GreenTemplate greenDocTemplate = CreateGreenTemplate(docTemplate, "myGreenDoc");
            string patientXpath = "recordTarget/patientRole/patient";
            var greenPatient = CreateGreenConstraint(greenDocTemplate, patient, null, "Patient", "patient", patientXpath);

            string genderXpath = "recordTarget/patientRole/patient/administrativeGenderCode";
            var ceDataType = this.mockRepo.FindOrAddDataType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "CE");
            var greenPatientGender = CreateGreenConstraint(greenDocTemplate, patientGender, greenPatient, "Gender", "gender", genderXpath, ceDataType);

            GreenSchemaPackage package = GreenSchemaGenerator.Generate(mockRepo, docTemplate);

            // Is the schema content a valid schema?
            AssertSchemaValid(package.GreenSchemaContent);

            // Load the schema into an XmlDocument so we can assert XPATH against it
            XmlDocument schemaDoc = new XmlDocument();
            schemaDoc.LoadXml(package.GreenSchemaContent);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(schemaDoc.NameTable);
            nsManager.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

            ///////////
            // Test the schema generation results
            ///////////

            // XPATH = /xs:schema/xs:element[@name='myGreenDoc']
            var rootElementDocTemplate = schemaDoc.SelectSingleNode(
                "/xs:schema/xs:element[@name='" + greenDocTemplate.Name + "' and @type='" + greenDocTemplate.Name + "']", nsManager);
            Assert.IsNotNull(rootElementDocTemplate, "Could not find correct root element for document template");

            // XPATH = /xs:schema/xs:complexType[@name='myGreenDoc']
            var rootComplexTypeDocTemplate = schemaDoc.SelectSingleNode(
                "/xs:schema/xs:complexType[@name='" + greenDocTemplate.Name + "']", nsManager);
            Assert.IsNotNull(rootComplexTypeDocTemplate, "Could not find complex type for document template");

            // XPATH = xs:sequence/xs:element[@name='patient' and @minOccurs='1' and @maxOccurs='unbounded']
            var patientNode = rootComplexTypeDocTemplate.SelectSingleNode(
                "xs:sequence/xs:element[@name='patient' and @minOccurs='1' and @maxOccurs='unbounded']", nsManager);
            Assert.IsNotNull(patientNode, "Could not find patient element within myGreenDoc that has min and max occurs set correctly.");

            // XPATH = xs:complexType/xs:sequence/xs:element[@name='gender' and @minOccurs='1' and @maxOccurs='1']
            var patientGenderNode = patientNode.SelectSingleNode(
                "xs:complexType/xs:sequence/xs:element[@name='gender' and @minOccurs='1' and @maxOccurs='1']", nsManager);
            Assert.IsNotNull(patientGenderNode, "Could not find gender element with correct min and max occurs within patient element.");
        }

        /// <summary>
        /// Tests that collapsing a set of constraints in the green model produces the correct schema generation
        /// results. Collapses on a different element then in the CollapsedConstraint1 test.
        /// Additionally, tests that constraints for gender, name (which includes first name and last name) generate correctly.
        /// </summary>
        /// <remarks>
        /// Collapses the patientRole and patient elements, so that "recordTarget" is the top-level element.
        /// </remarks>
        [TestMethod()]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void BuildGreenSchemaTest_CollapsedConstraint2()
        {
            ImplementationGuide ig = this.mockRepo.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "Testing");
            Template docTemplate = this.mockRepo.GenerateTemplate("5.4.3.2.1", "Document", "Testing Collapsed", ig, "ClinicalDocument", "ClinicalDocument");
            var recordTarget = this.mockRepo.GenerateConstraint(docTemplate, null, null, "recordTarget", "SHALL", "1..*");
            var patientRole = this.mockRepo.GenerateConstraint(docTemplate, recordTarget, null, "patientRole", "SHALL", "1..1");
            var patient = this.mockRepo.GenerateConstraint(docTemplate, patientRole, null, "patient", "SHALL", "1..1");
            var patientGender = this.mockRepo.GenerateConstraint(docTemplate, patient, null, "administrativeGenderCode", "SHALL", "1..1");
            var patientName = this.mockRepo.GenerateConstraint(docTemplate, patient, null, "name", "SHALL", "1..1");
            var patientFirstName = this.mockRepo.GenerateConstraint(docTemplate, patientName, null, "given", "SHALL", "1..1");
            var patientLastName = this.mockRepo.GenerateConstraint(docTemplate, patientName, null, "family", "SHALL", "1..1");

            GreenTemplate greenDocTemplate = CreateGreenTemplate(docTemplate, "myGreenDoc");
            string recordTargetXpath = "recordTarget";
            var greenRecordTarget = CreateGreenConstraint(greenDocTemplate, recordTarget, null, "Patient", "patient", recordTargetXpath);

            string genderXpath = "recordTarget/patientRole/patient/administrativeGenderCode";
            var ceDataType = this.mockRepo.FindOrAddDataType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "CE");
            var greenPatientGender = CreateGreenConstraint(greenDocTemplate, patientGender, greenRecordTarget, "Gender", "gender", genderXpath, ceDataType);

            string nameXpath = "recordTarget/patientRole/patient/name";
            var greenPatientName = CreateGreenConstraint(greenDocTemplate, patientName, greenRecordTarget, "Name", "name", nameXpath);

            string firstNameXpath = "recordTarget/patientRole/patient/name/given";
            var stDataType = this.mockRepo.FindOrAddDataType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "ST");
            var greenPatientFirstName = CreateGreenConstraint(greenDocTemplate, patientFirstName, greenPatientName, "First", "first", firstNameXpath, stDataType);

            string lastNameXpath = "recordTarget/patientRole/patient/name/family";
            var greenPatientLastName = CreateGreenConstraint(greenDocTemplate, patientLastName, greenPatientName, "Last", "last", lastNameXpath, stDataType);

            GreenSchemaPackage package = GreenSchemaGenerator.Generate(mockRepo, docTemplate);

            // Is the schema content a valid schema?
            AssertSchemaValid(package.GreenSchemaContent);

            // Load the schema into an XmlDocument so we can assert XPATH against it
            XmlDocument schemaDoc = new XmlDocument();
            schemaDoc.LoadXml(package.GreenSchemaContent);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(schemaDoc.NameTable);
            nsManager.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

            ///////////
            // Test the schema generation results
            ///////////

            // XPATH = /xs:schema/xs:element[@name='myGreenDoc']
            var rootElementDocTemplate = schemaDoc.SelectSingleNode(
                "/xs:schema/xs:element[@name='" + greenDocTemplate.Name + "' and @type='" + greenDocTemplate.Name + "']", nsManager);
            Assert.IsNotNull(rootElementDocTemplate, "Could not find correct root element for document template");

            // XPATH = /xs:schema/xs:complexType[@name='myGreenDoc']
            var rootComplexTypeDocTemplate = schemaDoc.SelectSingleNode(
                "/xs:schema/xs:complexType[@name='" + greenDocTemplate.Name + "']", nsManager);
            Assert.IsNotNull(rootComplexTypeDocTemplate, "Could not find complex type for document template");

            // XPATH = xs:sequence/xs:element[@name='patient' and @minOccurs='1' and @maxOccurs='unbounded']
            var patientNode = rootComplexTypeDocTemplate.SelectSingleNode(
                "xs:sequence/xs:element[@name='patient' and @minOccurs='1' and @maxOccurs='unbounded']", nsManager);
            Assert.IsNotNull(patientNode, "Could not find patient element within myGreenDoc that has min and max occurs set correctly.");

            // XPATH = xs:complexType/xs:sequence/xs:element[@name='gender' and @minOccurs='1' and @maxOccurs='1']
            var patientGenderNode = patientNode.SelectSingleNode(
                "xs:complexType/xs:sequence/xs:element[@name='gender' and @minOccurs='1' and @maxOccurs='1']", nsManager);
            Assert.IsNotNull(patientGenderNode, "Could not find gender element with correct min and max occurs within patient element.");

            // XPATH = xs:complexType/xs:sequence/xs:element[@name='name' and @minOccurs='1' and @maxOccurs='1']
            var patientNameNode = patientNode.SelectSingleNode(
                "xs:complexType/xs:sequence/xs:element[@name='name' and @minOccurs='1' and @maxOccurs='1']", nsManager);
            Assert.IsNotNull(patientGenderNode, "Could not find name element with correct min and max occurs within patient element.");

            // XPATH = xs:complexType/xs:sequence/xs:element[@name='first' and @minOccurs='1' and @maxOccurs='1' and @type='ST']
            var patientFirstNameNode = patientNameNode.SelectSingleNode(
                "xs:complexType/xs:sequence/xs:element[@name='first' and @minOccurs='1' and @maxOccurs='1' and @type='ST']", nsManager);
            Assert.IsNotNull(patientFirstNameNode, "Could not find name element with correct min and max occurs within patient element.");

            // XPATH = xs:complexType/xs:sequence/xs:element[@name='last' and @minOccurs='1' and @maxOccurs='1' and @type='ST']
            var patientLastNameNode = patientNameNode.SelectSingleNode(
                "xs:complexType/xs:sequence/xs:element[@name='last' and @minOccurs='1' and @maxOccurs='1' and @type='ST']", nsManager);
            Assert.IsNotNull(patientLastNameNode, "Could not find name element with correct min and max occurs within patient element.");
        }

        /// <summary>
        ///A test for BuildSchema
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void BuildGreenSchemaTest_Basic()
        {
            var ivlts_dt = this.mockRepo.FindOrAddDataType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IVL_TS");

            Template docTemplate = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4");
            Template baseSectionTemplate = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1");
            Template sectionTemplateReq = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1.1");
            Template sectionTemplateOpt = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1.2");
            Template entryTemplate = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1.3");

            GreenTemplate greenDocTemplate = CreateGreenTemplate(docTemplate, "myGreenDoc");
            string sectionXpath = "component/structuredBody/component/section";
            var greenDoc = CreateGreenConstraint(greenDocTemplate, this.mockRepo.TemplateConstraints.Single(y => y.Number == 39704), null, "My Section", "mySection", sectionXpath);
            CreateGreenConstraint(greenDocTemplate, this.mockRepo.TemplateConstraints.Single(y => y.Number == 98023), null, "MyCode", "myCode", "code", ivlts_dt);

            GreenTemplate greenSectionTemplateReq = CreateGreenTemplate(sectionTemplateReq, "myGreenSection");
            string entryXpath = "entry/observation";
            var greenSection = CreateGreenConstraint(greenSectionTemplateReq, this.mockRepo.TemplateConstraints.Single(y => y.Number == 39712), null, "My Entry", "myEntry", entryXpath);

            GreenTemplate greenEntryTemplate = CreateGreenTemplate(entryTemplate, "myEntry");
            string hemoglobinXpath = "value";
            var cdDataType = this.mockRepo.FindOrAddDataType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "CD");
            var greenEntry = CreateGreenConstraint(greenEntryTemplate, this.mockRepo.TemplateConstraints.Single(y => y.Number == 39714), null, "Hemoglobin", "hemoglobin", hemoglobinXpath, cdDataType);

            GreenSchemaPackage package = GreenSchemaGenerator.Generate(mockRepo, docTemplate);

            // Is the schema content a valid schema?
            AssertSchemaValid(package.GreenSchemaContent);

            // Load the schema into an XmlDocument so we can assert XPATH against it
            XmlDocument schemaDoc = new XmlDocument();
            schemaDoc.LoadXml(package.GreenSchemaContent);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(schemaDoc.NameTable);
            nsManager.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

            ///////////
            // Test the schema generation results
            ///////////

            // XPATH = /xs:schema/xs:element[@name='myGreenDoc']
            var rootElementDocTemplate = schemaDoc.SelectSingleNode(
                "/xs:schema/xs:element[@name='" + greenDocTemplate.Name + "']", nsManager);
            Assert.IsNotNull(rootElementDocTemplate, "Could not find correct root element for document template");

            // XPATH = //xs:complexType[@name='myGreenDoc']
            var docTemplateComplexType = schemaDoc.SelectSingleNode(
                "//xs:complexType[@name='" + greenDocTemplate.Name + "']", nsManager);
            Assert.IsNotNull(docTemplateComplexType, "Could not find complex type for document template");

            // XPATH = xs:sequence/xs:choice/xs:element[@name='myGreenSection' and @type='myGreenSection']
            var docTemplateElement = docTemplateComplexType.SelectSingleNode(
                "xs:sequence/xs:choice/xs:element[@name='" + greenSectionTemplateReq.Name + "' and @type='" + greenSectionTemplateReq.Name + "']", nsManager);
            Assert.IsNotNull(docTemplateElement, "Could not find section element within doc template");

            // XPATH = //xs:complexType[@name='myGreenSection']
            var sectionComplexType = schemaDoc.SelectSingleNode(
                "//xs:complexType[@name='" + greenSectionTemplateReq.Name + "']", nsManager);
            Assert.IsNotNull(sectionComplexType, "Could not find complex type for green section template");

            // XPATH = xs:sequence/xs:element[@name='myEntry' and @type='myEntry']
            var sectionTemplateElement = sectionComplexType.SelectSingleNode(
                "xs:sequence/xs:element[@name='" + greenEntryTemplate.Name + "' and @type='" + greenEntryTemplate.Name + "']", nsManager);
            Assert.IsNotNull(sectionTemplateElement, "Could not find entry element within section template");

            // XPATH = //xs:complexType[@name='myEntry']
            var entryComplexType = schemaDoc.SelectSingleNode(
                "//xs:complexType[@name='" + greenEntryTemplate.Name + "']", nsManager);
            Assert.IsNotNull(entryComplexType, "Could not find complex type for green entry template");

            // XPATH = xs:sequence/xs:element[@name='hemoglobin']
            var hemoglobinElement = entryComplexType.SelectSingleNode(
                "xs:sequence/xs:element[@name='hemoglobin']", nsManager);
            Assert.IsNotNull(hemoglobinElement, "Could not find element in entry for hemoglobin constraint.");
            Assert.IsNotNull(hemoglobinElement.Attributes["type"], "Hemoglobin element does not have a data-type specified.");
            Assert.AreEqual("CD", hemoglobinElement.Attributes["type"].Value, "Hemoglobin element's data type is not 'CD'.");

            // XPATH = //xs:complexType[@name='CD']
            var dataTypeCDComplexType = schemaDoc.SelectSingleNode(
                "//xs:complexType[@name='CD']", nsManager);
            Assert.IsNotNull(dataTypeCDComplexType, "Could not find complex type 'CD' copied from base schema.");

            // XPATH = //xs:complexType[@name='ANY']
            var dataTypeANYComplexType = schemaDoc.SelectSingleNode(
                "//xs:complexType[@name='ANY']", nsManager);
            Assert.IsNotNull(dataTypeANYComplexType, "Could not find complex type 'ANY' (referenced from 'CD') copied from base schema.");
        }

        [DeploymentItem("Schemas\\", "Schemas\\")]
        [TestMethod]
        public void BuildGreenSchemaTest_SeparatedDataTypes()
        {
            Template docTemplate = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4");
            Template baseSectionTemplate = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1");
            Template sectionTemplateReq = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1.1");
            Template sectionTemplateOpt = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1.2");
            Template entryTemplate = this.mockRepo.Templates.Single(y => y.Oid == "1.2.3.4.1.3");

            GreenTemplate greenDocTemplate = CreateGreenTemplate(docTemplate, "myGreenDoc");
            string sectionXpath = "component/structuredBody/component/section";
            var greenDoc = CreateGreenConstraint(greenDocTemplate, this.mockRepo.TemplateConstraints.Single(y => y.Number == 39704), null, "My Section", "mySection", sectionXpath);

            GreenTemplate greenSectionTemplateReq = CreateGreenTemplate(sectionTemplateReq, "myGreenSection");
            string entryXpath = "entry/observation";
            var greenSection = CreateGreenConstraint(greenSectionTemplateReq, this.mockRepo.TemplateConstraints.Single(y => y.Number == 39712), null, "My Entry", "myEntry", entryXpath);

            GreenTemplate greenEntryTemplate = CreateGreenTemplate(entryTemplate, "myEntry");
            string hemoglobinXpath = "value";
            var cdDataType = this.mockRepo.FindOrAddDataType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "CD");
            var greenEntry = CreateGreenConstraint(greenEntryTemplate, this.mockRepo.TemplateConstraints.Single(y => y.Number == 39714), null, "Hemoglobin", "hemoglobin", hemoglobinXpath, cdDataType);

            GreenSchemaPackage package = GreenSchemaGenerator.Generate(mockRepo, docTemplate, true);

            Assert.IsNotNull(package);

            Assert.IsNotNull(package.GreenSchemaContent);
            Assert.AreNotEqual(0, package.GreenSchemaContent.Length);
            Assert.IsNotNull(package.GreenSchemaFileName);
            Assert.AreNotEqual(0, package.GreenSchemaFileName.Length);

            bool foundImport = package.GreenSchemaContent.IndexOf("<xs:import") > 0;
            Assert.IsTrue(foundImport, "Could not find <xs:import> in green schema content");

            Assert.IsNotNull(package.DataTypesContent);
            Assert.AreNotEqual(0, package.DataTypesContent.Length);
            Assert.IsNotNull(package.DataTypesFileName);
            Assert.AreNotEqual(0, package.DataTypesFileName.Length);
        }

        /// <summary>
        /// Fails the test if compiling the schema fails
        /// </summary>
        /// <param name="schemaContent"></param>
        private void AssertSchemaValid(string schemaContent)
        {
            Assert.IsNotNull(schemaContent);
            Assert.AreNotEqual(0, schemaContent.Length);

            using (StringReader sr = new StringReader(schemaContent))
            {
                XmlSchema schema = XmlSchema.Read(sr, null);

                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(schema);

                try
                {
                    schemaSet.Compile();
                }
                catch (Exception ex)
                {
                    Assert.Fail("Schema is not valid: " + ex.Message);
                }
            }
        }
    }
}
