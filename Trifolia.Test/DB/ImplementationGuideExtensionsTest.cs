using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.DB;

namespace Trifolia.Test.DB
{
    [TestClass]
    public class ImplementationGuideExtensionsTest
    {
        /// <summary>
        /// Each version of the implementation guide makes changes to the one template
        /// </summary>
        [TestMethod]
        public void GetRecursiveTemplates_ThreeVersions()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();
            
            // Version 1
            var implementationGuide1 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V1");
            var template1 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V1", implementationGuide1, "ClinicalDocument", "ClinicalDocument");

            // Version 2
            var implementationGuide2 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V2", previousVersion: implementationGuide1);

            var template2 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V2", implementationGuide2, "ClinicalDocument", "ClinicalDocument", previousVersion: template1);

            // Version 3
            var implementationGuide3 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V3", previousVersion: implementationGuide2);

            var template3 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V3", implementationGuide3, "ClinicalDocument", "ClinicalDocument", previousVersion: template2);

            // Check list of templates for version 1
            var templates = implementationGuide1.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 1 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 1 returned incorrect number of templates");

            // Check list of templates for version 2
            templates = implementationGuide2.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 2 returned incorrect number of templates");

            // Check list of templates for version 3
            templates = implementationGuide3.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 3 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 3 returned incorrect number of templates");
        }

        /// <summary>
        /// Version 2 and 3 of the IG does not have any changes to the original template
        /// </summary>
        [TestMethod]
        public void GetRecursiveTemplates_ThreeVersionsUnchanged()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            // Version 1
            var implementationGuide1 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V1");
            var template1 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V1", implementationGuide1, "ClinicalDocument", "ClinicalDocument");

            // Version 2
            var implementationGuide2 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V2", previousVersion: implementationGuide1);
            // No template changes in version 2

            // Version 3
            var implementationGuide3 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V3", previousVersion: implementationGuide2);
            // No template changes in version 3

            // Check list of templates for version 1
            var templates = implementationGuide1.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 1 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 1 returned incorrect number of templates");

            // Check list of templates for version 2
            templates = implementationGuide2.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 2 returned incorrect number of templates");

