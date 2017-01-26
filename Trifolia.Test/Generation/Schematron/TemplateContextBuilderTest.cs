using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Export.Schematron;
using Trifolia.DB;

namespace Trifolia.Test.Generation.Schematron
{
    /// <summary>
    /// Summary description for TemplateContextBuilder
    /// </summary>
    [TestClass]
    public class TemplateContextBuilderTest
    {
        private MockObjectRepository tdb = null;
        private ImplementationGuideType cdaIgType = null;

        [TestInitialize]
        public void Setup()
        {
            this.tdb = new MockObjectRepository();
            this.tdb.InitializeCDARepository();

            this.cdaIgType = this.tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
        }

        /// <summary>
        /// Tests that build the rule xpath (context) for a CDA "Document" template, with a plain OID identifier, produces the
        /// correct rule context xpath (specifically that @root is specified).
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForDocument()
        {
            var docTemplateType = tdb.FindOrCreateTemplateType(this.cdaIgType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(this.cdaIgType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(this.cdaIgType);

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
            var docTemplateType = tdb.FindOrCreateTemplateType(this.cdaIgType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(this.cdaIgType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(this.cdaIgType);

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
            var docTemplateType = tdb.FindOrCreateTemplateType(this.cdaIgType, MockObjectRepository.DEFAULT_CDA_DOC_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(this.cdaIgType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(this.cdaIgType);

            Template template = tdb.CreateTemplate("http://test.com/doc/test", docTemplateType, "Test Template", ig, "ClinicalDocument", "ClinicalDocument");
            var contextString = tcb.BuildContextString(template);

            Assert.AreEqual("cda:ClinicalDocument[cda:templateId[@root='http://test.com/doc/test']]", contextString);
        }

        /// <summary>
        /// Tests that building the rule xpath (context) for a CDA template representing an addr[AD] produces the correct results.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TestBuildContextStringForAddress()
        {
            MockObjectRepository tdb = new MockObjectRepository();
            tdb.InitializeCDARepository();

            var igType = tdb.FindImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME);
            var unspecifiedTemplateType = tdb.FindOrCreateTemplateType(igType, MockObjectRepository.DEFAULT_CDA_UNSPECIFIED_TYPE);
            var ig = tdb.FindOrCreateImplementationGuide(igType, "Test IG");
            TemplateContextBuilder tcb = new TemplateContextBuilder(igType);

            Template template = tdb.CreateTemplate("urn:oid:1.2.3.4", unspecifiedTemplateType, "Test Template", ig, "addr", "AD");
            var contextString = tcb.BuildContextString(template);

            Assert.Inconclusive("Need to finish implementing");
            //Assert.AreEqual("cda:addr[cda:templateId[@root='1.2.3.4']]", contextString);
        }
    }
}
