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
    }
}
