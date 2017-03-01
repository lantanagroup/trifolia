using System;
using System.Data.Common;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Reflection;
using Moq;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Authorization;
using System.Collections.ObjectModel;

namespace Trifolia.Test
{
    [Serializable]
    public class MockObjectRepository : IObjectRepository
    {
        public const string DEFAULT_ORGANIZATION = "LCG";
        public const string DEFAULT_FHIR_DSTU1_IG_TYPE_NAME = "FHIR DSTU1";
        public const string DEFAULT_FHIR_DSTU2_IG_TYPE_NAME = "FHIR DSTU2";
        public const string DEFAULT_FHIR_STU3_IG_TYPE_NAME = "FHIR Latest";
        public const string DEFAULT_CDA_IG_TYPE_NAME = "CDA";
        public const string DEFAULT_HQMF_R2_IG_TYPE_NAME = "HQMF R2";
        public const string DEFAULT_USERNAME = "admin";

        public const string DEFAULT_CDA_DOC_TYPE = "Document";
        public const string DEFAULT_CDA_SECTION_TYPE = "Section";
        public const string DEFAULT_CDA_ENTRY_TYPE = "Entry";
        public const string DEFAULT_CDA_SUBENTRY_TYPE = "Subentry";
        public const string DEFAULT_CDA_UNSPECIFIED_TYPE = "Unspecified";

        public const string ADMIN_ROLE = "admin";

        public MockObjectRepository()
        {
            this.PublishStatuses.Add(new PublishStatus()
            {
                Id = 1,
                Status = "Draft"
            });
            this.PublishStatuses.Add(new PublishStatus()
            {
                Id = 2,
                Status = "Ballot"
            });
            this.PublishStatuses.Add(new PublishStatus()
            {
                Id = 3,
                Status = "Published"
            });
            this.PublishStatuses.Add(new PublishStatus()
            {
                Id = 4,
                Status = "Deprecated"
            });
            this.PublishStatuses.Add(new PublishStatus()
            {
                Id = 5,
                Status = "Retired"
            });

            // Add the default admin role
            var adminRole = this.FindOrCreateRole(ADMIN_ROLE);
            adminRole.IsAdmin = true;

            // Add all securables to the system
            this.FindOrCreateSecurables(SecurableNames.ADMIN, SecurableNames.CODESYSTEM_EDIT, SecurableNames.CODESYSTEM_LIST, SecurableNames.EXPORT_GREEN, SecurableNames.EXPORT_SCHEMATRON, 
                SecurableNames.EXPORT_VOCAB, SecurableNames.EXPORT_WORD, SecurableNames.EXPORT_XML, SecurableNames.GREEN_MODEL, SecurableNames.IMPLEMENTATIONGUIDE_AUDIT_TRAIL, 
                SecurableNames.IMPLEMENTATIONGUIDE_EDIT, SecurableNames.IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS, SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT, SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW, 
                SecurableNames.IMPLEMENTATIONGUIDE_LIST, SecurableNames.IMPLEMENTATIONGUIDE_NOTES, SecurableNames.IMPLEMENTATIONGUIDE_PRIMITIVES, SecurableNames.LANDING_PAGE, SecurableNames.ORGANIZATION_DETAILS, 
                SecurableNames.ORGANIZATION_LIST, SecurableNames.PUBLISH_SETTINGS, SecurableNames.REPORT_TEMPLATE_COMPLIANCE, SecurableNames.REPORT_TEMPLATE_REVIEW, SecurableNames.TEMPLATE_COPY, 
                SecurableNames.TEMPLATE_DELETE, SecurableNames.TEMPLATE_EDIT, SecurableNames.TEMPLATE_LIST, SecurableNames.TEMPLATE_MOVE, SecurableNames.TERMINOLOGY_OVERRIDE, SecurableNames.VALUESET_EDIT, 
                SecurableNames.VALUESET_LIST, SecurableNames.WEB_IG);
        }

        /// <summary>
        /// Creates the ImplementationGuideType for CDA, as well as 5 template types (Document, Section, Entry, Subentry, Unspecified)
        /// </summary>
        public void InitializeCDARepository()
        {
            ImplementationGuideType cdaType = this.FindOrCreateImplementationGuideType(DEFAULT_CDA_IG_TYPE_NAME, "cda.xsd", "cda", "urn:hl7-org:v3");

            this.FindOrCreateTemplateType(cdaType, DEFAULT_CDA_DOC_TYPE, "ClinicalDocument", "ClinicalDocument", 1);
            this.FindOrCreateTemplateType(cdaType, DEFAULT_CDA_SECTION_TYPE, "section", "Section", 2);
            this.FindOrCreateTemplateType(cdaType, DEFAULT_CDA_ENTRY_TYPE, "entry", "Entry", 3);
            this.FindOrCreateTemplateType(cdaType, DEFAULT_CDA_SUBENTRY_TYPE, "entry", "Entry", 4);
            this.FindOrCreateTemplateType(cdaType, DEFAULT_CDA_UNSPECIFIED_TYPE, "", "", 4);
        }

        public void InitializeFHIRRepository()
        {
            ImplementationGuideType fhirType = this.FindOrCreateImplementationGuideType(DEFAULT_FHIR_DSTU1_IG_TYPE_NAME, "fhir-all.xsd", "fhir", "http://hl7.org/fhir");

            this.FindOrCreateTemplateType(fhirType, "AdverseReaction", "AdverseReaction", "AdverseReaction", 1);
            this.FindOrCreateTemplateType(fhirType, "Alert", "Alert", "Alert", 2);
            this.FindOrCreateTemplateType(fhirType, "AllergyIntolerance", "AllergyIntolerance", "AllergyIntolerance", 3);
            this.FindOrCreateTemplateType(fhirType, "Binary", "Binary", "Binary", 4);
            this.FindOrCreateTemplateType(fhirType, "CarePlan", "CarePlan", "CarePlan", 5);
            this.FindOrCreateTemplateType(fhirType, "Composition", "Composition", "Composition", 6);
            this.FindOrCreateTemplateType(fhirType, "ConceptMap", "ConceptMap", "ConceptMap", 7);
            this.FindOrCreateTemplateType(fhirType, "Condition", "Condition", "Condition", 8);
            this.FindOrCreateTemplateType(fhirType, "Conformance", "Conformance", "Conformance", 9);
            this.FindOrCreateTemplateType(fhirType, "Device", "Device", "Device", 10);
            this.FindOrCreateTemplateType(fhirType, "DeviceObservationReport", "DeviceObservationReport", "DeviceObservationReport", 11);
            this.FindOrCreateTemplateType(fhirType, "DiagnosticOrder", "DiagnosticOrder", "DiagnosticOrder", 12);
            this.FindOrCreateTemplateType(fhirType, "DiagnosticReport", "DiagnosticReport", "DiagnosticReport", 13);
            this.FindOrCreateTemplateType(fhirType, "DocumentReference", "DocumentReference", "DocumentReference", 14);
            this.FindOrCreateTemplateType(fhirType, "DocumentManifest", "DocumentManifest", "DocumentManifest", 15);
            this.FindOrCreateTemplateType(fhirType, "Encounter", "Encounter", "Encounter", 16);
            this.FindOrCreateTemplateType(fhirType, "FamilyHistory", "FamilyHistory", "FamilyHistory", 17);
            this.FindOrCreateTemplateType(fhirType, "Group", "Group", "Group", 18);
            this.FindOrCreateTemplateType(fhirType, "ImagingStudy", "ImagingStudy", "ImagingStudy", 19);
            this.FindOrCreateTemplateType(fhirType, "Immunization", "Immunization", "Immunization", 20);
            this.FindOrCreateTemplateType(fhirType, "ImmunizationRecommendation", "ImmunizationRecommendation", "ImmunizationRecommendation", 21);
            this.FindOrCreateTemplateType(fhirType, "List", "List", "List", 22);
            this.FindOrCreateTemplateType(fhirType, "Location", "Location", "Location", 23);
            this.FindOrCreateTemplateType(fhirType, "Media", "Media", "Media", 24);
            this.FindOrCreateTemplateType(fhirType, "Medication", "Medication", "Medication", 25);
            this.FindOrCreateTemplateType(fhirType, "MedicationAdministration", "MedicationAdministration", "MedicationAdministration", 26);
            this.FindOrCreateTemplateType(fhirType, "MedicationDispense", "MedicationDispense", "MedicationDispense", 27);
            this.FindOrCreateTemplateType(fhirType, "MedicationPrescription", "MedicationPrescription", "MedicationPrescription", 28);
            this.FindOrCreateTemplateType(fhirType, "MedicationStatement", "MedicationStatement", "MedicationStatement", 29);
            this.FindOrCreateTemplateType(fhirType, "MessageHeader", "MessageHeader", "MessageHeader", 30);
            this.FindOrCreateTemplateType(fhirType, "Observation", "Observation", "Observation", 31);
            this.FindOrCreateTemplateType(fhirType, "OperationOutcome", "OperationOutcome", "OperationOutcome", 32);
            this.FindOrCreateTemplateType(fhirType, "Order", "Order", "Order", 33);
            this.FindOrCreateTemplateType(fhirType, "OrderResponse", "OrderResponse", "OrderResponse", 34);
            this.FindOrCreateTemplateType(fhirType, "Organization", "Organization", "Organization", 35);
            this.FindOrCreateTemplateType(fhirType, "Other", "Other", "Other", 36);
            this.FindOrCreateTemplateType(fhirType, "Patient", "Patient", "Patient", 37);
            this.FindOrCreateTemplateType(fhirType, "Practitioner", "Practitioner", "Practitioner", 38);
            this.FindOrCreateTemplateType(fhirType, "Procedure", "Procedure", "Procedure", 39);
            this.FindOrCreateTemplateType(fhirType, "Profile", "Profile", "Profile", 40);
            this.FindOrCreateTemplateType(fhirType, "Provenance", "Provenance", "Provenance", 41);
            this.FindOrCreateTemplateType(fhirType, "Query", "Query", "Query", 42);
            this.FindOrCreateTemplateType(fhirType, "Questionnaire", "Questionnaire", "Questionnaire", 43);
            this.FindOrCreateTemplateType(fhirType, "RelatedPerson", "RelatedPerson", "RelatedPerson", 44);
            this.FindOrCreateTemplateType(fhirType, "SecurityEvent", "SecurityEvent", "SecurityEvent", 45);
            this.FindOrCreateTemplateType(fhirType, "Specimen", "Specimen", "Specimen", 46);
            this.FindOrCreateTemplateType(fhirType, "Substance", "Substance", "Substance", 47);
            this.FindOrCreateTemplateType(fhirType, "Supply", "Supply", "Supply", 48);
            this.FindOrCreateTemplateType(fhirType, "ValueSet", "ValueSet", "ValueSet", 49);
            this.FindOrCreateTemplateType(fhirType, "Extension", "Extension", "Extension", 50);
        }

