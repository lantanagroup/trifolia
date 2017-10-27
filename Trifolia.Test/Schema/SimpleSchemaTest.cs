using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Schema;
using System.Collections.Generic;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Test.Schema
{
    
    
    /// <summary>
    ///This is a test class for SimpleSchemaTest and is intended
    ///to contain all SimpleSchemaTest Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class SimpleSchemaTest
    {
        private SimpleSchema eMeasureSchema = null;
        private SimpleSchema cdaSchema = null;

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
        
        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            this.cdaSchema = SimpleSchema.CreateSimpleSchema(Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(
                new ImplementationGuideType()
                    {
                        Name = "CDA",
                        SchemaLocation = "CDA.xsd"
                    }));

            this.eMeasureSchema = SimpleSchema.CreateSimpleSchema(Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(
                new ImplementationGuideType()
                    {
                        Name = "eMeasure",
                        SchemaLocation = "schemas/EMeasure.xsd"
                    }));

            Assert.IsNotNull(this.eMeasureSchema);
            Assert.IsNotNull(this.cdaSchema);
        }
        
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        #endregion

        #region Schema Choice

        /// <summary>
        /// Test that the term "body" is used in the name of the choice element when the common term "body" is found among the choice options.
        /// The cardinality and conformance of children should be set to null/empty, since they cannot change the conf/card of the choice itself.
        /// </summary>
        [TestMethod, TestCategory("Schema")]
        public void TestSchemaChoice_Component2()
        {
            Assert.Inconclusive("Schema Choice is not currently supported for CDA");

            var component2 = this.cdaSchema.FindFromType("Component2");
            Assert.IsNotNull(component2);
            Assert.IsNotNull(component2.Children);
            Assert.AreEqual(7, component2.Children.Count);
            Assert.AreEqual("bodyChoice", component2.Children[6].Name);
            Assert.IsTrue(component2.Children[6].IsChoice);
            Assert.IsNotNull(component2.Children[6].Children);
            Assert.AreEqual(2, component2.Children[6].Children.Count);

            var childOne = component2.Children[6].Children[0];
            Assert.AreEqual("nonXMLBody", childOne.Name);
            Assert.IsNull(childOne.Cardinality);
            Assert.IsNull(childOne.Conformance);

            var childTwo = component2.Children[6].Children[1];
            Assert.AreEqual("structuredBody", childTwo.Name);
            Assert.IsNull(childTwo.Cardinality);
            Assert.IsNull(childTwo.Conformance);
        }

        /// <summary>
        /// Test that "choice" is used as the choice element's name when no common term is found among the choice options
        /// </summary>
        [TestMethod, TestCategory("Schema")]
        public void TestSchemaChoice_CDA_Entry()
        {
            Assert.Inconclusive("Schema Choice is not currently supported for CDA");

            var entry = this.cdaSchema.FindFromType("Entry");
            Assert.IsNotNull(entry);
            Assert.IsNotNull(entry.Children);
            Assert.AreEqual(7, entry.Children.Count);
            Assert.AreEqual("choice", entry.Children[6].Name);
        }

        #endregion

        /// <summary>
        /// Tests that the IVL_TS is properly populated with @nullFlavor, @value, low/@nullFlavor and low/@value
        /// </summary>
        [TestMethod, TestCategory("Schema")]
        public void FindFromTypeTest()
        {
            string type = "IVL_TS";
            SimpleSchema.SchemaObject actual = this.cdaSchema.FindFromType(type);
            Assert.IsTrue(actual.Children.Exists(y => y.Name == "nullFlavor" && y.IsAttribute), "Could not find @nullFlavor attribute on IVL_TS");
            Assert.IsTrue(actual.Children.Exists(y => y.Name == "value" && y.IsAttribute), "Could not find @value attribute on IVL_TS");

            /* TODO: Schema Choice support temporarily removed from non-FHIR schemas
            var choiceSchemaObject = actual.Children.SingleOrDefault(y => y.Name == "choice" && y.IsChoice);
            Assert.IsNotNull(choiceSchemaObject, "Expected to find a choice");

            SimpleSchema.SchemaObject lowSchemaObject = choiceSchemaObject.Children.SingleOrDefault(y => y.Name == "low" && !y.IsAttribute);
            Assert.IsNotNull(lowSchemaObject, "Could not find <low> within IVL_TS");
            Assert.IsTrue(lowSchemaObject.Children.Exists(y => y.Name == "nullFlavor" && y.IsAttribute), "Could now find low/@nullFlavor within IVL_TS");
            Assert.IsTrue(lowSchemaObject.Children.Exists(y => y.Name == "value" && y.IsAttribute), "Could now find low/@value within IVL_TS");
            */

            SimpleSchema.SchemaObject lowSchemaObject = actual.Children.SingleOrDefault(y => y.Name == "low" && !y.IsAttribute);
            Assert.IsNotNull(lowSchemaObject, "Could not find <low> within IVL_TS");
            Assert.IsTrue(lowSchemaObject.Children.Exists(y => y.Name == "nullFlavor" && y.IsAttribute), "Could now find low/@nullFlavor within IVL_TS");
            Assert.IsTrue(lowSchemaObject.Children.Exists(y => y.Name == "value" && y.IsAttribute), "Could now find low/@value within IVL_TS");
        }

        /// <summary>
        /// Tests that using SimpleSchema with the CDA schema returns derived types for the TS and CS types.
        /// </summary>
        [TestMethod, TestCategory("Schema")]
        public void GetDerivedTypesTest()
        {
            List<SimpleSchema.SchemaObject> actual = this.cdaSchema.GetDerivedTypes("TS");
            Assert.IsNotNull(actual, "Expected GetDerivedTypes() to return a list of types, rather than null");
            Assert.IsTrue(actual.Exists(y => y.Name == "IVL_TS"), "Expected IVL_TS to be a derived type for TS");

            actual = this.cdaSchema.GetDerivedTypes("CS");
            Assert.IsNotNull(actual, "Expected GetDerivedTypes() to return a list of types, rather than null");
            Assert.IsTrue(actual.Exists(y => y.Name == "CS"), "Expected CS to be a derived type for CS");
        }

        /// <summary>
        /// Tests that FindFromPath returns a ClinicalDocument, id, and @root attributes when calling FindFromPath.
        /// </summary>
        /// <remarks>
        /// Tests multi-level requests, ie: An id within a ClinicalDocument, a @root within an id within an ClinicalDocument.
        /// </remarks>
        [TestMethod, TestCategory("Schema")]
        public void FindFromPathTest()
        {
            SimpleSchema.SchemaObject documentObj = this.cdaSchema.FindFromPath("ClinicalDocument[ClinicalDocument]");
            Assert.IsNotNull(documentObj, "FindFromPath should have returned a ClinicalDocument schema object.");
            Assert.AreNotEqual(0, documentObj.Children.Count, "Expected the ClinicalDocument schema object to have children.");

            SimpleSchema.SchemaObject idObj = this.cdaSchema.FindFromPath("ClinicalDocument[ClinicalDocument]/id[II]");
            Assert.IsNotNull(idObj, "FindFromPath should have returned an id schema object.");
            Assert.IsFalse(idObj.IsAttribute, "id schema object should not be an attribute.");

            // Should be able to find without specifying the types for each level
            SimpleSchema.SchemaObject rootObj = this.cdaSchema.FindFromPath("ClinicalDocument/id/@root");
            Assert.IsNotNull(rootObj, "FindFromPath should have returned a root schemaobject.");
            Assert.IsTrue(rootObj.IsAttribute, "Expected FindFromPath to return an attribute for @root.");

            // Should be able to find WITH specifying the types for each level
            rootObj = this.cdaSchema.FindFromPath("ClinicalDocument[ClinicalDocument]/id[II]/@root[uid]");
            Assert.IsNotNull(rootObj);
            Assert.IsTrue(rootObj.IsAttribute, "Expected FindFromPath to return an attribute for @root.");

            SimpleSchema.SchemaObject invalidObj = this.cdaSchema.FindFromPath("ClinicalDocument[ClinicalDocument]/test[test]");
            Assert.IsNull(invalidObj, "Expected not to find an element.");
        }

        /// <summary>
        /// Tests that requesting a schema for Observation returns valid results.
        /// </summary>
        /// <remarks>
        /// Tests that the Observation schema returned contains children and that one of the children is an id element.
        /// </remarks>
        [TestMethod, TestCategory("Schema")]
        public void GetSchemaFromContext()
        {
            SimpleSchema observationSchema = this.cdaSchema.GetSchemaFromContext("Observation");

            Assert.IsNotNull(observationSchema, "Expected GetSchemaFromContext to return a non-null value for Observation.");
            Assert.AreNotEqual(0, observationSchema.Children, "Expected the returned schema to have children.");

            List<SimpleSchema.SchemaObject> observationIds = observationSchema.Children.Where(y => y.Name == "id" && y.IsAttribute == false).ToList();
            Assert.AreNotEqual(0, observationIds.Count, "Expected the returned schema to have a child id element.");
            Assert.AreEqual(1, observationIds.Count, "Expected the returned schema to have only one child id element.");
        }

        [TestMethod, TestCategory("Schema")]
        public void TestObservationDataTypes()
        {
            SimpleSchema observationSchema = this.cdaSchema.GetSchemaFromContext("Observation");
            var children = observationSchema.Children;

            Assert.AreEqual(30, children.Count);

            var methodCodeChild = children.Single(y => y.Name == "methodCode" && !y.IsAttribute);
            Assert.AreEqual("CE", methodCodeChild.DataType);

            var targetSiteCodeChild = children.Single(y => y.Name == "targetSiteCode" && !y.IsAttribute);
            Assert.AreEqual("CD", targetSiteCodeChild.DataType);
        }
    }
}
