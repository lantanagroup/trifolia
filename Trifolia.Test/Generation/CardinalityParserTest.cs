using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Generation.Schematron;
using Trifolia.Shared;
using Trifolia.DB;

namespace Schematron.Test.Generation
{
    [TestClass]
    public class CardinalityParserTest
    {
        [TestMethod]
        public void TestZeroToMany()
        {
            var c = CardinalityParser.Parse("0..*");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == 0, "Left side is not correct. Expected 0, Actual {0}", c.Left);
            Assert.IsTrue(c.Right == Cardinality.MANY, "Right side is not correct. Expected MANY (*) or {0}, Actual {1}", Cardinality.MANY, c.Right);
            Assert.IsTrue(c.IsZeroToMany(), "Expected IsZeroToMany() to return true instead it returned false");
            Assert.IsFalse(c.IsOneToMany(), "Expected IsOneToMany() to return false instead it returned true");
        }

        [TestMethod]
        public void TestOneToMany()
        {
            var c = CardinalityParser.Parse("1..*");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == 1, "Left side is not correct. Expected 1, Actual {0}", c.Left);
            Assert.IsTrue(c.Right == Cardinality.MANY, "Right side is not correct. Expected MANY (*) or {0}, Actual {1}", Cardinality.MANY, c.Right);
            Assert.IsTrue(c.IsOneToMany(), "Expected IsOneToMany() to return true instead it returned false");
        }

        [TestMethod]
        public void TestOneToOne()
        {
            var c = CardinalityParser.Parse("1..1");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == 1, "Left side is not correct. Expected 1, Actual {0}", c.Left);
            Assert.IsTrue(c.Right == 1, "Right side is not correct. Expected 1, Actual {1}", c.Right);
            Assert.IsTrue(c.IsOneToOne(), "Expected IsOneToOne() to return true instead it returned false");
        }

        [TestMethod]
        public void TestZeroToZero()
        {
            var c = CardinalityParser.Parse("0..0");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == 0, "Left side is not correct. Expected 0, Actual {0}", c.Left);
            Assert.IsTrue(c.Right == 0, "Right side is not correct. Expected 0, Actual {1}", c.Right);
            Assert.IsTrue(c.IsZeroToZero(), "Expected IsZeroToZero() to return true instead it returned false");
        }

        [TestMethod]
        public void TestZeroToOne()
        {
            var c = CardinalityParser.Parse("0..1");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == 0, "Left side is not correct. Expected 0, Actual {0}", c.Left);
            Assert.IsTrue(c.Right == 1, "Right side is not correct. Expected 1, Actual {1}", c.Right);
        }

        [TestMethod]
        public void TestManyToMany()
        {
            var c = CardinalityParser.Parse("*..*");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == Cardinality.MANY, "Left side is not correct. Expected MANY (*) or {0}, Actual {1}", Cardinality.MANY, c.Left);
            Assert.IsTrue(c.Right == Cardinality.MANY, "Right side is not correct. Expected MANY (*) or {0}, Actual {1}", Cardinality.MANY, c.Right);
        }

        [TestMethod]
        public void TestOneToFive()
        {
            var c = CardinalityParser.Parse("1..5");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == 1, "Left side is not correct. Expected 1, Actual {0}", c.Left);
            Assert.IsTrue(c.Right == 5, "Right side is not correct. Expected 5, Actual {1}", c.Right);
        }

        [TestMethod]
        public void TestZeroToNine()
        {
            var c = CardinalityParser.Parse("0..9");
            Assert.IsNotNull(c, "No cardinality instance returned");
            Assert.IsTrue(c.Left == 0, "Left side is not correct. Expected 0, Actual {0}", c.Left);
            Assert.IsTrue(c.Right == 9, "Right side is not correct. Expected 9, Actual {1}", c.Right);
        }

        [TestMethod]
        public void TestCompletelyInvalid()
        {
            try
            {
                var c = CardinalityParser.Parse("adfads");
                Assert.IsTrue(false, "Expected an error to be thrown before this code executed.");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void TestInvalidLeft()
        {
            try
            {
                var c = CardinalityParser.Parse("a..1");
                Assert.IsTrue(false, "Expected an error to be thrown before this code executed.");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void TestInvalidRight()
        {
            try
            {
                var c = CardinalityParser.Parse("1..a");
                Assert.IsTrue(false, "Expected an error to be thrown before this code executed.");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
