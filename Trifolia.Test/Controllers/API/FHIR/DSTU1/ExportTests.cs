using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.DB;
using Trifolia.Export.FHIR.DSTU1;
using Trifolia.Shared;
using System.Xml;

namespace Trifolia.Test.Controllers.API.FHIR.DSTU1
{
    [TestClass]
    [DeploymentItem("Schemas\\", "Schemas\\")]
    public class ExportTests
    {
        private MockObjectRepository tdb;
        private ImplementationGuide ig;

        [TestInitialize]
        public void Setup()
        {
            this.tdb = new MockObjectRepository();
            this.tdb.InitializeFHIRRepository();
            this.tdb.InitializeLCGAndLogin();

            this.ig = this.tdb.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_FHIR_DSTU1_IG_TYPE_NAME, "Test IG");
        }

        [TestMethod, TestCategory("FHIR1")]
        public void TestSingleValueBinding()
        {
            // Setup data for the test
            var cs = this.tdb.FindOrCreateCodeSystem("Test Code System", "http://test.com/codesystem");
            var template1 = this.tdb.GenerateTemplate("http://test.com/composition", "Composition", "Test Composition", this.ig, "Composition", "Composition");
            var template2 = this.tdb.GenerateTemplate("http://test.com/section", "List", "Test Section", this.ig, "List", "List");
            var sectionConstraint = this.tdb.AddConstraintToTemplate(template1, null, null, "section", "SHALL", "1..1", isBranch: true);
            this.tdb.AddConstraintToTemplate(template1, sectionConstraint, null, "code", "SHALL", "1..1", null, null, "1234-x", "Test Code", null, cs, isBranchIdentifier: true);
            this.tdb.AddConstraintToTemplate(template1, sectionConstraint, template2, "content", "SHALL", "1..1");

            // Export the templates
            string export = FHIRExporter.GenerateExport(this.tdb, this.tdb.Templates.ToList(), new IGSettingsManager(this.tdb, this.ig.Id));

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(export);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("fhir", "http://hl7.org/fhir");
            nsManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");

            var profile = doc.SelectSingleNode("//atom:entry/atom:content/fhir:Profile[fhir:identifier/@value = 'http://test.com/composition']", nsManager);
            var section = profile.SelectSingleNode("fhir:structure/fhir:element[fhir:path/@value = 'Composition.section']", nsManager);
            var sectionCode = profile.SelectSingleNode("fhir:structure/fhir:element[fhir:path/@value = 'Composition.section.code']", nsManager);

            AssertXML.XPathExists(section, nsManager, "fhir:name[@value]");
            AssertXML.XPathExists(section, nsManager, "fhir:slicing[fhir:discriminator/@value='code'][fhir:ordered/@value='false'][fhir:rules/@value='open']");

            Assert.IsNotNull(sectionCode);
            AssertXML.XPathExists(sectionCode, nsManager, "fhir:definition/fhir:valueCodeableConcept/fhir:coding[fhir:system/@value = 'http://test.com/codesystem'][fhir:code/@value = '1234-x'][fhir:display/@value = 'Test Code']");
        }
    }
}
