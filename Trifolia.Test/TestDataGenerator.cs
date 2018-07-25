using System.Linq;

using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Test
{
    public static class TestDataGenerator
    {
        public const string DS1_IG_NAME = "Test Implementation Guide";
        public const string DS2_IG_NAME = "The IG";

        #region Data Set 1 - IG Generation and Trifolia Export Testing

        /// <summary>
        /// Data set is used to test implementation guide generation (the MS Word document export) and to test importing/exporting 
        /// implementation guides in the legacy trifolia format.
        /// </summary>
        public static MockObjectRepository GenerateMockDataset1()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();

            Organization internalOrg = mockRepo.FindOrCreateOrganization("Lantana");
            Organization testOrg = mockRepo.FindOrCreateOrganization("Test Organization");

            ImplementationGuideType igType = mockRepo.FindOrCreateImplementationGuideType(Constants.IGTypeNames.CDA, Constants.IGTypeSchemaLocations.CDA, Constants.IGTypePrefixes.CDA, Constants.IGTypeNamespaces.CDA);

            TemplateType docType = mockRepo.FindOrCreateTemplateType(igType, "Document", "ClinicalDocument", "ClinicalDocument", 1);
            TemplateType sectionType = mockRepo.FindOrCreateTemplateType(igType, "Section", "Section", "Section", 2);
            TemplateType entryType = mockRepo.FindOrCreateTemplateType(igType, "Entry", "entry", "Entry", 3);
            TemplateType subEntryType = mockRepo.FindOrCreateTemplateType(igType, "Sub-Entry", "entry", "Entry", 4);
            TemplateType otherType = mockRepo.FindOrCreateTemplateType(igType, "Other", string.Empty, string.Empty, 5);

            mockRepo.FindOrCreateValueSet("Test Value Set", "9.8.7.6.5.4.3.2.1");

            ImplementationGuide ig = mockRepo.FindOrCreateImplementationGuide(igType, DS1_IG_NAME, internalOrg);
            mockRepo.FindOrCreateImplementationGuide(igType, "Test IG 2", testOrg);
            mockRepo.FindOrCreateImplementationGuide(igType, "Test IG 3", internalOrg);

            Template template1 = mockRepo.CreateTemplate("1.2.3.4.5", docType, "Test Template 1", ig, null, null, "Test Description 2", "Template Level Note for Test Template 1");
            template1.Notes = "Template 1 Note";

            template1.TemplateSamples.Add(new TemplateSample()
            {
                XmlSample = @"<observation/>",
                Name = "Test_Template_1_Example"
            });

            // Basic constraint, nothing special
            TemplateConstraint t1tc1 = mockRepo.AddConstraintToTemplate(template1, null, null, "templateId", "SHALL", "1..1");
            t1tc1.Notes = "Constraint TemplateId comment for Test Template 1";

            // Constraint with a child
            TemplateConstraint t1tc2 = mockRepo.AddConstraintToTemplate(template1, null, null, "code", "SHALL", "1..1");
            TemplateConstraint t1tc2_2 = mockRepo.AddConstraintToTemplate(template1, t1tc2, null, "@code", "SHALL", "1..1", null, null, "12345X", "Test Static Value");

            // Constraint with a child (child has valueset)
            ValueSet t1tc3_vs = mockRepo.FindOrCreateValueSet("Test Valueset", "9.8.7.6.5.4.3.2.1");
            TemplateConstraint t1tc3 = mockRepo.AddConstraintToTemplate(template1, null, null, "code", "SHALL", "1..1");
            TemplateConstraint t1tc3_1 = mockRepo.AddConstraintToTemplate(template1, t1tc3, null, "@code", "SHALL", "1..1", "CE", "SHALL", null, null, t1tc3_vs);
            t1tc3_1.IsStatic = true;

            Template template2 = mockRepo.CreateTemplate("1.2.3.4.5.6", docType, "Test Template 2", ig, null, null, "Test Description 1", "Template Level Note for Test Template 2");
            template2.ImpliedTemplate = template1;

            // Constraint with a child
            TemplateConstraint t2tc1 = mockRepo.AddConstraintToTemplate(template2, null, null, "code", "SHALL", "1..1");

            Template template3 = mockRepo.CreateTemplate("1.2.3.4.5.6.7", docType, "Test Template 3", ig, null, null, "Test Description 3", "Template Level Note for Test Template 3");

            TemplateConstraint t3tc1 = mockRepo.AddConstraintToTemplate(template3, null, template2, null, "SHALL", "1..1");
            TemplateConstraint t3tc2 = mockRepo.AddConstraintToTemplate(template3, null, null, "entry", "SHALL", "1..1");
            TemplateConstraint t3tc2_1 = mockRepo.AddConstraintToTemplate(template3, t3tc2, template2, "observation", "SHALL", "1..1");

            Template template4 = mockRepo.CreateTemplate("8.2234.19.234.11", docType, "Test Constraint Description Template", ig, null, null, null, null);
            mockRepo.AddConstraintToTemplate(template4, null, null, "code", "SHALL", "1..1", "CD", null, null, null, null, null, "Test constraint description");

            return mockRepo;
        }

        #endregion

        #region Data Set 2 - Narrative Constraint Generation Testing

        /// <summary>
        /// Used to test different narrative constraint generation combinations.
        /// Populates the mock repository with CDA implementation guide and some basic templates
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

            ImplementationGuideType igType = mockRepo.FindOrCreateImplementationGuideType(Constants.IGTypeNames.CDA, Constants.IGTypeSchemaLocations.CDA, Constants.IGTypePrefixes.CDA, Constants.IGTypeNamespaces.CDA);

            ImplementationGuide ig = mockRepo.FindOrCreateImplementationGuide(igType, DS2_IG_NAME);

            TemplateType docType = mockRepo.FindOrCreateTemplateType(igType, "Document", "ClinicalDocument", "ClinicalDocument", 1);

            Template template1 = mockRepo.CreateTemplate("1.2.3.4.5.6", docType, "Test Template 1", ig, null, null, "Test Description 1", "Test Notes 1");
            Template template2 = mockRepo.CreateTemplate("1.2.3.4.5.6.5", docType, "Test Template 2", ig, null, null, "Test Description 2", "Test Notes 2");

            mockRepo.AddConstraintToTemplate(template1, null, null, "value", "SHALL", "1..1", null, null, null, null, null, null);
            mockRepo.AddConstraintToTemplate(template1, null, null, "@classCode", "SHALL", "1..1", null, null, "OBS", "Observation", null, mockRepo.CodeSystems.Single(y => y.Id == 2));
            mockRepo.AddConstraintToTemplate(template1, null, null, "templateId/@root", "SHALL", "1..1", null, null, "22.4.47", null, null, null);
            mockRepo.AddConstraintToTemplate(template1, null, null, "code", "SHALL", "1..1", "CD", null, null, null, null, null);
            mockRepo.AddConstraintToTemplate(template1, null, template2, null, "MAY", "0..1", null, null, null, null, null, null);
            mockRepo.AddConstraintToTemplate(template1, null, null, "administrativeGenderCode/@code", "SHALL", "1..1", null, "MAY", null, null, mockRepo.ValueSets.Single(y => y.Id == 1), null);
            mockRepo.AddConstraintToTemplate(template1, null, null, "statusCode", "SHALL", "1..1", null, null, "completed", "Completed", null, mockRepo.CodeSystems.Single(y => y.Id == 2));
            mockRepo.AddConstraintToTemplate(template1, null, null, "code/@code", "SHALL", "1..1", "CD", "SHALL", "1234-X", "Test Disp", null, mockRepo.CodeSystems.Single(y => y.Id == 1));
            mockRepo.AddConstraintToTemplate(template1, null, null, "code", "SHALL", "1..1", "CD", "SHALL", "1234-X", "Test Disp", null, mockRepo.CodeSystems.Single(y => y.Id == 1));

            Template template3 = mockRepo.CreateTemplate("1.2.3.4.5.6.7", docType, "Test Template 3", ig, null, null, "Test Description 3", "");

            TemplateConstraint template3_c1 = mockRepo.AddConstraintToTemplate(template3, null, null, "code", "SHALL", "1..1");
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

            ImplementationGuideType igType = mockRepo.FindOrCreateImplementationGuideType(Constants.IGTypeNames.CDA, Constants.IGTypeSchemaLocations.CDA, Constants.IGTypePrefixes.CDA, Constants.IGTypeNamespaces.CDA);
            TemplateType docType = mockRepo.FindOrCreateTemplateType(igType, "Document", "ClinicalDocument", "ClinicalDocument", 1);
            TemplateType sectionType = mockRepo.FindOrCreateTemplateType(igType, "Section", "section", "Section", 2);

            mockRepo.FindOrCreateDataType(igType, "II");
            mockRepo.FindOrCreateDataType(igType, "INT");
            mockRepo.FindOrCreateDataType(igType, "TS");
            mockRepo.FindOrCreateDataType(igType, "CE");

            ImplementationGuide ig1 = mockRepo.FindOrCreateImplementationGuide(igType, "Test IG 1");
            Template t1 = mockRepo.CreateTemplate("urn:oid:1.2.3.4", docType, "Test Template 1", ig1, null, null, null);

            TemplateConstraint tc1 = mockRepo.AddConstraintToTemplate(t1, null, null, "code", "SHALL", "1..1", "CE");
            TemplateConstraint tc1_1 = mockRepo.AddConstraintToTemplate(t1, tc1, null, "@code", "SHALL", "1..1", null, "SHALL", "1234-x", "Test Doc Code", null, null);
            TemplateConstraint tc1_2 = mockRepo.AddConstraintToTemplate(t1, tc1, null, "@codeSystem", "SHALL", "1..1", null, "SHALL", "1.5.4.2.3", "Test Code System OID", null, null);
            TemplateConstraint tc2 = mockRepo.AddConstraintToTemplate(t1, null, null, "setId", "SHALL", "1..1", "II");
            TemplateConstraint tc3 = mockRepo.AddConstraintToTemplate(t1, null, null, "versionNumber", "SHALL", "1..1", "INT");
            TemplateConstraint tc4 = mockRepo.AddConstraintToTemplate(t1, null, null, "recordTarget", "SHALL", "1..*", null);
            TemplateConstraint tc4_1 = mockRepo.AddConstraintToTemplate(t1, tc4, null, "patientRole", "SHALL", "1..1", null);
            TemplateConstraint tc4_1_1 = mockRepo.AddConstraintToTemplate(t1, tc4_1, null, "id", "SHALL", "1..1", "II");
            TemplateConstraint tc4_1_2 = mockRepo.AddConstraintToTemplate(t1, tc4_1, null, "patient", "SHALL", "1..1", null);
            TemplateConstraint tc4_1_2_1 = mockRepo.AddConstraintToTemplate(t1, tc4_1_2, null, "birthTime", "SHALL", "1..1", "TS");
            TemplateConstraint tc4_1_2_2 = mockRepo.AddConstraintToTemplate(t1, tc4_1_2, null, "administrativeGenderCode", "SHALL", "1..1", "CE");

            return mockRepo;
        }

        #endregion

        #region Data Set 4 - IG Generation and Trifolia Export Content and Settings Testing 

        public static MockObjectRepository GenerateMockDataset4()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();

            Organization internalOrg = mockRepo.FindOrCreateOrganization("Lantana");
            Organization testOrg = mockRepo.FindOrCreateOrganization("Test Organization");

            ImplementationGuideType igType = mockRepo.FindOrCreateImplementationGuideType(Constants.IGTypeNames.CDA, Constants.IGTypeSchemaLocations.CDA, Constants.IGTypePrefixes.CDA, Constants.IGTypeNamespaces.CDA);

            TemplateType docType = mockRepo.FindOrCreateTemplateType(igType, "Document", "ClinicalDocument", "ClinicalDocument", 1);
            TemplateType sectionType = mockRepo.FindOrCreateTemplateType(igType, "Section", "Section", "Section", 2);
            TemplateType entryType = mockRepo.FindOrCreateTemplateType(igType, "Entry", "entry", "Entry", 3);
            TemplateType subEntryType = mockRepo.FindOrCreateTemplateType(igType, "Sub-Entry", "entry", "Entry", 4);
            TemplateType otherType = mockRepo.FindOrCreateTemplateType(igType, "Other", string.Empty, string.Empty, 5);

            

            mockRepo.FindOrCreateCodeSystem("SNOMED CT", "urn:oid:2.16.840.1.113883.6.96");
            mockRepo.FindOrCreateCodeSystem("LOINC", "urn:oid:2.16.840.1.113883.6.1");
            mockRepo.FindOrCreateCodeSystem("HL7ParticipationType", "urn:oid:2.16.840.1.113883.5.90");
            mockRepo.FindOrCreateCodeSystem("HL7ActStatus", "urn:oid:113883.5.14");
            CodeSystem hl7CodeSystem = mockRepo.FindOrCreateCodeSystem("SNOMED CT", "urn:oid:2.16.840.1.113883.6.96");

            var valueSet = mockRepo.FindOrCreateValueSet("Treatment status", "urn:oid:2.16.840.1.114222.4.11.3203");
            mockRepo.FindOrCreateValueSetMember(valueSet, hl7CodeSystem, "5561003", "Active");
            mockRepo.FindOrCreateValueSetMember(valueSet, hl7CodeSystem, "73425007", "Inactive");
            mockRepo.FindOrCreateValueSetMember(valueSet, hl7CodeSystem, "14321245", "On hold");
            mockRepo.FindOrCreateValueSetMember(valueSet, hl7CodeSystem, "5382145", "Requested");
            mockRepo.FindOrCreateValueSetMember(valueSet, hl7CodeSystem, "7789215", "Cancelled");

            ImplementationGuide ig = mockRepo.FindOrCreateImplementationGuide(igType, DS1_IG_NAME, internalOrg);
            mockRepo.FindOrCreateImplementationGuide(igType, "Test IG 2", testOrg);
            mockRepo.FindOrCreateImplementationGuide(igType, "Test IG 3", internalOrg);

            Template template1 = mockRepo.CreateTemplate("1.2.3.4.5", docType, "Test Template 1", ig, null, null, "Test Description 2", "Template Level Note for Test Template 1");
            template1.Notes = "Template Level Note for Test Template 1";


            mockRepo.AddConstraintToTemplate(template1, null, null, "code", "SHALL", "1..1", "CD", "SHALL", "51897-7", "Test Disp", null, mockRepo.CodeSystems.Single(y => y.Id == 2));

            TemplateConstraint t1 = mockRepo.AddConstraintToTemplate(template1, null, null, "participant", "SHALL", "1..1", "CD", "SHALL", null, null, null, null, null, true, true);
            
            mockRepo.AddConstraintToTemplate(template1, t1, null, "@typeCode", "SHALL", "1..1", null, "SHALL", "LOC", "Location", null, mockRepo.CodeSystems.Single(y => y.Id == 3));

            // Primitive constraint
            mockRepo.AddPrimitiveToTemplate(template1, null, "SHALL", "A templateId element SHALL be present representing conformance to this release of the Implementation Guide");


            template1.TemplateSamples.Add(new TemplateSample()
            {
                XmlSample = @"<observation/>",
                Name = "Test_Template_1_Example"
            });

            // Basic constraint
            TemplateConstraint t1tc1 = mockRepo.AddConstraintToTemplate(template1, null, null, "templateId", "SHALL", "1..1", "CD", null, null, null, null, null, null, null, null);
            t1tc1.Notes = "Constraint *TemplateId* Note for Test Template 1";

            // Basic Constraint with a child
            TemplateConstraint t1tc2 = mockRepo.AddConstraintToTemplate(template1, null, null, "code", "SHALL", "1..1");
            TemplateConstraint t1tc2_2 = mockRepo.AddConstraintToTemplate(template1, t1tc2, null, "@value", "SHALL", "1..1", null, null, "236435004", "Test Static Value", null, mockRepo.CodeSystems.Single(y => y.Id == 1));

            // Constraint with a child (child has valueset)
            ValueSet t1tc3_vs = mockRepo.FindOrCreateValueSet("Test Valueset", "9.8.7.6.5.4.3.2.1");
            TemplateConstraint t1tc3 = mockRepo.AddConstraintToTemplate(template1, null, null, "code", "SHALL", "1..1");
            //TemplateConstraint t1tc3_1 = mockRepo.AddConstraintToTemplate(template1, t1tc3, null, "@code", "SHALL", "1..1", "CE", "SHALL", null, null, t1tc3_vs);
            TemplateConstraint t1tc3_1 = mockRepo.AddConstraintToTemplate(template1, t1tc3, null, "@code", "SHALL", "1..1", "CE", "SHALL", "55561003", "Active", mockRepo.ValueSets.Single(y => y.Id == 1));
            t1tc3.Notes = "Child *code* constraint with Value Set Notes";
            t1tc3_1.IsStatic = true;

            Template template2 = mockRepo.CreateTemplate("1.2.3.4.5.6", docType, "Test Template 2", ig, null, null, "Test Description 1", "Template Level Note for Test Template 2");
            template2.ImpliedTemplate = template1;

            // Constraint with a child
            TemplateConstraint t2tc1 = mockRepo.AddConstraintToTemplate(template2, null, null, "code", "SHALL", "1..1");

            Template template3 = mockRepo.CreateTemplate("1.2.3.4.5.6.7", docType, "Test Template 3", ig, null, null, "Test Description 3", "Template Level Note for Test Template 3");

            TemplateConstraint t3tc1 = mockRepo.AddConstraintToTemplate(template3, null, template2, null, "SHALL", "1..1");
            TemplateConstraint t3tc2 = mockRepo.AddConstraintToTemplate(template3, null, null, "entry", "SHALL", "1..1");
            TemplateConstraint t3tc2_1 = mockRepo.AddConstraintToTemplate(template3, t3tc2, template2, "observation", "SHALL", "1..1");

            Template template4 = mockRepo.CreateTemplate("8.2234.19.234.11", docType, "Test Constraint Description Template", ig, null, null, null, null);
            mockRepo.AddConstraintToTemplate(template4, null, null, "code", "SHALL", "1..1", "CD", null, null, null, null, null, "Test constraint description");

            // Create a new version of the implementation guide
            ImplementationGuide igVersion = mockRepo.FindOrCreateImplementationGuide(igType, DS1_IG_NAME + " Version 2");
            igVersion.SetPreviousVersion(ig);
            igVersion.Version = 2;

            // Create a new version of template 3 within the new version of the IG
            Template template3Version = mockRepo.CreateTemplate("urn:hl7ii:1.2.3.4.5.6.7:20180117", docType, "Test Template 3", igVersion, null, null, "Test Description 3 with changes", "Test Note 3 with changes");
            template3Version.SetPreviousVersion(template3);

            // Create an entirely new template for the new version of the IG
            Template template4Version = mockRepo.CreateTemplate("urn:oid:1.2.3.4.1.2.3.4", sectionType, "Test Template 4", igVersion);
            var t4tc1 = mockRepo.AddConstraintToTemplate(template4Version, null, null, "component", "SHALL", "1..1");
            mockRepo.AddConstraintToTemplate(template4Version, t4tc1, null, "section", "SHALL", "1..1");

            return mockRepo;
        }

        #endregion

    }
}
