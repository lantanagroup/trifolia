using Trifolia.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Trifolia.Test
{
    
    
    /// <summary>
    ///This is a test class for HL7AuthHelperTest and is intended
    ///to contain all HL7AuthHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HL7AuthHelperTest
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
        ///A test for GetEncrypted
        ///</summary>
        [TestMethod()]
        public void GetEncryptedTest()
        {
            string input = "test123";
            string key = "abcdef";
            string expected = "117666142a70ffe6b12aeed234dc61c216bb7ffd".ToUpper();
            string actual = HL7AuthHelper.GetEncrypted(input, key);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetTimestamp
        ///</summary>
        [TestMethod()]
        public void GetTimestampTest()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            int expected = (int)t.TotalSeconds;
            int actual = HL7AuthHelper.GetTimestamp();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetComplianceUrl
        ///</summary>
        [TestMethod()]
        public void GetComplianceUrlTest()
        {
            string destination = "test.htm";
            string username = "test";
            string description = "this & is a test description";

            int timestamp = HL7AuthHelper.GetTimestamp();
            string requestHash = string.Format("{0}|{1}|{2}|{3}",
                username,
                destination,
                timestamp,
                Trifolia.Config.AppSettings.HL7ApiKey);
            string expected = string.Format(
                "{0}?userid={1}&returnURL={2}&signingURL={2}&signingDescription={3}&requestHash={4}&timestampUTCEpoch={5}&apiKey={6}",
                "http://hl7.amg-hq.net/temp/mike/webservices/compliance_redirect.cfm",
                username,
                destination,
                "this+%26+is+a+test+description",
                HL7AuthHelper.GetEncrypted(requestHash, Trifolia.Config.AppSettings.HL7SharedKey),
                timestamp,
                Trifolia.Config.AppSettings.HL7ApiKey);
            
            string actual = HL7AuthHelper.GetComplianceUrl(destination, username, description, timestamp);
            Assert.AreEqual(expected, actual);
        }
    }
}