        public void InitializeFHIR2Repository()
        {
            ImplementationGuideType fhirType = this.FindOrCreateImplementationGuideType(DEFAULT_FHIR_DSTU2_IG_TYPE_NAME, "fhir-all.xsd", "fhir", "http://hl7.org/fhir");

            this.FindOrCreateTemplateType(fhirType, "Composition", "Composition", "Composition", 1);
            this.FindOrCreateTemplateType(fhirType, "Patient", "Patient", "Patient", 2);
            this.FindOrCreateTemplateType(fhirType, "Practitioner", "Practitioner", "Practitioner", 3);
            this.FindOrCreateTemplateType(fhirType, "StructureDefinition", "StructureDefinition", "StructureDefinition", 4);
            this.FindOrCreateTemplateType(fhirType, "ImplementationGuide", "ImplementationGuide", "ImplementationGuide", 5);
            this.FindOrCreateTemplateType(fhirType, "ValueSet", "ValueSet", "ValueSet", 6);

            this.FindOrCreateImplementationGuide(fhirType, "Unowned FHIR DSTU2 Profiles");
        }

        public void InitializeFHIR3Repository()
        {
            ImplementationGuideType fhirType = this.FindOrCreateImplementationGuideType(DEFAULT_FHIR_STU3_IG_TYPE_NAME, "fhir-all.xsd", "fhir", "http://hl7.org/fhir");

            this.FindOrCreateTemplateType(fhirType, "Account");
            this.FindOrCreateTemplateType(fhirType, "ActivityDefinition");
            this.FindOrCreateTemplateType(fhirType, "AllergyIntolerance");
            this.FindOrCreateTemplateType(fhirType, "Appointment");
            this.FindOrCreateTemplateType(fhirType, "AppointmentResponse");
            this.FindOrCreateTemplateType(fhirType, "AuditEvent");
            this.FindOrCreateTemplateType(fhirType, "Basic");
            this.FindOrCreateTemplateType(fhirType, "Binary");
            this.FindOrCreateTemplateType(fhirType, "BodySite");
            this.FindOrCreateTemplateType(fhirType, "Bundle");
            this.FindOrCreateTemplateType(fhirType, "CarePlan");
            this.FindOrCreateTemplateType(fhirType, "CareTeam");
            this.FindOrCreateTemplateType(fhirType, "Claim");
            this.FindOrCreateTemplateType(fhirType, "ClaimResponse");
            this.FindOrCreateTemplateType(fhirType, "ClinicalImpression");
            this.FindOrCreateTemplateType(fhirType, "CodeSystem");
            this.FindOrCreateTemplateType(fhirType, "Communication");
            this.FindOrCreateTemplateType(fhirType, "CommunicationRequest");
            this.FindOrCreateTemplateType(fhirType, "CompartmentDefinition");
            this.FindOrCreateTemplateType(fhirType, "Composition");
            this.FindOrCreateTemplateType(fhirType, "ConceptMap");
            this.FindOrCreateTemplateType(fhirType, "Condition");
            this.FindOrCreateTemplateType(fhirType, "Conformance");
            this.FindOrCreateTemplateType(fhirType, "Consent");
            this.FindOrCreateTemplateType(fhirType, "Contract");
            this.FindOrCreateTemplateType(fhirType, "Coverage");
            this.FindOrCreateTemplateType(fhirType, "DataElement");
            this.FindOrCreateTemplateType(fhirType, "DecisionSupportServiceModule");
            this.FindOrCreateTemplateType(fhirType, "DetectedIssue");
            this.FindOrCreateTemplateType(fhirType, "Device");
            this.FindOrCreateTemplateType(fhirType, "DeviceComponent");
            this.FindOrCreateTemplateType(fhirType, "DeviceMetric");
            this.FindOrCreateTemplateType(fhirType, "DeviceUseRequest");
            this.FindOrCreateTemplateType(fhirType, "DeviceUseStatement");
            this.FindOrCreateTemplateType(fhirType, "DiagnosticRequest");
            this.FindOrCreateTemplateType(fhirType, "DiagnosticReport");
            this.FindOrCreateTemplateType(fhirType, "DocumentManifest");
            this.FindOrCreateTemplateType(fhirType, "DocumentReference");
            this.FindOrCreateTemplateType(fhirType, "EligibilityRequest");
            this.FindOrCreateTemplateType(fhirType, "EligibilityResponse");
            this.FindOrCreateTemplateType(fhirType, "Encounter");
            this.FindOrCreateTemplateType(fhirType, "Endpoint");
            this.FindOrCreateTemplateType(fhirType, "EnrollmentRequest");
            this.FindOrCreateTemplateType(fhirType, "EnrollmentResponse");
            this.FindOrCreateTemplateType(fhirType, "EpisodeOfCare");
            this.FindOrCreateTemplateType(fhirType, "ExpansionProfile");
            this.FindOrCreateTemplateType(fhirType, "ExplanationOfBenefit");
            this.FindOrCreateTemplateType(fhirType, "Extension");
            this.FindOrCreateTemplateType(fhirType, "FamilyMemberHistory");
            this.FindOrCreateTemplateType(fhirType, "Flag");
            this.FindOrCreateTemplateType(fhirType, "Goal");
            this.FindOrCreateTemplateType(fhirType, "Group");
            this.FindOrCreateTemplateType(fhirType, "GuidanceResponse");
            this.FindOrCreateTemplateType(fhirType, "HealthcareService");
            this.FindOrCreateTemplateType(fhirType, "ImagingManifest");
            this.FindOrCreateTemplateType(fhirType, "ImagingStudy");
            this.FindOrCreateTemplateType(fhirType, "Immunization");
            this.FindOrCreateTemplateType(fhirType, "ImmunizationRecommendation");
            this.FindOrCreateTemplateType(fhirType, "ImplementationGuide");
            this.FindOrCreateTemplateType(fhirType, "Library");
            this.FindOrCreateTemplateType(fhirType, "Linkage");
            this.FindOrCreateTemplateType(fhirType, "List");
            this.FindOrCreateTemplateType(fhirType, "Location");
            this.FindOrCreateTemplateType(fhirType, "Measure");
            this.FindOrCreateTemplateType(fhirType, "MeasureReport");
            this.FindOrCreateTemplateType(fhirType, "Media");
            this.FindOrCreateTemplateType(fhirType, "Medication");
            this.FindOrCreateTemplateType(fhirType, "MedicationAdministration");
            this.FindOrCreateTemplateType(fhirType, "MedicationDispense");
            this.FindOrCreateTemplateType(fhirType, "MedicationOrder");
            this.FindOrCreateTemplateType(fhirType, "MedicationStatement");
            this.FindOrCreateTemplateType(fhirType, "MessageHeader");
            this.FindOrCreateTemplateType(fhirType, "NamingSystem");
            this.FindOrCreateTemplateType(fhirType, "NutritionRequest");
            this.FindOrCreateTemplateType(fhirType, "Observation");
            this.FindOrCreateTemplateType(fhirType, "OperationDefinition");
            this.FindOrCreateTemplateType(fhirType, "OperationOutcome");
            this.FindOrCreateTemplateType(fhirType, "Organization");
            this.FindOrCreateTemplateType(fhirType, "Parameters");
            this.FindOrCreateTemplateType(fhirType, "Patient");
            this.FindOrCreateTemplateType(fhirType, "PaymentNotice");
            this.FindOrCreateTemplateType(fhirType, "PaymentReconciliation");
            this.FindOrCreateTemplateType(fhirType, "Person");
            this.FindOrCreateTemplateType(fhirType, "PlanDefinition");
            this.FindOrCreateTemplateType(fhirType, "Practitioner");
            this.FindOrCreateTemplateType(fhirType, "PractitionerRole");
            this.FindOrCreateTemplateType(fhirType, "Procedure");
            this.FindOrCreateTemplateType(fhirType, "ProcedureRequest");
            this.FindOrCreateTemplateType(fhirType, "ProcessRequest");
            this.FindOrCreateTemplateType(fhirType, "ProcessResponse");
            this.FindOrCreateTemplateType(fhirType, "Provenance");
            this.FindOrCreateTemplateType(fhirType, "Questionnaire");
            this.FindOrCreateTemplateType(fhirType, "QuestionnaireResponse");
            this.FindOrCreateTemplateType(fhirType, "ReferralRequest");
            this.FindOrCreateTemplateType(fhirType, "RelatedPerson");
            this.FindOrCreateTemplateType(fhirType, "RiskAssessment");
            this.FindOrCreateTemplateType(fhirType, "Schedule");
            this.FindOrCreateTemplateType(fhirType, "SearchParameter");
            this.FindOrCreateTemplateType(fhirType, "Sequence");
            this.FindOrCreateTemplateType(fhirType, "Slot");
            this.FindOrCreateTemplateType(fhirType, "Specimen");
            this.FindOrCreateTemplateType(fhirType, "StructureDefinition");
            this.FindOrCreateTemplateType(fhirType, "StructureMap");
            this.FindOrCreateTemplateType(fhirType, "Subscription");
            this.FindOrCreateTemplateType(fhirType, "Substance");
            this.FindOrCreateTemplateType(fhirType, "SupplyDelivery");
            this.FindOrCreateTemplateType(fhirType, "SupplyRequest");
            this.FindOrCreateTemplateType(fhirType, "Task");
            this.FindOrCreateTemplateType(fhirType, "TestScript");
            this.FindOrCreateTemplateType(fhirType, "ValueSet");
            this.FindOrCreateTemplateType(fhirType, "VisionPrescription");

            this.FindOrCreateImplementationGuide(fhirType, "Unowned FHIR STU3 Profiles");
        }

