using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Export.Schematron;
using Trifolia.Export.Schematron.Model;
using Trifolia.Export.Schematron.ConstraintToDocumentElementMap;
using Trifolia.Shared;
using Trifolia.Shared.ImportExport;
using Trifolia.Shared.ImportExport.Model;
using ImportTemplate = Trifolia.Shared.ImportExport.Model.TrifoliaTemplate;
using Template = Trifolia.DB.Template;
using ValueSet = Trifolia.DB.ValueSet;
using CodeSystem = Trifolia.DB.CodeSystem;
using Trifolia.DB;
using Trifolia.Export.Schematron.Utilities;
using Trifolia.Import.Native;

namespace Trifolia.Test.Generation.Schematron
{
    [TestClass]
    [DeploymentItem("Trifolia.Plugins.dll")]
    public class SchematronGeneratorTest
    {
        private static MockObjectRepository tdb = new MockObjectRepository();
        private static ValueSet vs1;
        private static ImplementationGuide ig1;
        private static ImplementationGuide ig2;
        private static TemplateType docType;
        private static TemplateType secType;
        private static TemplateType entType;
        private static XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
        private static XDocument schDoc;
        private static string schContent;
        private static ImplementationGuideType cdaType;

        #region Test Context

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        #region Setup and Tear Down

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            #region Initial Data

            // IG Type
            cdaType = tdb.FindOrCreateImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "cda.xsd", "cda", "urn:hl7-org:v3");

            // Template Type
            docType = tdb.FindOrCreateTemplateType(cdaType, "document", "ClinicalDocument", "ClinicalDocument", 1);
            secType = tdb.FindOrCreateTemplateType(cdaType, "section", "section", "Section", 2);
            entType = tdb.FindOrCreateTemplateType(cdaType, "entry", "entry", "Entry", 3);

            // Code System
            CodeSystem cs1 = tdb.FindOrCreateCodeSystem("SNOMED CT", "6.36");

            // Value Set
            vs1 = tdb.FindOrCreateValueSet("Test Value Set 1", "urn:oid:1.2.3.4");
            tdb.FindOrCreateValueSetMember(vs1, cs1, "1234", "Test Member 1");
            tdb.FindOrCreateValueSetMember(vs1, cs1, "4321", "Test Member 2");

            #endregion

            // Implementation Guide
            ig1 = tdb.FindOrAddImplementationGuide(cdaType, "Test Implementation Guide");
            ig2 = tdb.FindOrAddImplementationGuide(cdaType, "Test 2 Implementation Guide");

            // Template 1
            Template t1 = tdb.GenerateTemplate("urn:oid:1.2.3.4", docType, "Test Template", owningImplementationGuide: ig1);
            tdb.AddConstraintToTemplate(t1, null, null, "title", "SHALL", "1..1");

            // Template 2
            Template t2 = tdb.GenerateTemplate("urn:oid:1.2.3.4.1", docType, "Test Template", owningImplementationGuide: ig1);
            TemplateConstraint t2_tc1 = tdb.AddConstraintToTemplate(t2, null, null, "title", "SHALL", "1..1");
            t2_tc1.Schematron = "count(cda:title)";

            TemplateConstraint t2_tc2 = tdb.GeneratePrimitive(t2, null, "SHALL", "This is a test primitive");

            tdb.GeneratePrimitive(t2, null, "SHALL", "This is test primitive #2", "count(cda:title) &gt; 0");

            // Template 3
            Template t3 = tdb.GenerateTemplate("urn:oid:1.2.3.4.2", docType, "Test Template", owningImplementationGuide: ig1);
            tdb.AddConstraintToTemplate(t1, null, null, "title", "SHOULD", "1..1");

            // Template 4
            Template t4 = tdb.GenerateTemplate("urn:oid:1.2.3.4.3", docType, "Test Template", owningImplementationGuide: ig1);
            TemplateConstraint t4_p1 = tdb.AddConstraintToTemplate(t4, null, null, "entryRelationship", "SHALL", "1..1", null, null, null, null, null, null, null, true);
            TemplateConstraint t4_p2 = tdb.AddConstraintToTemplate(t4, t4_p1, null, "@typeCode", "SHALL", "1..1", null, null, "DRIV");
            TemplateConstraint t4_p3 = tdb.AddConstraintToTemplate(t4, t4_p1, null, "observation", "SHALL", "1..1", null, null, "DRIV", null, null, null, null, true);

            // Template 5: Test line-feed replacement
            Template t5 = tdb.GenerateTemplate("urn:oid:1.2.3.4.4", docType, "Test Template", owningImplementationGuide: ig1);
            TemplateConstraint t5_p1 = tdb.GeneratePrimitive(t5, null, "SHALL", "Testing line-feed character in custom schematron", "count(cda:test)&#xA; = 1");

            // Template 6: Implied template and contained template
            Template t6 = tdb.GenerateTemplate("urn:oid:1.2.3.1.1.1", docType, "Test Template", owningImplementationGuide: ig2, impliedTemplate: t5);
            TemplateConstraint t6_p1 = tdb.AddConstraintToTemplate(t6, null, null, "code", "SHALL", "1..1");
            TemplateConstraint t6_p2 = tdb.AddConstraintToTemplate(t6, null, null, "entryRelationship", "SHALL", "1..1");
            TemplateConstraint t6_p2_1 = tdb.AddConstraintToTemplate(t6, t6_p2, t4, "observation", "SHALL", "1..1");

            // Template 7: MAY parent with SHALL primitive
            Template t7 = tdb.GenerateTemplate("urn:oid:1.2.3.4.5", docType, "Test Template", owningImplementationGuide: ig1);
            TemplateConstraint t7_p1 = tdb.AddConstraintToTemplate(t7, null, null, "entry", "MAY", "0..1", isBranch: true);
            TemplateConstraint t7_p1_1 = tdb.GeneratePrimitive(t7, t7_p1, "SHALL", "This entry SHALl contain some custom stuff", "count(test) > 1", isPrimitiveSchRooted: true, isInheritable: true);

            // Template 8: Code constraint with compound SHALL/SHOULD (element conformance / value conformance)
            Template t8 = tdb.GenerateTemplate("urn:oid:1.2.3.4.6", entType, "Test Template", owningImplementationGuide: ig1);
            TemplateConstraint t8_p1 = tdb.AddConstraintToTemplate(t8, null, null, "code", "SHALL", "1..1", valueConformance: "SHOULD", valueSet: vs1);


            // Add namespaces to the manager
            nsManager.AddNamespace("sch", "http://purl.oclc.org/dsdl/schematron");

            List<Template> allTemplates = ig1.GetRecursiveTemplates(tdb);
            SchematronGenerator generator = new SchematronGenerator(tdb, ig1, allTemplates, false);

            schContent = generator.Generate();
            Assert.IsNotNull(schContent, "Generate() returned null.");
            Assert.AreNotEqual(string.Empty, schContent, "Expected the generated schematron not to be empty.");

