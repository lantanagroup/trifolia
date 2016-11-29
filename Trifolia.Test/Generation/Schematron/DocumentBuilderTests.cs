using System;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Generation.Schematron;
using Trifolia.Generation.Schematron.Model;

namespace Schematron.Test.Generation.Schematron
{
    [TestClass]
    public class DocumentBuilderTests
    {
        [TestMethod, TestCategory("Schematron")]
        public void EmptyDocument_NoException()
        {
            SchematronDocument lDocument = new SchematronDocument();
            SchematronDocumentSerializer lBuilder = new SchematronDocumentSerializer();
            string lSerializedModel = lBuilder.SerializeDocument(lDocument);

            Assert.IsFalse(string.IsNullOrEmpty(lSerializedModel),
                           "A valid but empty document (root element only) was not created");

            XmlDocument lSerializedDocument = new XmlDocument();
            lSerializedDocument.LoadXml(lSerializedModel);

            XmlNamespaceManager lManager = new XmlNamespaceManager(lSerializedDocument.NameTable);
            lManager.AddNamespace("sch", "http://www.ascc.net/xml/schematron");

            XmlNode lPhaseNode =
                lSerializedDocument.SelectSingleNode("/schema/sch:phase", lManager);
            Assert.IsNull(lPhaseNode, "Phase node was not located in the document");
        }
        
        [TestMethod, TestCategory("Schematron")]
        public void PartialDocument_PartialSerializedDocumentCreated()
        {
            Phase lMockPhase1 = new Phase();
            lMockPhase1.ID = "error";
            
            SchematronDocument lDocument = new SchematronDocument();
            lDocument.Phases.Add(lMockPhase1);

            SchematronDocumentSerializer lBuilder = new SchematronDocumentSerializer();
            string lSerializedModel = lBuilder.SerializeDocument(lDocument);

            Assert.IsFalse(string.IsNullOrEmpty(lSerializedModel), "A valid partial document was not created");

            XmlDocument lSerializedDocument = new XmlDocument();
            lSerializedDocument.LoadXml(lSerializedModel);

            XmlNamespaceManager lManager = new XmlNamespaceManager(lSerializedDocument.NameTable);
            lManager.AddNamespace("sch", "http://purl.oclc.org/dsdl/schematron");

            XmlNode lPhaseNode =
                lSerializedDocument.SelectSingleNode(string.Format("/sch:schema/sch:phase[@id='{0}']", lMockPhase1.ID),
                                                     lManager);
            Assert.IsNotNull(lPhaseNode, "Phase node was not located in the document");
            XmlNode lActiveNode = lPhaseNode.FirstChild;
            Assert.IsNull(lActiveNode, "Active node was not created properly on Phase node");
        }

        [TestMethod, TestCategory("Schematron")]
        public void ValidSchematronDocument_1Rule_1Assertion_ValidSchematronDocumentEmitted()
        {
            Assertion lMockAssertion = new Assertion();
            lMockAssertion.AssertionMessage = "This test fails";
            lMockAssertion.Test = "count(@code) > 1";

            Rule lRule = new Rule();
            lRule.Assertions.Add(lMockAssertion);
            lRule.Context = "cda:code";

            Pattern lPattern = new Pattern();
            lPattern.ID = "pattern1";
            lPattern.Name = "mock-pattern";
            lPattern.Rules.Add(lRule);

            Phase lMockPhase1 = new Phase();
            lMockPhase1.ID = "error";
            lMockPhase1.ActivePatterns.Add(lPattern);

            SchematronDocument lDocument = new SchematronDocument();
            lDocument.Phases.Add(lMockPhase1);

            SchematronDocumentSerializer lBuilder = new SchematronDocumentSerializer();
            string lSerializedModel = lBuilder.SerializeDocument(lDocument);
            
            Assert.IsFalse(string.IsNullOrEmpty(lSerializedModel), "A valid Schematron document was not created!");

            XmlDocument lSerializedDocument = new XmlDocument();
            lSerializedDocument.LoadXml(lSerializedModel);

            XmlNamespaceManager lManager = new XmlNamespaceManager(lSerializedDocument.NameTable);
            lManager.AddNamespace("sch", "http://purl.oclc.org/dsdl/schematron");

            XmlNode lPhaseNode =
                lSerializedDocument.SelectSingleNode(string.Format("/sch:schema/sch:phase[@id='{0}']", lMockPhase1.ID),
                                                     lManager);
            Assert.IsNotNull(lPhaseNode, "Phase node was not located in the document");

            XmlNode lActiveNode = lPhaseNode.FirstChild;
            Assert.IsNotNull(lActiveNode, "Active node was not created properly on Phase node");
            Assert.IsNotNull(lActiveNode.Attributes, "Active node attributes were not created properly");

            var lExistingAttributes = from XmlAttribute a in lActiveNode.Attributes
                                      where a.Name.Equals("pattern", StringComparison.InvariantCultureIgnoreCase)
                                      select a;

            Assert.IsNotNull(lExistingAttributes, "Active node did not contain an attribute named 'pattern'");
            Assert.IsTrue(lExistingAttributes.Any(), "Active node did not contain an attribute named 'pattern'");
            Assert.AreEqual("pattern1", lActiveNode.Attributes["pattern"].Value, "Pattern attribute on Active was invalid");

            XmlNode lPatternNode =
                lSerializedDocument.SelectSingleNode(string.Format("/sch:schema/sch:pattern[@id='{0}']", lPattern.ID),
                                                     lManager);
            Assert.IsNotNull(lPatternNode, "The desired pattern node was not found");

            XmlNode lRuleNode =
                lSerializedDocument.SelectSingleNode(
                    string.Format("/sch:schema/sch:pattern[@id='{0}']/sch:rule[@context='{1}']", lPattern.ID, lRule.Context),
                    lManager);

            Assert.IsNotNull(lRuleNode, "The rule node did not exist in the pattern node");
        }
    }
}