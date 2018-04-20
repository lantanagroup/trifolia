using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Test.Extensions
{
    [TestClass]
    public class ImplementationGuideTest
    {
        private MockObjectRepository tdb;
        private ValueSet vs1;
        private ValueSet vs2;
        private ValueSet vs3;

        [TestInitialize()]
        public void Setup()
        {
            this.tdb = new MockObjectRepository();
            this.tdb.InitializeCDARepository();

            CodeSystem snomed = this.tdb.FindOrCreateCodeSystem("SNOMED-CT", "1.2.3.4");

            this.vs1 = this.tdb.FindOrCreateValueSet("ValueSet 1", "1.2.3");
            this.tdb.FindOrCreateValueSetMember(this.vs1, snomed, "1", "One");
            this.tdb.FindOrCreateValueSetMember(this.vs1, snomed, "2", "Two");
            this.tdb.FindOrCreateValueSetMember(this.vs1, snomed, "3", "Three");

            this.vs2 = this.tdb.FindOrCreateValueSet("ValueSet 2", "1.2.3.4");
            this.tdb.FindOrCreateValueSetMember(this.vs2, snomed, "1", "One");
            this.tdb.FindOrCreateValueSetMember(this.vs2, snomed, "2", "Two");

            this.vs3 = this.tdb.FindOrCreateValueSet("ValueSet 3", "1.2.3.4.5");
            this.vs3.IsIncomplete = true;
            this.vs3.Source = "http://www.lantanagroup.com";
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetValueSetsTest_NoPublishDate()
        {
            ImplementationGuide ig = this.tdb.FindOrCreateImplementationGuide(this.tdb.FindImplementationGuideType(Constants.IGType.CDA_IG_TYPE), "Test IG");
            Template t1 = this.tdb.CreateTemplate("1.2.3.4", "Document", "Test Document Template", ig);
            this.tdb.AddConstraintToTemplate(t1, null, null, "code", "SHALL", "1..1", valueSet: this.vs1);
            Template t2 = this.tdb.CreateTemplate("1.2.3.4", "Document", "Test Document Template", ig);
            var tc2 = this.tdb.AddConstraintToTemplate(t2, null, null, "code", "SHALL", "1..1", valueSet: this.vs2);
            tc2.ValueSetDate = new DateTime(2012, 1, 12);

            DateTime dateNow = DateTime.Now;
            var valueSets = ig.GetValueSets(this.tdb, true);

            Assert.IsNotNull(valueSets);
            Assert.AreEqual(2, valueSets.Count);

            // Cannot get an exact match on DateTime.Now used by GetValueSets(), so using a 1 second range
            var valueSet1 = valueSets[0];
            bool dateMatches = valueSet1.BindingDate > DateTime.Now.AddSeconds(-1) && valueSet1.BindingDate < DateTime.Now.AddSeconds(1);
            Assert.AreEqual(this.vs1, valueSet1.ValueSet);
            Assert.IsTrue(dateMatches);

            var valueSet2 = valueSets[1];
            Assert.AreEqual(this.vs2, valueSet2.ValueSet);
            Assert.AreEqual(new DateTime(2012, 1, 12), valueSet2.BindingDate);
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetValueSetsTest_PublishDate()
        {
            ImplementationGuide ig = this.tdb.FindOrCreateImplementationGuide(this.tdb.FindImplementationGuideType(Constants.IGType.CDA_IG_TYPE), "Test IG");
            ig.PublishDate = new DateTime(2012, 5, 1);

            Template t1 = this.tdb.CreateTemplate("1.2.3.4", "Document", "Test Document Template", ig);
            this.tdb.AddConstraintToTemplate(t1, null, null, "code", "SHALL", "1..1", valueSet: this.vs1);
            Template t2 = this.tdb.CreateTemplate("1.2.3.4", "Document", "Test Document Template", ig);
            var tc2 = this.tdb.AddConstraintToTemplate(t2, null, null, "code", "SHALL", "1..1", valueSet: this.vs2);
            tc2.ValueSetDate = new DateTime(2012, 1, 12);

            DateTime dateNow = DateTime.Now;
            var valueSets = ig.GetValueSets(this.tdb, true);

            Assert.IsNotNull(valueSets);
            Assert.AreEqual(2, valueSets.Count);

            // Cannot get an exact match on DateTime.Now used by GetValueSets(), so using a 1 second range
            var valueSet1 = valueSets[0];
            Assert.AreEqual(this.vs1, valueSet1.ValueSet);
            Assert.AreEqual(ig.PublishDate, valueSet1.BindingDate);

            var valueSet2 = valueSets[1];
            Assert.AreEqual(this.vs2, valueSet2.ValueSet);
            Assert.AreEqual(new DateTime(2012, 1, 12), valueSet2.BindingDate);
        }

        [TestMethod, TestCategory("Terminology")]
        public void GetValueSetsTest_Static()
        {
            ImplementationGuide ig = this.tdb.FindOrCreateImplementationGuide(this.tdb.FindImplementationGuideType(Constants.IGType.CDA_IG_TYPE), "Test IG");
            Template t1 = this.tdb.CreateTemplate("1.2.3.4", "Document", "Test Document Template", ig);
            var tc1 = this.tdb.AddConstraintToTemplate(t1, null, null, "code", "SHALL", "1..1", valueSet: this.vs1);
            tc1.IsStatic = false;

            Template t2 = this.tdb.CreateTemplate("1.2.3.4", "Document", "Test Document Template", ig);
            var tc2 = this.tdb.AddConstraintToTemplate(t2, null, null, "code", "SHALL", "1..1", valueSet: this.vs2);
            tc2.ValueSetDate = new DateTime(2012, 1, 12);
            tc2.IsStatic = true;
            var tc3 = this.tdb.AddConstraintToTemplate(t2, null, null, "value", "SHALL", "1..1", valueSet: this.vs3);

            DateTime dateNow = DateTime.Now;
            var valueSets = ig.GetValueSets(this.tdb, true);

            Assert.IsNotNull(valueSets);
            Assert.AreEqual(2, valueSets.Count);

            var valueSet1 = valueSets[0];
            Assert.AreEqual(this.vs2, valueSet1.ValueSet);
            Assert.AreEqual(new DateTime(2012, 1, 12), valueSet1.BindingDate);

            var valueSet2 = valueSets[1];
            Assert.AreEqual(this.vs3, valueSet2.ValueSet);
        }
    }
}
