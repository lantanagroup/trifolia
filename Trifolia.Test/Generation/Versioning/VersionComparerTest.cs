using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Generation.Versioning;
using Trifolia.DB;

namespace Trifolia.Test.Generation.Versioning
{
    [TestClass]
    public class VersionComparerTest
    {
        private MockObjectRepository mockRepo;
        private ImplementationGuideType cdaType;
        private TemplateType documentType;
        private ImplementationGuide ig;

        [TestInitialize]
        public void Setup()
        {
            this.mockRepo = new MockObjectRepository();

            cdaType = this.mockRepo.FindOrCreateImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, string.Empty, string.Empty, string.Empty);
            documentType = this.mockRepo.FindOrCreateTemplateType(cdaType, "Document Templates", "ClinicalDocument", "ClinicalDocument", 1);
            ig = this.mockRepo.FindOrAddImplementationGuide(cdaType, "Test");
        }

        [TestMethod]
        public void VersionComparerTest1()
        {
            Template parentTemplate = this.mockRepo.GenerateTemplate("3.2.1.4.3", this.documentType, "Parent Template", this.ig);

            Template aTemplate = this.mockRepo.GenerateTemplate("1.2.3.4", this.documentType, "Test Template", this.ig, description: "Test Description 1");
            aTemplate.IsOpen = false;
            this.mockRepo.AddConstraintToTemplate(aTemplate, null, null, "id", "SHALL", "1..1", number: 1);
            this.mockRepo.AddConstraintToTemplate(aTemplate, null, null, "effectiveTime", "SHALL", "1..1", number: 2);
            this.mockRepo.AddConstraintToTemplate(aTemplate, null, null, "entryRelationship", "SHALL", "1..1", number: 3);

            Template bTemplate = this.mockRepo.GenerateTemplate("4.3.2.1", this.documentType, "New Test Template", this.ig, description: "Test Description 2", impliedTemplate: parentTemplate);
            bTemplate.IsOpen = true;
            this.mockRepo.AddConstraintToTemplate(bTemplate, null, null, "id", "SHALL", "1..*", number: 1);
            this.mockRepo.AddConstraintToTemplate(bTemplate, null, null, "entryRelationship", "SHALL", "1..1", number: 3);
            this.mockRepo.AddConstraintToTemplate(bTemplate, null, null, "author", "SHALL", "1..1", number: 4);

            VersionComparer comparer = VersionComparer.CreateComparer(mockRepo);
            ComparisonResult compared = comparer.Compare(aTemplate, bTemplate);

            // Test template changes
            Assert.AreEqual(5, compared.ChangedFields.Count, "Expected to find 5 template fields changed");

            var changedName = compared.ChangedFields.Single(y => y.Name == "Name");
            Assert.AreEqual(aTemplate.Name, changedName.Old);
            Assert.AreEqual(bTemplate.Name, changedName.New);

            var changedDescription = compared.ChangedFields.Single(y => y.Name == "Description");
            Assert.AreEqual(aTemplate.Description, changedDescription.Old);
            Assert.AreEqual(bTemplate.Description, changedDescription.New);

            var changedOid = compared.ChangedFields.Single(y => y.Name == "Oid");
            Assert.AreEqual(aTemplate.Oid, changedOid.Old);
            Assert.AreEqual(bTemplate.Oid, changedOid.New);

            var changedImpliedTemplate = compared.ChangedFields.Single(y => y.Name == "Implied Template");
            Assert.AreEqual("", changedImpliedTemplate.Old);
            Assert.AreEqual("Parent Template (3.2.1.4.3)", changedImpliedTemplate.New);

            var changedIsOpen = compared.ChangedFields.Single(y => y.Name == "Open/Closed");
            Assert.AreEqual("Closed", changedIsOpen.Old);
            Assert.AreEqual("Open", changedIsOpen.New);

            // Test constraint changes
            Assert.AreEqual(4, compared.ChangedConstraints.Count, "Expected to find 3 changed constraints");

            var removedConstraint = compared.ChangedConstraints.Single(y => y.Type == CompareStatuses.Removed);
            Assert.AreEqual("1-2", removedConstraint.Number);
            Assert.AreEqual(0, removedConstraint.ChangedFields.Count);
            Assert.IsFalse(string.IsNullOrEmpty(removedConstraint.OldNarrative));

            var addedConstraint = compared.ChangedConstraints.Single(y => y.Type == CompareStatuses.Added);
            Assert.AreEqual("1-4", addedConstraint.Number);
            Assert.AreEqual(0, addedConstraint.ChangedFields.Count);
            Assert.IsFalse(string.IsNullOrEmpty(addedConstraint.NewNarrative));

            var changedConstraint = compared.ChangedConstraints.Single(y => y.Type == CompareStatuses.Modified);
            Assert.AreEqual("1-1", changedConstraint.Number);
            Assert.IsFalse(string.IsNullOrEmpty(changedConstraint.NewNarrative));
            Assert.IsFalse(string.IsNullOrEmpty(changedConstraint.OldNarrative));
            Assert.AreNotEqual(changedConstraint.OldNarrative, changedConstraint.NewNarrative);
            Assert.AreEqual(1, changedConstraint.ChangedFields.Count);
            Assert.IsNotNull(changedConstraint.ChangedFields.SingleOrDefault(y => y.Name == "Cardinality"));

            var unchangedConstraint = compared.ChangedConstraints.Single(y => y.Type == CompareStatuses.Unchanged);
            Assert.AreEqual("1-3", unchangedConstraint.Number);
        }
    }
}
