using Trifolia.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Trifolia.Test.Generation.IG
{
    
    
    /// <summary>
    ///This is a test class for CodeSystemsSectionTest and is intended
    ///to contain all CodeSystemsSectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CodeSystemsSectionTest
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
        ///A test for GetSection
        ///</summary>
        [TestMethod()]
        public void GetCodeSystemsSectionTest()
        {
            CodeSystemsSection actual = CodeSystemsSection.GetSection();
            Assert.IsNotNull(actual, "Expected to get a non-null value from GetSection()");
            Assert.IsNotNull(actual.Alternatives, "Expected alternatives collection not to be null.");
            Assert.AreEqual(4, actual.Alternatives.Count, "Expected to find 4 entries in the Alternatives collection.");
            Assert.AreEqual(actual.Alternatives[0].CodeSystemName, "SNOMEDCT", "codeSystemName for first element in collection should have been 'SNOMEDCT'");
            Assert.AreEqual(actual.Alternatives[0].Alternative, "SNOMED-CT", "alternative for first element in collection should have been 'SNOMED-CT'");

            CodeSystemAlternativeElement found = actual.Alternatives["ICD-9"];
            Assert.IsNotNull(found, "Expected to find an alternative for 'ICD-9', but did not.");
        }
    }
}
