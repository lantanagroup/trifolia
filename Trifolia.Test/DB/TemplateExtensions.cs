using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Test;

using Trifolia.Shared;

namespace Trifolia.Test.DB
{
    [TestClass]
    public class TemplateExtensions
    {
        [TestMethod]
        public void TestCloneTemplate()
        {
            MockObjectRepository mockDb = new MockObjectRepository();
            mockDb.InitializeCDARepository();
            var ig = mockDb.FindOrCreateImplementationGuide(Constants.IGTypeNames.CDA, "My Test IG");

            var template1 = mockDb.CreateTemplate("1.2.3.4.1", "Document", "My Test Template", ig);
            template1.AuthorId = 2;

            mockDb.AddConstraintToTemplate(template1, null, null, "recordTarget", "SHALL", "1..1");

            template1.TemplateSamples.Add(new TemplateSample()
            {
                Name = "My Test Sample",
                XmlSample = "<test></test>"
            });

            // Test 1
            var clone1 = template1.CloneTemplate(mockDb, null);

            Assert.IsNotNull(clone1);
            Assert.AreEqual("D_My_Test_Template (Copy)", clone1.Bookmark);
            Assert.AreEqual("My Test Template (Copy)", clone1.Name);
            Assert.AreEqual("1.2.3.4.1.1", clone1.Oid);
            Assert.AreEqual(2, clone1.AuthorId);
            Assert.AreEqual(1, clone1.Constraints.Count);

            // Test the sample for clone1
            Assert.AreEqual(1, clone1.TemplateSamples.Count);
            Assert.AreEqual("My Test Sample", clone1.TemplateSamples.First().Name);
            Assert.AreEqual("<test></test>", clone1.TemplateSamples.First().XmlSample);

            mockDb.Templates.Add(clone1);

            // Test 2
            var clone2 = template1.CloneTemplate(mockDb, 1);

            Assert.IsNotNull(clone2);
            Assert.AreEqual("D_My_Test_Template (Copy 2)", clone2.Bookmark);
            Assert.AreEqual("My Test Template (Copy 2)", clone2.Name);
            Assert.AreEqual("1.2.3.4.1.2", clone2.Oid);
            Assert.AreEqual(1, clone2.AuthorId);
            Assert.AreEqual(1, clone2.Constraints.Count);
        }
    }
}
