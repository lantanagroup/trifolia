using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Test.Extensions
{
    
    
    /// <summary>
    ///This is a test class for TemplateTest and is intended
    ///to contain all TemplateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TemplateTest
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
        /// Tests that CloneTemplate works in various scenarios.
        /// </summary>
        /// <remarks>
        /// Tests that the template title is correctly copied.
        /// Tests that the oid is correctly copied;
        /// </remarks>
        [TestMethod()]
        public void CloneTemplateTest()
        {
            MockObjectRepository repo = TestDataGenerator.GenerateMockDataset1();
            var user = repo.FindOrAddUser("test.user");
            Template firstTemplate = repo.Templates.First();

            Template secondTemplate = firstTemplate.CloneTemplate(repo, user.Id);
            repo.Templates.AddObject(secondTemplate);
            Assert.AreNotEqual(firstTemplate.Name, secondTemplate.Name, "Title for the first template should be different than the cloned template.");
            Assert.AreEqual(firstTemplate.Oid + ".1", secondTemplate.Oid, "Expected the second template's oid to have .1 added to it.");

            // Test that other properties were copied
            Assert.AreEqual(firstTemplate.ImplementationGuideTypeId, secondTemplate.ImplementationGuideTypeId);
            Assert.AreEqual(firstTemplate.TemplateTypeId, secondTemplate.TemplateTypeId);
            Assert.AreEqual(firstTemplate.OwningImplementationGuideId, secondTemplate.OwningImplementationGuideId);
            Assert.AreEqual(firstTemplate.ImpliedTemplateId, secondTemplate.ImpliedTemplateId);
            Assert.AreEqual(firstTemplate.PrimaryContext, secondTemplate.PrimaryContext);
            Assert.AreEqual(firstTemplate.PrimaryContextType, secondTemplate.PrimaryContextType);
            Assert.AreEqual(firstTemplate.IsOpen, secondTemplate.IsOpen);

            // Test that making more than 10 copies properly generates the title and oid
            Template lastCopiedTemplate = null;
            int count = 1;
            while (count < 11)
            {
                lastCopiedTemplate = firstTemplate.CloneTemplate(repo, user.Id);
                repo.Templates.AddObject(lastCopiedTemplate);
                count++;
            }

            Assert.AreEqual(firstTemplate.Oid + ".12", lastCopiedTemplate.Oid, "Generated oid for copied templates is not working when there are more than 10 copies.");
            Assert.AreEqual(firstTemplate.Name + " (Copy 11)", lastCopiedTemplate.Name, "Genrated title for copied templates is not working when there are more than 10 copies.");
        }

        /// <summary>
        /// Tests that the correct bookmark is generated for a variation of titles and types
        /// </summary>
        [TestMethod()]
        public void GenerateBookmarkTest()
        {
            string actual = Template.GenerateBookmark("This is a test", "TEST");
            Assert.AreEqual("TEST_This_is_a_test", actual);

            actual = Template.GenerateBookmark("This is a super long test of characters exceeding 39", "TE");
            Assert.AreEqual(39, actual.Length, "The length of the generated bookmark is longer than 39 characters.");

            actual = Template.GenerateBookmark("This' has s0me $ weird \"characters\"", "TE");
            Assert.AreEqual("TE_This_has_s0me_weird_characters", actual, "Generated bookmark contains non-standard characters.");

            actual = Template.GenerateBookmark("Lots of     spaces   in this line.", "T");
            Assert.AreEqual("T_Lots_of_spaces_in_this_line", actual, "Multiple spaces where not properly replaced by one underscore.");
        }
    }
}