            schDoc = XDocument.Parse(schContent);
        }

        #endregion

        #region Generic Tests

        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_PrimitiveRootedWithinBranch()
        {
            var asserts = schDoc.XPathSelectElements("//sch:pattern[@id='p-urn-oid-1.2.3.4.5-errors']/sch:rule[@id='r-urn-oid-1.2.3.4.5-errors-abstract']/sch:assert", nsManager);
            Assert.AreEqual(1, asserts.Count(), "Expected to find one error assertion for template urn:oid:1.2.3.4.5's rooted custom schematron");
        }

        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_CompoundConformanceNarrative()
        {
            var warningPattern = schDoc.XPathSelectElement("//sch:pattern[@id='p-urn-oid-1.2.3.4.6-warnings']", nsManager);
            Assert.IsNotNull(warningPattern);

            var assert = warningPattern.XPathSelectElement("//sch:assert[@id='a-1-15-v']", nsManager);
            Assert.IsNotNull(assert);

            Assert.AreEqual("SHALL contain exactly one [1..1] code, which SHOULD be selected from ValueSet Test Value Set 1 urn:oid:1.2.3.4 (CONF:1-15).", assert.Value);
        }

        /// <summary>
        /// Tests that when creating a SchematronGenerator with NO inferred templates, the templates that are referenced via an implied 
        /// or contained relationship are not included in the output.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_NotInferred()
        {
            SchematronGenerator generator = new SchematronGenerator(tdb, ig2, ig2.ChildTemplates.ToList(), false);

            string content = generator.Generate();
            Assert.IsNotNull(content, "Generate() returned null.");
            Assert.AreNotEqual(string.Empty, content, "Expected the generated schematron not to be empty.");

            XDocument doc = XDocument.Parse(content);

            IEnumerable<XElement> foundPatterns1 = doc.XPathSelectElements("//sch:pattern[@id='p-oid-1.2.3.4.3-errors']", nsManager);
            Assert.AreEqual(0, foundPatterns1.Count(), "Shouldn't have found contained template from other implementation guide");

            IEnumerable<XElement> foundPatterns2 = doc.XPathSelectElements("//sch:pattern[@id='p-oid-1.2.3.4.4-errors']", nsManager);
            Assert.AreEqual(0, foundPatterns2.Count(), "Shouldn't have found implied template from other implementation guide");

            // Make sure the implying template does not extend the implied template (which shouldn't exist)
            IEnumerable<XElement> foundExtends = doc.XPathSelectElements("//sch:pattern[@id='p-1.2.3.1.1.1-errors']/sch:rule[@abstract='true']/sch:extends[@rule='r-oid-1.2.3.4.4-errors-abstract']", nsManager);
            Assert.AreEqual(0, foundExtends.Count(), "Should not have found an extends definition for the implying template.");
        }

        /// <summary>
        /// Tests that when asking to include inferred templates in the implementation guide, both the implied and 
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_Inferred()
        {
            SchematronGenerator generator = new SchematronGenerator(tdb, ig2, ig2.GetRecursiveTemplates(tdb), false);

            string content = generator.Generate();
            Assert.IsNotNull(content, "Generate() returned null.");
            Assert.AreNotEqual(string.Empty, content, "Expected the generated schematron not to be empty.");

            XDocument doc = XDocument.Parse(content);

            // Make sure we find the implied or contained templates
            IEnumerable<XElement> foundPatterns1 = doc.XPathSelectElements("//sch:pattern[@id='p-urn-oid-1.2.3.4.3-errors']", nsManager);
            Assert.AreEqual(1, foundPatterns1.Count(), "Expected to find contained template from other implementation guide");

            IEnumerable<XElement> foundPatterns2 = doc.XPathSelectElements("//sch:pattern[@id='p-urn-oid-1.2.3.4.4-errors']", nsManager);
            Assert.AreEqual(1, foundPatterns2.Count(), "Expected to find implied template from other implementation guide");

            // Make sure the implying template DOES extend the implied template
            IEnumerable<XElement> foundExtends = doc.XPathSelectElements("//sch:pattern[@id='p-urn-oid-1.2.3.1.1.1-errors']/sch:rule[@abstract='true']/sch:extends[@rule='r-urn-oid-1.2.3.4.4-errors-abstract']", nsManager);
            Assert.AreEqual(1, foundExtends.Count(), "Should have found an extends definition for the implying template.");
        }

        /// <summary>
        /// Ensure that line-feed characters are replaced with spaces
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_LineFeed()
        {
            bool foundLineFeed = schContent.Contains("&#xA;") || schContent.Contains("&amp;#xA;");

            Assert.IsFalse(foundLineFeed, "Should not have found line-feed character in plain-text schematron output.");
        }

        /// <summary>
        /// Tests that the resulting document includes a pattern for the first template.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_HasPattern()
        {
            bool foundPattern = schDoc.XPathSelectElements("//sch:pattern[@id='p-oid-1.2.3.4-errors']", nsManager) != null;
            Assert.IsTrue(foundPattern, "Expected to find the pattern with id 'p-oid-1.2.3.4-errors'.");
        }

        /// <summary>
        /// Tests that the generated schematron document has a phase element for errors
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_HasErrorsPhase()
        {
            bool foundPattern = schDoc.XPathSelectElements("//sch:phase[@id='errors']", nsManager) != null;
            Assert.IsTrue(foundPattern, "Expected to find a phase for errors.");
        }

        /// <summary>
        /// Tests that the generated schematron document has a phase element for warnings
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_HasWarningsPhase()
        {
            bool foundPattern = schDoc.XPathSelectElements("//sch:phase[@id='warnings']", nsManager) != null;
            Assert.IsTrue(foundPattern, "Expected to find a phase for warnings.");
        }

        /// <summary>
        /// Tests that a computable constraint without predefined schematron generates new schematron and includes it in the document.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_Computable_NoSchematron()
        {
            XElement assert = schDoc.XPathSelectElement("//sch:assert[@id='a-1-1']", nsManager);
            Assert.IsNotNull(assert, "Expected to find an assertion for constraint 1.");

            XAttribute test = assert.Attribute("test");
            Assert.IsNotNull(test, "Expected constraint 1 to have a test.");

            Assert.IsTrue(!string.IsNullOrEmpty(test.Value), "Expected assert for constraint 1 to have a test.");
        }

        /// <summary>
        /// Tests that a computable constraint with pre-defined schematron outputs the predefined schematron, rather than generating new schematron.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_Computable_WithSchematron()
        {
            XElement assert = schDoc.XPathSelectElement("//sch:assert[@id='a-1-2-c']", nsManager);
            Assert.IsNotNull(assert, "Expected to find an assertion for constraint 2.");

            XAttribute test = assert.Attribute("test");
            Assert.IsNotNull(test, "Expected constraint 2 to have a test.");

            Assert.AreEqual("count(cda:title)", test.Value, "Expected to find an assertion that had a pre-defined test.");
        }

        /// <summary>
        /// Tests that when a primitive constraint has no pre-defined schematron, it outputs a comment in the resulting schematron document.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_Primitive_NoSchematron()
        {
            var comments = (from node in schDoc.Elements().DescendantNodesAndSelf()
                            where node.NodeType == XmlNodeType.Comment
                            select node as XComment);

            Assert.AreEqual(1, comments.Count(), "Expected to find a comment.");
            XComment comment = comments.First();

            Assert.AreEqual("No schematron defined for primitive constraint 3 on template 2", comment.Value);
        }

        /// <summary>
        /// Tests that when a constraint is primitive and has pre-defined schematron, that the predefined schematron is output, rather than generated new.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_Primitive_WithSchematron()
        {
            XElement assert = schDoc.XPathSelectElement("//sch:assert[@id='a-1-4-c']", nsManager);
            Assert.IsNotNull(assert, "Expected to find an assertion for constraint 4.");

            XAttribute test = assert.Attribute("test");
            Assert.IsNotNull(test, "Expected constraint 4 to have a test.");

            Assert.AreEqual("count(cda:title) &gt; 0", test.Value, "Expected constraint 4 to have a test.");
            Assert.AreEqual("This is test primitive #2 (CONF:1-4).", assert.Value, "Narrative for primitive constraint 4 does not match.");
        }

        /// <summary>
        /// tests that a computable constraint shows up in the correct error pattern.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_Computable_Error()
        {
            XElement assert = schDoc.XPathSelectElement("//sch:assert[@id='a-1-1']", nsManager);
            Assert.IsNotNull(assert, "Expected to find an assertion for constraint 1.");

            XElement pattern = assert.Parent.Parent;
            string patternId = pattern.Attribute("id").Value;

            XElement warningEntry = schDoc.Root.XPathSelectElement("//sch:phase[@id='errors']/sch:active[@pattern='" + patternId + "']", nsManager);
            Assert.IsNotNull(warningEntry, "Expected to find constraint 5 as part of a error pattern.");
        }

        /// <summary>
        /// Tests that a computable constraint shows up in the correct warning pattern.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_Computable_Warning()
        {
            XElement assert = schDoc.XPathSelectElement("//sch:assert[@id='a-1-5']", nsManager);
            Assert.IsNotNull(assert, "Expected to find an assertion for constraint 5.");

            XElement pattern = assert.Parent.Parent;
            string patternId = pattern.Attribute("id").Value;

            XElement warningEntry = schDoc.Root.XPathSelectElement("//sch:phase[@id='warnings']/sch:active[@pattern='" + patternId + "']", nsManager);
            Assert.IsNotNull(warningEntry, "Expected to find constraint 5 as part of a warning pattern.");
        }

        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_SHALLwithSHOULDvalueset()
        {
            //// Template 5: Test "SHALL code SHOULD valueset"
            //Template t5 = tdb.GenerateTemplate("urn:oid:1.2.3.4.4", docType, "Test Template", null, null, null, ig);
            ////these two constraints should generate 4 assertions, 2 errors, 2 warnings
            //TemplateConstraint t5_p1 = tdb.GenerateConstraint(t5, null, null, "code", "SHALL", "1..1", valueConformance: "SHOULD", valueSet: vs1);
            //TemplateConstraint t5_p2 = tdb.GenerateConstraint(t5, null, null, "code", "SHALL", "1..1", valueConformance: "SHOULD", value: "1234-X");
            // Implementation Guide
            var ig = tdb.FindOrAddImplementationGuide(cdaType, "Test Implementation Guide");
            ig.ChildTemplates.Clear();
            // Template 5: Test "SHALL code SHOULD valueset"
            Template t5 = tdb.GenerateTemplate("urn:oid:1.2.3.4.4", docType, "Test Template", ig, null, null, null);
            //these two constraints should generate 4 assertions, 2 errors, 2 warnings
            TemplateConstraint t5_p1 = tdb.AddConstraintToTemplate(t5, null, null, "code", "SHALL", "1..1", valueConformance: "SHOULD", valueSet: vs1);

            // Add namespaces to the manager
            nsManager.AddNamespace("sch", "http://purl.oclc.org/dsdl/schematron");

            SchematronGenerator generator = new SchematronGenerator(tdb, ig, ig.GetRecursiveTemplates(tdb), false);
            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            generator.AddTemplate(t5, errorPhase, warningPhase);
            var schContent = generator.Generate();
            Assert.IsNotNull(schContent, "Generate() returned null.");
            Assert.AreNotEqual(string.Empty, schContent, "Expected the generated schematron not to be empty.");

            //did we generate both error section and warning section?
            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one warning phase.");

            //does the warning section have 1 rule?
            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Assert.AreEqual(1, warningPattern.Rules.Count, "Expected to find one warning rule.");
            //does the warning section's rule have 1 assertion?
            Assert.AreEqual(1, warningPattern.Rules[0].Assertions.Count, "Expected to fine one warning assertion.");

            //does the error section have 1 rule?
            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Assert.AreEqual(1, errorPattern.Rules.Count, "Expected to find one error rule.");
            //does the error section's rule have 1 assertion?
            Assert.AreEqual(1, errorPattern.Rules[0].Assertions.Count, "Expected to fine one error assertion.");

            //counts are correct, check contents
            Assert.AreEqual("count(cda:code)=1", errorPattern.Rules[0].Assertions[0].Test, "Wrong assertion test generated for error pattern.");
            Assert.AreEqual("count(cda:code[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor])=1", warningPattern.Rules[0].Assertions[0].Test, "Wrong assertion test generated for warning pattern.");

            //IEnumerable<XElement> asserts = schDoc.XPathSelectElements("//sch:assert[@id='a-1-9']", nsManager);
            //Assert.AreEqual(2, asserts.Count());      // Expect to find the assertion in both errors and warnings

            //XElement assert1 = asserts.First();
            //XElement pattern1 = assert1.Parent.Parent;

            //// The error should only test the code element
            //Assert.AreEqual("count(cda:code)=1", assert1.Attribute("test").Value);
            //Assert.AreEqual("p-oid-1.2.3.4.4-errors", pattern1.Attribute("name").Value);

            //XElement assert2 = asserts.Last();
            //XElement pattern2 = assert2.Parent.Parent;

            //// The warning should test the code element AND the value
            //Assert.AreEqual("count(cda:code[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='urn:oid:1.2.3.4']/voc:code/@value])=1", assert2.Attribute("test").Value);
            //Assert.AreEqual("p-oid-1.2.3.4.4-warnings", pattern2.Attribute("name").Value);
        }

        [TestMethod, TestCategory("Schematron")]
        public void GenerateTest_SHALLwithSHOULDvalue()
        {
            // Implementation Guide
            var ig = tdb.FindOrAddImplementationGuide(cdaType, "Test Implementation Guide");
            // Template 5: Test "SHALL code SHOULD valueset"
            Template t5 = tdb.GenerateTemplate("urn:oid:1.2.3.4.4", docType, "Test Template", ig, null, null, null);
            //these two constraints should generate 4 assertions, 2 errors, 2 warnings
            TemplateConstraint t5_p2 = tdb.AddConstraintToTemplate(t5, null, null, "code", "SHALL", "1..1", valueConformance: "SHOULD", value: "1234-X");

            // Add namespaces to the manager
            nsManager.AddNamespace("sch", "http://purl.oclc.org/dsdl/schematron");

            SchematronGenerator generator = new SchematronGenerator(tdb, ig, ig.GetRecursiveTemplates(tdb), false);
            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            generator.AddTemplate(t5, errorPhase, warningPhase);
            var schContent = generator.Generate();
            Assert.IsNotNull(schContent, "Generate() returned null.");
            Assert.AreNotEqual(string.Empty, schContent, "Expected the generated schematron not to be empty.");

            //did we generate both error section and warning section?
            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one warning phase.");

            //does the warning section have 1 rule?
            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Assert.AreEqual(1, warningPattern.Rules.Count, "Expected to find one warning rule.");
            //does the warning section's rule have 1 assertion?
            Assert.AreEqual(1, warningPattern.Rules[0].Assertions.Count, "Expected to fine one warning assertion.");

            //does the error section have 1 rule?
            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Assert.AreEqual(1, errorPattern.Rules.Count, "Expected to find one error rule.");
            //does the error section's rule have 1 assertion?
            Assert.AreEqual(1, errorPattern.Rules[0].Assertions.Count, "Expected to fine one error assertion.");

            //counts are correct, check contents
            Assert.AreEqual("count(cda:code)=1", errorPattern.Rules[0].Assertions[0].Test, "Wrong assertion test generated for error pattern.");
            Assert.AreEqual("count(cda:code[@code='1234-X'])=1", warningPattern.Rules[0].Assertions[0].Test, "Wrong assertion test generated for warning pattern.");
        }

        [TestMethod, TestCategory("Schematron")]
        public void SerializeSchematronTest()
        {
            SchematronDocumentSerializer sds = new SchematronDocumentSerializer();
            SchematronDocument sd = new SchematronDocument();
            Phase errors = new Phase();
            sd.Phases.Add(errors);

            Pattern template1 = new Pattern()
            {
                ID = "template1",
                Name = "Template 1"
            };

            Rule template1_rule1 = new Rule()
            {
                Context = "cda:observation[cda:templateId[@root='1.2.3.4']]"
            };

            Assertion template1_rule1_assert1 = new Assertion()
            {
                Id = "1",
                AssertionMessage = "Test Assertion 1",
                Test = "count(.) = 1"
            };

            Assertion template1_rule1_assert2 = new Assertion()
            {
                Id = "2",
                AssertionMessage = "Test Assertion 2",
                Test = "count(.) < 1"
            };

            Assertion template1_rule1_assert3 = new Assertion()
            {
                Comment = "Couldn't generate Test Assertion 3"
            };

            template1_rule1.Assertions.Add(template1_rule1_assert1);
            template1_rule1.Assertions.Add(template1_rule1_assert2);
            template1_rule1.Assertions.Add(template1_rule1_assert3);

            template1.Rules.Add(template1_rule1);

            errors.ActivePatterns.Add(template1);

            string serialized = sds.SerializeDocument(sd);

            bool containsIncorrectAmp = serialized.Contains("&amp;lt;");
            Assert.IsFalse(containsIncorrectAmp, "Document contains &amp;lt; when it should not.");
        }

        #endregion


        #region Branch Context Builder

        [TestMethod, TestCategory("Schematron")]
        public void ShouldWithShallChildSiblingShould_BuildBranchContext_GivenRootElement()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true);
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "templateId", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58", isBranchIdentifier: true);
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            // TODO: Need to have flag for primitive schematron... where does the schematron go? rooted from template or from the parent?
            TemplateConstraint tc8666 = tdb.GeneratePrimitive(template, tc8665, "SHALL", "The data type of Observation/participant/time in a verification SHALL be TS (time stamp)");
            TemplateConstraint tc8825 = tdb.AddConstraintToTemplate(template, tc8662, null, "participantRole", "SHALL", "1..1", isBranchIdentifier: true);

            DocumentTemplateElement element = null;
            DocumentTemplateElementAttribute attribute = null;
            ConstraintToDocumentElementHelper.ParseContextForElementAndAttribute(tc10486, out element, out attribute);
            DocumentTemplateElement parentElement =
                ConstraintToDocumentElementHelper.CreateParentElementForAttribute(tc10486, attribute);

            Assert.AreEqual(parentElement.ElementName, tc10486.ParentConstraint.Context, "Failed to properly parse the constraint to a document element.");
            Assert.AreEqual(attribute.AttributeName, tc10486.Context.Replace("@", ""), "Failed to properly parse the constraint to a document element.");

            TemplateContextBuilder contextBuilder = new TemplateContextBuilder(consolidationIg.ImplementationGuideType);
            string path = contextBuilder.CreateFullBranchedParentContext(template, tc8662);
            string expectedPath = "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]";
            Assert.AreEqual(expectedPath, path, "Invalid path returned from ConstraintToDocumentElementMapper");
        }
        #endregion

        #region Context and Branching Tests

        [TestMethod, TestCategory("Schematron")]
        public void TemplateApplyToNonDefaultContext()
        {
            ImplementationGuide qrdaIg = tdb.FindOrAddImplementationGuide(cdaType, "QRDA");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.234234", entType, "Fulfill", qrdaIg, "sdtc:inFulfillmentOf1", "sdtc:inFulfillmentOf1");
            tdb.AddConstraintToTemplate(template, null, null, "@typeCode", "SHALL", "1..1", value: "FLFS", displayName: "Fulfills");
            var c2 = tdb.AddConstraintToTemplate(template, null, null, "sdtc:templateId", "SHALL", "1..1", isBranch: true);
            tdb.AddConstraintToTemplate(template, c2, null, "@root", "SHALL", "1..1", isBranchIdentifier: true, value: "urn:oid:2.16.234234");
            tdb.AddConstraintToTemplate(template, null, null, "sdtc:actReference", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, qrdaIg, qrdaIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count);
            Assert.AreEqual(1, errorPhase.ActivePatterns[0].Rules.Count);
            Assert.AreEqual("sdtc:inFulfillmentOf1[cda:templateId[@root='2.16.234234']]", errorPhase.ActivePatterns[0].Rules[0].Context);
        }

        /// <summary>
        /// Basic test to ensure that branches get split out into a separate rule. Tests that the number of rules produced for the given
        /// template is as expected and that the rules are of the correct type (warnings vs errors). Tests that the context generated is correct 
        /// (ie: includes the templates' root context + the context of the branch).
        /// The branch includes children SHALL constraints that are marked as identifiers, just the same as the 2.5 deployment's update script should set the identifiers to.
        /// </summary>
        /// <remarks>
        /*
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-warnings">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" />
      <sch:assert id="a-1-9" test="count(cda:participant[@typeCode='VRF'][count(cda:templateId[@root='2.16.840.1.113883.10.20.1.58'])=1][count(cda:participantRole)=1]) &gt; 0">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it</sch:assert>
      <sch:assert id="a-1-14" test="count(cda:code) &lt; 2">SHOULD contain zero or one [0..1] code (CONF:1-14).</sch:assert>
      <sch:assert id="a-1-15" test="not(cda:code) or cda:code[@code]">The code, if present, SHALL contain exactly one [1..1] @code (CONF:1-15).</sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" abstract="true">
      <!--No schematron defined for primitive constraint 16 on template 5-->
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" />
      <sch:assert id="a-1-13-branch-9" test="count(cda:time) &lt; 2">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHOULD contain zero or one [0..1] time (CONF:1-13).</sch:assert>
      <sch:assert id="a-1-16-branch-9" test="not(.)">The data type of Observation/participant/time in a verification SHALL be TS (time stamp)</sch:assert>
    </sch:rule>
  </sch:pattern>
         */
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void ShouldWithShallChild_DefaultIdentifiers()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true);
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem:hl7PT, isBranchIdentifier:true);
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "templateId", "SHALL", "1..1", isBranchIdentifier:true);
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58", isBranchIdentifier:true);
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            TemplateConstraint tc8668 = tdb.AddConstraintToTemplate(template, null, null, "code", "SHOULD", "0..1");
            TemplateConstraint tc8669 = tdb.AddConstraintToTemplate(template, tc8668, null, "@code", "SHALL", "1..1");
            // TODO: Need to have flag for primitive schematron... where does the schematron go? rooted from template or from the parent?
            TemplateConstraint tc8666 = tdb.GeneratePrimitive(template, tc8665, "SHALL", "The data type of Observation/participant/time in a verification SHALL be TS (time stamp)");
            TemplateConstraint tc8825 = tdb.AddConstraintToTemplate(template, tc8662, null, "participantRole", "SHALL", "1..1", isBranchIdentifier: true);

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase, the pattern should contain two rules, one with the template and one with a branch.");

            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Pattern errorPattern = errorPhase.ActivePatterns[0];

            Assert.AreEqual(2, warningPattern.Rules.Count, "Expected to find 2 rules. One with template context and one with branch context");

            //expect 1 to be returned
            Rule templatePatternRule = warningPattern.Rules.Where(p => p.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(2, templatePatternRule.Assertions.Count, "Expected to find 2 assertions."); //test code, test time
            Assert.IsTrue(
                templatePatternRule.Assertions.Exists(y => y.Test.Contains("cda:code")),
                "Should have found an assertion for code");
            Assert.IsTrue(
                templatePatternRule.Assertions.Exists(y => y.Test.Contains("count(cda:participant[@typeCode='VRF'][count(cda:templateId[@root='2.16.840.1.113883.10.20.1.58'])=1][count(cda:participantRole)=1]) > 0")),
                "Should have found an assertion for participant");

            // Check which constraints were included in the assertions for branch
            Rule branchWarningPatternRule = warningPattern.Rules.Where(p => p.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]").First();
            Rule branchPatternErrorRule = errorPattern.Rules.Where(p => p.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]").First();
            Assert.AreEqual(1, branchWarningPatternRule.Assertions.Count, "Expected 1 assertion");
            Assert.AreEqual(1, branchPatternErrorRule.Assertions.Count, "Expected 1 assertion");
            Assert.IsTrue(
                branchPatternErrorRule.Assertions.Exists(y => y.Test.Contains("not(.)")),
                "Should have found an assertion for missing primitive"); //the primitive is a SHALL and has a SHOULD parent so it has to be in the error section
        }

        /// <summary>
        /// Tests that a primitive within the template's branched constraint gets properly rooted when isSchRooted is specified.
        /// </summary>
        /// <remarks>
        /*
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-warnings">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" />
      <sch:assert id="a-1-9" test="count(cda:participant[@typeCode='VRF'][count(cda:templateId[@root='2.16.840.1.113883.10.20.1.58'])=1][count(cda:participantRole)=1]) &gt; 0">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it</sch:assert>
      <sch:assert id="a-1-14" test="count(cda:code) &lt; 2">SHOULD contain zero or one [0..1] code (CONF:1-14).</sch:assert>
      <sch:assert id="a-1-15" test="not(cda:code) or cda:code[@code]">The code, if present, SHALL contain exactly one [1..1] @code (CONF:1-15).</sch:assert>
      <sch:assert id="a-1-16" test="some random test">The data type of Observation/participant/time in a verification SHALL be TS (time stamp) (CONF:1-16).</sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" />
      <sch:assert id="a-1-13-branch-9" test="count(cda:time) &lt; 2">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHOULD contain zero or one [0..1] time (CONF:1-13).</sch:assert>
    </sch:rule>
  </sch:pattern>
         */
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void ShouldWithShallChild_DefaultIdentifiers_PrimitiveIsRooted()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true);
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "templateId", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58", isBranchIdentifier: true);
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            TemplateConstraint tc8668 = tdb.AddConstraintToTemplate(template, null, null, "code", "SHOULD", "0..1");
            TemplateConstraint tc8669 = tdb.AddConstraintToTemplate(template, tc8668, null, "@code", "SHALL", "1..1");
            TemplateConstraint tc8666 = tdb.GeneratePrimitive(template, tc8665, "SHALL", "The data type of Observation/participant/time in a verification SHALL be TS (time stamp)", schematronTest:"some random test", isPrimitiveSchRooted:true);
            TemplateConstraint tc8825 = tdb.AddConstraintToTemplate(template, tc8662, null, "participantRole", "SHALL", "1..1", isBranchIdentifier: true);

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase, the pattern should contain two rules, one with the template and one with a branch.");

            Pattern warningPattern = warningPhase.ActivePatterns[0];

            Assert.AreEqual(2, warningPattern.Rules.Count, "Expected to find 2 rules. One with template context and one with branch context");

            //expect 1 to be returned
            Rule templatePatternRule = warningPattern.Rules.Where(p => p.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(2, templatePatternRule.Assertions.Count, "Expected to find 2 assertions."); // test time, test code
            Assert.IsTrue(
                templatePatternRule.Assertions.Exists(y => y.Test.Contains("cda:code")),
                "Should have found an assertion for code");
            Assert.IsTrue(
                templatePatternRule.Assertions.Exists(y => y.Test.Contains("count(cda:participant[@typeCode='VRF'][count(cda:templateId[@root='2.16.840.1.113883.10.20.1.58'])=1][count(cda:participantRole)=1]) > 0")),
                "Should have found an assertion for participant");

            // Check which constraints were included in the assertions for branch
            Rule branchPatternRule = warningPattern.Rules.Where(p => p.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]").First();
            Assert.IsTrue(branchPatternRule.Assertions.Count == 1, "Expected 1 assertions");
        }

        /// <summary>
        /// Tests that a primitive within the template's branched constraint gets properly included in the branch rule when isSchRooted is not specified.
        /// </summary>
        /// <remarks>
        /*
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-warnings">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" />
      <sch:assert id="a-1-9" test="count(cda:participant[@typeCode='VRF'][count(cda:templateId[@root='2.16.840.1.113883.10.20.1.58'])=1][count(cda:participantRole)=1]) &gt; 0">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it</sch:assert>
      <sch:assert id="a-1-14" test="count(cda:code) &lt; 2">SHOULD contain zero or one [0..1] code (CONF:1-14).</sch:assert>
      <sch:assert id="a-1-15" test="not(cda:code) or cda:code[@code]">The code, if present, SHALL contain exactly one [1..1] @code (CONF:1-15).</sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" />
      <sch:assert id="a-1-13-branch-9" test="count(cda:time) &lt; 2">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHOULD contain zero or one [0..1] time (CONF:1-13).</sch:assert>
      <sch:assert id="a-1-16-branch-9" test="some random test">The data type of Observation/participant/time in a verification SHALL be TS (time stamp) (CONF:1-16).</sch:assert>
    </sch:rule>
  </sch:pattern>
         */
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void ShouldWithShallChild_DefaultIdentifiers_PrimitiveIsNotRooted()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true);
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "templateId", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58", isBranchIdentifier: true);
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            TemplateConstraint tc8668 = tdb.AddConstraintToTemplate(template, null, null, "code", "SHOULD", "0..1");
            TemplateConstraint tc8669 = tdb.AddConstraintToTemplate(template, tc8668, null, "@code", "SHALL", "1..1");
            TemplateConstraint tc8666 = tdb.GeneratePrimitive(template, tc8665, "SHALL", "The data type of Observation/participant/time in a verification SHALL be TS (time stamp)", schematronTest: "some random test", isPrimitiveSchRooted: false);
            TemplateConstraint tc8825 = tdb.AddConstraintToTemplate(template, tc8662, null, "participantRole", "SHALL", "1..1", isBranchIdentifier: true);

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase, the pattern should contain two rules, one with the template and one with a branch.");

            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Pattern errorPattern = errorPhase.ActivePatterns[0];

            Assert.AreEqual(2, warningPattern.Rules.Count, "Expected to find 2 rules. One with template context and one with branch context");

            //expect 1 to be returned
            Rule templatePatternRule = warningPattern.Rules.Where(p => p.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(2, templatePatternRule.Assertions.Count, "Expected to find 2 assertions."); //test code, test time
            Assert.IsTrue(
                templatePatternRule.Assertions.Exists(y => y.Test.Contains("cda:code")),
                "Should have found an assertion for code");
            Assert.IsTrue(
                templatePatternRule.Assertions.Exists(y => y.Test.Contains("count(cda:participant[@typeCode='VRF'][count(cda:templateId[@root='2.16.840.1.113883.10.20.1.58'])=1][count(cda:participantRole)=1]) > 0")),
                "Should have found an assertion for participant");

            // Check which constraints were included in the assertions for branch
            Rule branchPatternRule = errorPattern.Rules.Where(p => p.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:templateId[@root='2.16.840.1.113883.10.20.1.58']][cda:participantRole]").First();
            Assert.IsTrue(branchPatternRule.Assertions.Count == 1, "Expected 1 assertions");
            Assert.IsTrue(
                branchPatternRule.Assertions.Exists(y => y.Test.Contains("some random test")),
                "Should have found an assertion for missing primitive");
        }

        /// <summary>
        /// Data based on the participant element within the Advance Directives Observation template within Consolidation.
        /// Data is assumed to have been cleaned up by an analyst so that the correct branching identifiers are used.
        /// Tests that an error and warning rule have been generated, and that the correct assertions are included within each of those rules.
        /// </summary>
        /// <remarks>
        /*
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-warnings">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" />
      <sch:assert id="a-1-9" test="count(cda:participant[@typeCode='VRF']) &gt; 0">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it</sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" abstract="true">
      <!--No schematron defined for primitive constraint 14 on template 5-->
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" />
      <sch:assert id="a-1-11-branch-9" test="count(cda:templateId)=1">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHALL contain exactly one [1..1] templateId (CONF:1-11).</sch:assert>
      <sch:assert id="a-1-12-branch-9" test="cda:templateId[@root='2.16.840.1.113883.10.20.1.58']">This templateId SHALL contain exactly one [1..1] @root="urn:oid:2.16.840.1.113883.10.20.1.58" (CONF:1-12).</sch:assert>
      <sch:assert id="a-1-13-branch-9" test="count(cda:time) &lt; 2">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHOULD contain zero or one [0..1] time (CONF:1-13).</sch:assert>
      <sch:assert id="a-1-14-branch-9" test="not(.)">The data type of Observation/participant/time in a verification SHALL be TS (time stamp)</sch:assert>
      <sch:assert id="a-1-15-branch-9" test="count(cda:participantRole)=1">SHOULD contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHALL contain exactly one [1..1] participantRole (CONF:1-15).</sch:assert>
    </sch:rule>
  </sch:pattern>
         */
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void ShouldWithShallChild_FixedIdentifiers()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            //1 warning in template
            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true); 
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);
            //1 warning in branch context
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "templateId", "SHALL", "1..1");
            //1 warning branch context
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58");
            //1 warning in branch context
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            //1 error in branch context
            // TODO: Need to have flag for primitive schematron... where does the schematron go? rooted from template or from the parent?
            TemplateConstraint tc8666 = tdb.GeneratePrimitive(template, tc8665, "SHALL", "The data type of Observation/participant/time in a verification SHALL be TS (time stamp)");
            // 1 warning in branch context
            TemplateConstraint tc8825 = tdb.AddConstraintToTemplate(template, tc8662, null, "participantRole", "SHALL", "1..1");

            //expect = 1 warning in template context for the branch, 5 warnings in branch context, 2 errors (1 default rule of '.')

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(2, errorPhase.ActivePatterns[0].Rules.Count, "Expected to find two rules within the error phase rules.");
            Assert.AreEqual("cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]", errorPhase.ActivePatterns[0].Rules[0].Context, "Expected the context to be the template");
            Assert.AreEqual(1, errorPhase.ActivePatterns[0].Rules[0].Assertions.Count, "Expected to find 1 assertion in the template rules");
            Assert.IsTrue(errorPhase.ActivePatterns[0].Rules[0].Assertions[0].Test.Contains("."), "Expected assertion to be blank test '(.)'");
            
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase. This pattern would contain 2 rules. One for template and one for branch.");

            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Pattern errorPattern = errorPhase.ActivePatterns[0];

            Assert.AreEqual(2, warningPattern.Rules.Count, "This pattern would contain 2 rules. One for template and one for branch.");
            Assert.IsTrue(warningPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"),
                "The template context was expected to in the warning pattern rules.");
            Assert.IsTrue(warningPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']"),
                "The branch context was expected to in the warning pattern rules.");
            Rule templateRule = warningPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Rule branchContextWarningRule = warningPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']").First();
            Rule branchContextErrorRule = errorPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']").First();

            Assert.AreEqual(1, templateRule.Assertions.Count, "Expected to find 1 assertion (the one for the branch).");
            Assert.AreEqual(1, branchContextWarningRule.Assertions.Count, "Expected to find 1 assertion.");
            Assert.AreEqual(4, branchContextErrorRule.Assertions.Count, "Expected to find 4 assertions.");
            //typecode should only appear in the template rule assertions
            Assert.IsTrue(
                templateRule.Assertions.Exists(y => y.Test.Contains("@typeCode")),
                "Should not have found an assertion for @typeCode");
            //not type code since this is a isBranchIdentifier
            Assert.IsFalse(
                branchContextWarningRule.Assertions.Exists(y => y.Test.Contains("@typeCode")),
                "Should not have found an assertion for @typeCode");
        }

        /// <summary>
        /// Test a non branch that has a should with a shall child
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void ShouldWithShallChild_NoBranch()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            //1 warning in template
            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "authenticator", "SHOULD", "0..1");
            //1 error in template
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "signatureCode", "SHALL", "1..1");

            //expect = 1 warning, 1 error 

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            //Rule templateRule = errorPhase.ActivePatterns[0].Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
        }
        
        /// <summary>
        /// Data based on the participant element within the Advance Directives Observation template within Consolidation.
        /// Data is assumed to have been cleaned up by an analyst so that the correct branching identifiers are used.
        /// Data is changed so that top-level participant element has a SHALL instead of a SHOULD to verify that errors are generated appropriately, instead of primarily warnings.
        /// Tests that an error and warning rule have been generated, and that the correct assertions are included within each of those rules.
        /// </summary>
        /// <remarks>
        /*
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-errors">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-errors-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-errors" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-errors-abstract" />
      <sch:assert id="a-1-9" test="count(cda:participant[@typeCode='VRF']) &gt; 0">SHALL contain at least one [1..*] participant (CONF:1-9) such that it</sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-errors-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-errors" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-errors-abstract" />
      <sch:assert id="a-1-11-branch-9" test="count(cda:templateId)=1">SHALL contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHALL contain exactly one [1..1] templateId (CONF:1-11).</sch:assert>
      <sch:assert id="a-1-12-branch-9" test="cda:templateId[@root='2.16.840.1.113883.10.20.1.58']">This templateId SHALL contain exactly one [1..1] @root="urn:oid:2.16.840.1.113883.10.20.1.58" (CONF:1-12).</sch:assert>
      <sch:assert id="a-1-15-branch-9" test="count(cda:participantRole)=1">SHALL contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHALL contain exactly one [1..1] participantRole (CONF:1-15).</sch:assert>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-warnings">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" abstract="true">
      <sch:assert test="."></sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" />
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" abstract="true">
      <!--No schematron defined for primitive constraint 14 on template 5-->
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-9-branch-9-warnings-abstract" />
      <sch:assert id="a-1-13-branch-9" test="count(cda:time) &lt; 2">SHALL contain at least one [1..*] participant (CONF:1-9) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10). SHOULD contain zero or one [0..1] time (CONF:1-13).</sch:assert>
      <sch:assert id="a-1-14-branch-9" test="not(.)">The data type of Observation/participant/time in a verification SHALL be TS (time stamp)</sch:assert>
    </sch:rule>
  </sch:pattern>
         */
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void ShallWithShallChild_FixedIdentifiers()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            //1 error in template rules
            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHALL", "1..*", isBranch: true);            
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);
            //1 error in branch rules
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "templateId", "SHALL", "1..1");
            //1 error in branch rules
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58");
            //1 warning in branch rules
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            //1 error in branch rules
            TemplateConstraint tc8666 = tdb.GeneratePrimitive(template, tc8665, "SHALL", "The data type of Observation/participant/time in a verification SHALL be TS (time stamp)");
            //1 error in branch rules
            TemplateConstraint tc8825 = tdb.AddConstraintToTemplate(template, tc8662, null, "participantRole", "SHALL", "1..1");
            //sum = 3 error rules in branch context, 2 warning rules in branch context, 1 error rule in template rules, 1 warning rule in template rules (default '.')

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase.");

            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Assert.IsTrue(errorPattern.Rules.Count == 2);
            Assert.IsTrue(errorPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']"));
            Assert.IsTrue(errorPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule branchErrorRule = errorPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']").First();
            Assert.AreEqual(4, branchErrorRule.Assertions.Count(), "Expected to find 4 errors in the branch error rule.");
            Rule templateErrorRule = errorPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(1, templateErrorRule.Assertions.Count(), "Expected to find 1 error in the branch error rule.");
            
            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Assert.IsTrue(warningPattern.Rules.Count == 2);
            Assert.IsTrue(warningPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Assert.IsTrue(warningPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']"));
            Rule branchWarningRule = warningPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']").First();
            Assert.AreEqual(1, branchWarningRule.Assertions.Count(), "Expected to find 1 warning in the branch warning rule.");
            Rule templatewarningRule = warningPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(1, templatewarningRule.Assertions.Count(), "Expected to find 1 warning in the branch warning rule.");
            Assert.AreEqual(".", templatewarningRule.Assertions[0].Test, "Expected to find default '.' test");
        }

        /// <summary>
        /// Does not test to the depth of other unit tests.
        /// Tests that multiple rules are generated for multiple branch statements and that a rule is created for the core template itself.
        /// </summary>
        /// <remarks>
        /*
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-errors">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-errors-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-errors" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-errors-abstract" />
      <sch:assert id="a-1-9" test="@classCode='OBS'">SHALL contain exactly one [1..1] @classCode="OBS" (CONF:1-9).</sch:assert>
      <sch:assert id="a-1-10" test="@moodCode='EVN'">SHALL contain exactly one [1..1] @moodCode="EVN" (CONF:1-10).</sch:assert>
      <sch:assert id="a-1-11" test="count(cda:id[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='evn']) &gt; 0">SHALL contain at least one [1..*] id="EVN" (CONF:1-11).</sch:assert>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-warnings">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" />
      <sch:assert id="a-1-12" test="count(cda:participant[@typeCode='VRF']) &gt; 0">SHOULD contain at least one [1..*] participant (CONF:1-12) such that it</sch:assert>
      <sch:assert id="a-1-19" test="count(cda:participant[@typeCode='CST'])=1">SHOULD contain exactly one [1..1] participant (CONF:1-19) such that it</sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-12-branch-12-warnings-abstract" abstract="true">
      <!--No schematron defined for primitive constraint 17 on template 5-->
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-12-branch-12-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-12-branch-12-warnings-abstract" />
      <sch:assert id="a-1-14-branch-12" test="count(cda:templateId)=1">SHOULD contain at least one [1..*] participant (CONF:1-12) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-13). SHALL contain exactly one [1..1] templateId (CONF:1-14).</sch:assert>
      <sch:assert id="a-1-15-branch-12" test="cda:templateId[@root='2.16.840.1.113883.10.20.1.58']">This templateId SHALL contain exactly one [1..1] @root="urn:oid:2.16.840.1.113883.10.20.1.58" (CONF:1-15).</sch:assert>
      <sch:assert id="a-1-16-branch-12" test="count(cda:time) &lt; 2">SHOULD contain at least one [1..*] participant (CONF:1-12) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-13). SHOULD contain zero or one [0..1] time (CONF:1-16).</sch:assert>
      <sch:assert id="a-1-17-branch-12" test="not(.)">The data type of Observation/participant/time in a verification SHALL be TS (time stamp)</sch:assert>
      <sch:assert id="a-1-18-branch-12" test="count(cda:participantRole)=1">SHOULD contain at least one [1..*] participant (CONF:1-12) such that it SHALL contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-13). SHALL contain exactly one [1..1] participantRole (CONF:1-18).</sch:assert>
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-19-branch-19-warnings-abstract" abstract="true">
      <!--No schematron defined for primitive constraint 27 on template 5-->
    </sch:rule>
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-19-branch-19-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='CST']">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-19-branch-19-warnings-abstract" />
      <sch:assert id="a-1-21-branch-19" test="count(cda:participantRole)=1">SHOULD contain exactly one [1..1] participant (CONF:1-19) such that it SHALL contain exactly one [1..1] @typeCode="CST" Custodian (CONF:1-20). SHALL contain exactly one [1..1] participantRole (CONF:1-21).</sch:assert>
      <sch:assert id="a-1-22-branch-19" test="cda:participantRole[@classCode='AGNT']">This participantRole SHALL contain exactly one [1..1] @classCode="AGNT" Agent (CodeSystem: RoleClass 2.16.840.1.113883.5.110) (CONF:1-22).</sch:assert>
      <sch:assert id="a-1-23-branch-19" test="cda:participantRole[count(cda:addr) &lt; 2]">This participantRole SHOULD contain zero or one [0..1] addr (CONF:1-23).</sch:assert>
      <sch:assert id="a-1-24-branch-19" test="cda:participantRole[count(cda:telecom) &lt; 2]">This participantRole SHOULD contain zero or one [0..1] telecom (CONF:1-24).</sch:assert>
      <sch:assert id="a-1-25-branch-19" test="cda:participantRole[count(cda:playingEntity)=1]">This participantRole SHALL contain exactly one [1..1] playingEntity (CONF:1-25).</sch:assert>
      <sch:assert id="a-1-26-branch-19" test="cda:participantRole/cda:playingEntity[count(cda:name)=1]">This playingEntity SHALL contain exactly one [1..1] name (CONF:1-26).</sch:assert>
      <sch:assert id="a-1-27-branch-19" test="not(.)">he name of the agent who can provide a copy of the Advance Directive SHALL be recorded in the name element inside the playingEntity element</sch:assert>
    </sch:rule>
  </sch:pattern>
         */
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void TwoShouldsWithShallChildrenAndTopLevelConstraints()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");
            CodeSystem roleClass = tdb.FindOrCreateCodeSystem("RoleClass", "2.16.840.1.113883.5.110");

            // Top-level constraints which don't have parents
            //1 error in template rule
            TemplateConstraint tc8648 = tdb.AddConstraintToTemplate(template, null, null, "@classCode", "SHALL", "1..1", value:"OBS");
            //1 error in template rule
            TemplateConstraint tc8649 = tdb.AddConstraintToTemplate(template, null, null, "@moodCode", "SHALL", "1..1", value:"EVN");
            //1 error in template rule
            TemplateConstraint tc8654 = tdb.AddConstraintToTemplate(template, null, null, "id", "SHALL", "1..*", value:"EVN");
            //3 error rules in template

            // First SHOULD parent
            //1 warning in template rule
            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true);
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);
            //1 warning in branch rule
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, tc8662, null, "templateId", "SHALL", "1..1");
            //1 warning in branch rule
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58");
            //1 warning in branch rule
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            //1 warning in branch rule
            // TODO: Need to have flag for primitive schematron... where does the schematron go? rooted from template or from the parent?
            TemplateConstraint tc8666 = tdb.GeneratePrimitive(template, tc8665, "SHALL", "The data type of Observation/participant/time in a verification SHALL be TS (time stamp)");
            //1 warning in branch rule
            TemplateConstraint tc8825 = tdb.AddConstraintToTemplate(template, tc8662, null, "participantRole", "SHALL", "1..1");
            //5 warning rules in branch, 1 warning in template

            // Second SHOULD parent
            //1 warning in template rule
            TemplateConstraint tc8667 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..1", isBranch: true);
            TemplateConstraint tc8668 = tdb.AddConstraintToTemplate(template, tc8667, null, "@typeCode", "SHALL", "1..1", value: "CST", displayName: "Custodian", isBranchIdentifier: true);
            //1 warning in branch rule
            TemplateConstraint tc8669 = tdb.AddConstraintToTemplate(template, tc8667, null, "participantRole", "SHALL", "1..1");
            //1 warning in branch rule
            TemplateConstraint tc8670 = tdb.AddConstraintToTemplate(template, tc8669, null, "@classCode", "SHALL", "1..1", value: "AGNT", displayName: "Agent", codeSystem: roleClass);
            //1 warning in branch rule
            TemplateConstraint tc8671 = tdb.AddConstraintToTemplate(template, tc8669, null, "addr", "SHOULD", "0..1");
            //1 warning in branch rule
            TemplateConstraint tc8672 = tdb.AddConstraintToTemplate(template, tc8669, null, "telecom", "SHOULD", "0..1");
            //1 warning in branch rule
            TemplateConstraint tc8824 = tdb.AddConstraintToTemplate(template, tc8669, null, "playingEntity", "SHALL", "1..1");
            //1 warning in branch rule
            TemplateConstraint tc8673 = tdb.AddConstraintToTemplate(template, tc8824, null, "name", "SHALL", "1..1");
            //1 warning in branch rule
            TemplateConstraint tc8674 = tdb.GeneratePrimitive(template, tc8673, "SHALL", "he name of the agent who can provide a copy of the Advance Directive SHALL be recorded in the name element inside the playingEntity element");
            //7 warnings in branch, 1 warning in template

            //totals: 2 branchs, 1 template
            //template: 3 errors, 2 warnings
            //branch 1: 1 warnings, 4 errors 
            //branch 2: 2 warnings, 5 errors

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase.");

            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Pattern warningPattern = warningPhase.ActivePatterns[0];

            Assert.AreEqual(3, errorPattern.Rules.Count, "Expected to find three rules in the error pattern.");
            Assert.AreEqual(3, warningPattern.Rules.Count, "Expected to find three fules in the warning pattern (1 template, 2 branch)");

            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templateErrorRules = errorPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(3, templateErrorRules.Assertions.Count(), "Expected to find 3 error assertions in the template context");

            Assert.IsTrue(warningPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']"));
            Rule branchWarningRules = warningPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']").First();
            Assert.AreEqual(1, branchWarningRules.Assertions.Count(), "Expected to find 1 warning assertions in the 1st branch context");

            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']"));
            Rule brancherrorRules = errorPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF']").First();
            Assert.AreEqual(4, brancherrorRules.Assertions.Count(), "Expected to find 4 error assertions in the 1st branch context");
            
            Assert.IsTrue(warningPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='CST']"));
            branchWarningRules = warningPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='CST']").First();
            Assert.AreEqual(2, branchWarningRules.Assertions.Count(), "Expected to find 2 warning assertions in the 2nd branch context");

            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='CST']"));
            brancherrorRules = errorPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='CST']").First();
            Assert.AreEqual(5, brancherrorRules.Assertions.Count(), "Expected to find 5 error assertions in the 2nd branch context");
        }

        /// <summary>
        /// Tests that MAY constraints with child SHOULD/SHALL constraints are respected properly. THe MAY constraint itself should not generate any warning or error,
        /// but the child SHOULD and SHALL constraints should generate a warning/error.
        /// </summary>
        /// <remarks>
        /*
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-errors">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-errors-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-errors" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-errors-abstract" />
      <sch:assert id="a-1-11" test="count(cda:templateId)=1">SHALL contain exactly one [1..1] templateId (CONF:1-11).</sch:assert>
      <sch:assert id="a-1-12" test="cda:templateId[@root='2.16.840.1.113883.10.20.1.58']">This templateId SHALL contain exactly one [1..1] @root="urn:oid:2.16.840.1.113883.10.20.1.58" (CONF:1-12).</sch:assert>
    </sch:rule>
  </sch:pattern>
  <sch:pattern id="p-urn:oid:2.16.840.1.113883.10.20.22.4.48-warnings">
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" abstract="true" />
    <sch:rule id="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings" context="cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]">
      <sch:extends rule="r-oid-2.16.840.1.113883.10.20.22.4.48-warnings-abstract" />
      <sch:assert id="a-1-10" test="cda:participant[@typeCode='VRF']">Such participants SHOULD contain exactly one [1..1] @typeCode="VRF" Verifier (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:1-10).</sch:assert>
      <sch:assert id="a-1-13" test="cda:participant[count(cda:time) &lt; 2]">Such participants SHOULD contain zero or one [0..1] time (CONF:1-13).</sch:assert>
      <sch:assert id="a-1-15" test="cda:code[@code]">This code SHALL contain exactly one [1..1] @code (CONF:1-15).</sch:assert>
    </sch:rule>
  </sch:pattern>
         */
        /// </remarks>
        [TestMethod, TestCategory("Schematron")]
        public void MayWithShouldAndShallChild()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            //0 warning in template rules
            TemplateConstraint tc8662 = tdb.AddConstraintToTemplate(template, null, null, "participant", "MAY", "1..*");
            //1 warning in template rules
            TemplateConstraint tc8663 = tdb.AddConstraintToTemplate(template, tc8662, null, "@typeCode", "SHOULD", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT);
            //1 error in template rules
            TemplateConstraint tc8664 = tdb.AddConstraintToTemplate(template, null, null, "templateId", "SHALL", "1..1");
            //1 error in template rules
            TemplateConstraint tc10486 = tdb.AddConstraintToTemplate(template, tc8664, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.1.58");
            //1 warning in template rules
            TemplateConstraint tc8665 = tdb.AddConstraintToTemplate(template, tc8662, null, "time", "SHOULD", "0..1");
            //0 warning in template rules
            TemplateConstraint tc8666 = tdb.AddConstraintToTemplate(template, null, null, "code", "MAY", "1..1");
            //1 warning in tempalte rules
            TemplateConstraint tc8667 = tdb.AddConstraintToTemplate(template, tc8666, null, "@code", "SHALL", "1..1");
            //sum = 2 error rules in template context, 2 warning rules in template context

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase.");

            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Assert.IsTrue(errorPattern.Rules.Count == 1);
            Assert.IsTrue(errorPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templateErrorRule = errorPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(3, templateErrorRule.Assertions.Count(), "Expected to find 3 errors in the template error rule.");

            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Assert.IsTrue(warningPattern.Rules.Count == 1);
            Assert.IsTrue(warningPattern.Rules.Any(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templatewarningRule = warningPattern.Rules.Where(r => r.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(2, templatewarningRule.Assertions.Count(), "Expected to find 2 warning in the template warning rule.");
        }


        /// <summary>
        /// Testing the situation where we have 
        /// author   (SHALL, 1..*)
        ///   |--  assignedAuthor (SHALL, 1..1)
        ///      |--   id   (SHALL, 1..1, isBranch)
        ///         |--    @root  (SHALL, 1..1, isBranchIdentifier)
        ///   |--  time (SHALL, 1..1)
        ///   
        /// We would expect only a test in the template context for author, assignedAuthor, id, id/@root, time.
        /// We would not expect any tests in the branch context since there are no children.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void BranchWithNonBranchParent_NoChildConstraintsInBranch()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");
            CodeSystem roleClass = tdb.FindOrCreateCodeSystem("RoleClass", "2.16.840.1.113883.5.110");
            
            //1 test in template error
            TemplateConstraint tc5444 = tdb.AddConstraintToTemplate(template, null, null, "author", "SHALL", "1..*");
            //1 test in template error
            TemplateConstraint tc5448 = tdb.AddConstraintToTemplate(template, tc5444, null, "assignedAuthor", "SHALL", "1..1");
            //1 test in template error
            TemplateConstraint tc5449 = tdb.AddConstraintToTemplate(template, tc5448, null, "id", "SHALL", "1..1", isBranch:true);
            TemplateConstraint tc16786 = tdb.AddConstraintToTemplate(template, tc5449, null, "@root", "SHALL", "1..1", isBranchIdentifier:true);
            //1 test in template error
            TemplateConstraint tc5445 = tdb.AddConstraintToTemplate(template, tc5444, null, "time", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase.");

            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Pattern warningPattern = warningPhase.ActivePatterns[0];

            Assert.AreEqual(1, errorPattern.Rules.Count, "Expected to find one rule in the error pattern (1 template)");
            Assert.AreEqual(1, warningPattern.Rules.Count, "Expected to find one rule in the warning pattern (1 template)");

            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templateErrorRules = errorPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(4, templateErrorRules.Assertions.Count(), "Expected to find 4 error assertions in the template context");

            Assert.IsTrue(warningPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templateWarningRules = warningPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(1, templateWarningRules.Assertions.Count(), "Expected to find 1 warning assertions in the template context");
            Assert.AreEqual(templateWarningRules.Assertions[0].Test, ".", "Expected to find default warning (.)");
        }

        /// <summary>
        /// Testing the situation where we have 
        /// author   (SHALL, 1..*)
        ///   |--  assignedAuthor (SHALL, 1..1)
        ///      |--   id   (SHALL, 1..1, isBranch)
        ///         |--    @root  (SHALL, 1..1, isBranchIdentifier)
        ///         |--   time (SHALL, 1..1)
        ///   
        /// We would expect tests in the template context for author, assignedAuthor, id, id/@root, time.
        /// We would expect a test in the branch context for time.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void BranchWithNonBranchParent_ChildConstraintsInBranch()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");
            CodeSystem roleClass = tdb.FindOrCreateCodeSystem("RoleClass", "2.16.840.1.113883.5.110");

            //1 test in template error
            TemplateConstraint tc5444 = tdb.AddConstraintToTemplate(template, null, null, "author", "SHALL", "1..*");
            //1 test in template error
            TemplateConstraint tc5448 = tdb.AddConstraintToTemplate(template, tc5444, null, "assignedAuthor", "SHALL", "1..1");
            //1 test in template error
            TemplateConstraint tc5449 = tdb.AddConstraintToTemplate(template, tc5448, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint tc16786 = tdb.AddConstraintToTemplate(template, tc5449, null, "@root", "SHALL", "1..1", isBranchIdentifier: true);
            //1 test in branch error
            TemplateConstraint tc5445 = tdb.AddConstraintToTemplate(template, tc5449, null, "time", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase.");

            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Pattern warningPattern = warningPhase.ActivePatterns[0];

            Assert.AreEqual(2, errorPattern.Rules.Count, "Expected to find two rules in the error pattern (2 template)");
            Assert.AreEqual(1, warningPattern.Rules.Count, "Expected to find one rule in the warning pattern (1 template)");

            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templateErrorRules = errorPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(3, templateErrorRules.Assertions.Count(), "Expected to find 3 error assertions in the template context");

            Rule branchErrorRules = errorPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:author/cda:assignedAuthor/cda:id[@root]").First();
            Assert.AreEqual(1, branchErrorRules.Assertions.Count(), "Expected to find 1 error assertions in the branch context");

            Assert.IsTrue(warningPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templateWarningRules = warningPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(1, templateWarningRules.Assertions.Count(), "Expected to find 1 warning assertions in the template context");
            Assert.AreEqual(templateWarningRules.Assertions[0].Test, ".", "Expected to find default warning (.)");
        }

        /// <summary>
        /// Testing the situation where we have 
        /// author   (SHALL, 1..*)
        ///   |--  assignedAuthor (SHALL, 1..1)
        ///      |--   id   (SHALL, 1..1, isBranch)
        ///         |--    @root  (SHALL, 1..1, isBranchIdentifier, value: urn:oid:1.2.3.4)
        ///         |--   time (SHALL, 1..1)
        ///         |--   id   (SHALL, 1..1, isBranch)
        ///             |--    @root  (SHALL, 1..1, isBranchIdentifier, value: 2.3.4.5)
        ///             |--   code (SHALL, 1..1)
        ///   
        /// We would expect tests in the template context for author, assignedAuthor, id, id/@root[urn:oid:1.2.3.4], time, id/@root[2.3.4.5]
        /// We would expect a test in the branch 1 context for time.
        /// We would expect a test in the branch 2 context for code.
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void BranchWithinBranch_ChildConstraintsInBothBranches()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");
            CodeSystem roleClass = tdb.FindOrCreateCodeSystem("RoleClass", "2.16.840.1.113883.5.110");

            //1 test in template error
            TemplateConstraint tc5444 = tdb.AddConstraintToTemplate(template, null, null, "author", "SHALL", "1..*");
            //1 test in template error
            TemplateConstraint tc5448 = tdb.AddConstraintToTemplate(template, tc5444, null, "assignedAuthor", "SHALL", "1..1");
            //1 test in template error
            TemplateConstraint tc5449 = tdb.AddConstraintToTemplate(template, tc5448, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint tc16786 = tdb.AddConstraintToTemplate(template, tc5449, null, "@root", "SHALL", "1..1", value: "1.2.3.4", isBranchIdentifier: true);
            //1 test in 1st branch error
            TemplateConstraint tc5445 = tdb.AddConstraintToTemplate(template, tc5449, null, "time", "SHALL", "1..1");
            //1 test in 1st branch error
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(template, tc5449, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint tc2 = tdb.AddConstraintToTemplate(template, tc1, null, "@root", "SHALL", "1..1", value: "2.3.4.5", isBranchIdentifier: true);
            //1 test in 2nd branch error
            TemplateConstraint tc3 = tdb.AddConstraintToTemplate(template, tc1, null, "time", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase.");

            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Pattern warningPattern = warningPhase.ActivePatterns[0];

            Assert.AreEqual(3, errorPattern.Rules.Count, "Expected to find one rule in the error pattern (2 template)");
            Assert.AreEqual(1, warningPattern.Rules.Count, "Expected to find one rule in the warning pattern (1 template)");

            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"));
            Rule templateErrorRules = errorPattern.Rules.Where(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]").First();
            Assert.AreEqual(3, templateErrorRules.Assertions.Count(), "Expected to find 3 error assertions in the template context");

            string branch1Context = "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:author/cda:assignedAuthor/cda:id[@root='1.2.3.4']";
            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == branch1Context), "Can't find rule for branch with context: " + branch1Context);
            Rule branch1ErrorRules = errorPattern.Rules.Where(y => y.Context == branch1Context).First();
            Assert.AreEqual(2, branch1ErrorRules.Assertions.Count(), "Expected to find 2 error assertions in the branch context. The time assertion and the template 2 assertion.");

            string branch2Context = "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:author/cda:assignedAuthor[cda:id[@root='1.2.3.4']]/cda:id[@root='2.3.4.5']";
            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == branch2Context), "Can't find rule for branch with context: " + branch2Context);
            Rule branch2ErrorRules = errorPattern.Rules.Where(y => y.Context == branch2Context).First();
            Assert.AreEqual(1, branch2ErrorRules.Assertions.Count(), "Expected to find 1 error assertions in the branch context");

            string warningRuleContext = "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]";
            Assert.IsTrue(warningPattern.Rules.Any(y => y.Context == warningRuleContext), "Can't find rule for warning with context: " + warningRuleContext);
            Rule templateWarningRules = warningPattern.Rules.Where(y => y.Context == warningRuleContext).First();
            Assert.AreEqual(1, templateWarningRules.Assertions.Count(), "Expected to find 1 warning assertions in the template context");
            Assert.AreEqual(templateWarningRules.Assertions[0].Test, ".", "Expected to find default warning (.)");
        }

        [TestMethod, TestCategory("Schematron")]
        public void BranchWithIdentifier_IdentifierCustomSchematron()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            tdb.FindOrCreateCodeSystem("Test Code System 1", "2.16.840.1.113883.5.6");
            tdb.FindOrCreateCodeSystem("Test Code System 2", "2.16.840.1.113883.5.1001");
            tdb.FindOrCreateCodeSystem("Test Code System 3", "2.16.840.1.113883.5.14");
            tdb.FindOrCreateCodeSystem("Test Code System 3", "2.16.840.1.113883.5.1002");

            Template template = ImportTemplate(tdb, "Trifolia.Test.Generation.Schematron.Data.BranchWithIdentifier_IdentifierCustomSchematron.xml");
            List<Template> templates = new List<Template>();
            templates.Add(template);

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, templates, true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            string output = generator.Generate();
        }

        /// <summary>
        /// Testing the situation where we have 
        /// SHALL contain 1..1 reference (CONF:1-12982) such that it      BRANCH
        ///   SHALL contain 1..1 @typeCode="REFR" refers to (CodeSystem: HL7ActRelationshipType 2.16.840.1.113883.5.1002 STATIC) (CONF:1-12983).      IDENTIFIER
        ///   SHALL contain 1..1 externalDocument (CONF:1-12984).     IDENTIFIER
        ///      This externalDocument SHALL contain 1..1 @classCode (CodeSystem: HL7ActClass 2.16.840.1.113883.5.6) (CONF:1-19534).      IDENTIFIER
        ///      This externalDocument SHALL contain 1..1 id (CONF:1-12985) such that it    BRANCH
        ///         SHALL contain 1..1 @root (CONF:1-12986).      IDENTIFIER
        ///            This ID references the ID of the Quality Measure (CONF:1-12987).       PRIMITIVE
        ///         This externalDocument SHOULD contain 0..1 text (CONF:1-12997).
        ///            This text is the title of the eMeasure (CONF:1-12998). 
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void BranchWithinGrandparentBranch_WithPrimitives()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            tdb.FindOrCreateCodeSystem("Test Code System 1", "2.16.840.1.113883.5.6");
            tdb.FindOrCreateCodeSystem("Test Code System 2", "2.16.840.1.113883.5.1001");
            tdb.FindOrCreateCodeSystem("Test Code System 3", "2.16.840.1.113883.5.14");
            tdb.FindOrCreateCodeSystem("Test Code System 3", "2.16.840.1.113883.5.1002");

            Template template = ImportTemplate(tdb, "Trifolia.Test.Generation.Schematron.Data.BranchWithinGrandparentBranch_WithPrimitives.xml");
            List<Template> templates = new List<Template>();
            templates.Add(template);

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, templates, true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            string generated = generator.Generate();

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one pattern within the error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one pattern within the warning phase.");

            Pattern errorPattern = errorPhase.ActivePatterns[0];
            Pattern warningPattern = warningPhase.ActivePatterns[0];

            Assert.AreEqual(2, errorPattern.Rules.Count, "Expected to find two error rules for the template.");
            Assert.AreEqual(3, errorPattern.Rules[0].Assertions.Count, "Expected to find three assertions in the template's error rule.");
            Assert.AreEqual(2, warningPattern.Rules.Count, "Expected to find two warning rules for the template. One for the template itself and one for the branch");
            Assert.AreEqual(1, warningPattern.Rules[0].Assertions.Count, "Expected to find one warning assertion.");

            bool foundFirstBranch = errorPattern.Rules[0].Assertions.Exists(y => y.Test == "count(cda:reference[@typeCode='REFR'][count(cda:externalDocument[@classCode])=1])=1");
            bool foundSecondBranch = errorPattern.Rules[0].Assertions.Exists(y => y.Test == "cda:reference[@typeCode='REFR'][cda:externalDocument[@classCode]]/cda:externalDocument/cda:id[@root]");
            bool foundFirstPrimitive = errorPattern.Rules[0].Assertions.Exists(y => y.Test == "not(testable)");

            Assert.IsTrue(foundFirstBranch, "Did not find the correct test for the first branch.");
            Assert.IsTrue(foundFirstBranch, "Did not find the correct test for the second branch.");

            Rule branchWarningRule = warningPattern.Rules.SingleOrDefault(y => y.Context == "cda:organizer[cda:templateId[@root='2.16.840.1.113883.10.20.24.3.98_xml']]/cda:reference[@typeCode='REFR'][cda:externalDocument[@classCode]]");
            Assert.IsNotNull(branchWarningRule, "The context for the warning branch is not correct.");
        }

        #endregion

        #region Assertions within Branches

        [TestMethod, TestCategory("Schematron")]
        public void AssertionWithinBranch_TestAssertionWithParentAsIdentifier()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            //1 warning in template context
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true);
            TemplateConstraint tc2 = tdb.AddConstraintToTemplate(template, tc1, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);            
            TemplateConstraint tc2_1 = tdb.AddConstraintToTemplate(template, tc1, null, "participantRole", "SHALL", "1..1", isBranchIdentifier: true);
            //1 warning in branch context
            TemplateConstraint tc2_1_1 = tdb.AddConstraintToTemplate(template, tc2_1, null, "code", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            string output = generator.Generate();

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one warning phase.");

            Pattern warningPattern = warningPhase.ActivePatterns[0];
            Pattern errorPattern = errorPhase.ActivePatterns[0];

            Assert.IsNotNull(errorPattern, "Expected error pattern to contain an object.");

            Assert.AreEqual(1, warningPattern.Rules.Count, "Expected to find one warning rule");
            Assert.AreEqual(2, errorPattern.Rules.Count, "Expected to find two error rules");


            Assert.IsTrue(warningPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"), "Template context not found.");
            Rule coreRule = warningPattern.Rules.First(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]");

            Assert.IsTrue(errorPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:participantRole]"), "No branch context found");
            Rule branchRule = errorPattern.Rules.First(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]/cda:participant[@typeCode='VRF'][cda:participantRole]");

            Assert.IsNotNull(coreRule, "Expected to find a core warning rule for the template");
            Assert.AreEqual(1, coreRule.Assertions.Count, "Expected to find one assertion for the branch within the core rule.");
            Assert.IsNotNull(branchRule, "Expected to find the branch warning rule for the template's participant element.");
            Assert.AreEqual(1, branchRule.Assertions.Count, "Expected to find one assertion for the participantRole's code within the warning branch.");

            Assertion codeAssert = branchRule.Assertions.SingleOrDefault(y => y.Test == "cda:participantRole[count(cda:code)=1]");

            Assert.IsNotNull(codeAssert, "Expected to find an assertion for the participantRole's code that includes the participantRole in the xpath");
        }

        [TestMethod, TestCategory("Schematron")]
        public void AssertionWithinBranch_TestAssertionWithBranchWithMayChild()
        {
            ImplementationGuide consolidationIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template template = tdb.GenerateTemplate("urn:oid:2.16.840.1.113883.10.20.22.4.48", entType, "Advance Directive Observation", consolidationIg, "observation", "Observation");
            CodeSystem hl7PT = tdb.FindOrCreateCodeSystem("HL7ParticipationType", "2.16.840.1.113883.5.90");

            //1 warning in template context
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(template, null, null, "participant", "SHOULD", "1..*", isBranch: true);
            TemplateConstraint tc2 = tdb.AddConstraintToTemplate(template, tc1, null, "@typeCode", "SHALL", "1..1", value: "VRF", displayName: "Verifier", codeSystem: hl7PT, isBranchIdentifier: true);
            TemplateConstraint tc2_1 = tdb.AddConstraintToTemplate(template, tc1, null, "participantRole", "SHALL", "1..1", isBranchIdentifier: true);
            //no warning in branch context
            TemplateConstraint tc2_1_1 = tdb.AddConstraintToTemplate(template, tc2_1, null, "code", "MAY", "0..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, consolidationIg, consolidationIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(template, errorPhase, warningPhase);

            string output = generator.Generate();

            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one error phase.");
            Assert.AreEqual(1, warningPhase.ActivePatterns.Count, "Expected to find one warning phase.");

            Pattern warningPattern = warningPhase.ActivePatterns[0];

            Assert.AreEqual(1, warningPattern.Rules.Count, "Expected to find two warning rules.");

            Assert.IsTrue(warningPattern.Rules.Any(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]"), "No template context found");
            Rule coreRule = warningPattern.Rules.First(y => y.Context == "cda:observation[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']]");

            Assert.IsNotNull(coreRule, "Expected to find a core warning rule for the template");
            Assert.AreEqual(1, coreRule.Assertions.Count, "Expected to find one assertion for the branch within the core rule.");
        }
        #endregion

        #region Closed templates with child template id's
        [TestMethod, TestCategory("Schematron")]
        public void ClosedTemplateWithOneChildTemplate()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", entType, "Parent", myIg, "organizer");
            parentTemplate.IsOpen = false;
            Template childTemplate = tdb.GenerateTemplate("urn:oid:2.3.4.5", entType, "child", myIg, "observation");
            childTemplate.IsOpen = false;
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");
            TemplateConstraint constraintChild = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate, "observation", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddClosedTemplateConstraints(parentTemplate, errorPhase);
            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid)).FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid));
            Assert.AreEqual("cda:organizer[cda:templateId[@root='1.2.3.4']]", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("count(.//cda:templateId[@root != '1.2.3.4' and @root != '2.3.4.5'])=0", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
     
        }

        [TestMethod, TestCategory("Schematron")]
        public void ClosedTemplateWithTwoChildTemplates()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", entType, "Parent", myIg, "organizer");
            parentTemplate.IsOpen = false;
            Template childTemplate1 = tdb.GenerateTemplate("urn:oid:2.3.4.5.1", entType, "child", myIg, "observation");
            childTemplate1.IsOpen = false;
            Template childTemplate2 = tdb.GenerateTemplate("urn:oid:2.3.4.5.2", entType, "child", myIg, "observation");
            childTemplate2.IsOpen = false;
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");
            TemplateConstraint constraintChild1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate1, "observation", "SHALL", "1..1");
            TemplateConstraint constraintChild2 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate2, "observation", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddClosedTemplateConstraints(parentTemplate, errorPhase);
            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid)).FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid));
            Assert.AreEqual("cda:organizer[cda:templateId[@root='1.2.3.4']]", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("count(.//cda:templateId[@root != '1.2.3.4' and @root != '2.3.4.5.1' and @root != '2.3.4.5.2'])=0", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        [TestMethod, TestCategory("Schematron")]
        public void ClosedTemplateWithOneChildTemplateAndOneNestedChildTemplate()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", entType, "Parent", myIg, "organizer");
            parentTemplate.IsOpen = false;
            Template childTemplate1 = tdb.GenerateTemplate("urn:oid:2.3.4.5.1", entType, "child", myIg, "observation");
            childTemplate1.IsOpen = false;
            Template childTemplate2 = tdb.GenerateTemplate("urn:oid:2.3.4.5.2", entType, "child", myIg, "organizer");
            childTemplate2.IsOpen = false;
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");
            TemplateConstraint constraintChild1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate1, "observation", "SHALL", "1..1");
            TemplateConstraint constraintChild2 = tdb.AddConstraintToTemplate(childTemplate1, constraintChild1, childTemplate2, "organizer", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddClosedTemplateConstraints(parentTemplate, errorPhase);
            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid)).FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid));
            Assert.AreEqual("cda:organizer[cda:templateId[@root='1.2.3.4']]", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("count(.//cda:templateId[@root != '1.2.3.4' and @root != '2.3.4.5.1' and @root != '2.3.4.5.2'])=0", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        [TestMethod, TestCategory("Schematron")]
        public void ClosedTemplateWithOneChildTemplateAndOneImpliedTemplate()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", entType, "Parent", myIg, "organizer");
            parentTemplate.IsOpen = false;
            Template implTemplate = tdb.GenerateTemplate("urn:oid:2.3.4.5.2", entType, "child", myIg, "organizer");
            implTemplate.IsOpen = false;
            Template childTemplate1 = tdb.GenerateTemplate("urn:oid:2.3.4.5.1", entType, "child", myIg, "observation", impliedTemplate:implTemplate);
            childTemplate1.IsOpen = false;
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");
            TemplateConstraint constraintChild1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate1, "observation", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddClosedTemplateConstraints(parentTemplate, errorPhase);
            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid)).FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid));
            Assert.AreEqual("cda:organizer[cda:templateId[@root='1.2.3.4']]", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("count(.//cda:templateId[@root != '1.2.3.4' and @root != '2.3.4.5.1' and @root != '2.3.4.5.2'])=0", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        [TestMethod, TestCategory("Schematron")]
        public void ClosedTemplateWithTwoChildTemplatesAndTwoNestedChildTemplates()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", entType, "Parent", myIg, "organizer");
            parentTemplate.IsOpen = false;
            Template childTemplate1 = tdb.GenerateTemplate("urn:oid:2.3.4.5.1", entType, "child", myIg, "observation");
            childTemplate1.IsOpen = false;
            Template grandChildTemplate1 = tdb.GenerateTemplate("urn:oid:3.4.5.6.1", entType, "child", myIg, "organizer");
            grandChildTemplate1.IsOpen = false;
            Template childTemplate2 = tdb.GenerateTemplate("urn:oid:2.3.4.5.2", entType, "child", myIg, "observation");
            childTemplate2.IsOpen = false;
            Template grandChildTemplate2 = tdb.GenerateTemplate("urn:oid:3.4.5.6.2", entType, "child", myIg, "organizer");
            grandChildTemplate2.IsOpen = false;
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");
            TemplateConstraint constraintChild1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate1, "observation", "SHALL", "1..1");
            TemplateConstraint constraintChild2 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate2, "observation", "SHALL", "1..1");
            TemplateConstraint constraintChild3 = tdb.AddConstraintToTemplate(childTemplate1, constraintParent, grandChildTemplate1, "organizer", "SHALL", "1..1");
            TemplateConstraint constraintChild4 = tdb.AddConstraintToTemplate(childTemplate2, constraintParent, grandChildTemplate2, "organizer", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddClosedTemplateConstraints(parentTemplate, errorPhase);
            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid)).FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid));
            Assert.AreEqual("cda:organizer[cda:templateId[@root='1.2.3.4']]", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("count(.//cda:templateId[@root != '1.2.3.4' and @root != '2.3.4.5.1' and @root != '3.4.5.6.1' and @root != '2.3.4.5.2' and @root != '3.4.5.6.2'])=0", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        [TestMethod, TestCategory("Schematron")]
        public void ClosedTemplateWithTwoChildTemplateAndTwoNestedRepeatedChildTemplates()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", entType, "Parent", myIg, "organizer");
            parentTemplate.IsOpen = false;
            Template childTemplate1 = tdb.GenerateTemplate("urn:oid:2.3.4.5.1", entType, "child", myIg, "observation");
            childTemplate1.IsOpen = false;
            Template grandChildTemplate1 = tdb.GenerateTemplate("urn:oid:3.4.5.6.1", entType, "child", myIg, "organizer");
            grandChildTemplate1.IsOpen = false;
            Template childTemplate2 = tdb.GenerateTemplate("urn:oid:2.3.4.5.2", entType, "child", myIg, "observation");
            childTemplate2.IsOpen = false;
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");
            TemplateConstraint constraintChild1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate1, "observation", "SHALL", "1..1");
            TemplateConstraint constraintChild2 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate2, "observation", "SHALL", "1..1");
            TemplateConstraint constraintChild3 = tdb.AddConstraintToTemplate(childTemplate1, constraintParent, grandChildTemplate1, "organizer", "SHALL", "1..1");
            TemplateConstraint constraintChild4 = tdb.AddConstraintToTemplate(childTemplate2, constraintParent, grandChildTemplate1, "organizer", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddClosedTemplateConstraints(parentTemplate, errorPhase);
            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid)).FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Oid));
            Assert.AreEqual("cda:organizer[cda:templateId[@root='1.2.3.4']]", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("count(.//cda:templateId[@root != '1.2.3.4' and @root != '2.3.4.5.1' and @root != '3.4.5.6.1' and @root != '2.3.4.5.2'])=0", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        [TestMethod, TestCategory("Schematron")]
        public void CallClosedTemplateGeneratorWithOpenTemplate()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "Consolidation");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", entType, "Parent", myIg, "organizer");
            parentTemplate.IsOpen = true;
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddClosedTemplateConstraints(parentTemplate, errorPhase);
            Assert.IsNull(result, "The result should have been null");
            Assert.AreEqual(1, errorPhase.ActivePatterns.Count, "Expected to find one error phase.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Id)).FirstOrDefault();
            Assert.IsNull(pattern, "WRONG! A pattern found with Id of " + string.Format("p-{0}-CLOSEDTEMPLATE", parentTemplate.Id));
        }

        #endregion

        #region Document Level Templates

        /// <summary>
        /// Tests that an implementation guide with a single template that is a document-level template produces a
        /// schematron pattern that checks that documents validated against the schematron have that top-level template oid
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void OneDocumentLevelTemplates()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "OneDocumentLevelTemplates");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", docType, "Parent", myIg, "organizer");
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "component", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);
            var result = generator.AddDocumentLevelTemplateConstraints(myIg, errorPhase);

            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phase patterns.");

            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == "p-DOCUMENT-TEMPLATE").FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + "p-DOCUMENT-TEMPLATE");
            Assert.AreEqual("cda:ClinicalDocument", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("cda:templateId[@root='1.2.3.4']", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        /// <summary>
        /// Tests that an implementation guide with two document-level templates produces a pattern that checks either template A or template B's oid
        /// is specified at the document/header-level
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void TwoDocumentLevelTemplates()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "TwoDocumentLevelTemplates");
            myIg.ChildTemplates.Clear();
            Template parentTemplate1 = tdb.GenerateTemplate("urn:oid:1.2.3.4", docType, "Parent", myIg, "organizer");
            Template parentTemplate2 = tdb.GenerateTemplate("urn:oid:1.2.3.4.1", docType, "Parent", myIg, "organizer");
            TemplateConstraint constraintParent1 = tdb.AddConstraintToTemplate(parentTemplate1, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint constraintParent2 = tdb.AddConstraintToTemplate(parentTemplate2, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate1, constraintParent1, null, "component", "SHALL", "1..1");
            TemplateConstraint tc2 = tdb.AddConstraintToTemplate(parentTemplate2, constraintParent2, null, "component", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate1, errorPhase, warningPhase);
            generator.AddTemplate(parentTemplate2, errorPhase, warningPhase);
            
            var result = generator.AddDocumentLevelTemplateConstraints(myIg, errorPhase);
            
            Assert.AreEqual(3, errorPhase.ActivePatterns.Count, "Expected to find three error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == "p-DOCUMENT-TEMPLATE").FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + "p-DOCUMENT-TEMPLATE");
            
            Assert.AreEqual("cda:ClinicalDocument", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("cda:templateId[@root='1.2.3.4'] or cda:templateId[@root='1.2.3.4.1']", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        /// <summary>
        /// Tests that an implementation guide with one top-level template and two child-level templates produces a schematron rule
        /// that ensures the top-level document templateid is specified
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void OneDocumentLevelTemplatesWithTwoChildTemplates()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "OneDocumentLevelTemplatesWithTwoChildTemplates");
            Template parentTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4", docType, "Parent", myIg, "ClinicalDocument");
            Template childTemplate1 = tdb.GenerateTemplate("urn:oid:2.3.4.5.1", secType, "child", myIg, "section");
            Template childTemplate2 = tdb.GenerateTemplate("urn:oid:2.3.4.5.2", secType, "child", myIg, "section");
            TemplateConstraint constraintParent = tdb.AddConstraintToTemplate(parentTemplate, null, null, "component", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, null, "section", "SHALL", "1..1");
            TemplateConstraint constraintChild1 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate1, "observation", "SHALL", "1..1");
            TemplateConstraint constraintChild2 = tdb.AddConstraintToTemplate(parentTemplate, constraintParent, childTemplate2, "observation", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate, errorPhase, warningPhase);

            var result = generator.AddDocumentLevelTemplateConstraints(myIg, errorPhase);
            Assert.AreEqual(2, errorPhase.ActivePatterns.Count, "Expected to find two error phases.");
            
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == "p-DOCUMENT-TEMPLATE").FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + "p-DOCUMENT-TEMPLATE");
            Assert.AreEqual("cda:ClinicalDocument", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("cda:templateId[@root='1.2.3.4']", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        [TestMethod, TestCategory("Schematron")]
        public void OneDocumentLevelTemplateThatImpliesAnotherTemplate()
        {
            ImplementationGuide myIg = tdb.FindOrAddImplementationGuide(cdaType, "OneDocumentLevelTemplateThatImpliesAnotherTemplate");
            myIg.ChildTemplates.Clear();
            Template implTemplate = tdb.GenerateTemplate("urn:oid:1.2.3.4.1", docType, "Parent", myIg, "organizer" );
            Template parentTemplate1 = tdb.GenerateTemplate("urn:oid:1.2.3.4", docType, "Parent", myIg, "organizer", impliedTemplate: implTemplate);
            TemplateConstraint constraintParent1 = tdb.AddConstraintToTemplate(parentTemplate1, null, null, "organizer", "SHALL", "1..1");
            TemplateConstraint tc1 = tdb.AddConstraintToTemplate(parentTemplate1, constraintParent1, null, "component", "SHALL", "1..1");

            Phase errorPhase = new Phase();
            Phase warningPhase = new Phase();
            SchematronGenerator generator = new SchematronGenerator(tdb, myIg, myIg.GetRecursiveTemplates(tdb), true);
            generator.AddTemplate(parentTemplate1, errorPhase, warningPhase);
            var result = generator.AddDocumentLevelTemplateConstraints(myIg, errorPhase);
            Assert.AreEqual(3, errorPhase.ActivePatterns.Count, "Expected to find three error phases.");
            var pattern = errorPhase.ActivePatterns.Where(p => p.ID == "p-DOCUMENT-TEMPLATE").FirstOrDefault();
            Assert.IsNotNull(pattern, "No pattern found with Id of " + "p-DOCUMENT-TEMPLATE");
            Assert.AreEqual("cda:ClinicalDocument", pattern.Rules[0].Context, "Incorrect context generated");
            Assert.AreEqual("cda:templateId[@root='1.2.3.4.1'] or cda:templateId[@root='1.2.3.4']", pattern.Rules[0].Assertions[0].Test, "Incorrect rule generated");
        }

        #endregion

        private static Template ImportTemplate(IObjectRepository tdb, string location)
        {
            TemplateImporter importer = new TemplateImporter(tdb);
            List<Template> importedTemplates = importer.Import(Helper.GetSampleContents(location));
            Template template = importedTemplates[0];

            foreach (TemplateConstraint cConstraint in template.ChildConstraints)
            {
                cConstraint.TemplateId = cConstraint.Template.Id;
                cConstraint.Number = cConstraint.Id;
            }

            foreach (TemplateConstraint cConstraint in template.ChildConstraints)
            {
                if (cConstraint.ParentConstraint != null)
                    cConstraint.ParentConstraintId = cConstraint.ParentConstraint.Id;
            }

            return template;
        }
    }
}
