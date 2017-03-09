using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Export.Schematron;
using Trifolia.Export.Schematron.Model;
using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Test.Generation.Schematron
{
    [TestClass]
    [DeploymentItem("Trifolia.Plugins.dll")]
    [DeploymentItem("Schemas\\", "Schemas\\")]
	public class SchematronAssertionGeneratorTest
    {
        private static MockObjectRepository ruleRepo = new MockObjectRepository();
        private static Dictionary<int, TemplateConstraint> ruleConstraints = new Dictionary<int,TemplateConstraint>();
        private static Dictionary<int, VocabularyOutputType> vocabularyOutputMap = new Dictionary<int, VocabularyOutputType>();

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
            // IG Type
            ImplementationGuideType cdaType = ruleRepo.FindOrCreateImplementationGuideType(MockObjectRepository.DEFAULT_CDA_IG_TYPE_NAME, "cda.xsd", "cda", "urn:hl7-org:v3");
            ImplementationGuideType hqmfType = ruleRepo.FindOrCreateImplementationGuideType(MockObjectRepository.DEFAULT_HQMF_R2_IG_TYPE_NAME, "schemas\\EMeasure.xsd", "hqmf", "urn:hl7-org:v3");

            // CDA IG
            ImplementationGuide cdaIg = ruleRepo.FindOrCreateImplementationGuide(cdaType, "Test CDA IG");

            // HQMF IG
            ImplementationGuide hqmfIg = ruleRepo.FindOrCreateImplementationGuide(hqmfType, "Test HQMF IG");

            // CDA Template Type
            TemplateType cdaDocType = ruleRepo.FindOrCreateTemplateType(cdaType, "document", "ClinicalDocument", "ClinicalDocument", 1);
            TemplateType cdaSecType = ruleRepo.FindOrCreateTemplateType(cdaType, "section", "section", "Section", 2);
            TemplateType cdaEntType = ruleRepo.FindOrCreateTemplateType(cdaType, "entry", "entry", "Entry", 3);

            TemplateType hqmfDocType = ruleRepo.FindOrCreateTemplateType(hqmfType, "Document", "QualityMeasureDocument", "QualityMeasureDocument", 1);
            TemplateType hqmfSecType = ruleRepo.FindOrCreateTemplateType(hqmfType, "Section", "component", "Component2", 2);
            TemplateType hqmfEntType = ruleRepo.FindOrCreateTemplateType(hqmfType, "Entry", "entry", "SourceOf", 3);

            // HQMF Template Type

            // Code System
            CodeSystem cs1 = ruleRepo.FindOrCreateCodeSystem("SNOMED CT", "6.36");

            // Value Set
            ValueSet vs1 = ruleRepo.FindOrCreateValueSet("Test Value Set 1", "1.2.3.4");
            ruleRepo.FindOrCreateValueSetMember(vs1, cs1, "1234", "Test Member 1");
            ruleRepo.FindOrCreateValueSetMember(vs1, cs1, "4321", "Test Member 2");

            // CDA Template
            Template cdaTemplate1 = ruleRepo.CreateTemplate("urn:oid:1.2.3.4.5", cdaDocType, "Test Document Template", cdaIg);
            Template cdaTemplate2 = ruleRepo.CreateTemplate("urn:oid:5.4.3.2.1", cdaDocType, "Test Referenced Document Template", cdaIg);

            // HQMF Template
            Template hqmfTemplate1 = ruleRepo.CreateTemplate("urn:oid:3.2.1.4.5", hqmfDocType, "Template HQMF Doc Template", hqmfIg);
            Template hqmfTemplate2 = ruleRepo.CreateTemplate("urn:oid:3.2.1.4.5.1", hqmfDocType, "Template HQMF Doc Template 2", hqmfIg);

            // Rule #1
            AddConstraintForRule(1,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "realmCode", "SHALL", "0..*"));

            // Rule #2
            AddConstraintForRule(2,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "componentOf", "SHALL", "0..0"));

            // Rule #3
            AddConstraintForRule(3,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1"));

            // Rule #4
            AddConstraintForRule(4,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "title", "SHALL", "1..1", null, null, "Test Document"));

            // Rule #5
            TemplateConstraint p5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "templateId", "SHALL", "1..1");
            AddConstraintForRule(5,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p5, null, "@root", "SHALL", "1..1", null, null, "2.16.840.1.113883.10.20.22.2.21.1"));

            // Rule #6
            AddConstraintForRule(6,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "1..1"));

            // Rule #7
            AddConstraintForRule(7,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@moodCode", "SHALL", "1..1", null, null, "EVN"));

            // Rule #8
            AddConstraintForRule(8,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "0..1"));

            // Rule #9
            AddConstraintForRule(9,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL NOT", "0..0"));

            // Rule #10
            AddConstraintForRule(10,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL NOT", "0..*"));

            // Rule #10
            AddConstraintForRule(11,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL NOT", "1..1"));

            // Rule #12
            AddConstraintForRule(12,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "componentOf", "SHALL", "0..0", null, null, "1234"));

            // Rule #13
            AddConstraintForRule(13,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..*", null, null));

            // Rule #14
            AddConstraintForRule(14,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..3", null, null));

            // Rule #15
            AddConstraintForRule(15,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL NOT", "0..1", null, null));

            // Rule #16
            AddConstraintForRule(16,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL NOT", "0..3", null, null));

            // Rule #17
            AddConstraintForRule(17,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL NOT", "1..*", null, null));

            // Rule #18
            AddConstraintForRule(18,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL NOT", "1..3", null, null));

            // Rule #19
            TemplateConstraint p19 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "0..0");
            AddConstraintForRule(19,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p19, null, "@code", "SHALL", "0..0", null, null));

            // Rule #20
            TemplateConstraint p20 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "0..0");
            AddConstraintForRule(20,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p20, null, "@code", "SHALL", "0..0", null, null, "1234"));

            // Rule #21
            TemplateConstraint p21 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "0..0");
            AddConstraintForRule(21,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p21, null, "@code", "SHALL", "0..1", null, null));

            // Rule #22
            TemplateConstraint p22 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "0..0");
            AddConstraintForRule(22,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p21, null, "@code", "SHALL", "0..1", null, null, "1234"));

            // Rule #23
            AddConstraintForRule(23,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "0..3", null, null));

            // Rule #24
            AddConstraintForRule(24,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "0..1", null, null, "1234"));

            // Rule #25
            AddConstraintForRule(25,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL NOT", "0..1", null, null, "1234"));

            // Rule #26
            AddConstraintForRule(26,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL NOT", "1..*", null, null, "1234"));

            // Rule #27
            AddConstraintForRule(27,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL NOT", "1..*", null, null));

            // Rule #28
            AddConstraintForRule(28,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "0..0", null, null, "1234"));

            // Rule #29
            AddConstraintForRule(29,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "0..0", null, null));

            // Rule #30
            AddConstraintForRule(30,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "title", "SHALL", "0..1", null, null, "1234"));

            // Rule #31
            AddConstraintForRule(31,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "0..1", null, null));

            // Rule #32
            AddConstraintForRule(32,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "0..1", "CD"));

            // Rule #33
            AddConstraintForRule(33,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "0..1", "CD", null, null, null, vs1));

            // Rule #34
            AddConstraintForRule(34,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "0..1", null, null, null, null, vs1));

            // Rule #35
            AddConstraintForRule(35,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code/@code", "SHALL", "0..1", null, null));

            // Rule #36
            AddConstraintForRule(36,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code/@code", "SHALL", "0..1", null, null, "1234"));

            // Rule #37
            AddConstraintForRule(37,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code/@code", "SHALL", "0..1", null, null, null, null, vs1));

            // Rule #38
            AddConstraintForRule(38,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "0..1", null, null));

            // Rule #39
            AddConstraintForRule(39,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "0..1", null, null, "Test Title"));

            // Rule #40
            AddConstraintForRule(40,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/value", "SHALL", "0..1", "CD", null));

            // Rule #41
            AddConstraintForRule(41,
                    ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entry/observation/title", "SHALL", "0..1", null, null));

            // Rule #42
            AddConstraintForRule(42,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "1..1", null, null));

            // Rule #43
            AddConstraintForRule(43,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "1..1", null, null, "Test Title"));

            // Rule #44
            AddConstraintForRule(44,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/value", "SHALL", "1..1", "CD", null));

            // Rule #45
            AddConstraintForRule(45,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entry/observation/title", "SHALL", "1..1", null, null));

            // Rule #46
            AddConstraintForRule(46,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "title", "SHALL", "1..*", null, null, "Test Title"));

            // Rule #47
            AddConstraintForRule(47,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "1..*", null, null));

            // Rule #48
            AddConstraintForRule(48,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "1..*", null, null, "Test Title"));

            // Rule #49
            AddConstraintForRule(49,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/value", "SHALL", "1..*", "CD", null));

            // Rule #50
            AddConstraintForRule(50,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entry/observation/title", "SHALL", "1..*", null, null));

            // Rule #51
            TemplateConstraint p51 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(51,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p51, null, "title", "SHALL", "1..*", null, null));

            // Rule #52
            TemplateConstraint p52 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(52,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p52, null, "value", "SHALL", "1..*", "CD", null));

            // Rule #53
            TemplateConstraint p53 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(53,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p53, null, "value", "SHALL", "1..*", null, null, null, null, vs1)); //should support nullFlavor

            // Rule #54
            TemplateConstraint p54 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(54,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p54, null, "value", "SHALL", "1..*", "CD", null, null, null, vs1));

            // Rule #55
            AddConstraintForRule(55,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/value", "SHALL", "1..*", null, null, null, null, vs1));

            // Rule #56
            AddConstraintForRule(56,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/value", "SHALL", "1..*", "CD", null, null, null, vs1));

            // Rule #57
            AddConstraintForRule(57,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..3", null, null));

            // Rule #58
            AddConstraintForRule(58,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "title", "SHALL", "1..3", null, null, "Test Title"));

            // Rule #59
            AddConstraintForRule(59,
                    ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "1..3", null, null));

            // Rule #60
            AddConstraintForRule(60,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/title", "SHALL", "1..3", null, null, "Test Title"));

            // Rule #61
            AddConstraintForRule(61,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/value", "SHALL", "1..3", "CD", null));

            // Rule #62
            AddConstraintForRule(62,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entry/observation/title", "SHALL", "1..3", null, null));

            // Rule #63
            TemplateConstraint p63 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(63,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p63, null, "code", "SHALL", "0..*", null, null));

            // Rule #64
            TemplateConstraint p64 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(64,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p64, null, "value", "SHALL", "0..*", "CD", null));

            // Rule #65
            TemplateConstraint p65 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(65,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p65, null, "value", "SHALL", "0..*", null, null, null, null, vs1));

            // Rule #66
            TemplateConstraint p66 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(66,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p66, null, "value", "SHALL", "0..*", "CD", null, null, null, vs1));

            // Rule #67
            TemplateConstraint p67 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(67,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p67, null, "title", "SHALL", "0..*", null, null, "Test Title"));

            // Rule #68
            TemplateConstraint p68 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(68,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p68, null, "code", "SHALL", "0..3", null, null));

            // Rule #69
            AddConstraintForRule(69,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation/code", "SHALL", "0..3", null, null));

            // Rule #70
            AddConstraintForRule(70,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "0..3", "CD", null));

            // Rule #71
            TemplateConstraint p71 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(71,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p71, null, "value", "SHALL", "0..3", "CD", null));

            // Rule #72
            AddConstraintForRule(72,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "0..3", null, null, null, null, vs1));

            // Rule #73
            AddConstraintForRule(73,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "0..3", "CD", null, null, null, vs1));

            // Rule #74
            AddConstraintForRule(74,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "title", "SHALL", "0..3", null, null, "Test Title"));

            // Rule #75
            TemplateConstraint p75 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(75,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p75, null, "title", "SHALL", "0..3", null, null, "Test Title"));

            // Rule #76
            TemplateConstraint p76 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(76,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p76, null, "value", "SHALL", "0..3", "CD", null, null, null, vs1));

            // Rule #77
            TemplateConstraint p77 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(77,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p77, null, "title", "SHALL", "1..3", null, null));

            // Rule #78
            TemplateConstraint p78 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(78,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p78, null, "value", "SHALL", "1..3", "CD", null));

            // Rule #79
            TemplateConstraint p79 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(79,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p79, null, "value", "SHALL", "1..3", null, null, null, null, vs1));

            // Rule #80
            TemplateConstraint p80 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(80,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p80, null, "value", "SHALL", "1..3", "CD", null, null, null, vs1));

            // Rule #81
            TemplateConstraint p81 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(81,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p81, null, "title", "SHALL", "1..3", null, null, "Test Title"));

            // Rule #82
            TemplateConstraint p82 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "0..1", isBranch: true);
            AddConstraintForRule(82,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p82, null, "code", "SHALL", "1..1", null, null));

            // Rule #83
            TemplateConstraint p83 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p83_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p83, null, "code", "SHALL", "1..1");
            AddConstraintForRule(83, p83);
            
            // Rule #84
            TemplateConstraint p84_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1");
            TemplateConstraint p84_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p84_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p84_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p84_1, null, "observation", "SHALL", "1..1");
            TemplateConstraint p84_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p84_3, null, "@classCode", "SHALL", "1..1", null, null, "OBS");
            TemplateConstraint p84_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p84_3, null, "code", "SHALL", "1..1");
            AddConstraintForRule(84,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p84_5, null, "@code", "SHALL", "1..1"));

            // Rule #85
            TemplateConstraint p85_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1");
            TemplateConstraint p85_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p85_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p85_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p85_1, null, "observation", "SHALL", "1..1");
            TemplateConstraint p85_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p85_3, null, "@classCode", "SHALL", "1..1", null, null, "OBS");
            TemplateConstraint p85_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p85_3, null, "code", "SHALL", "1..1");
            AddConstraintForRule(85,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p85_5, null, "@code", "SHOULD", "0..1"));

            // Rule #86
            // Note: Branch is only specified on elements because attributes should automatically be included in the XPATH test for the elements
            TemplateConstraint p86_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p86_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p86_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier: true);
            TemplateConstraint p86_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p86_1, null, "observation", "SHALL", "1..*", isBranchIdentifier: true);
            TemplateConstraint p86_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p86_3, null, "@classCode", "SHALL", "1..1", null, null, "OBS", isBranchIdentifier: true);
            TemplateConstraint p86_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p86_3, null, "code", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint p86_6 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p86_3, null, "code", "SHALL", "1..1", isBranchIdentifier: true);
            AddConstraintForRule(86, p86_1);

            // Rule #87
            // Note: Branch is only specified on elements because attributes should automatically be included in the XPATH test for the elements
            TemplateConstraint p87_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1", null, null, null, null, null, null, null, isBranch: true);
            TemplateConstraint p87_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p87_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier: true);
            TemplateConstraint p87_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p87_1, null, "observation", "SHALL", "1..1", null, null, null, null, null, null, null, isBranchIdentifier: true);
            TemplateConstraint p87_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p87_3, null, "@classCode", "SHALL", "1..1", null, null, "OBS", isBranchIdentifier: true);
            TemplateConstraint p87_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p87_3, null, "statusCode", "SHALL", "1..1", null, null, null, null, null, null, null, isBranchIdentifier: true);
            TemplateConstraint p87_6 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p87_5, null, "@code", "SHALL", "1..1", null, null, "active", isBranchIdentifier: true);
            TemplateConstraint p87_7 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p87_3, null, "code", "SHALL", "1..1", isBranchIdentifier: true);
            AddConstraintForRule(87, p87_1);

            // Rule #88
            // NOTE: Data-types would not usually be supported on an attribute, but we have production data with this scenario
            AddConstraintForRule(88,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "1..1", "CD", null, "OBS"));

            // Rule #89
            // Branched parents, where one of the children is a primitive that is not branched
            TemplateConstraint p89_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1", isBranch:true);
            TemplateConstraint p89_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p89_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier:true);
            TemplateConstraint p89_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p89_1, null, "observation", "SHALL", "1..1", isBranchIdentifier:true);
            TemplateConstraint p89_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p89_3, null, "@classCode", "SHALL", "1..1", null, null, "OBS", isBranchIdentifier: true);
            TemplateConstraint p89_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p89_3, null, "statusCode", "SHALL", "1..1", isBranchIdentifier:true);
            TemplateConstraint p89_6 = ruleRepo.AddPrimitiveToTemplate(cdaTemplate1, p89_3, "SHALL", "Test primitive constraint", null, false);
            TemplateConstraint p89_7 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p89_5, null, "@code", "SHALL", "1..1", null, null, "active", isBranchIdentifier: true);
            TemplateConstraint p89_8 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p89_3, null, "code", "SHALL", "1..1", isBranchIdentifier: true);
            AddConstraintForRule(89,p89_1);

            // Rule #90
            TemplateConstraint p90_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1");
            AddConstraintForRule(90,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p90_1, cdaTemplate2, null, "SHALL", "1..1"));

            // Rule #91
            TemplateConstraint p91_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1");
            AddConstraintForRule(91,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p91_1, cdaTemplate2, "observation", "SHALL", "1..1"));

            // Rule #92
            TemplateConstraint p92_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHALL", "1..1", isBranch: true);
            AddConstraintForRule(92,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p92_1, cdaTemplate2, "observation", "SHALL", "1..1"));

            // Rule #93
            TemplateConstraint p93_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "templateId", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p93_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p93_1, null, "@root", "SHALL", "1..1", null, null, "1.2.3.4.5", isBranchIdentifier: true);
            AddConstraintForRule(93, p93_1);

            // Rule #94
            TemplateConstraint p94_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "templateId", "SHALL", "0..1", isBranch:true);
            TemplateConstraint p94_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p94_1, null, "@root", "SHALL", "1..1", null, null, "1.2.3.4.5", isBranchIdentifier: true);
            AddConstraintForRule(94, p94_1);

            // Rule #95
            TemplateConstraint p95_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..1", isBranch: true);
            TemplateConstraint p95_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p95_1, null, "@typeCode", "SHALL", "1..1", null, null, "RSON", isBranchIdentifier: true);
            AddConstraintForRule(95, p95_1);

            // Rule #96
            // Note: Branch is only specified on elements because attributes should automatically be included in the XPATH test for the elements
            TemplateConstraint p96_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..1", isBranch:true);
            TemplateConstraint p96_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p96_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier:true);
            TemplateConstraint p96_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p96_1, null, "observation", "SHALL", "1..1", isBranchIdentifier:true);
            TemplateConstraint p96_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p96_3, null, "@classCode", "SHALL", "1..1", null, null, "OBS", isBranchIdentifier: true);
            TemplateConstraint p96_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p96_3, null, "statusCode", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint p96_6 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p96_5, null, "@code", "SHALL", "1..1", null, null, "active", isBranchIdentifier: true);
            TemplateConstraint p96_7 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p96_3, null, "code", "SHALL", "1..1",isBranchIdentifier:true);
            AddConstraintForRule(96, p96_1);

            // Rule #97
            AddConstraintForRule(97,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "statusCode", "SHALL", "1..1", null, null, "1.2.3.4"));

            // Rule #98
            AddConstraintForRule(98,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1", null, null, "4.3.2.1"));

            // Rule #99
            TemplateConstraint p99_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..*", isBranch: true);
            TemplateConstraint p99_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p99_1, null, "@typeCode", "SHALL", "1..1", null, null, "DRIV", isBranchIdentifier:true);
            AddConstraintForRule(99, p99_1); //occurs at the template level

            // Rule #100
            TemplateConstraint p100_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..*", isBranch: true);
            TemplateConstraint p100_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p100_1, null, "@typeCode", "SHALL", "1..1", null, null, "DRIV", isBranchIdentifier:true);
            TemplateConstraint p100_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p100_1, cdaTemplate2, "act", "SHALL", "1..1");
            AddConstraintForRule(100, p100_1); //occurs at template level

            // Rule #101
            TemplateConstraint p101_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..*", isBranch: true);
            TemplateConstraint p101_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p101_1, null, "@typeCode", "SHALL", "1..1", null, null, "DRIV", isBranchIdentifier: true);
            TemplateConstraint p101_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p101_1, cdaTemplate2, "act", "SHALL", "1..1", isBranchIdentifier: true);
            AddConstraintForRule(101, p101_1); //generate test from the template level context

            // Rule #102
            TemplateConstraint p102_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p102_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p102_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier: true);
            TemplateConstraint p102_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p102_1, null, "externalDocument", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint p102_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p102_3, null, "id", "SHALL", "1..1", isBranch:true);
            TemplateConstraint p102_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p102_4, cdaTemplate2, "@root", "SHALL", "1..1");
            AddConstraintForRule(102, p102_1); //generates test from the template level context

            // Rule #103
            TemplateConstraint p103_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p103_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p103_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p103_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p103_1, null, "externalDocument", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p103_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p103_3, null, "id", "SHALL", "1..1", isBranch: true);
            AddConstraintForRule(103,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p103_4, cdaTemplate2, "@root", "SHALL", "1..1"));

            //this tests to be sure that we get the data type in the path along with the MAY optional parent. Data Type is a special attribute that should appear in parent path.
            // Rule #104
            TemplateConstraint p104_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "1..1", "CD");
            TemplateConstraint p104_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p104_1, null, "qualifier", "MAY", "0..*");
            AddConstraintForRule(104,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p104_2, null, "name", "SHALL", "1..1"));

            // Rule #105
            TemplateConstraint p105_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHOULD", "0..1");
            TemplateConstraint p105_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p105_1, null, "originalText", "SHOULD", "0..1");
            AddConstraintForRule(105,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p105_2, null, "reference", "SHOULD", "0..1"));

            // Rule #106
            TemplateConstraint p106_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHOULD", "0..1");
            TemplateConstraint p106_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p106_1, null, "originalText", "SHOULD", "0..1");
            TemplateConstraint p106_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p106_2, null, "reference", "SHOULD", "0..1");
            AddConstraintForRule(106,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p106_3, null, "@value", "SHOULD", "0..1"));

            // Rule #107
            TemplateConstraint p107_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "text", "SHOULD", "0..1");
            AddConstraintForRule(107,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p107_1, null, "reference", "SHOULD", "0..1"));

            // Rule #108
            TemplateConstraint p108_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "1..1", "CD");
            AddConstraintForRule(108,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p108_1, null, "@code", "SHALL", "1..1", "CS", null, null, null, vs1));

            // Rule #109
            TemplateConstraint p109_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "1..1", "CD");
            AddConstraintForRule(109,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p109_1, null, "@code", "SHALL", "1..1", null, null, null, null, vs1));

            // Rule #110
            TemplateConstraint p110_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "1..1", "CD");
            AddConstraintForRule(110,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p110_1, null, "@code", "SHALL", "0..1", null, null, null, null, vs1));

            // Rule #111
            TemplateConstraint p111_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "specimen", "SHALL", "0..*");
            TemplateConstraint p111_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p111_1, null, "specimenRole", "SHALL", "1..1");
            AddConstraintForRule(111,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p111_2, null, "id", "SHALL", "0..*"));

            // Rule #112
            TemplateConstraint p112_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "1..1", "CD");
            AddConstraintForRule(112,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p112_1, null, "@code", "SHALL", "1..1", "CS", null, null, null, vs1));
            vocabularyOutputMap.Add(112, VocabularyOutputType.SVS_SingleValueSet);

            // Rule #113
            TemplateConstraint p113_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "1..1", "CD");
            AddConstraintForRule(113,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p113_1, null, "@code", "SHALL", "1..1", "CS", null, null, null, vs1));
            vocabularyOutputMap.Add(113, VocabularyOutputType.SVS);

            // Rule #114
            TemplateConstraint p114_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1", "CD");
            AddConstraintForRule(114,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p114_1, null, "@code", "SHALL", "1..1", "CS", null, "1234", null, null, cs1));

            // Rule #115
            TemplateConstraint p115_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "observation", "SHALL", "1..1");
            AddConstraintForRule(115,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p115_1, null, "code", "SHALL", "1..1", null, null, "1234", null, null, cs1));

            // Rule #116
            AddConstraintForRule(116,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@code", "SHALL", "1..1", null, null, "1234", null, null, cs1));

            // Rule #117
            TemplateConstraint p117_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "encounter", "SHALL", "1..1");
            AddConstraintForRule(117,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p117_1, null, "@classCode", "SHALL", "1..1", null, null, "ENC", null, null, cs1));

            // Rule #118
            AddConstraintForRule(118,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "@classCode", "SHALL", "1..1", null, null, "ENC", null, null, cs1));

            // Rule #119
            AddConstraintForRule(119, ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "externalDocument", "SHALL", "1..1", codeSystem: cs1, value: "DOC"));

            // Rule #120, this is causing [value[[@code]]]
            TemplateConstraint p120_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "value", "SHALL", "1..1", "CD");
            AddConstraintForRule(120,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p120_1, null, "@code", "SHALL", "1..1", "CE"));

            // Rule #121, this is causing not(cda:serviceEvent[@classCode='']) or cda:serviceEvent[@classCode=''][cda:serviceEvent[@classCode='PCPR']] instead of not(cda:documentationOf) or cda:documentationOf[cda:serviceEvent[@classCode='PCPR']]
            TemplateConstraint p121_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "documentationOf", "SHOULD", "0..1");
            AddConstraintForRule(121,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p121_1, null, "serviceEvent/@classCode", "SHALL", "1..1", value: "PCPR"));

            // Rule #122, this is causing not(cda:subject/cda:relatedSubject[@classCode='']/cda:subject) or cda:subject/cda:relatedSubject[@classCode='']/cda:subject[count(cda:administrativeGenderCode)=1] instead of not(cda:subject/cda:relatedSubject/cda:subject) or cda:subject/cda:relatedSubject/cda:subject[count(cda:administrativeGenderCode)=1]
            TemplateConstraint p122_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "subject", "SHALL", "1..1");
            TemplateConstraint p122_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p122_1, null, "relatedSubject/@classCode", "SHALL", "1..1", value: "PRS");
            TemplateConstraint p122_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p122_2, null, "subject", "SHOULD", "0..1");
            AddConstraintForRule(122,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p122_3, null, "administrativeGenderCode", "SHALL", "1..1"));

            // Rule #123, this is causing "not(cda:performer[@typeCode='']/cda:assignedEntity[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.87']]/cda:code) or cda:performer[@typeCode='']/cda:assignedEntity[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.87']]/cda:code[@code]" instead of not(cda:performer[@typeCode]/cda:assignedEntity[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.87']]/cda:code) or cda:performer[@typeCode]/cda:assignedEntity[cda:templateId[@root='2.16.840.1.113883.10.20.22.4.87']]/cda:code[@code]
            TemplateConstraint p123_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "performer", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p123_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p123_1, null, "@typeCode", "SHALL", "1..1", value: null, isBranchIdentifier:true);
            TemplateConstraint p123_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p123_1, null, "assignedEntity", "SHALL", "1..1", isBranchIdentifier:true);
            TemplateConstraint p123_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p123_1, null, "templateId", "SHALL", "1..1", isBranchIdentifier:true);
            TemplateConstraint p123_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p123_4, null, "@root", "SHALL", "1..1", value: "2.16.840.1.113883.10.20.22.4.87", isBranchIdentifier:true);
            TemplateConstraint p123_6 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p123_3, null, "code", "SHOULD", "0..1", isBranchIdentifier:true);
            TemplateConstraint p123_7 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p123_6, null, "@code", "SHALL", "1..1");
            AddConstraintForRule(123, p123_1);

            // Rule #124
            TemplateConstraint p124_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..1");
            AddConstraintForRule(124,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p124_1, null, "@typeCode", "SHALL", "1..1", null, null, "RSON"));

            // Rule #125
            TemplateConstraint p125_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "SHOULD", "0..2");
            AddConstraintForRule(125,
                ruleRepo.AddConstraintToTemplate(cdaTemplate1, p125_1, null, "@typeCode", "SHALL", "0..1", null, null, "RSON"));

            // Rule #126
            //Send in the isBranchIdentifier, this should only produce a [@typeCode] check, 
            //the GenerateSchematron class would generally not do this but need to make sure AssertionLineBuilder handles it by outputting the proper check.
            TemplateConstraint p126_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..*", isBranch: true);
            TemplateConstraint p126_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p126_1, null, "@typeCode", "SHALL", "1..1", "CD", null, "DRIV", isBranchIdentifier: true);
            AddConstraintForRule(126, p126_2); //test that we only generate the branch identifier

            // Rule #127
            //Test for child dependent on branch, this would be generated within the branch's context so shouldn't have full parent context specified
            TemplateConstraint p127_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "entryRelationship", "MAY", "0..*", isBranch: true);
            TemplateConstraint p127_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p127_1, null, "@typeCode", "SHALL", "1..1", null, null, "DRIV", isBranchIdentifier: true);
            TemplateConstraint p127_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p127_1, cdaTemplate2, "act", "SHALL", "1..1");
            AddConstraintForRule(127, p127_3);

            // Rule #128
            TemplateConstraint p128_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p128_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p128_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier: true);
            TemplateConstraint p128_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p128_1, null, "externalDocument", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint p128_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p128_3, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p128_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p128_4, cdaTemplate2, "@root", "SHALL", "1..1", isBranchIdentifier:true);
            AddConstraintForRule(128, p128_4); //generates test from the branch context for the dependent child constraint

            // Rule #129
            TemplateConstraint p129_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p129_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p129_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier: true);
            TemplateConstraint p129_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p129_1, null, "externalDocument", "SHALL", "1..1", isBranchIdentifier: true);
            TemplateConstraint p129_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p129_3, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p129_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p129_4, cdaTemplate2, "@root", "SHALL", "1..1");
            AddConstraintForRule(129, p129_4); //generates test from the template context

            // Rule #130
            TemplateConstraint p130_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p130_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p130_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p130_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p130_1, null, "externalDocument", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p130_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p130_3, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p130_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p130_4, cdaTemplate2, "@root", "SHALL", "1..1");
            AddConstraintForRule(130, p130_1);

            // Rule #131
            TemplateConstraint p131_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p131_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p131_1, null, "@typeCode", "SHALL", "1..1", "CD", null, "REFR");
            TemplateConstraint p131_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p131_1, null, "externalDocument", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p131_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p131_3, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p131_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p131_4, cdaTemplate2, "@root", "SHALL", "1..1");
            AddConstraintForRule(131, p131_2);

            // Rule #132
            TemplateConstraint p132_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p132_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p132_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p132_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p132_1, null, "externalDocument", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p132_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p132_3, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p132_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p132_4, cdaTemplate2, "@root", "SHALL", "1..1");
            AddConstraintForRule(132, p132_3);

            // Rule #133
            TemplateConstraint p133_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p133_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p133_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p133_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p133_1, null, "externalDocument", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p133_4 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p133_3, null, "id", "SHALL", "1..1", isBranch: true);
            TemplateConstraint p133_5 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p133_4, cdaTemplate2, "@root", "SHALL", "1..1");
            AddConstraintForRule(133, p133_4); 

            // Rule #134
            TemplateConstraint p134_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHALL", "1..1");  //no isBranch on parent
            TemplateConstraint p134_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p134_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p134_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p134_1, null, "externalDocument", "SHALL", "1..1", isBranch: true);
            AddConstraintForRule(134, p134_3);


            // Rule #135
            TemplateConstraint p135_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHOULD", "0..1"); //no isBranch on parent and parent is optional
            TemplateConstraint p135_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p135_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR");
            TemplateConstraint p135_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p135_1, null, "externalDocument", "SHALL", "1..1", isBranch: true);
            AddConstraintForRule(135, p135_3);

            // Rule #136
            TemplateConstraint p136_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "reference", "SHOULD", "1..1", isBranch: true); //isBranch on parent and parent is optional
            TemplateConstraint p136_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p136_1, null, "@typeCode", "SHALL", "1..1", null, null, "REFR", isBranchIdentifier: true); //isBranch has additional branch identifier
            TemplateConstraint p136_3 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p136_1, null, "externalDocument", "SHALL", "1..1");
            AddConstraintForRule(136, p136_3);

            // Rule #137
            TemplateConstraint p137_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "sdtc:raceCode", "SHALL", "1..1");
            AddConstraintForRule(137, p137_1);

            // Rule #138
            TemplateConstraint p138_1 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1");
            TemplateConstraint p138_2 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p138_1, null, "@code", "SHALL NOT", "0..0");
            AddConstraintForRule(138, p138_2);

            // Rule #139
            TemplateConstraint p139 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1", valueConformance: "SHOULD", valueSet: vs1); 
            AddConstraintForRule(139, p139);

            // Rule #140
            TemplateConstraint p140 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1", valueConformance: "SHOULD", value: "1234-X");
            AddConstraintForRule(140, p140);

            // Rule #141
            TemplateConstraint p141 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1", valueSet: vs1); //should check for valueset and nullflavor 
            AddConstraintForRule(141, p141);

            // Rule #142
            TemplateConstraint p142 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "code", "SHALL", "1..1", codeSystem: cs1); //should check for codeSystem and nullflavor 
            AddConstraintForRule(142, p142);

            // Rule #143
            TemplateConstraint p143 = ruleRepo.AddConstraintToTemplate(hqmfTemplate1, null, hqmfTemplate2, "observationCriteria", "SHALL", "1..1");
            AddConstraintForRule(143, p143);

            // Rule #144
            TemplateConstraint p144_parent = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "sdtc:actReference", "SHALL", "1..1");
            TemplateConstraint p144 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p144_parent, null, "@classCode", "SHALL", "1..1");
            AddConstraintForRule(144, p144);

            // Rule #145
            TemplateConstraint p145_parent = ruleRepo.AddConstraintToTemplate(cdaTemplate1, null, null, "sdtc:actReference", "SHALL", "1..1");
            TemplateConstraint p145 = ruleRepo.AddConstraintToTemplate(cdaTemplate1, p145_parent, null, "@classCode", "SHALL", "1..*");
            AddConstraintForRule(145, p145);
        }


        private static void AddConstraintForRule(int ruleNumber, TemplateConstraint constraint)
        {
            ruleConstraints.Add(ruleNumber, constraint);
        }
        
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
        }

        #endregion

        #region Rule-Based Tests

        public static string GenerateAssertionForRule(int number)
        {
            var tc = ruleConstraints[number];
            var vocabularyOutputType = vocabularyOutputMap.ContainsKey(number) ? vocabularyOutputMap[number] : VocabularyOutputType.Default;
            var cParser = new ConstraintParser(ruleRepo, tc, tc.Template.ImplementationGuideType, vocabularyOutputType: vocabularyOutputType);
            var builder = cParser.CreateAssertionLineBuilder();

            return builder.ToString();
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule1()
        {
            string actual = GenerateAssertionForRule(1);
            string expected = "not(cda:realmCode) or cda:realmCode";

            Assert.AreEqual(expected, actual, "Expected rule 1 to generate an empty string.");
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule2()
        {
            string actual = GenerateAssertionForRule(2);
            string expected = "count(cda:componentOf)=0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule3()
        {
            string actual = GenerateAssertionForRule(3);
            string expected = "count(cda:code)=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule4()
        {
            string actual = GenerateAssertionForRule(4);
            string expected = "count(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test document'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule5()
        {
            string actual = GenerateAssertionForRule(5);
            string expected = "cda:templateId[@root='2.16.840.1.113883.10.20.22.2.21.1']";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule6()
        {
            string actual = GenerateAssertionForRule(6);
            string expected = "@classCode";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule7()
        {
            string actual = GenerateAssertionForRule(7);
            string expected = "@moodCode='EVN'";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule8()
        {
            string actual = GenerateAssertionForRule(8);
            string expected = "count(cda:code) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule9()
        {
            string actual = GenerateAssertionForRule(9);
            string expected = "count(cda:code)=0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule10()
        {
            string actual = GenerateAssertionForRule(10);
            string expected = "not(cda:code) or cda:code";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule11()
        {
            string actual = GenerateAssertionForRule(11);
            string expected = "not(count(cda:code)=1)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule12()
        {
            string actual = GenerateAssertionForRule(12);
            string expected = "count(cda:componentOf[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='1234'])=0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule13()
        {
            string actual = GenerateAssertionForRule(13);
            string expected = "count(cda:code) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule14()
        {
            string actual = GenerateAssertionForRule(14);
            string expected = "count(cda:code) = (1 or 2 or 3)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule15()
        {
            string actual = GenerateAssertionForRule(15);
            string expected = "not(count(cda:code) < 2)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule16()
        {
            string actual = GenerateAssertionForRule(16);
            string expected = "not(count(cda:code) < 4)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule17()
        {
            string actual = GenerateAssertionForRule(17);
            string expected = "not(count(cda:code) > 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule18()
        {
            string actual = GenerateAssertionForRule(18);
            string expected = "not(count(cda:code) = (1 or 2 or 3))";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule19()
        {
            string actual = GenerateAssertionForRule(19);
            string expected = "not(cda:code) or cda:code[not(@code)]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule20()
        {
            string actual = GenerateAssertionForRule(20);
            string expected = "not(cda:code) or cda:code[not(@code='1234')]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule21()
        {
            string actual = GenerateAssertionForRule(21);
            string expected = "not(cda:code) or cda:code[@code]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule22()
        {
            string actual = GenerateAssertionForRule(22);
            string expected = "not(cda:code) or cda:code[@code='1234']";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule23()
        {
            string actual = GenerateAssertionForRule(23);
            string expected = "count(cda:code) < 4";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule24()
        {
            string actual = GenerateAssertionForRule(24);
            string expected = "not(@classCode='1234') or @classCode='1234'";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]  
        public void TestAssertionLineBuilderRule25()
        {
            string actual = GenerateAssertionForRule(25);
            string expected = "not(not(@classCode='1234') or @classCode='1234')";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule26()
        {
            string actual = GenerateAssertionForRule(26);
            string expected = "not(count(@classCode='1234') > 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule27()
        {
            string actual = GenerateAssertionForRule(27);
            string expected = "not(count(@classCode) > 0)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule28()
        {
            string actual = GenerateAssertionForRule(28);
            string expected = "not(@classCode='1234')";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule29()
        {
            string actual = GenerateAssertionForRule(29);
            string expected = "not(@classCode)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule30()
        {
            string actual = GenerateAssertionForRule(30);
            string expected = "count(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='1234']) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule31()
        {
            string actual = GenerateAssertionForRule(31);
            string expected = "not(@classCode) or @classCode";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule32()
        {
            string actual = GenerateAssertionForRule(32);
            string expected = "count(cda:value[@xsi:type='CD']) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule33()
        {
            string actual = GenerateAssertionForRule(33);
            string expected = "count(cda:value[@xsi:type='CD' and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule34()
        {
            string actual = GenerateAssertionForRule(34);
            string expected = "not(@classCode) or @classCode=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule35()
        {
            string actual = GenerateAssertionForRule(35);
            string expected = "not(cda:code[@code]) or cda:code[@code]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule36()
        {
            string actual = GenerateAssertionForRule(36);
            string expected = "not(cda:code[@code='1234']) or cda:code[@code='1234']";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule37()
        {
            string actual = GenerateAssertionForRule(37);
            string expected = "not(cda:code) or cda:code[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule38()
        {
            string actual = GenerateAssertionForRule(38);
            string expected = "count(cda:observation[cda:title]) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule39()
        {
            string actual = GenerateAssertionForRule(39);
            string expected = "count(cda:observation[cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']]) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule40()
        {
            string actual = GenerateAssertionForRule(40);
            string expected = "count(cda:observation[cda:value[@xsi:type='CD']]) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule41()
        {
            string actual = GenerateAssertionForRule(41);
            string expected = "count(cda:entry/cda:observation[cda:title]) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule42()
        {
            string actual = GenerateAssertionForRule(42);
            string expected = "count(cda:observation[cda:title])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule43()
        {
            string actual = GenerateAssertionForRule(43);
            string expected = "count(cda:observation[cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule44()
        {
            string actual = GenerateAssertionForRule(44);
            string expected = "count(cda:observation[cda:value[@xsi:type='CD']])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule45()
        {
            string actual = GenerateAssertionForRule(45);
            string expected = "count(cda:entry/cda:observation[cda:title])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule46()
        {
            string actual = GenerateAssertionForRule(46);
            string expected = "count(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule47()
        {
            string actual = GenerateAssertionForRule(47);
            string expected = "count(cda:observation[cda:title]) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule48()
        {
            string actual = GenerateAssertionForRule(48);
            string expected = "count(cda:observation[cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']]) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule49()
        {
            string actual = GenerateAssertionForRule(49);
            string expected = "count(cda:observation[cda:value[@xsi:type='CD']]) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule50()
        {
            string actual = GenerateAssertionForRule(50);
            string expected = "count(cda:entry/cda:observation[cda:title]) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule51()
        {
            string actual = GenerateAssertionForRule(51);
            string expected = "cda:observation[count(cda:title) > 0]";

            Assert.AreEqual(expected, actual);
        }


        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule52()
        {
            string actual = GenerateAssertionForRule(52);
            string expected = "cda:observation[count(cda:value[@xsi:type='CD']) > 0]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule53()
        {
            string actual = GenerateAssertionForRule(53);
            string expected = "cda:observation[count(cda:value[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) > 0]";
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule54()
        {
            string actual = GenerateAssertionForRule(54);
            string expected = "cda:observation[count(cda:value[@xsi:type='CD' and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) > 0]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule55()
        {
            string actual = GenerateAssertionForRule(55);
            string expected = "count(cda:observation[cda:value[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]]) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule56()
        {
            string actual = GenerateAssertionForRule(56);
            string expected = "count(cda:observation[cda:value[@xsi:type='CD' and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]]) > 0";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule57()
        {
            string actual = GenerateAssertionForRule(57);
            string expected = "count(cda:code) = (1 or 2 or 3)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule58()
        {
            string actual = GenerateAssertionForRule(58);
            string expected = "count(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']) = (1 or 2 or 3)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule59()
        {
            string actual = GenerateAssertionForRule(59);
            string expected = "count(cda:observation[cda:title]) = (1 or 2 or 3)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule60()
        {
            string actual = GenerateAssertionForRule(60);
            string expected = "count(cda:observation[cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']]) = (1 or 2 or 3)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule61()
        {
            string actual = GenerateAssertionForRule(61);
            string expected = "count(cda:observation[cda:value[@xsi:type='CD']]) = (1 or 2 or 3)";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule62()
        {
            string actual = GenerateAssertionForRule(62);
            string expected = "count(cda:entry/cda:observation[cda:title]) = (1 or 2 or 3)";

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule63()
        {
            string actual = GenerateAssertionForRule(63);
            string expected = "cda:observation[not(cda:code) or cda:code]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule64()
        {
            string actual = GenerateAssertionForRule(64);
            string expected = "cda:observation[not(cda:value) or cda:value[@xsi:type='CD']]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule65()
        {
            string actual = GenerateAssertionForRule(65);
            string expected = "cda:observation[not(cda:value) or cda:value[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule66()
        {
            string actual = GenerateAssertionForRule(66);
            string expected = "cda:observation[not(cda:value) or cda:value[@xsi:type='CD' and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule67()
        {
            string actual = GenerateAssertionForRule(67);
            string expected = "cda:observation[not(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']) or cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule68()
        {
            string actual = GenerateAssertionForRule(68);
            string expected = "cda:observation[count(cda:code) < 4]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule69()
        {
            string actual = GenerateAssertionForRule(69);
            string expected = "count(cda:observation[cda:code]) < 4";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule70()
        {
            string actual = GenerateAssertionForRule(70);
            string expected = "count(cda:value[@xsi:type='CD']) < 4";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule71()
        {
            string actual = GenerateAssertionForRule(71);
            string expected = "cda:observation[count(cda:value[@xsi:type='CD']) < 4]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule72()
        {
            string actual = GenerateAssertionForRule(72);
            string expected = "count(cda:value[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) < 4";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule73()
        {
            string actual = GenerateAssertionForRule(73);
            string expected = "count(cda:value[@xsi:type='CD' and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) < 4";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule74()
        {
            string actual = GenerateAssertionForRule(74);
            string expected = "count(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']) < 4";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule75()
        {
            string actual = GenerateAssertionForRule(75);
            string expected = "cda:observation[count(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']) < 4]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule76()
        {
            string actual = GenerateAssertionForRule(76);
            string expected = "cda:observation[count(cda:value[@xsi:type='CD' and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) < 4]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule77()
        {
            string actual = GenerateAssertionForRule(77);
            string expected = "cda:observation[count(cda:title) = (1 or 2 or 3)]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule78()
        {
            string actual = GenerateAssertionForRule(78);
            string expected = "cda:observation[count(cda:value[@xsi:type='CD']) = (1 or 2 or 3)]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule79()
        {
            string actual = GenerateAssertionForRule(79);
            string expected = "cda:observation[count(cda:value[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) = (1 or 2 or 3)]";

            Assert.AreEqual(expected, actual);
            // TODO: Need to remove outer count() for parent xpath, and only perform a count on 
            // the current constraint (within the brackets of the parent). This needs to happen for
            // all constraints where there is one (or multiple levels of a) parent
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule80()
        {
            string actual = GenerateAssertionForRule(80);
            string expected = "cda:observation[count(cda:value[@xsi:type='CD' and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor]) = (1 or 2 or 3)]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule81()
        {
            string actual = GenerateAssertionForRule(81);
            string expected = "cda:observation[count(cda:title[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test title']) = (1 or 2 or 3)]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule82()
        {
            string actual = GenerateAssertionForRule(82);
            string expected = "count(cda:code)=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule83()
        {
            string actual = GenerateAssertionForRule(83);
            string expected = "count(cda:observation)=1";  

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule84()
        {
            string actual = GenerateAssertionForRule(84);
            string expected = "cda:entryRelationship/cda:observation/cda:code[@code]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule85()
        {
            string actual = GenerateAssertionForRule(85);
            string expected = "cda:entryRelationship/cda:observation/cda:code[@code]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule86()
        {
            string actual = GenerateAssertionForRule(86);
            string expected = "count(cda:entryRelationship[@typeCode='REFR'][count(cda:observation[@classCode='OBS'][count(cda:code)=1][count(cda:code)=1]) > 0])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule87()
        {
            string actual = GenerateAssertionForRule(87);
            string expected = "count(cda:entryRelationship[@typeCode='REFR'][count(cda:observation[@classCode='OBS'][count(cda:statusCode[@code='active'])=1][count(cda:code)=1])=1])=1";
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule88()
        {
            string actual = GenerateAssertionForRule(88);
            string expected = "count(self::node()[@classCode='OBS'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule89()
        {
            string actual = GenerateAssertionForRule(89);
            string expected = "count(cda:entryRelationship[@typeCode='REFR'][count(cda:observation[@classCode='OBS'][count(cda:statusCode[@code='active'])=1][count(cda:code)=1])=1])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule90()
        {
            string actual = GenerateAssertionForRule(90);
            string expected = "cda:entryRelationship[count(*[cda:templateId[@root='5.4.3.2.1']])=1]";
            // Note: Rule 91 shows when a context is specific for the template. This rule is for when a context is not specified for the template.
            // In this case, it should just look for any child elements that have a templateId matching the template.
            // The parent, in this case, is not a branched constraint, so we don't need to wrap the assertion in a count, because the 
            // count will be tested in a different assertion for the parent

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule91()
        {
            string actual = GenerateAssertionForRule(91);
            string expected = "cda:entryRelationship[count(cda:observation[cda:templateId[@root='5.4.3.2.1']])=1]";
            // Note: Rule 90 shows when a context is NOT specified for the template. This rule is for when a context IS specific to the template
            // In this case, look for a child observation which has a template with the specified template id
            // The parent, in this case, is not a branched constraint, so we don't need to wrap the assertion in a count, because the 
            // count will be tested in a different assertion for the parent

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule92()
        {
            string actual = GenerateAssertionForRule(92);
            // Note: This is similar to 91, except the parent is branched.
            string expected = "count(cda:observation[cda:templateId[@root='5.4.3.2.1']])=1";  

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule93()
        {
            string actual = GenerateAssertionForRule(93);
            string expected = "count(cda:templateId[@root='1.2.3.4.5'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule94()
        {
            string actual = GenerateAssertionForRule(94);
            string expected = "count(cda:templateId[@root='1.2.3.4.5']) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule95()
        {
            string actual = GenerateAssertionForRule(95);
            string expected = "count(cda:entryRelationship[@typeCode='RSON']) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule96()
        {
            string actual = GenerateAssertionForRule(96);
            string expected = "count(cda:entryRelationship[@typeCode='REFR'][count(cda:observation[@classCode='OBS'][count(cda:statusCode[@code='active'])=1][count(cda:code)=1])=1]) < 2";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule97()
        {
            string actual = GenerateAssertionForRule(97);
            string expected = "count(cda:statusCode[@code='1.2.3.4'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule98()
        {
            string actual = GenerateAssertionForRule(98);
            string expected = "count(cda:code[@code='4.3.2.1'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule99()
        {
            string actual = GenerateAssertionForRule(99);
            string expected = "not(cda:entryRelationship[@typeCode='DRIV']) or cda:entryRelationship[@typeCode='DRIV']";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule100()
        {
            string actual = GenerateAssertionForRule(100);
            string expected = "not(cda:entryRelationship[@typeCode='DRIV']) or cda:entryRelationship[@typeCode='DRIV']";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule101()
        {
            string actual = GenerateAssertionForRule(101);
            string expected = "not(cda:entryRelationship[@typeCode='DRIV'][count(cda:act[cda:templateId[@root='5.4.3.2.1']])=1]) or cda:entryRelationship[@typeCode='DRIV'][count(cda:act[cda:templateId[@root='5.4.3.2.1']])=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule102()
        {
            string actual = GenerateAssertionForRule(102);
            string expected = "count(cda:reference[@typeCode='REFR'][count(cda:externalDocument)=1])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule104()
        {
            string actual = GenerateAssertionForRule(104);
            string expected = "not(cda:value[@xsi:type='CD']/cda:qualifier) or cda:value[@xsi:type='CD']/cda:qualifier[count(cda:name)=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule105()
        {
            string actual = GenerateAssertionForRule(105);
            string expected = "not(cda:code/cda:originalText) or cda:code/cda:originalText[count(cda:reference)=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule106()
        {
            string actual = GenerateAssertionForRule(106);
            string expected = "not(cda:code/cda:originalText/cda:reference) or cda:code/cda:originalText/cda:reference[@value]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule107()
        {
            string actual = GenerateAssertionForRule(107);
            string expected = "not(cda:text) or cda:text[count(cda:reference)=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule108()
        {
            string actual = GenerateAssertionForRule(108);
            string expected = "cda:value[@xsi:type='CD'][@code and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value]";
            // Note: [@code][@code=...] because the first is asserting cardinality and the second is asserting the VALUE of the @code

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule109()
        {
            string actual = GenerateAssertionForRule(109);
            string expected = "cda:value[@xsi:type='CD'][@code and @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value]";
            // Note: [@code][@code=...] because the first is asserting cardinality and the second is asserting the VALUE of the @code

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")] 
        public void TestAssertionLineBuilderRule110()
        {
            string actual = GenerateAssertionForRule(110);
            string expected = "cda:value[@xsi:type='CD'][not(@code) or @code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value]";
            // Note: [@code][@code=...] because the first is asserting cardinality and the second is asserting the VALUE of the @code

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule111()
        {
            string actual = GenerateAssertionForRule(111);
            string expected = "not(cda:specimen/cda:specimenRole) or cda:specimen/cda:specimenRole[not(cda:id) or cda:id]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule112()
        {
            string actual = GenerateAssertionForRule(112);
            string expected = "cda:value[@xsi:type='CD'][@code and @code=document('1.2.3.4.xml')/svs:RetrieveValueSetResponse/svs:ValueSet[@id='1.2.3.4']/svs:ConceptList/svs:Concept/@code]";            

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule113()
        {
            string actual = GenerateAssertionForRule(113);
            string expected = "cda:value[@xsi:type='CD'][@code and @code=document('voc.xml')/svs:RetrieveMultipleValueSetsResponse/svs:DescribedValueSet[@ID='1.2.3.4']/svs:ConceptList/svs:Concept/@code]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule114()
        {
            string actual = GenerateAssertionForRule(114);
            string expected = "cda:code[count(self::node()[@code='1234' and @codeSystem='6.36'])=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule115()
        {
            string actual = GenerateAssertionForRule(115);
            string expected = "cda:observation[count(cda:code[@code='1234'][@codeSystem='6.36' or @nullFlavor])=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule116()
        {
            string actual = GenerateAssertionForRule(116);
            string expected = "@code='1234'";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule117()
        {
            string actual = GenerateAssertionForRule(117);
            string expected = "cda:encounter[@classCode='ENC']";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule118()
        {
            string actual = GenerateAssertionForRule(118);
            string expected = "@classCode='ENC'";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule119()
        {
            string actual = GenerateAssertionForRule(119);
            string expected = "count(cda:externalDocument[@classCode='DOC'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule120()
        {
            string actual = GenerateAssertionForRule(120);
            string expected = "cda:value[@xsi:type='CD'][@code]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule121()
        {
            string actual = GenerateAssertionForRule(121);
            string expected = "not(cda:documentationOf) or cda:documentationOf[cda:serviceEvent[@classCode='PCPR']]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule122()
        {
            string actual = GenerateAssertionForRule(122);
            string expected = "not(cda:subject/cda:relatedSubject/cda:subject) or cda:subject/cda:relatedSubject/cda:subject[count(cda:administrativeGenderCode)=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule123()
        {
            string actual = GenerateAssertionForRule(123);
            string expected = "count(cda:performer[@typeCode][count(cda:assignedEntity[count(cda:code)=1])=1][count(cda:templateId[@root='2.16.840.1.113883.10.20.22.4.87'])=1])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule124()
        {
            string actual = GenerateAssertionForRule(124);
            string expected = "not(cda:entryRelationship) or cda:entryRelationship[@typeCode='RSON']";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule126()
        {
            string actual = GenerateAssertionForRule(126);
            string expected = "count(self::node()[@typeCode='DRIV'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule127()
        {
            string actual = GenerateAssertionForRule(127);
            string expected = "count(cda:act[cda:templateId[@root='5.4.3.2.1']])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule128()
        {
            string actual = GenerateAssertionForRule(128);
            string expected = "cda:externalDocument[count(cda:id[@root])=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule129()
        {
            string actual = GenerateAssertionForRule(129);
            string expected = "cda:externalDocument[count(cda:id)=1]";

            Assert.AreEqual(expected, actual);
        }


        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule130()
        {
            string actual = GenerateAssertionForRule(130);
            string expected = "count(cda:reference)=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule131()
        {
            string actual = GenerateAssertionForRule(131);
            string expected = "count(self::node()[@typeCode='REFR'])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule132()
        {
            string actual = GenerateAssertionForRule(132);
            string expected = "count(cda:externalDocument)=1";

            Assert.AreEqual(expected, actual);
        }


        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule133()
        {
            string actual = GenerateAssertionForRule(133);
            string expected = "count(cda:id)=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule134()
        {
            string actual = GenerateAssertionForRule(134);
            string expected = "cda:reference[count(cda:externalDocument)=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule135()
        {
            string actual = GenerateAssertionForRule(135);
            string expected = "not(cda:reference) or cda:reference[count(cda:externalDocument)=1]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule136()
        {
            string actual = GenerateAssertionForRule(136);
            string expected = "count(cda:externalDocument)=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule137()
        {
            string actual = GenerateAssertionForRule(137);
            string expected = "count(sdtc:raceCode)=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule138()
        {
            string actual = GenerateAssertionForRule(138);
            string expected = "cda:code[not(@code)]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule139()
        {
            string actual = GenerateAssertionForRule(139);
            string expected = "count(cda:code)=1"; //SHALL with SHOULD value conformance (valueset specified)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule140()
        {
            string actual = GenerateAssertionForRule(140); //SHALL with SHOULD value conformance
            string expected = "count(cda:code)=1";

            Assert.AreEqual(expected, actual);
        }


        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule141()
        {
            string actual = GenerateAssertionForRule(141);
            string expected = "count(cda:code[@code=document('voc.xml')/voc:systems/voc:system[@valueSetOid='1.2.3.4']/voc:code/@value or @nullFlavor])=1"; //SHALL with a valueset 
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule142()
        {
            string actual = GenerateAssertionForRule(142);
            string expected = "count(cda:code[@codeSystem='6.36' or @nullFlavor])=1"; 
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule143()
        {
            string actual = GenerateAssertionForRule(143);
            string expected = "count(hqmf:observationCriteria[hqmf:templateId/hqmf:item[@root='3.2.1.4.5.1']])=1";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule144()
        {
            string actual = GenerateAssertionForRule(144);
            string expected = "sdtc:actReference[@classCode]";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestAssertionLineBuilderRule145()
        {
            string actual = GenerateAssertionForRule(145);
            string expected = "sdtc:actReference[count(@classCode) > 0]";

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Context Generation Tests

        [TestMethod, TestCategory("Schematron")]
        public void TestParticipant1()
        {
            var ig = ruleRepo.FindOrCreateImplementationGuide("CDA", "Test IG");
            var template = ruleRepo.CreateTemplate("urn:oid:2.16.92.3.1.5.2.34", "document", "Test Template", ig, "ClinicalDocument", "ClinicalDocument");

            var participant = ruleRepo.AddConstraintToTemplate(template, null, null, "participant", "MAY", "0..*");
            var associatedEntity = ruleRepo.AddConstraintToTemplate(template, participant, null, "associatedEntity", "SHALL", "1..1");
            var id = ruleRepo.AddConstraintToTemplate(template, associatedEntity, null, "id", "SHALL", "1..1", isBranch: true);
            var nullFlavor = ruleRepo.AddConstraintToTemplate(template, id, null, "@nullFlavor", "SHALL NOT", "0..0");
            var root = ruleRepo.AddConstraintToTemplate(template, id, null, "@root", "SHALL", "1..1", isBranchIdentifier: true, value: "2.16.840.1.113883.3.2074.1", displayName: "CMS EHR Certification Number");
            var extension = ruleRepo.AddConstraintToTemplate(template, id, null, "@extension", "SHALL", "1..1");

            TemplateContextBuilder contextBuilder = new TemplateContextBuilder(ig.ImplementationGuideType);
            string actual = contextBuilder.CreateFullBranchedParentContext(template, id);
            string expected = "cda:ClinicalDocument[cda:templateId[@root='2.16.92.3.1.5.2.34']]/cda:participant/cda:associatedEntity/cda:id[@root='2.16.840.1.113883.3.2074.1']";

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