        public void InitializeLCG()
        {
            var org = this.FindOrCreateOrganization(DEFAULT_ORGANIZATION);
            this.FindOrCreateUser(DEFAULT_USERNAME);
            this.AssociateUserWithRole(DEFAULT_USERNAME, ADMIN_ROLE);
        }

        public void InitializeLCGAndLogin()
        {
            this.InitializeLCG();
            Helper.AuthLogin(this, DEFAULT_USERNAME, DEFAULT_ORGANIZATION);
        }

        #region IObjectRepository

        public void AuditChanges(string auditUserName, string auditIP)
        {

        }

        private static DbSet<T> CreateMockDbSet<T>() where T: class
        {
            var data = new List<T>();
            var dataQueryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(dataQueryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(dataQueryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(dataQueryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => dataQueryable.GetEnumerator());
            mockSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>((s) => data.Remove(s));
            
            mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) =>
            {
                Template template = s as Template;
                TemplateConstraint constraint = s as TemplateConstraint;
                ImplementationGuide ig = s as ImplementationGuide;
                ValueSet vs = s as ValueSet;
                ValueSetMember vsm = s as ValueSetMember;

                if (template != null)
                {
                    if (template.Id == 0)
                        template.Id = data.Select(y => (y as Template).Id).DefaultIfEmpty(0).Max() + 1;

                    if (template.OwningImplementationGuide != null && !template.OwningImplementationGuide.ChildTemplates.Contains(template))
                        template.OwningImplementationGuide.ChildTemplates.Add(template);

                    if (template.ImplementationGuideType != null && !template.ImplementationGuideType.Templates.Contains(template))
                        template.ImplementationGuideType.Templates.Add(template);

                    if (template.Status != null && !template.Status.Templates.Contains(template))
                        template.Status.Templates.Add(template);

                    if (template.ImpliedTemplate != null && !template.ImpliedTemplate.ImplyingTemplates.Contains(template))
                        template.ImpliedTemplate.ImplyingTemplates.Add(template);

                    if (template.PreviousVersion != null && !template.PreviousVersion.NextVersions.Contains(template))
                        template.PreviousVersion.NextVersions.Add(template);

                    if (template.Author != null && !template.Author.Templates.Contains(template))
                        template.Author.Templates.Add(template);
                }
                
                if (constraint != null)
                {
                    if (constraint.Id == 0)
                        constraint.Id = data.Select(y => (y as TemplateConstraint).Id).DefaultIfEmpty(0).Max() + 1;

                    if (constraint.CodeSystem != null && !constraint.CodeSystem.Constraints.Contains(constraint))
                        constraint.CodeSystem.Constraints.Add(constraint);

                    if (constraint.ContainedTemplate != null && !constraint.ContainedTemplate.ContainingConstraints.Contains(constraint))
                        constraint.ContainedTemplate.ContainingConstraints.Add(constraint);

                    if (constraint.ParentConstraint != null && !constraint.ParentConstraint.ChildConstraints.Contains(constraint))
                        constraint.ParentConstraint.ChildConstraints.Add(constraint);

                    if (constraint.Template != null && !constraint.Template.ChildConstraints.Contains(constraint))
                        constraint.Template.ChildConstraints.Add(constraint);

                    if (constraint.ValueSet != null && !constraint.ValueSet.Constraints.Contains(constraint))
                        constraint.ValueSet.Constraints.Add(constraint);
                }

                if (ig != null)
                {
                    if (ig.Id == 0)
                        ig.Id = data.Select(y => (y as ImplementationGuide).Id).DefaultIfEmpty(0).Max() + 1;

                    if (ig.AccessManager != null && !ig.AccessManager.AccessManagerImplemntationGuides.Contains(ig))
                        ig.AccessManager.AccessManagerImplemntationGuides.Add(ig);

                    if (ig.ImplementationGuideType != null && !ig.ImplementationGuideType.ImplementationGuides.Contains(ig))
                        ig.ImplementationGuideType.ImplementationGuides.Add(ig);

                    if (ig.Organization != null && !ig.Organization.ImplementationGuides.Contains(ig))
                        ig.Organization.ImplementationGuides.Add(ig);

                    if (ig.PreviousVersion != null && !ig.PreviousVersion.NextVersions.Contains(ig))
                        ig.PreviousVersion.NextVersions.Add(ig);

                    if (ig.Organization != null && !ig.Organization.ImplementationGuides.Contains(ig))
                        ig.Organization.ImplementationGuides.Add(ig);

                    if (ig.PublishStatus != null && !ig.PublishStatus.ImplementationGuides.Contains(ig))
                        ig.PublishStatus.ImplementationGuides.Add(ig);
                }
                
                if (vs != null)
                {
                    if (vs.Id == 0)
                        vs.Id = data.Select(y => (y as ValueSet).Id).DefaultIfEmpty(0).Max() + 1;
                }

                if (vsm != null)
                {
                    if (vsm.Id == 0)
                        vsm.Id = data.Select(y => (y as ValueSetMember).Id).DefaultIfEmpty(0).Max() + 1;

                    if (vsm.CodeSystem != null && !vsm.CodeSystem.Members.Contains(vsm))
                        vsm.CodeSystem.Members.Add(vsm);

                    if (vsm.ValueSet != null && !vsm.ValueSet.Members.Contains(vsm))
                        vsm.ValueSet.Members.Add(vsm);
                }

                data.Add(s);
            });
            mockSet.Setup(d => d.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>((s) => 
            {
                foreach (var a in s)
                    mockSet.Object.Add(a);
            });

            return mockSet.Object;
        }

        DbSet<AuditEntry> auditEntries = CreateMockDbSet<AuditEntry>();
        DbSet<Template> templates = CreateMockDbSet<Template>();
        DbSet<CodeSystem> codeSystems = CreateMockDbSet<CodeSystem>();
        DbSet<GreenConstraint> greenConstraints = CreateMockDbSet<GreenConstraint>();
        DbSet<GreenTemplate> greenTemplates = CreateMockDbSet<GreenTemplate>();
        DbSet<ImplementationGuide> implementationGuides = CreateMockDbSet<ImplementationGuide>();
        DbSet<ImplementationGuideSetting> implementationGuideSettings = CreateMockDbSet<ImplementationGuideSetting>();
        DbSet<TemplateConstraint> constraints = CreateMockDbSet<TemplateConstraint>();
        DbSet<TemplateType> templateTypes = CreateMockDbSet<TemplateType>();
        DbSet<ValueSet> valuesets = CreateMockDbSet<ValueSet>();
        DbSet<ValueSetMember> valuesetMembers = CreateMockDbSet<ValueSetMember>();
        DbSet<ImplementationGuideTemplateType> implementationGuideTemplateTypes = CreateMockDbSet<ImplementationGuideTemplateType>();
        DbSet<ImplementationGuideTypeDataType> dataTypes = CreateMockDbSet<ImplementationGuideTypeDataType>();
        DbSet<ImplementationGuideType> implementationGuideTypes = CreateMockDbSet<ImplementationGuideType>();
        DbSet<ImplementationGuideFile> implementationGuideFiles = CreateMockDbSet<ImplementationGuideFile>();
        DbSet<ImplementationGuideFileData> implementationGuideFileDatas = CreateMockDbSet<ImplementationGuideFileData>();
        DbSet<Organization> organizations = CreateMockDbSet<Organization>();
        DbSet<ImplementationGuideSchematronPattern> implementationGuideSchematronPatterns = CreateMockDbSet<ImplementationGuideSchematronPattern>();
        DbSet<PublishStatus> publishStatuses = CreateMockDbSet<PublishStatus>();
        DbSet<Role> roles = CreateMockDbSet<Role>();
        DbSet<AppSecurable> appSecurables = CreateMockDbSet<AppSecurable>();
        DbSet<RoleAppSecurable> roleAppSecurables = CreateMockDbSet<RoleAppSecurable>();
        DbSet<UserRole> userRoles = CreateMockDbSet<UserRole>();
        DbSet<User> users = CreateMockDbSet<User>();
        DbSet<RoleRestriction> roleRestrictions = CreateMockDbSet<RoleRestriction>();
        DbSet<Group> groups = CreateMockDbSet<Group>();
        DbSet<UserGroup> userGroups = CreateMockDbSet<UserGroup>();
        DbSet<GroupManager> groupManagers = CreateMockDbSet<GroupManager>();
        DbSet<ImplementationGuidePermission> implementationGuidePermissions = CreateMockDbSet<ImplementationGuidePermission>();
        DbSet<TemplateConstraintSample> templateConstraintSamples = CreateMockDbSet<TemplateConstraintSample>();
        DbSet<TemplateSample> templateSamples = CreateMockDbSet<TemplateSample>();
        DbSet<ImplementationGuideSection> implementationGuideSections = CreateMockDbSet<ImplementationGuideSection>();
        DbSet<TemplateExtension> templateExtensions = CreateMockDbSet<TemplateExtension>();

        public DbSet<AuditEntry> AuditEntries
        {
            get { return auditEntries; }
        }

        public DbSet<CodeSystem> CodeSystems
        {
            get { return codeSystems; }
        }

        public DbSet<GreenConstraint> GreenConstraints
        {
            get { return greenConstraints; }
        }

        public DbSet<GreenTemplate> GreenTemplates
        {
            get { return greenTemplates; }
        }

        public DbSet<ImplementationGuide> ImplementationGuides
        {
            get { return implementationGuides; }
        }

        public DbSet<ImplementationGuideSetting> ImplementationGuideSettings
        {
            get { return implementationGuideSettings; }
        }

        public DbSet<Template> Templates
        {
            get { return this.templates; }
        }

        public DbSet<TemplateConstraint> TemplateConstraints
        {
            get { return constraints; }
        }

        public DbSet<TemplateType> TemplateTypes
        {
            get { return templateTypes; }
        }

        public DbSet<ValueSet> ValueSets
        {
            get { return valuesets; }
        }

        public DbSet<ValueSetMember> ValueSetMembers
        {
            get { return valuesetMembers; }
        }

        public DbSet<ImplementationGuideTemplateType> ImplementationGuideTemplateTypes
        {
            get { return implementationGuideTemplateTypes; }
        }

        public DbSet<ImplementationGuideTypeDataType> ImplementationGuideTypeDataTypes
        {
            get { return dataTypes; }
        }

        public DbSet<ImplementationGuideType> ImplementationGuideTypes
        {
            get { return implementationGuideTypes; }
        }

        public DbSet<ImplementationGuideFile> ImplementationGuideFiles
        {
            get { return implementationGuideFiles; }
        }

        public DbSet<ImplementationGuideFileData> ImplementationGuideFileDatas
        {
            get { return implementationGuideFileDatas; }
        }

        public DbSet<Organization> Organizations
        {
            get { return organizations; }
        }

        public DbSet<ImplementationGuideSchematronPattern> ImplementationGuideSchematronPatterns
        {
            get { return implementationGuideSchematronPatterns; }
        }

        public DbSet<PublishStatus> PublishStatuses
        {
            get { return publishStatuses; }
        }

        public DbSet<Role> Roles
        {
            get { return roles; }
        }

        public DbSet<AppSecurable> AppSecurables
        {
            get { return appSecurables; }
        }

        public DbSet<RoleAppSecurable> RoleAppSecurables
        {
            get { return roleAppSecurables; }
        }

        public DbSet<UserRole> UserRoles
        {
            get { return userRoles; }
        }

        public DbSet<User> Users
        {
            get { return users; }
        }

        public DbSet<RoleRestriction> RoleRestrictions
        {
            get { return roleRestrictions; }
        }

        public DbSet<Group> Groups
        {
            get { return groups; }
        }

        public DbSet<UserGroup> UserGroups
        {
            get { return userGroups; }
        }

        public DbSet<GroupManager> GroupManagers
        {
            get { return this.groupManagers; }
        }

        public DbSet<ImplementationGuidePermission> ImplementationGuidePermissions
        {
            get { return implementationGuidePermissions; }
        }

        public DbSet<TemplateConstraintSample> TemplateConstraintSamples
        {
            get { return templateConstraintSamples; }
        }

        public DbSet<TemplateSample> TemplateSamples
        {
            get { return templateSamples; }
        }

        public DbSet<ViewIGAuditTrail> ViewIGAuditTrails
        {
            get
            {
                return null;
            }
        }

        public DbSet<ImplementationGuideSection> ImplementationGuideSections
        {
            get
            {
                return implementationGuideSections;
            }
        }

        public DbSet<TemplateExtension> TemplateExtensions
        {
            get
            {
                return templateExtensions;
            }
        }

        public DbSet<ViewTemplatePermission> ViewTemplatePermissions
        {
            get
            {
                var results = (from ig in this.ImplementationGuides
                               join igp in this.implementationGuidePermissions on ig.Id equals igp.ImplementationGuideId
                               join t in this.Templates on ig.Id equals t.OwningImplementationGuideId
                               join u in this.Users on igp.UserId equals u.Id
                               select new ViewTemplatePermission()
                               {
                                   Permission = igp.Permission,
                                   TemplateId = t.Id,
                                   UserId = u.Id
                               })
                               .Union(from ig in this.ImplementationGuides
                                      join igp in this.ImplementationGuidePermissions on ig.Id equals igp.ImplementationGuideId
                                      join t in this.Templates on ig.Id equals t.OwningImplementationGuideId
                                      join ug in this.UserGroups on igp.GroupId equals ug.GroupId
                                      select new ViewTemplatePermission()
                                      {
                                          Permission = igp.Permission,
                                          TemplateId = t.Id,
                                          UserId = ug.UserId
                                      });

                var mockDbSet = CreateMockDbSet<ViewTemplatePermission>();
                mockDbSet.AddRange(results);
                return mockDbSet;
            }
        }

        public DbSet<ViewImplementationGuidePermission> ViewImplementationGuidePermissions
        {
            get
            {
                var results = (from igp in this.implementationGuidePermissions
                               join u in this.Users on igp.UserId equals u.Id
                               select new ViewImplementationGuidePermission()
                               {
                                   Permission = igp.Permission,
                                   ImplementationGuideId = igp.ImplementationGuideId,
                                   UserId = u.Id
                               })
                               .Union(from igp in this.ImplementationGuidePermissions
                                      join ug in this.UserGroups on igp.GroupId equals ug.GroupId
                                      select new ViewImplementationGuidePermission()
                                      {
                                          Permission = igp.Permission,
                                          ImplementationGuideId = igp.ImplementationGuideId,
                                          UserId = ug.UserId
                                      });

                var mockDbSet = CreateMockDbSet<ViewImplementationGuidePermission>();
                mockDbSet.AddRange(results);
                return mockDbSet;
            }
        }

        public DbSet<ViewImplementationGuideTemplate> ViewImplementationGuideTemplates
        {
            get
            {
                var results = (from ig in this.ImplementationGuides
                               join t in this.Templates on ig.Id equals t.OwningImplementationGuideId
                               select new ViewImplementationGuideTemplate()
                               {
                                   ImplementationGuideId = ig.Id,
                                   TemplateId = t.Id
                               })
                               .Union(
                               from ig in this.ImplementationGuides
                               join t in this.Templates on ig.PreviousVersionImplementationGuideId equals t.OwningImplementationGuideId
                               where this.Templates.Count(y => y.PreviousVersionTemplateId == t.Id) == 0
                               select new ViewImplementationGuideTemplate()
                               {
                                   ImplementationGuideId = ig.Id,
                                   TemplateId = t.Id
                               });

                var mockDbSet = CreateMockDbSet<ViewImplementationGuideTemplate>();
                mockDbSet.AddRange(results);
                return mockDbSet;
            }
        }

        public DbSet<ViewUserSecurable> ViewUserSecurables
        {
            get
            {
                var results = (from ur in this.UserRoles
                               join asr in this.RoleAppSecurables on ur.RoleId equals asr.RoleId
                               join aps in this.AppSecurables on asr.AppSecurableId equals aps.Id
                               select new ViewUserSecurable()
                               {
                                   UserId = ur.UserId,
                                   SecurableName = aps.Name
                               }).Distinct();

                var mockDbSet = CreateMockDbSet<ViewUserSecurable>();
                mockDbSet.AddRange(results);
                return mockDbSet;
            }
        }

        public DbSet<ViewImplementationGuideFile> ViewImplementationGuideFiles
        {
            get
            {
                var maxFileDataDates = (from ifgd in this.ImplementationGuideFileDatas 
                                        group ifgd by ifgd.ImplementationGuideFileId into g
                                        select new
                                        {
                                            ImplementationGuideFileId = g.Key,
                                            LastUpdateDate = g.Max(y => y.UpdatedDate)
                                        });

                var results = (from igf in this.ImplementationGuideFiles
                               join mfdd in maxFileDataDates on igf.Id equals mfdd.ImplementationGuideFileId
                               join igfd in this.ImplementationGuideFileDatas on mfdd.ImplementationGuideFileId equals igfd.ImplementationGuideFileId
                               where igfd.UpdatedDate == mfdd.LastUpdateDate
                               select new ViewImplementationGuideFile()
                               {
                                   ContentType = igf.ContentType,
                                   Data = igfd.Data,
                                   ExpectedErrorCount = igf.ExpectedErrorCount,
                                   FileName = igf.FileName,
                                   Id = igfd.Id,
                                   ImplementationGuideId = igf.ImplementationGuideId,
                                   MimeType = igf.MimeType,
                                   Note = igfd.Note,
                                   UpdatedBy = igfd.UpdatedBy,
                                   UpdatedDate = igfd.UpdatedDate
                               });

                var mockDbSet = CreateMockDbSet<ViewImplementationGuideFile>();
                mockDbSet.AddRange(results);
                return mockDbSet;
            }
        }

        public DbSet<ViewTemplate> ViewTemplates
        {
            get
            {
                var results = (from t in this.Templates
                               select new ViewTemplate()
                               {
                                   ConstraintCount = t.ChildConstraints.Count,
                                   ContainedTemplateCount = 0,  // TODO: Fix
                                   Id = t.Id,
                                   ImplementationGuideTypeName = t.ImplementationGuideType.Name,
                                   ImpliedTemplateCount = t.ImplyingTemplates.Count,
                                   ImpliedTemplateOid = t.ImpliedTemplate != null ? t.ImpliedTemplate.Oid : null,
                                   ImpliedTemplateTitle = t.ImpliedTemplate != null ? t.ImpliedTemplate.Name : null,
                                   IsOpen = t.IsOpen,
                                   Name = t.Name,
                                   Oid = t.Oid,
                                   OrganizationName = t.OwningImplementationGuide != null && t.OwningImplementationGuide.Organization != null ? t.OwningImplementationGuide.Organization.Name : null,
                                   OwningImplementationGuideId = t.OwningImplementationGuideId,
                                   OwningImplementationGuideTitle = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.Name : null,
                                   PrimaryContext = t.PrimaryContext,
                                   PublishDate = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.PublishDate : null,
                                   TemplateTypeDisplay = t.TemplateType != null ? t.TemplateType.Name : null,
                                   TemplateTypeId = t.TemplateTypeId,
                                   TemplateTypeName = t.TemplateType != null ? t.TemplateType.Name : null
                               }).Distinct();

                var mockDbSet = CreateMockDbSet<ViewTemplate>();
                mockDbSet.AddRange(results);
                return mockDbSet;
            }
        }

        public DbSet<ViewTemplateList> ViewTemplateLists
        {
            get
            {
                var results = (from t in this.Templates
                               select new ViewTemplateList()
                               {
                                   Id = t.Id,
                                   Oid = t.Oid,
                                   Name = t.Name,
                                   Open = t.IsOpen ? "Yes" : "No",
                                   Organization = t.OwningImplementationGuide != null && t.OwningImplementationGuide.Organization != null ? t.OwningImplementationGuide.Organization.Name : null,
                                   ImplementationGuide = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.Name : null,
                                   PublishDate = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.PublishDate : null,
                                   TemplateType = t.TemplateType.Name + " (" + t.TemplateType.ImplementationGuideType.Name + ")",
                                   ImpliedTemplateName = t.ImpliedTemplate != null ? t.ImpliedTemplate.Name : null,
                                   ImpliedTemplateOid = t.ImpliedTemplate != null ? t.ImpliedTemplate.Oid : null
                                   // TODO: Fill in other fields
                               }).Distinct();

                var mockDbSet = CreateMockDbSet<ViewTemplateList>();
                mockDbSet.AddRange(results);
                return mockDbSet;
            }
        }

        #endregion

        #region Generic Test Data Generation

        public Organization FindOrCreateOrganization(string name, string contactName = null, string contactEmail = null, string contactPhone = null)
        {
            Organization foundOrganization = this.organizations.SingleOrDefault(y => y.Name == name);

            if (foundOrganization == null)
            {
                foundOrganization = new Organization()
                {
                    Id = this.Organizations.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    ContactName = contactName,
                    ContactEmail = contactEmail,
                    ContactPhone = contactPhone
                };
                this.Organizations.Add(foundOrganization);
            }

            return foundOrganization;
        }

        public Template CreateTemplate(
            string oid, 
            string typeName, 
            string title, 
            ImplementationGuide owningImplementationGuide, 
            string primaryContext = null, 
            string primaryContextType = null, 
            string description = null, 
            string notes = null, 
            Organization organization = null, 
            Template impliedTemplate = null,
            Template previousVersion = null,
            string status = null)
        {
            TemplateType templateType = this.FindTemplateType(owningImplementationGuide.ImplementationGuideType.Name, typeName);
            PublishStatus publishStatus = null;

            if (!string.IsNullOrEmpty(status))
                publishStatus = this.publishStatuses.Single(y => y.Status == status);

            var template = CreateTemplate(oid, templateType, title, owningImplementationGuide, primaryContext, primaryContextType, description, notes, impliedTemplate, publishStatus);

            if (previousVersion != null)
                template.SetPreviousVersion(previousVersion);

            return template;
        }
        
        /// <summary>
        /// Creates a new instance of a template with the specified parameters.
        /// </summary>
        /// <remarks>Generates a bookmark automatically based on the title and type of the template.</remarks>
        /// <param name="oid">Required. The oid of the template</param>
        /// <param name="type">Required. The type of template</param>
        /// <param name="title">Required. The title of the template</param>
        /// <param name="description">The description of the template</param>
        /// <param name="notes">The notes of the template</param>
        /// <param name="owningImplementationGuide">The implementation guide that owns the template.</param>
        /// <returns>A new instance of Template that has been appropriately added to the mock object repository.</returns>
        public Template CreateTemplate(string oid, TemplateType type, string title, ImplementationGuide owningImplementationGuide, string primaryContext = null, string primaryContextType = null, string description = null, string notes = null, Template impliedTemplate = null, PublishStatus status = null)
        {
            if (string.IsNullOrEmpty(oid))
                throw new ArgumentNullException("oid");

            if (type == null)
                throw new ArgumentNullException("type");

            if (string.IsNullOrEmpty("title"))
                throw new ArgumentNullException("title");

            if (status == null)
                status = PublishStatus.GetDraftStatus(this);

            Template template = new Template()
            {
                Id = this.Templates.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                OwningImplementationGuideId = owningImplementationGuide.Id,
                OwningImplementationGuide = owningImplementationGuide,
                ImplementationGuideTypeId = type.ImplementationGuideTypeId,
                ImplementationGuideType = type.ImplementationGuideType,
                ImpliedTemplateId = impliedTemplate != null ? (int?)impliedTemplate.Id : null,
                ImpliedTemplate = impliedTemplate,
                PrimaryContext = primaryContext,
                PrimaryContextType = primaryContextType,
                TemplateType = type,
                TemplateTypeId = type.Id,
                Name = title,
                Oid = oid,
                Description = description,
                Notes = notes,
                Status = status,
                StatusId = status.Id
            };

            template.Bookmark = title
                .Replace(" ", "_")
                .Replace("\t", "_");

            switch (type.Id)
            {
                case 1:
                    template.Bookmark = "D_" + template.Bookmark;
                    break;
                case 2:
                    template.Bookmark = "S_" + template.Bookmark;
                    break;
                case 3:
                    template.Bookmark = "E_" + template.Bookmark;
                    break;
                case 4:
                    template.Bookmark = "SE_" + template.Bookmark;
                    break;
                case 5:
                    template.Bookmark = "O_" + template.Bookmark;
                    break;
            }

            this.Templates.Add(template);

            if (owningImplementationGuide != null)
                owningImplementationGuide.ChildTemplates.Add(template);

            return template;
        }

        /// <summary>
        /// Generates a data-type for the specified name that can be used by the green libraries.
        /// </summary>
        /// <param name="igType">The implementation guide type for the data type</param>
        /// <param name="name">The name of the data-type</param>
        /// <returns>A new instance of DataType that has been appropriately added to the mock object repository.</returns>
        public ImplementationGuideTypeDataType FindOrCreateDataType(ImplementationGuideType igType, string name)
        {
            ImplementationGuideTypeDataType dt = this.ImplementationGuideTypeDataTypes.SingleOrDefault(y => y.ImplementationGuideTypeId == igType.Id && y.DataTypeName == name);

            if (dt == null)
            {
                dt = new ImplementationGuideTypeDataType()
                {
                    Id = this.ImplementationGuideTypeDataTypes.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    ImplementationGuideType = igType,
                    ImplementationGuideTypeId = igType.Id,
                    DataTypeName = name
                };

                this.ImplementationGuideTypeDataTypes.Add(dt);
                igType.DataTypes.Add(dt);
            }

            return dt;
        }

        /// <summary>
        /// Attempts to find the implementation guide type based on the name specified.
        /// If none can be found, a new implementation guide type is created and added to the mock object repository.
        /// </summary>
        /// <param name="name">Required. The name of the implementation guide type. This value is used to perform the search.</param>
        /// <param name="schemaLocation">Required. The location of the schema for the implementation guide type.</param>
        /// <param name="prefix">The prefix of the schema for the implementation guide type.</param>
        /// <param name="uri">The namespace uri of the schema for the implementation guide type.</param>
        /// <returns>Either the found implementation guide type, or a new implementation guide tpye that has been appropriately added to the mock object repository.</returns>
        public ImplementationGuideType FindOrCreateImplementationGuideType(string name, string schemaLocation, string prefix, string uri)
        {
            ImplementationGuideType igType = this.ImplementationGuideTypes.SingleOrDefault(y => y.Name == name);

            if (igType == null)
            {
                igType = new ImplementationGuideType()
                {
                    Id = this.ImplementationGuideTypes.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    SchemaLocation = schemaLocation,
                    SchemaPrefix = prefix,
                    SchemaURI = uri
                };

                this.implementationGuideTypes.Add(igType);
            }

            return igType;
        }

        public TemplateType FindOrCreateTemplateType(ImplementationGuideType igType, string name)
        {
            int nextOrder = igType.TemplateTypes.DefaultIfEmpty().Max(y => y != null ? y.Id : 1) + 1;
            return this.FindOrCreateTemplateType(igType, name, name, name, nextOrder);
        }

        /// <summary>
        /// Attempts to find a template type based on the implementation guide type and name specified.
        /// If none can be found, a new instance is created and added to the mock object repository and ig type.
        /// </summary>
        /// <param name="igType">Required. The implementation guide type. This value is used to perform the search.</param>
        /// <param name="name">The name of the template type. This value is used to perform the search.</param>
        /// <param name="context">The default contetx of the template type.</param>
        /// <returns>Either the found template type, or a new one which has been added to the mock object repository and the ig type.</returns>
        public TemplateType FindOrCreateTemplateType(ImplementationGuideType igType, string name, string context, string contextType, int outputOrder)
        {
            if (igType == null)
                throw new ArgumentNullException("igType");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            TemplateType type = this.TemplateTypes.SingleOrDefault(y => y.ImplementationGuideTypeId == igType.Id && y.Name == name);

            if (type == null)
            {
                type = new TemplateType()
                {
                    Id = this.TemplateTypes.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    ImplementationGuideType = igType,
                    ImplementationGuideTypeId = igType.Id,
                    RootContext = context,
                    RootContextType = contextType,
                    OutputOrder = outputOrder
                };

                igType.TemplateTypes.Add(type);
                this.TemplateTypes.Add(type);
            }

            return type;
        }

        public ImplementationGuide FindOrCreateImplementationGuide(string igTypeName, string title, Organization organization = null, DateTime? publishDate = null, ImplementationGuide previousVersion = null)
        {
            ImplementationGuideType igType = this.FindImplementationGuideType(igTypeName);

            var ig = FindOrCreateImplementationGuide(igType, title, organization, publishDate);

            if (previousVersion != null)
                ig.SetPreviousVersion(previousVersion);

            return ig;
        }

        /// <summary>
        /// Craetes a new implementation guide for the specified implementation guide type.
        /// The new implementation guide is added to the mock object repository and the implementation guide type.
        /// </summary>
        /// <param name="igType">Required. The implementation guide type for the implementation guide (ex: CDA vs eMeasure)</param>
        /// <param name="title">Required. The title of the implementation guide.</param>
        /// <param name="organization">The organization for the implementation guide.</param>
        /// <param name="publishDate">The publishing date for the implementation guide.</param>
        /// <returns>A new instance of an implementation guide that has been added to the mock object repository.</returns>
        public ImplementationGuide FindOrCreateImplementationGuide(ImplementationGuideType igType, string title, Organization organization = null, DateTime? publishDate = null)
        {
            if (igType == null)
                throw new ArgumentNullException("igType");

            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException("title");

            ImplementationGuide ig = this.implementationGuides.SingleOrDefault(y => y.ImplementationGuideType == igType && y.Name.ToLower() == title.ToLower());

            if (organization == null)
                organization = this.FindOrCreateOrganization(DEFAULT_ORGANIZATION);

            if (ig == null)
            {
                ig = new ImplementationGuide()
                {
                    Id = this.ImplementationGuides.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = title,
                    ImplementationGuideType = igType,
                    ImplementationGuideTypeId = igType.Id,
                    Organization = organization,
                    OrganizationId = organization.Id,
                    PublishDate = publishDate,
                    Version = 1
                };

                igType.ImplementationGuides.Add(ig);
                this.ImplementationGuides.Add(ig);
            }

            return ig;
        }

        public TemplateConstraint CreatePrimitive(
            Template template, 
            TemplateConstraint parent, 
            string conformanceText, 
            string narrative, 
            string schematronTest = null, 
            bool? isBranch = null, 
            bool isPrimitiveSchRooted=false, 
            bool isInheritable=false, 
            int? number = null)
        {
            TemplateConstraint constraint = new TemplateConstraint()
            {
                Id = this.TemplateConstraints.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                IsPrimitive = true,
                Template = template,
                TemplateId = template.Id,
                Conformance = conformanceText,
                PrimitiveText = narrative,
                Schematron = schematronTest,
                IsBranch = isBranch != null ? isBranch.Value : false,
                IsSchRooted = isPrimitiveSchRooted,
                IsInheritable = isInheritable
            };

            if (number == null)
                constraint.Number = constraint.Id;
            else
                constraint.Number = number;

            if (parent != null)
            {
                constraint.ParentConstraint = parent;
                constraint.ParentConstraintId = parent.Id;
            }
            
            template.ChildConstraints.Add(constraint);
            this.TemplateConstraints.Add(constraint);

            return constraint;
        }

        /// <summary>
        /// Creates a new constraint for the specified template.
        /// The constraint is added to the mock object repository and the template.
        /// </summary>
        /// <param name="template">Required. The template for the constraint.</param>
        /// <param name="parentConstraint">The parent constraint.</param>
        /// <param name="containedTemplate">The contained template.</param>
        /// <param name="context">The context of the constraint.</param>
        /// <param name="conformance">The conformance of the constraint (SHALL, SHOULD, MAY, etc)</param>
        /// <param name="cardinality">The cardinality of the constraint ("1..1", "0..1", etc)</param>
        /// <param name="dataType">The data type of the constraint ("CD")</param>
        /// <param name="valueConformance">The value conformance of the constraint.</param>
        /// <param name="value">The value of the constraint (ex: "23423-X")</param>
        /// <param name="displayName">The display name for the value of the constraint.</param>
        /// <param name="valueSet">The valueset associated with the constraint.</param>
        /// <param name="codeSystem">The code system associated with the constraint.</param>
        /// <param name="description">The description of the constraint.</param>
        /// <returns>A new instance of TemplateConstraint that has been added to the specified Template's child constraints and to the mock object repository.</returns>
        public TemplateConstraint AddConstraintToTemplate(
            Template template, 
            TemplateConstraint parentConstraint = null, 
            Template containedTemplate = null, 
            string context = null, 
            string conformance = null, 
            string cardinality = null, 
            string dataType = null, 
            string valueConformance = null, 
            string value = null, 
            string displayName = null, 
            ValueSet valueSet = null, 
            CodeSystem codeSystem = null,
            string description = null,
            bool? isBranch = null,
            bool? isBranchIdentifier = null,
            bool isPrimitiveSchRooted = false,
            int? number = null,
            string category = null,
            bool isChoice = false)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            TemplateConstraint constraint = new TemplateConstraint()
            {
                Id = this.TemplateConstraints.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                Template = template,
                Context = context,
                Conformance = conformance,
                ValueConformance = valueConformance,
                TemplateId = template.Id,
                IsSchRooted = isPrimitiveSchRooted,
                Category = category,
                IsChoice = isChoice
            };

            if (number == null)
                constraint.Number = constraint.Id;
            else
                constraint.Number = number;

            if (parentConstraint != null)
            {
                parentConstraint.ChildConstraints.Add(constraint);
                constraint.ParentConstraint = parentConstraint;
                constraint.ParentConstraintId = parentConstraint.Id;
            }

            if (containedTemplate != null)
            {
                containedTemplate.ContainingConstraints.Add(constraint);
                constraint.ContainedTemplate = containedTemplate;
                constraint.ContainedTemplateId = containedTemplate.Id;
            }

            if (!string.IsNullOrEmpty(cardinality))
                constraint.Cardinality = cardinality;

            if (!string.IsNullOrEmpty(dataType))
                constraint.DataType = dataType;

            if (!string.IsNullOrEmpty(value))
                constraint.Value = value;

            if (!string.IsNullOrEmpty(displayName))
                constraint.DisplayName = displayName;

            if (valueSet != null)
            {
                valueSet.Constraints.Add(constraint);
                constraint.ValueSet = valueSet;
                constraint.ValueSetId = valueSet.Id;
            }

            if (codeSystem != null)
            {
                codeSystem.Constraints.Add(constraint);
                constraint.CodeSystem = codeSystem;
                constraint.CodeSystemId = codeSystem.Id;
            }

            if (!string.IsNullOrEmpty(description))
                constraint.Description = description;

            if (isBranch != null)
                constraint.IsBranch = isBranch.Value;

            if (isBranchIdentifier != null)
                constraint.IsBranchIdentifier = isBranchIdentifier.Value;

            template.ChildConstraints.Add(constraint);
            this.TemplateConstraints.Add(constraint);

            return constraint;
        }

