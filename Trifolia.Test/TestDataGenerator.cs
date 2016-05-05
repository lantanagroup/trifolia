using System.Linq;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Test
{
    public static class TestDataGenerator
    {
        #region Data Set 1 - IG Generation and Trifolia Export Testing

        /// <summary>
        /// Data set is used to test implementation guide generation (the MS Word document export) and to test importing/exporting 
        /// implementation guides in the legacy trifolia format.
        /// </summary>
        public static MockObjectRepository GenerateMockDataset1()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();

            Organization internalOrg = mockRepo.FindOrAddOrganization("Lantana");
            Organization testOrg = mockRepo.FindOrAddOrganization("Test Organization");

            ImplementationGuideType igType = mockRepo.FindOrCreateImplementationGuideType("CDA", "CDA.xsd", "cda", "urn:hl7-org:v3");

            TemplateType docType = mockRepo.FindOrCreateTemplateType(igType, "Document", "ClinicalDocument", "ClinicalDocument", 1);
            TemplateType sectionType = mockRepo.FindOrCreateTemplateType(igType, "Section", "Section", "Section", 2);
            TemplateType entryType = mockRepo.FindOrCreateTemplateType(igType, "Entry", "entry", "Entry", 3);
            TemplateType subEntryType = mockRepo.FindOrCreateTemplateType(igType, "Sub-Entry", "entry", "Entry", 4);
            TemplateType otherType = mockRepo.FindOrCreateTemplateType(igType, "Other", string.Empty, string.Empty, 5);

            mockRepo.FindOrCreateValueSet("Test Value Set", "9.8.7.6.5.4.3.2.1");

            ImplementationGuide ig = mockRepo.FindOrAddImplementationGuide(igType, "Test Implementation Guide", internalOrg);
            mockRepo.FindOrAddImplementationGuide(igType, "Test IG 2", testOrg);
            mockRepo.FindOrAddImplementationGuide(igType, "Test IG 3", internalOrg);

            Template template1 = mockRepo.GenerateTemplate("1.2.3.4.5", docType, "Test Template 1", ig, null, null, "Test Description 2", "Test Note 1", internalOrg);
            template1.Notes = "This is a test note";

            // Basic constraint, nothing special
            TemplateConstraint t1tc1 = mockRepo.GenerateConstraint(template1, null, null, "templateId", "SHALL", "1..1");
            t1tc1.Notes = "This is a test constraint comment";

            // Constraint with a child
            TemplateConstraint t1tc2 = mockRepo.GenerateConstraint(template1, null, null, "code", "SHALL", "1..1");
            TemplateConstraint t1tc2_2 = mockRepo.GenerateConstraint(template1, t1tc2, null, "@code", "SHALL", "1..1", null, null, "12345X", "Test Static Value");

            // Constraint with a child (child has valueset)
            ValueSet t1tc3_vs = mockRepo.FindOrCreateValueSet("Test Valueset", "9.8.7.6.5.4.3.2.1");
            TemplateConstraint t1tc3 = mockRepo.GenerateConstraint(template1, null, null, "code", "SHALL", "1..1");
            TemplateConstraint t1tc3_1 = mockRepo.GenerateConstraint(template1, t1tc3, null, "@code", "SHALL", "1..1", "CE", "SHALL", null, null, t1tc3_vs);
            t1tc3_1.IsStatic = true;

            Template template2 = mockRepo.GenerateTemplate("1.2.3.4.5.6", docType, "Test Template 2", ig, null, null, "Test Description 1", "Test Note 2", internalOrg);
            template2.ImpliedTemplate = template1;

            // Constraint with a child
            TemplateConstraint t2tc1 = mockRepo.GenerateConstraint(template2, null, null, "code", "SHALL", "1..1");

            Template template3 = mockRepo.GenerateTemplate("1.2.3.4.5.6.7", docType, "Test Template 3", ig, null, null, "Test Description 3", "Test Note 3", internalOrg);

            TemplateConstraint t3tc1 = mockRepo.GenerateConstraint(template3, null, template2, null, "SHALL", "1..1");
            TemplateConstraint t3tc2 = mockRepo.GenerateConstraint(template3, null, null, "entry", "SHALL", "1..1");
            TemplateConstraint t3tc2_1 = mockRepo.GenerateConstraint(template3, t3tc2, template2, "observation", "SHALL", "1..1");

            Template template4 = mockRepo.GenerateTemplate("8.2234.19.234.11", docType, "Test Constraint Description Template", ig, null, null, null, null, testOrg);
            mockRepo.GenerateConstraint(template4, null, null, "code", "SHALL", "1..1", "CD", null, null, null, null, null, "Test constraint description");

            return mockRepo;
        }

        #endregion

        #region Data Set 2 - Narrative Constraint Generation Testing

        /// <summary>
        /// Used to test different narrative constraint generation combinations.
        /// </summary>
        /// <returns></returns>
        public static MockObjectRepository GenerateMockDataset2()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();

            mockRepo.FindOrCreateCodeSystem("SNOMED CT", "6.96");
            mockRepo.FindOrCreateCodeSystem("HL7ActStatus", "113883.5.14");
            CodeSystem hl7CodeSystem = mockRepo.FindOrCreateCodeSystem("HL7", "1.2.3");

            ValueSet genderCodeValueSet = mockRepo.FindOrCreateValueSet("GenderCode", "11.1");
            mockRepo.FindOrCreateValueSetMember(genderCodeValueSet, hl7CodeSystem, "M", "Male", "active", "01/02/2012");
            mockRepo.FindOrCreateValueSetMember(genderCodeValueSet, hl7CodeSystem, "F", "Female", "active", "01/02/2012");
            mockRepo.FindOrCreateValueSetMember(genderCodeValueSet, hl7CodeSystem, "U", "Undetermined", "active", "01/02/2012");

            ImplementationGuideType igType = mockRepo.FindOrCreateImplementationGuideType("CDA", "CDA.xsd", "cda", "urn:hl7-org:v3");

            ImplementationGuide ig = mockRepo.FindOrAddImplementationGuide(igType, "The IG");

            TemplateType docType = mockRepo.FindOrCreateTemplateType(igType, "Document", "ClinicalDocument", "ClinicalDocument", 1);

            Template template1 = mockRepo.GenerateTemplate("1.2.3.4.5.6", docType, "Test Template 1", ig, null, null, "Test Description 1", "Test Notes 1");
            Template template2 = mockRepo.GenerateTemplate("1.2.3.4.5.6.5", docType, "Test Template 2", ig, null, null, "Test Description 2", "Test Notes 2");

            mockRepo.GenerateConstraint(template1, null, null, "value", "SHALL", "1..1", null, null, null, null, null, null);
            mockRepo.GenerateConstraint(template1, null, null, "@classCode", "SHALL", "1..1", null, null, "OBS", "Observation", null, mockRepo.CodeSystems.Single(y => y.Id == 2));
            mockRepo.GenerateConstraint(template1, null, null, "templateId/@root", "SHALL", "1..1", null, null, "22.4.47", null, null, null);
            mockRepo.GenerateConstraint(template1, null, null, "code", "SHALL", "1..1", "CD", null, null, null, null, null);
            mockRepo.GenerateConstraint(template1, null, template2, null, "MAY", "0..1", null, null, null, null, null, null);
            mockRepo.GenerateConstraint(template1, null, null, "administrativeGenderCode/@code", "SHALL", "1..1", null, "MAY", null, null, mockRepo.ValueSets.Single(y => y.Id == 1), null);
            mockRepo.GenerateConstraint(template1, null, null, "statusCode", "SHALL", "1..1", null, null, "completed", "Completed", null, mockRepo.CodeSystems.Single(y => y.Id == 2));
            mockRepo.GenerateConstraint(template1, null, null, "code/@code", "SHALL", "1..1", "CD", "SHALL", "1234-X", "Test Disp", null, mockRepo.CodeSystems.Single(y => y.Id == 1));
            mockRepo.GenerateConstraint(template1, null, null, "code", "SHALL", "1..1", "CD", "SHALL", "1234-X", "Test Disp", null, mockRepo.CodeSystems.Single(y => y.Id == 1));

            Template template3 = mockRepo.GenerateTemplate("1.2.3.4.5.6.7", docType, "Test Template 3", ig, null, null, "Test Description 3", "");

            TemplateConstraint template3_c1 = mockRepo.GenerateConstraint(template3, null, null, "code", "SHALL", "1..1");
            template3_c1.Category = "TestCategory";

            return mockRepo;
        }

        #endregion

        #region Template Generators

        public static Template GenerateTemplate()
        {
            MockObjectRepository lRepository = GenerateGreenMockDataset1();
            return lRepository.Templates.First();
        }

        #endregion

        #region Data Set 3 - Green Schema/Transform Testing

        /// <summary>
        /// Used to test green artifact generation.
        /// </summary>
        /// <returns></returns>
        public static MockObjectRepository GenerateGreenMockDataset1()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();

            mockRepo.FindOrCreateCodeSystem("SNOMED CT", "6.96");
            mockRepo.FindOrCreateCodeSystem("HL7ActStatus", "113883.5.14");
            mockRepo.FindOrCreateValueSet("GenderCode", "11.1");

            ImplementationGuideType igType = mockRepo.FindOrCreateImplementationGuideType("CDA", "CDA.xsd", "cda", "urn:hl7-org:v3");
            TemplateType docType = mockRepo.FindOrCreateTemplateType(igType, "Document", "ClinicalDocument", "ClinicalDocument", 1);
            TemplateType sectionType = mockRepo.FindOrCreateTemplateType(igType, "Section", "section", "Section", 2);

            mockRepo.GenerateDataType(igType, "II");
            mockRepo.GenerateDataType(igType, "INT");
            mockRepo.GenerateDataType(igType, "TS");
            mockRepo.GenerateDataType(igType, "CE");

            ImplementationGuide ig1 = mockRepo.FindOrAddImplementationGuide(igType, "Test IG 1");
            Template t1 = mockRepo.GenerateTemplate("urn:oid:1.2.3.4", docType, "Test Template 1", ig1, null, null, null);

            TemplateConstraint tc1 = mockRepo.GenerateConstraint(t1, null, null, "code", "SHALL", "1..1", "CE");
            TemplateConstraint tc1_1 = mockRepo.GenerateConstraint(t1, tc1, null, "@code", "SHALL", "1..1", null, "SHALL", "1234-x", "Test Doc Code", null, null);
            TemplateConstraint tc1_2 = mockRepo.GenerateConstraint(t1, tc1, null, "@codeSystem", "SHALL", "1..1", null, "SHALL", "1.5.4.2.3", "Test Code System OID", null, null);
            TemplateConstraint tc2 = mockRepo.GenerateConstraint(t1, null, null, "setId", "SHALL", "1..1", "II");
            TemplateConstraint tc3 = mockRepo.GenerateConstraint(t1, null, null, "versionNumber", "SHALL", "1..1", "INT");
            TemplateConstraint tc4 = mockRepo.GenerateConstraint(t1, null, null, "recordTarget", "SHALL", "1..*", null);
            TemplateConstraint tc4_1 = mockRepo.GenerateConstraint(t1, tc4, null, "patientRole", "SHALL", "1..1", null);
            TemplateConstraint tc4_1_1 = mockRepo.GenerateConstraint(t1, tc4_1, null, "id", "SHALL", "1..1", "II");
            TemplateConstraint tc4_1_2 = mockRepo.GenerateConstraint(t1, tc4_1, null, "patient", "SHALL", "1..1", null);
            TemplateConstraint tc4_1_2_1 = mockRepo.GenerateConstraint(t1, tc4_1_2, null, "birthTime", "SHALL", "1..1", "TS");
            TemplateConstraint tc4_1_2_2 = mockRepo.GenerateConstraint(t1, tc4_1_2, null, "administrativeGenderCode", "SHALL", "1..1", "CE");

            // Green Info

            GreenTemplate gt1 = new GreenTemplate()
            {
                Id = 1,
                Template = t1,
                TemplateId = t1.Id,
                Name = "Test Green Template 1"
            };
            mockRepo.GreenTemplates.AddObject(gt1);
            t1.GreenTemplates.Add(gt1);

            GreenConstraint gc1 = mockRepo.GenerateGreenConstraint(gt1, tc2, null, 1, "VersionSet", true);
            GreenConstraint gc2 = mockRepo.GenerateGreenConstraint(gt1, tc3, null, 2, "VersionNumber", true);
            GreenConstraint gc3 = mockRepo.GenerateGreenConstraint(gt1, tc4, null, 3, "Patient", false);
            GreenConstraint gc4 = mockRepo.GenerateGreenConstraint(gt1, tc4_1_1, gc3, 1, "Id", true);
            GreenConstraint gc5 = mockRepo.GenerateGreenConstraint(gt1, tc4_1_2_1, gc3, 2, "BirthDate", true);
            GreenConstraint gc6 = mockRepo.GenerateGreenConstraint(gt1, tc4_1_2_2, gc3, 3, "Gender", true);

            return mockRepo;
        }

        #endregion
    }
}
