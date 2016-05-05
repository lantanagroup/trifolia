using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using TemplateDatabase.Shared;
using TemplateDatabase.Green.Transform;

namespace TemplateDatabase.ImplementationGuideGeneration.Test
{
    /// <summary>
    ///This is a test class for NormativeTransformGeneratorTest and is intended
    ///to contain all NormativeTransformGeneratorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NormativeTransformGeneratorTest
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

        /// <summary>
        ///A test for BuildTransform
        ///</summary>
        [TestMethod()]
        public void BuildNormativeTransformTest()
        {
            MockObjectRepository mockRepo = TestDataGeneration.GenerateMockDataset1();
            long implementationGuideId = 1; 
            NormativeTransformGenerator target = new NormativeTransformGenerator(mockRepo, implementationGuideId);
            target.BuildTransform();
            string transform = target.GetTransform();
            Console.WriteLine(transform);
        }
    }
}