        /// <summary>
        /// Attempts to find a Code System based on the oid specified.
        /// If none is found, a new instance is created and added to the mock object repository.
        /// </summary>
        /// <param name="name">Required. The name of the code system (Ex: "SNOMED CT")</param>
        /// <param name="oid">Required. The oid of the code system. This value is used to perform the search.</param>
        /// <param name="description">The description of the code system.</param>
        /// <param name="lastUpdate">The last update date of the code system.</param>
        /// <returns></returns>
        public CodeSystem FindOrCreateCodeSystem(string name, string oid, string description = null, DateTime? lastUpdate = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(oid))
                throw new ArgumentNullException("oid");

            CodeSystem codeSystem = this.CodeSystems.SingleOrDefault(y => y.Oid == oid);

            if (codeSystem == null)
            {
                codeSystem = new CodeSystem()
                {
                    Id = this.CodeSystems.Count() > 0 ? this.CodeSystems.Max(y => y.Id) + 1 : 1,
                    Name = name,
                    Oid = oid,
                    Description = description,
                    LastUpdate = lastUpdate != null ? lastUpdate.Value : DateTime.Now
                };

                this.CodeSystems.Add(codeSystem);
            }

            return codeSystem;
        }

        /// <summary>
        /// Attempts to find a Value Set based on the oid specified.
        /// If none is found, a new instance is created and added to the mock object repository.
        /// </summary>
        /// <param name="name">Required. The name of the value set.</param>
        /// <param name="oid">Required. The oid of the value set. This value is used to perform the search.</param>
        /// <param name="code">The code of the value set.</param>
        /// <param name="description">The description of the valueset.</param>
        /// <param name="intensional">Indicates whether the value set is intensional or extensional.</param>
        /// <param name="intensionalDefinition">Describes the algorithm used for intensional value sets.</param>
        /// <param name="lastUpdate">The last update date of the value set.</param>
        /// <returns>Either the found ValueSet or a new instance of one, which has been added to the mock object repository.</returns>
        public ValueSet FindOrCreateValueSet(string name, string oid, string code = null, string description = null, bool? intensional = null, string intensionalDefinition = null, DateTime? lastUpdate = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(oid))
                throw new ArgumentNullException("oid");

