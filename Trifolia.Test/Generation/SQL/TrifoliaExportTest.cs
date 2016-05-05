using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

using Trifolia.Generation.IG;
using Trifolia.Shared;

namespace Trifolia.Test.Generation.SQL
{
    /// <summary>
    /// This is a test class for TrifoliaExportTest and is intended
    /// to contain all TrifoliaExportTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TrifoliaExportTest
    {
        #region Context

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

        #endregion

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
        /// Test that an insert statement is created properly. Tests simple values, such as numeric values, 
        /// and also tests more complicated string values, that may include quotes
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Trifolia.Shared.dll")]
        public void CreateInsertStatementTest()
        {
            string table = "test_table";
            Dictionary<string, string> columnValues = new Dictionary<string, string>()
            {
                { "column1", "value1" },
                { "column2", "value2" },
                { "column3", "\"value3\"" }
            };

            string expected = "INSERT INTO `test_table` (`column1`, `column2`, `column3`) VALUES (value1, value2, \"value3\");";
            string actual = TrifoliaExport.CreateInsertStatement(table, columnValues);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Builds a mock object repository (dataset #2) and exports it using the TrifoliaExport class.
        /// Tests that each of the tables includes an expected insert statement.
        ///</summary>
        [TestMethod()]
        public void ExportTrifoliaTest()
        {
            MockObjectRepository mockRepo = TestDataGenerator.GenerateMockDataset2();

            int implementationGuideId = 1;
            TrifoliaExport target = new TrifoliaExport(mockRepo, implementationGuideId);
            target.BuildExport();
            string expected = string.Empty;
            string actual = target.GetExport();

            bool foundContext = actual.Contains("INSERT INTO `dictionarycontext` (`ID`, `context`, `lastUpdate`) VALUES (1, 'value',");
            bool foundCodeSystem = actual.Contains("INSERT INTO `dictionarycontext` (`ID`, `OID`, `codeSystemName`, `description`, `lastUpdate`) VALUES (1, '6.96', 'SNOMED CT',");
            bool foundValueSet = actual.Contains("INSERT INTO `valueset` (`ID`, `OID`, `valueSetName`, `valueSetCode`, `description`, `intensional`, `intensionalDefinition`, `lastUpdate`) VALUES (1, '11.1', 'GenderCode', ");
            bool foundValueSetMember = actual.Contains("INSERT INTO `valuesetmember` (`ID`, `valueSetId`, `valueSetOId`, `codeSystemId`, `code`, `codeSystemOID`, `displayName`, `dateOfValueSetStatus`, `lastUpdate`) VALUES (3, 1, '11.1', '3', 'U', '1.2.3', 'Undetermined', '1/2/2012 12:00:00 AM',");
            bool foundConformance = actual.Contains("INSERT INTO `conformance_type` (`id`, `conftype`, `lastUpdate`) VALUES (1, 'SHALL',");
            bool foundTemplateType = actual.Contains("INSERT INTO `template_type` (`id`, `templatetype`, `lastUpdate`) VALUES (1, 'Document',");
            bool foundImplementationGuide = actual.Contains("INSERT INTO `implementationguide` (`id`, `title`, `lastUpdate`) VALUES (1, 'The IG',");
            bool foundUser = actual.Contains("INSERT INTO `tdb_users` (id, user_name, user_password) VALUES (1,'defaultUser','ž·Ó');");
            bool foundTemplate = actual.Contains("INSERT INTO `template` (`ID`, `OID`, `isOpen`, `title`, `templatetype`, `description`, `keywords`, `primaryContext`, `impliedTemplate`, `notes`, `lastUpdate`, `uname`) VALUES (1, '1.2.3.4.5.6', 0, 'Test Template 1', 1, 'Test Description 1', NULL, NULL, NULL, 'Test Notes 1',");
            bool foundConstraint = actual.Contains("INSERT INTO `template_constraint` (`ID`, `parentConstraintID`, `templateID`, `order`, `isBranch`, `businessConformance`, `cardinality`, `context`, `containedTemplate`, `isPrimitive`, `constraintNarrative`, `valueConformance`, `staticDynamic`, `valueSetOID`, `version`, `codeOrValue`, `codeSystemOID`, `displayName`, `datatype`, `schematronTest`, `lastUpdate`, `uname`) VALUES (1, NULL, 1, 0, 0, 1, '1..1', 1, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,");
            bool foundIGTemplateAssociation = actual.Contains("INSERT INTO `associationtemplateimplementationguide` (`ID`, `templateId`, `implementationGuideId`, `lastUpdate`) VALUES (1, 1, 1,");

            Assert.IsTrue(foundContext, "Expected to find an insert statement for context 'statusCode'");
            Assert.IsTrue(foundCodeSystem, "Expected to find an insert statement for 'SNOMED CT (6.96)'");
            Assert.IsTrue(foundValueSet, "Expected to find an insert statement for valueset '11.1'");
            Assert.IsTrue(foundValueSetMember, "Expected to find an insert statement for valueset member '3'");
            Assert.IsTrue(foundConformance, "Expected to find an insert statement for conformance 'SHALL'");
            Assert.IsTrue(foundTemplateType, "Expected to find an insert statement for template type 'Document'");
            Assert.IsTrue(foundImplementationGuide, "Expected to find an insert statement for implementation guide 'The IG'");
            Assert.IsTrue(foundUser, "Expected to find an insert statement for user 'defaultUser'");
            Assert.IsTrue(foundTemplate, "Expected to find an insert statement for template 'Test Template 1'");
            Assert.IsTrue(foundIGTemplateAssociation, "Expected to find an insert statement associating ig 'The IG' to template 'Test Template 1'");
        }
    }
}
