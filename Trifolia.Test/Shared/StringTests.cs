using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Shared;

namespace Trifolia.Test.Shared
{
    [TestClass]
    public class StringTests
    {
        [TestMethod]
        public void TestAESEncryption()
        {
            string testString = "Trifolia WB Test with special chars *, @, $ and has some really really long text";
            string encrypted = testString.EncryptStringAES();
            string decrypted = encrypted.DecryptStringAES();

            Assert.AreNotEqual(testString, encrypted);
            Assert.AreEqual(testString, decrypted);
            Assert.IsTrue(encrypted.Length < 255, "Encrypted length is too long for the database");
        }
    }
}