            ValueSet valueSet = this.ValueSets.SingleOrDefault(y => y.Oid == oid);

            if (valueSet == null)
            {
                valueSet = new ValueSet()
                {
                    Id = this.ValueSets.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    Oid = oid,
                    Description = intensionalDefinition,
                    Intensional = intensional,
                    IntensionalDefinition = intensionalDefinition,
                    Code = code,
                    LastUpdate = lastUpdate != null ? lastUpdate.Value : DateTime.Now
                };

                this.ValueSets.Add(valueSet);
            }

            return valueSet;
        }

        /// <summary>
        /// Attempts to find a ValueSetMember based on the valueset and code specified.
        /// If none is found, a new instance is created and added to the mock object repository.
        /// </summary>
        /// <param name="valueSet">Required. The valueset owning the member. This value is used to perform the search.</param>
        /// <param name="codeSystem">Required. The code system for the member.</param>
        /// <param name="code">Required. The code for the member. This value is used to perform the search.</param>
        /// <param name="displayName">The display name of the member.</param>
        /// <param name="valueSetStatus">The status of the member (ex: "active" or "inactive").</param>
        /// <param name="dateOfValueSetStatus">The date that the status of the member was set.</param>
        /// <param name="lastUpdate">The last update date of the value set member.</param>
        /// <returns>Either the found ValueSetMember, or a new instance of one which has been added to the valueset and the mock object repository.</returns>
        public ValueSetMember FindOrCreateValueSetMember(ValueSet valueSet, CodeSystem codeSystem, string code, string displayName, string valueSetStatus = null, string dateOfValueSetStatus = null, DateTime? lastUpdate = null)
        {
            if (valueSet == null)
                throw new ArgumentNullException("valueSet");

            if (codeSystem == null)
                throw new ArgumentNullException("codeSystem");

            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException("code");
            
            DateTime? statusDate = !string.IsNullOrEmpty(dateOfValueSetStatus) ? new DateTime?(DateTime.Parse(dateOfValueSetStatus)) : null;
            ValueSetMember member = this.ValueSetMembers.SingleOrDefault(y => y.ValueSetId == valueSet.Id && y.Code == code && y.Status == valueSetStatus && y.StatusDate == statusDate);

            if (member == null)
            {
                member = new ValueSetMember()
                {
                    Id = this.ValueSetMembers.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    ValueSet = valueSet,
                    ValueSetId = valueSet.Id,
                    CodeSystem = codeSystem,
                    CodeSystemId = codeSystem.Id,
                    Code = code,
                    DisplayName = displayName,
                    Status = valueSetStatus,
                    StatusDate = statusDate
                };

                this.ValueSetMembers.Add(member);
                valueSet.Members.Add(member);
            }

            return member;
        }

        public ImplementationGuideFile CreateImplementationGuideFile(ImplementationGuide ig, string fileName, string contentType, string mimeType, int? expectedErrorCount=null, byte[] content=null)
        {
            ImplementationGuideFile igf = new ImplementationGuideFile()
            {
                Id = this.ImplementationGuideFiles.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                FileName = fileName,
                ContentType = contentType,
                MimeType = mimeType,
                ExpectedErrorCount = expectedErrorCount,
                ImplementationGuide = ig,
                ImplementationGuideId = ig.Id
            };

            if (content != null)
            {
                this.CreateImplementationGuideFileData(igf, content);
            }

            this.ImplementationGuideFiles.Add(igf);
            ig.Files.Add(igf);

            return igf;
        }

        public ImplementationGuideFileData CreateImplementationGuideFileData(ImplementationGuideFile igFile, byte[] content, string note=null, DateTime? updatedDate = null)
        {
            ImplementationGuideFileData igFileData = new ImplementationGuideFileData()
            {
                Id = this.ImplementationGuideFileDatas.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                ImplementationGuideFile = igFile,
                ImplementationGuideFileId = igFile.Id,
                Data = content,
                Note = note,
                UpdatedDate = updatedDate != null ? updatedDate.Value : DateTime.Now,
                UpdatedBy = "unit test"
            };

            this.ImplementationGuideFileDatas.Add(igFileData);
            igFile.Versions.Add(igFileData);

            return igFileData;
        }

        public User FindOrCreateUser(string username)
        {
            User foundUser = this.Users.SingleOrDefault(y => y.UserName.ToLower() == username.ToLower());

            if (foundUser != null)
                return foundUser;

            User newUser = new User()
            {
                Id = this.Users.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                UserName = username
            };

            this.Users.Add(newUser);

            return newUser;
        }

        public Role FindOrCreateRole(string roleName)
        {
            Role role = this.Roles.SingleOrDefault(y => y.Name.ToLower() == roleName.ToLower());

            if (role != null)
                return role;

            role = new Role()
            {
                Id = this.Roles.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                Name = roleName
            };

            this.Roles.Add(role);

            return role;
        }

        public void CreateRole(string roleName, params string[] securables)
        {
            Role role = FindOrCreateRole(roleName);

            AssociateSecurableToRole(roleName, securables);
        }

        public void AssociateSecurableToRole(string roleName, params string[] securables)
        {
            Role role = FindOrCreateRole(roleName);

            foreach (string cSecurable in securables)
            {
                AppSecurable securable = FindOrCreateSecurable(cSecurable);

                RoleAppSecurable newRoleAppSecurable = new RoleAppSecurable()
                {
                    Id = this.RoleAppSecurables.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    AppSecurableId = securable.Id,
                    AppSecurable = securable,
                    RoleId = role.Id,
                    Role = role
                };

                this.RoleAppSecurables.Add(newRoleAppSecurable);
                role.AppSecurables.Add(newRoleAppSecurable);
            }
        }

        public void RemoveSecurableFromRole(string roleName, params string[] securables)
        {
            Role role = FindOrCreateRole(roleName);

            foreach (string cSecurable in securables)
            {
                AppSecurable securable = FindOrCreateSecurable(cSecurable);

                RoleAppSecurable foundRoleSecurable = this.RoleAppSecurables.Single(y => y.Role == role && y.AppSecurable == securable);

                if (foundRoleSecurable != null)
                {
                    role.AppSecurables.Remove(foundRoleSecurable);
                    this.RoleAppSecurables.Remove(foundRoleSecurable);
                }
            }
        }

        public void AssociateUserWithRole(string userName, string roleName)
        {
            User foundUser = this.Users.Single(y => y.UserName.ToLower() == userName.ToLower());
            Role foundRole = this.Roles.Single(y => y.Name.ToLower() == roleName.ToLower());

            if (foundUser.Roles.Count(y => y.RoleId == foundRole.Id) > 0)
                return;

            UserRole newUserRole = new UserRole()
            {
                Id = this.UserRoles.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                UserId = foundUser.Id,
                User = foundUser,
                RoleId = foundRole.Id,
                Role = foundRole
            };

            foundUser.Roles.Add(newUserRole);
            this.UserRoles.Add(newUserRole);
        }

        public AppSecurable FindOrCreateSecurable(string securableName, string description = null)
        {
            AppSecurable securable = this.AppSecurables.SingleOrDefault(y => y.Name.ToLower() == securableName.ToLower());

            if (securable != null)
                return securable;

            securable = new AppSecurable()
            {
                Id = this.AppSecurables.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                Name = securableName,
                Description = description
            };

            this.AppSecurables.Add(securable);

            return securable;
        }

        public void FindOrCreateSecurables(params string[] securableNames)
        {
            foreach (string securableName in securableNames)
            {
                this.FindOrCreateSecurable(securableName);
            }
        }

        public void GrantImplementationGuidePermission(User user, ImplementationGuide ig, string permission)
        {
            var newPerm = new ImplementationGuidePermission()
            {
                ImplementationGuide = ig,
                ImplementationGuideId = ig.Id,
                User = user,
                UserId = user.Id,
                Permission = permission,
                Type = "User"
            };

            this.ImplementationGuidePermissions.Add(newPerm);
            ig.Permissions.Add(newPerm);
        }

        public ImplementationGuideTypeDataType FindOrCreateDataType(string igTypeName, string dataTypeName)
        {
            var igType = this.FindImplementationGuideType(igTypeName);
            var dataType = igType.DataTypes.SingleOrDefault(y => y.DataTypeName == dataTypeName);

            if (dataType == null)
            {
                dataType = new ImplementationGuideTypeDataType()
                {
                    DataTypeName = dataTypeName,
                    ImplementationGuideType = igType
                };

                this.ImplementationGuideTypeDataTypes.Add(dataType);
            }

            return dataType;
        }

        #endregion

        #region Simple Find Methods

        public ImplementationGuideType FindImplementationGuideType(string igType)
        {
            return this.ImplementationGuideTypes.SingleOrDefault(y => y.Name == igType);
        }

        public TemplateType FindTemplateType(string igType, string templateType)
        {
            return this.TemplateTypes.SingleOrDefault(y => y.ImplementationGuideType.Name == igType && y.Name == templateType);
        }

        #endregion

        #region Unimplemented Methods

        public DbConnection Connection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void ChangeObjectState(object entity, System.Data.Entity.EntityState entityState)
        {
            
        }

        public System.Data.IDbTransaction BeginTransaction()
        {
            return null;
        }

        public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return null;
        }


        public int SaveChanges()
        {
            return 0;
        }

        public void Dispose()
        {

        }

        #endregion

        #region Stored Procedures

        private IEnumerable<Template> GetTemplateReferences(List<Template> checkedTemplates, Template template, bool includeInferred)
        {
            List<Template> templates = new List<Template>();

            if (checkedTemplates.Contains(template))
                return templates;

            // Add the template to the list of checked templates right away, because we know we are going to check it
            // and in case one of the contained or implied template references itself, directly.
            checkedTemplates.Add(template);

            foreach (var constraint in template.ChildConstraints.Where(y => y.ContainedTemplate != null))
            {
                var templateAlreadyIncluded = templates.Contains(constraint.ContainedTemplate);
                var externalTemplate = constraint.ContainedTemplate.OwningImplementationGuideId != template.OwningImplementationGuideId;

                if ((!externalTemplate || includeInferred) && !templateAlreadyIncluded)
                {
                    templates.Add(constraint.ContainedTemplate);

                    templates.AddRange(
                        GetTemplateReferences(checkedTemplates, constraint.ContainedTemplate, includeInferred));
                }
            }

            if (template.ImpliedTemplate != null)
            {
                var externalTemplate = template.ImpliedTemplate.OwningImplementationGuideId != template.OwningImplementationGuideId;
                var templateAlreadyIncluded = templates.Contains(template.ImpliedTemplate);

                if ((!externalTemplate || includeInferred) && !templateAlreadyIncluded)
                {
                    templates.Add(template.ImpliedTemplate);

                    templates.AddRange(
                        GetTemplateReferences(checkedTemplates, template.ImpliedTemplate, includeInferred));
                }
            }

            return templates.Distinct();
        }

        public IEnumerable<SearchValueSetResult> SearchValueSet(Nullable<global::System.Int32> userId, global::System.String searchText, Nullable<global::System.Int32> count, Nullable<global::System.Int32> page, global::System.String orderProperty, Nullable<global::System.Boolean> orderDesc)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId, string[] categories)
        {
            var implementationGuide = this.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            // A list of templates that will be used by GetTemplateReferences() to determine
            // if a template has already been checked, so that an endless loop does not occur
            List<Template> checkedTemplates = new List<Template>();

            List<Template> templates = new List<Template>();
            var currentVersionId = (Nullable<int>)implementationGuide.Id;
            var retiredStatus = PublishStatus.GetRetiredStatus(this);

            while (currentVersionId.HasValue)
            {
                ImplementationGuide currentVersion = this.ImplementationGuides.Single(ig => ig.Id == currentVersionId);

                // Ignore previous version IG templates that have been retired
                var childTemplates = currentVersion.ChildTemplates.Where(y => y.OwningImplementationGuideId == implementationGuide.Id || y.Status != retiredStatus);

                // Loop through all templates in the current version
                foreach (Template lTemplate in childTemplates)
                {
                    List<Template> nextVersionTemplates = new List<Template>();
                    var nextCurrent = lTemplate;

                    // Build a list of all this template's next ids
                    while (nextCurrent != null)
                    {
                        nextVersionTemplates.Add(nextCurrent);

                        // Make sure we stop at this version of the IG, and don't look at future versions of the IG
                        if (nextCurrent.OwningImplementationGuideId != implementationGuide.Id)
                            nextCurrent = nextCurrent.NextVersions.FirstOrDefault();
                        else
                            nextCurrent = null;
                    }

                    // Skip the tempalte if it has been retired in a future version
                    if (nextVersionTemplates.Count(y => y.Status == retiredStatus && y.OwningImplementationGuideId != implementationGuide.Id) > 0)
                        continue;

                    // Look if the previous version has a newer version already in the list
                    IEnumerable<Template> lNewerVersions = from t in templates
                                                           join nt in nextVersionTemplates on t.PreviousVersion equals nt
                                                           select t;

                    // If there are no newer versions of the template (which means it is not included yet), add it to the list
                    if (lNewerVersions.Count() == 0)
                    {
                        templates.Add(lTemplate);

                        templates.AddRange(
                            GetTemplateReferences(checkedTemplates, lTemplate, inferred == null ? true : inferred.Value));
                    }
                }

                // Move to the previous version of the IG
                currentVersionId = currentVersion.PreviousVersionImplementationGuideId;
            }

            return templates.Select(y => (int?)y.Id);
        }

        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.SearchTemplates(Nullable<global::System.Int32> userId, Nullable<global::System.Int32> filterImplementationGuideId, global::System.String filterName, global::System.String filterIdentifier, Nullable<global::System.Int32> filterTemplateTypeId, Nullable<global::System.Int32> filterOrganizationId, global::System.String filterContextType, global::System.String queryText)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public static class TemplateConstraintExtensions
    {
        public static TemplateConstraint AddChildConstraintToTemplate(
            this TemplateConstraint parentConstraint,
            MockObjectRepository tdb,
            Template template,
            Template containedTemplate = null,
            string context = null,
            string conformance = null,
            string cardinality = null,
            string dataType = null,
            string valueConformance = null,
            string value = null,
            string displayName = null,
            ValueSet valueSet = null,
            CodeSystem codeSystem = null,
            string description = null,
            bool? isBranch = null,
            bool? isBranchIdentifier = null,
            bool isPrimitiveSchRooted = false,
            int? number = null,
            string category = null)
        {
            return tdb.AddConstraintToTemplate(template, parentConstraint, containedTemplate, context, conformance, cardinality, dataType, valueConformance, value, displayName, valueSet, codeSystem, description, isBranch, isBranchIdentifier, isPrimitiveSchRooted, number, category);
        }
    }
}