            // Check list of templates for version 3
            templates = implementationGuide3.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 3 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 3 returned incorrect number of templates");
        }
        /// <summary>
        /// Second version of IG does not change template, third version does
        /// </summary>
        [TestMethod]
        public void GetRecursiveTemplates_ThreeVersions_V3Changed()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            // Version 1
            var implementationGuide1 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V1");
            var template1 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V1", implementationGuide1, "ClinicalDocument", "ClinicalDocument");

            // Version 2
            var implementationGuide2 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V2", previousVersion: implementationGuide1);

            // Version 3
            var implementationGuide3 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V3", previousVersion: implementationGuide2);
            var template2 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V3", implementationGuide3, "ClinicalDocument", "ClinicalDocument", previousVersion: template1);

            // Check list of templates for version 1
            var templates = implementationGuide1.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 1 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 1 returned incorrect number of templates");

            // Check list of templates for version 2
            templates = implementationGuide2.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 2 returned incorrect number of templates");

            // Check list of templates for version 3
            templates = implementationGuide3.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 3 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 3 returned incorrect number of templates");
        }

        /// <summary>
        /// Second version deprecates the template, but the template still appears in version 3
        /// </summary>
        [TestMethod]
        public void GetRecursiveTemplates_DeprecatedTemplate()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            // Version 1
            var implementationGuide1 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V1");
            var template1 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V1", implementationGuide1, "ClinicalDocument", "ClinicalDocument");

            // Version 2
            var implementationGuide2 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V2", previousVersion: implementationGuide1);

            var template2 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V2", implementationGuide2, "ClinicalDocument", "ClinicalDocument", previousVersion: template1, status: "Deprecated");

            // Version 3
            var implementationGuide3 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V3", previousVersion: implementationGuide2);

            // Check list of templates for version 1
            var templates = implementationGuide1.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 1 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 1 returned incorrect number of templates");

            // Check list of templates for version 2
            templates = implementationGuide2.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 2 returned incorrect number of templates");

            // Check list of templates for version 3
            templates = implementationGuide3.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 3 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 3 returned incorrect number of templates");
        }

        /// <summary>
        /// Second version retires the template, and the template DOES NOT show up in version 3
        /// </summary>
        [TestMethod]
        public void GetRecursiveTemplates_RetiredTemplate()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            // Version 1
            var implementationGuide1 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V1");
            var template1 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V1", implementationGuide1, "ClinicalDocument", "ClinicalDocument");

            // Version 2
            var implementationGuide2 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V2", previousVersion: implementationGuide1);

            var template2 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V2", implementationGuide2, "ClinicalDocument", "ClinicalDocument", previousVersion: template1, status: "Retired");

            // Version 3
            var implementationGuide3 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V3", previousVersion: implementationGuide2);

            // Check list of templates for version 1
            var templates = implementationGuide1.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 1 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 1 returned incorrect number of templates");

            // Check list of templates for version 2
            templates = implementationGuide2.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 2 returned incorrect number of templates");

            // Check list of templates for version 3
            templates = implementationGuide3.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 3 returned null value");
            Assert.AreEqual(0, templates.Count, "Version 3 returned incorrect number of templates");
        }

        /// <summary>
        /// Tests life-cycle of templates with a contained external template
        /// </summary>
        [TestMethod]
        public void GetRecursiveTemplates_ContainedInferred_MultiVersions()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var externalIG = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "External IG");
            var exTemplate1 = tdb.GenerateTemplate("4.3.2.1", "Entry", "Ext Entry Template", externalIG, "observation", "Observation");

            // Version 1, create the template
            var implementationGuide1 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V1");
            var template1 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V1", implementationGuide1, "ClinicalDocument", "ClinicalDocument");
            var tc1 = tdb.AddConstraintToTemplate(template1, null, null, "entryRelationship", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(template1, tc1, exTemplate1, "observation", "SHALL", "1..1");

            // Version 2, deprecate the template, remove contained external template
            var implementationGuide2 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V2", previousVersion: implementationGuide1);
            var template2 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V2", implementationGuide2, "ClinicalDocument", "ClinicalDocument", previousVersion: template1);

            // Version 3, retire the template
            var implementationGuide3 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V3", previousVersion: implementationGuide2);
            var template3 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V3", implementationGuide3, "ClinicalDocument", "ClinicalDocument", previousVersion: template2, status: "Retired");

            // Version 4, retired template should no longer appear
            var implementationGuide4 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V4", previousVersion: implementationGuide3);

            // Check list of templates for version 1
            var templates = implementationGuide1.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 1 returned null value");
            Assert.AreEqual(2, templates.Count, "Version 1 returned incorrect number of templates");

            // Check list of templates for version 2
            templates = implementationGuide2.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 2 returned incorrect number of templates");

            // Check list of templates for version 3
            templates = implementationGuide3.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 3 returned incorrect number of templates");

            // Check list of templates for version 4
            templates = implementationGuide4.GetRecursiveTemplates(tdb);
            Assert.IsNotNull(templates, "Version 4 returned null value");
            Assert.AreEqual(0, templates.Count, "Version 4 returned incorrect number of templates");
        }

        /// <summary>
        /// Tests an external contained template with inferred templates turned off
        /// </summary>
        [TestMethod]
        public void GetRecursiveTemplates_ContainedNotInferred_MultiVersions()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var externalIG = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "External IG");
            var exTemplate1 = tdb.GenerateTemplate("4.3.2.1", "Entry", "Ext Entry Template", externalIG, "observation", "Observation");

            // Version 1, create the template
            var implementationGuide1 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V1");
            var template1 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V1", implementationGuide1, "ClinicalDocument", "ClinicalDocument");
            var tc1 = tdb.AddConstraintToTemplate(template1, null, null, "entryRelationship", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(template1, tc1, exTemplate1, "observation", "SHALL", "1..1");

            // Version 2, deprecate the template, remove contained external template
            var implementationGuide2 = tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "IG V2", previousVersion: implementationGuide1);
            var template2 = tdb.GenerateTemplate("1.2.3.4", "Document", "Doc Template V2", implementationGuide2, "ClinicalDocument", "ClinicalDocument", previousVersion: template1);

            // Check list of templates for version 1
            var templates = implementationGuide1.GetRecursiveTemplates(tdb, inferred: false);
            Assert.IsNotNull(templates, "Version 1 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 1 returned incorrect number of templates");

            // Check list of templates for version 2
            templates = implementationGuide2.GetRecursiveTemplates(tdb, inferred: false);
            Assert.IsNotNull(templates, "Version 2 returned null value");
            Assert.AreEqual(1, templates.Count, "Version 2 returned incorrect number of templates");
        }
    }
}
