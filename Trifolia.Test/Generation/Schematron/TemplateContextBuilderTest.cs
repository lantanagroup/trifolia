using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Export.Schematron;
using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Test.Generation.Schematron
{
    /// <summary>
    /// Summary description for TemplateContextBuilder
    /// </summary>
    [TestClass]
    public class TemplateContextBuilderTest
    {
        private MockObjectRepository tdb = null;
        private ImplementationGuideType igType = null;
        private SimpleSchema igTypeSchema = null;

        [TestInitialize]
        public void Setup()
        {
            this.tdb = new MockObjectRepository();
            this.tdb.InitializeCDARepository();

            this.igType = this.tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            this.igTypeSchema = this.igType.GetSimpleSchema();
        }

        /// <summary>
        /// Tests that build the rule xpath (context) for a CDA "Document" template, with a plain OID identifier, produces the
        /// correct rule context xpath (specifically that @root is specified).
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForDocument()
        {
            var docTemplateType = tdb.FindOrCreateTemplateType(this.igType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(this.igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(tdb, this.igType, this.igTypeSchema);

            Template template = tdb.CreateTemplate("urn:oid:1.2.3.4", docTemplateType, "Test Template", ig, "ClinicalDocument", "ClinicalDocument");
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='1.2.3.4']]", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template with a versioned identifier (urn:hl7ii:) produces 
        /// a rule context that includes @root and @extension
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForVersionIdentifier()
        {
            var docTemplateType = tdb.FindOrCreateTemplateType(this.igType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(this.igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(tdb, this.igType, this.igTypeSchema);

            Template template = tdb.CreateTemplate("urn:hl7ii:1.2.3.4:1234", docTemplateType, "Test Template", ig, "ClinicalDocument", "ClinicalDocument");
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='1.2.3.4' and @extension='1234']]", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template with an HTTP identifier produces the correct results.
        /// </summary>
        /// <remarks>
        /// Unlikely that HTTP identifier would be used for a CDA template, but possible.
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForHTTPIdentifier()
        {
            var docTemplateType = tdb.FindOrCreateTemplateType(this.igType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(this.igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(tdb, this.igType, this.igTypeSchema);

            Template template = tdb.CreateTemplate("http://test.com/doc/test", docTemplateType, "Test Template", ig, "ClinicalDocument", "ClinicalDocument");
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='http://test.com/doc/test']]", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template representing an addr[AD] produces the correct results.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForAddress_NotContained()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var unspecifiedTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_UNSPECIFIED_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(tdb, igType, igType.GetSimpleSchema());

            Template template = tdb.CreateTemplate("urn:oid:1.2.3.4", unspecifiedTemplateType, "Test Template", ig, "addr", "AD");
            var contextString = tcb.BuildContextString(template);
            
            Assert.AreEqual("cda:addr", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template representing an addr[AD] produces the correct results.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForAddress_ContainedByOne()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var unspecifiedTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_UNSPECIFIED_TYPE);
            var docTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var ig = tdb.FindOrCreateImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(tdb, igType, igType.GetSimpleSchema());

            Template addrTemplate = tdb.CreateTemplate("urn:oid:1.2.3.4", unspecifiedTemplateType, "Test Address Template", ig, "addr", "AD");
            Template containingTemplate = tdb.CreateTemplate("urn:oid:4.3.2.1", docTemplateType, "Test Doc Template", ig, "ClinicalDocument", "ClinicalDocument");
            var c1 = tdb.AddConstraintToTemplate(containingTemplate, null, null, "recordTarget", "SHALL", "1..1");
            var c2 = tdb.AddConstraintToTemplate(containingTemplate, c1, null, "patientRole", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(containingTemplate, c2, addrTemplate, "addr", "SHALL", "1..1");

            var contextString = tcb.BuildContextString(addrTemplate);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='4.3.2.1']]/cda:recordTarget/cda:patientRole/cda:addr", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template representing an addr[AD] produces the correct results.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForAddress_ContainedByMultiple()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var unspecifiedTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_UNSPECIFIED_TYPE);
            var docTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var entryTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_ENTRY_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(tdb, igType, igType.GetSimpleSchema());

            Template addrTemplate = tdb.CreateTemplate("urn:oid:1.2.3.4", unspecifiedTemplateType, "Test Address Template", ig, "addr", "AD");

            Template containingTemplate1 = tdb.CreateTemplate("urn:oid:4.3.2.1", docTemplateType, "Test Doc Template", ig, "ClinicalDocument", "ClinicalDocument");
            var c1_1 = tdb.AddConstraintToTemplate(containingTemplate1, null, null, "recordTarget", "SHALL", "1..1");
            var c1_2 = tdb.AddConstraintToTemplate(containingTemplate1, c1_1, null, "patientRole", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(containingTemplate1, c1_2, addrTemplate, "addr", "SHALL", "1..1");

            Template containingTemplate2 = tdb.CreateTemplate("urn:oid:3.2.1.4", entryTemplateType, "Test Entry Template", ig, "observation", "Observation");
            var c2_1 = tdb.AddConstraintToTemplate(containingTemplate2, null, null, "participant", "SHALL", "1..1");
            var c2_2 = tdb.AddConstraintToTemplate(containingTemplate2, c2_1, null, "participantRole", "SHALL", "1..1");
            tdb.AddConstraintToTemplate(containingTemplate2, c2_2, addrTemplate, "addr", "SHALL", "1..1");

            var contextString = tcb.BuildContextString(addrTemplate);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='4.3.2.1']]/cda:recordTarget/cda:patientRole/cda:addr | cda:observation[cda:templateId[@root='3.2.1.4']]/cda:participant/cda:participantRole/cda:addr", contextString);
        }
    }
}
