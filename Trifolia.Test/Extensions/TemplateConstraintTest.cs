using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Shared.FHIR;

namespace Trifolia.Test.Extensions
{
    [TestClass]
    public class TemplateConstraintTest
    {
        [TestMethod]
        public void GetFHIRElementPathTest()
        {
            TemplateConstraint tc1 = new TemplateConstraint();
            tc1.Context = "extension";

            TemplateConstraint tc2 = new TemplateConstraint();
            tc2.Context = "value[x]";
            tc2.IsChoice = true;
            tc2.ParentConstraint = tc1;

            TemplateConstraint tc3 = new TemplateConstraint();
            tc3.Context = "valueCodeableConcept";
            tc3.ParentConstraint = tc2;

            string path1 = tc1.GetElementPath("Observation");
            string path2 = tc2.GetElementPath("Observation");
            string path3 = tc3.GetElementPath("Observation");

            Assert.AreEqual("Observation.extension", path1);
            Assert.AreEqual("Observation.extension.value[x]", path2);
            Assert.AreEqual("Observation.extension.valueCodeableConcept", path3);
        }

        [TestMethod]
        public void GetSliceNameTest()
        {
            TemplateConstraint tc1 = new TemplateConstraint();
            tc1.Context = "section";
            tc1.IsBranch = true;
            tc1.Order = 1;

            TemplateConstraint tc2 = new TemplateConstraint();
            tc2.Context = "entry";
            tc2.ParentConstraint = tc1;

            string actual = tc1.GetSliceName();
            Assert.AreEqual("section1", actual);

            actual = tc2.GetSliceName();
            Assert.AreEqual("section1", actual);
        }

        [TestMethod]
        public void GetElementIdTest()
        {
            Template t = new Template();
            t.PrimaryContextType = "Composition";

            TemplateConstraint section1 = new TemplateConstraint();
            section1.Template = t;
            section1.Context = "section";
            section1.IsBranch = true;
            section1.Order = 1;

            TemplateConstraint section2 = new TemplateConstraint();
            section2.Template = t;
            section2.Context = "section";
            section2.IsBranch = true;
            section2.Order = 2;

            TemplateConstraint entry1 = new TemplateConstraint();
            entry1.Template = t;
            entry1.Context = "entry";
            entry1.ParentConstraint = section1;
            entry1.Order = 1;

            TemplateConstraint entry2 = new TemplateConstraint();
            entry2.Template = t;
            entry2.Context = "entry";
            entry2.ParentConstraint = section2;
            entry2.Order = 1;

            TemplateConstraint title2 = new TemplateConstraint();
            title2.Template = t;
            title2.Context = "title";
            title2.ParentConstraint = section2;
            title2.Order = 2;

            string actual = section1.GetElementId();
            Assert.AreEqual("Composition.section:section1", actual);

            actual = entry1.GetElementId();
            Assert.AreEqual("Composition.section:section1.entry", actual);

            actual = section2.GetElementId();
            Assert.AreEqual("Composition.section:section2", actual);

            actual = entry2.GetElementId();
            Assert.AreEqual("Composition.section:section2.entry", actual);

            actual = title2.GetElementId();
            Assert.AreEqual("Composition.section:section2.title", actual);
        }
    }
}
