using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedHelper = Trifolia.Shared.Helper;
using System.Collections.Generic;

namespace Trifolia.Test.Shared
{
    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public void GetWordsTest()
        {
            string[] words = SharedHelper.GetWords("valueCodeableConcept");
            Assert.AreEqual(3, words.Length);
            Assert.AreEqual("value", words[0]);
            Assert.AreEqual("Codeable", words[1]);
            Assert.AreEqual("Concept", words[2]);
        }

        [TestMethod]
        public void FindCommonWordTest_Found()
        {
            List<string[]> words = new List<string[]>();
            words.Add(SharedHelper.GetWords("valueCodeableConcept"));
            words.Add(SharedHelper.GetWords("valueString"));
            words.Add(SharedHelper.GetWords("valueCoding"));
            words.Add(SharedHelper.GetWords("valueReference"));

            string actual = SharedHelper.FindCommonWord(words);

            Assert.AreEqual("value", actual);
        }

        [TestMethod]
        public void FindCommonWordTest_NotFound()
        {
            List<string[]> words = new List<string[]>();
            words.Add(SharedHelper.GetWords("valueCodeableConcept"));
            words.Add(SharedHelper.GetWords("valueString"));
            words.Add(SharedHelper.GetWords("valueCoding"));
            words.Add(SharedHelper.GetWords("valueReference"));
            words.Add(SharedHelper.GetWords("testBlah"));

            string actual = SharedHelper.FindCommonWord(words);

            Assert.AreEqual(null, actual);
        }
    }
}
