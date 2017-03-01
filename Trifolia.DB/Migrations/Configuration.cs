namespace Trifolia.DB.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Trifolia.DB.TrifoliaDatabase>
    {
        private AppSecurable[] appSecurables;
        private Role[] roles;

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

            this.appSecurables = new AppSecurable[]
            {
                new AppSecurable() { Id = 1, Name = "TemplateList", DisplayName = "List Templates", Description = "NULL" },
                new AppSecurable() { Id = 2, Name = "IGManagementList", DisplayName = "List Implementation Guides", Description = "NULL" },
                new AppSecurable() { Id = 3, Name = "LandingPage", DisplayName = "Landing Page", Description = "NULL" },
                new AppSecurable() { Id = 4, Name = "ImplementationGuideEdit", DisplayName = "Edit Implementation Guide", Description = "NULL" },
                new AppSecurable() { Id = 5, Name = "ImplementationGuideEditBookmarks", DisplayName = "Edit Implementation Guide Bookmarks", Description = "" },
                new AppSecurable() { Id = 6, Name = "ImplementationGuideNotes", DisplayName = "View Implementation Guide Notes", Description = "NULL" },
                new AppSecurable() { Id = 7, Name = "ImplementationGuidePrimitives", DisplayName = "View Implementation Guide Primitives", Description = "NULL" },
                new AppSecurable() { Id = 8, Name = "ImplementationGuideAuditTrail", DisplayName = "View Implementation Guide Audit Trail", Description = "NULL" },
                new AppSecurable() { Id = 9, Name = "ValueSetList", DisplayName = "List Value Sets", Description = "NULL" },
                new AppSecurable() { Id = 10, Name = "ValueSetEdit", DisplayName = "Edit Value Sets", Description = "NULL" },
                new AppSecurable() { Id = 11, Name = "CodeSystemList", DisplayName = "List Code Systems", Description = "NULL" },
                new AppSecurable() { Id = 12, Name = "CodeSystemEdit", DisplayName = "Edit Code Systems", Description = "NULL" },
                new AppSecurable() { Id = 13, Name = "ExportWordDocuments", DisplayName = "Export MS Word Documents", Description = "NULL" },
                new AppSecurable() { Id = 14, Name = "ExportVocabulary", DisplayName = "Export Vocabulary", Description = "NULL" },
                new AppSecurable() { Id = 15, Name = "ExportSchematron", DisplayName = "Export Schematron", Description = "NULL" },
                new AppSecurable() { Id = 16, Name = "TemplateEdit", DisplayName = "Edit Templates", Description = "NULL" },
                new AppSecurable() { Id = 17, Name = "TemplateCopy", DisplayName = "Copy Templates", Description = "NULL" },
                new AppSecurable() { Id = 18, Name = "TemplateDelete", DisplayName = "Delete Templates", Description = "NULL" },
                new AppSecurable() { Id = 19, Name = "Admin", DisplayName = "Admin Functions", Description = "NULL" },
                new AppSecurable() { Id = 20, Name = "ReportTemplateReview", DisplayName = "View Template Review Report", Description = "NULL" },
                new AppSecurable() { Id = 21, Name = "ReportTemplateCompliance", DisplayName = "View Template Compliance Report", Description = "NULL" },
                new AppSecurable() { Id = 22, Name = "OrganizationList", DisplayName = "Organization List", Description = "NULL" },
                new AppSecurable() { Id = 23, Name = "OrganizationDetails", DisplayName = "Organization Details", Description = "NULL" },
                new AppSecurable() { Id = 24, Name = "PublishSettings", DisplayName = "Publish Settings", Description = "NULL" },
                new AppSecurable() { Id = 25, Name = "ExportXML", DisplayName = "Export Templates XML", Description = "Ability to export templates to an XML format" },
                new AppSecurable() { Id = 26, Name = "IGFileManagement", DisplayName = "IG File Management", Description = "Allows users to upload/update/remove files associated with an implementation guide." },
                new AppSecurable() { Id = 27, Name = "IGFileView", DisplayName = "IG File View", Description = "Allows users view files associated with implementation guides they have permission to access." },
                new AppSecurable() { Id = 28, Name = "ExportGreen", DisplayName = "Export green artifacts", Description = "NULL" },
                new AppSecurable() { Id = 29, Name = "TerminologyOverride", DisplayName = "Terminology Override", Description = "Ability to override locked value sets and code systems." },
                new AppSecurable() { Id = 30, Name = "GreenModel", DisplayName = "Green Modeling", Description = "Allows user to create, edit and delete green models for templates." },
                new AppSecurable() { Id = 31, Name = "TemplateMove", DisplayName = "Move Templates", Description = "Ability to move a template from one implementation guide to another" },
                new AppSecurable() { Id = 32, Name = "WebIG", DisplayName = "Web-based IG", Description = "Ability to view an implementation guide's web-based IG" },
                new AppSecurable() { Id = 33, Name = "Import", DisplayName = "Import", Description = "The ability to import implementation guides and templates into Trifolia" }
            };

            this.roles = new Role[] {
                new Role() { Name = "Administrators", IsDefault = true, IsAdmin = true },
                new Role() { Name = "Template Authors", IsDefault = false, IsAdmin = false },
                new Role() { Name = "Users", IsDefault = false, IsAdmin = false },
                new Role() { Name = "IG Admins", IsDefault = false, IsAdmin = false },
                new Role() { Name = "Terminology Admins", IsDefault = false, IsAdmin = false }
            };
        }

        private void AssignSecurableToRole(Trifolia.DB.TrifoliaDatabase context, string securableName, string roleName)
        {
            AppSecurable appSecurable = this.appSecurables.SingleOrDefault(y => y.Name == securableName);
            Role role = this.roles.SingleOrDefault(y => y.Name == roleName);

            if (context.RoleAppSecurables.FirstOrDefault(y => y.Role.Name == roleName && y.AppSecurable.Name == securableName) != null)
                return;

            context.RoleAppSecurables.Add(new RoleAppSecurable()
            {
                Role = role,
                AppSecurable = appSecurable
            });
        }

        private void SeedCDA(Trifolia.DB.TrifoliaDatabase context)
        {
            context.TemplateTypes.AddOrUpdate(tt => new { tt.ImplementationGuideTypeId, tt.Name },
                new TemplateType() { ImplementationGuideTypeId = 1, Name = "document", OutputOrder = 1, RootContext = "ClinicalDocument", RootContextType = "ClinicalDocument" },
                new TemplateType() { ImplementationGuideTypeId = 1, Name = "section", OutputOrder = 2, RootContext = "section", RootContextType = "Section" },
                new TemplateType() { ImplementationGuideTypeId = 1, Name = "entry", OutputOrder = 3, RootContext = "entry", RootContextType = "Entry" },
                new TemplateType() { ImplementationGuideTypeId = 1, Name = "subentry", OutputOrder = 4, RootContext = "entry", RootContextType = "Entry" },
                new TemplateType() { ImplementationGuideTypeId = 1, Name = "unspecified", OutputOrder = 5, RootContext = "", RootContextType = "" }
            );
        }

        private void SeedEMeasure(Trifolia.DB.TrifoliaDatabase context)
        {
            context.TemplateTypes.AddOrUpdate(tt => new { tt.ImplementationGuideTypeId, tt.Name },
                new TemplateType() { ImplementationGuideTypeId = 2, Name = "Document", OutputOrder = 1, RootContext = "QualityMeasureDocument", RootContextType = "QualityMeasureDocument" },
                new TemplateType() { ImplementationGuideTypeId = 2, Name = "Section", OutputOrder = 2, RootContext = "section", RootContextType = "Section" },
                new TemplateType() { ImplementationGuideTypeId = 2, Name = "Entry", OutputOrder = 3, RootContext = "entry", RootContextType = "Entry" }
            );
        }

        private void SeedHQMF(Trifolia.DB.TrifoliaDatabase context)
        {
            context.TemplateTypes.AddOrUpdate(tt => new { tt.ImplementationGuideTypeId, tt.Name },
                new TemplateType() { ImplementationGuideTypeId = 3, Name = "Document", OutputOrder = 1, RootContext = "QualityMeasureDocument", RootContextType = "QualityMeasureDocument" },
                new TemplateType() { ImplementationGuideTypeId = 3, Name = "Section", OutputOrder = 2, RootContext = "component", RootContextType = "Component2" },
                new TemplateType() { ImplementationGuideTypeId = 3, Name = "Entry", OutputOrder = 3, RootContext = "entry", RootContextType = "SourceOf" }
            );
        }

        private void SeedFHIRDSTU1(Trifolia.DB.TrifoliaDatabase context)
        {
            context.TemplateTypes.AddOrUpdate(tt => new { tt.ImplementationGuideTypeId, tt.Name },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "AdverseReaction", OutputOrder = 1, RootContext = "AdverseReaction", RootContextType = "AdverseReaction" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Alert", OutputOrder = 2, RootContext = "Alert", RootContextType = "Alert" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "AllergyIntolerance", OutputOrder = 3, RootContext = "AllergyIntolerance", RootContextType = "AllergyIntolerance" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Binary", OutputOrder = 4, RootContext = "Binary", RootContextType = "Binary" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "CarePlan", OutputOrder = 5, RootContext = "CarePlan", RootContextType = "CarePlan" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Composition", OutputOrder = 6, RootContext = "Composition", RootContextType = "Composition" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "ConceptMap", OutputOrder = 7, RootContext = "ConceptMap", RootContextType = "ConceptMap" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Condition", OutputOrder = 8, RootContext = "Condition", RootContextType = "Condition" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Conformance", OutputOrder = 9, RootContext = "Conformance", RootContextType = "Conformance" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Device", OutputOrder = 10, RootContext = "Device", RootContextType = "Device" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "DeviceObservationReport", OutputOrder = 11, RootContext = "DeviceObservationReport", RootContextType = "DeviceObservationReport" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "DiagnosticOrder", OutputOrder = 12, RootContext = "DiagnosticOrder", RootContextType = "DiagnosticOrder" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "DiagnosticReport", OutputOrder = 13, RootContext = "DiagnosticReport", RootContextType = "DiagnosticReport" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "DocumentReference", OutputOrder = 14, RootContext = "DocumentReference", RootContextType = "DocumentReference" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "DocumentManifest", OutputOrder = 15, RootContext = "DocumentManifest", RootContextType = "DocumentManifest" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Encounter", OutputOrder = 16, RootContext = "Encounter", RootContextType = "Encounter" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "FamilyHistory", OutputOrder = 17, RootContext = "FamilyHistory", RootContextType = "FamilyHistory" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Group", OutputOrder = 18, RootContext = "Group", RootContextType = "Group" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "ImagingStudy", OutputOrder = 19, RootContext = "ImagingStudy", RootContextType = "ImagingStudy" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Immunization", OutputOrder = 20, RootContext = "Immunization", RootContextType = "Immunization" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "ImmunizationRecommendation", OutputOrder = 21, RootContext = "ImmunizationRecommendation", RootContextType = "ImmunizationRecommendation" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "List", OutputOrder = 22, RootContext = "List", RootContextType = "List" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Location", OutputOrder = 23, RootContext = "Location", RootContextType = "Location" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Media", OutputOrder = 24, RootContext = "Media", RootContextType = "Media" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Medication", OutputOrder = 25, RootContext = "Medication", RootContextType = "Medication" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "MedicationAdministration", OutputOrder = 26, RootContext = "MedicationAdministration", RootContextType = "MedicationAdministration" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "MedicationDispense", OutputOrder = 27, RootContext = "MedicationDispense", RootContextType = "MedicationDispense" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "MedicationPrescription", OutputOrder = 28, RootContext = "MedicationPrescription", RootContextType = "MedicationPrescription" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "MedicationStatement", OutputOrder = 29, RootContext = "MedicationStatement", RootContextType = "MedicationStatement" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "MessageHeader", OutputOrder = 30, RootContext = "MessageHeader", RootContextType = "MessageHeader" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Observation", OutputOrder = 31, RootContext = "Observation", RootContextType = "Observation" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "OperationOutcome", OutputOrder = 32, RootContext = "OperationOutcome", RootContextType = "OperationOutcome" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Order", OutputOrder = 33, RootContext = "Order", RootContextType = "Order" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "OrderResponse", OutputOrder = 34, RootContext = "OrderResponse", RootContextType = "OrderResponse" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Organization", OutputOrder = 35, RootContext = "Organization", RootContextType = "Organization" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Other", OutputOrder = 36, RootContext = "Other", RootContextType = "Other" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Patient", OutputOrder = 37, RootContext = "Patient", RootContextType = "Patient" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Practitioner", OutputOrder = 38, RootContext = "Practitioner", RootContextType = "Practitioner" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Procedure", OutputOrder = 39, RootContext = "Procedure", RootContextType = "Procedure" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Profile", OutputOrder = 40, RootContext = "Profile", RootContextType = "Profile" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Provenance", OutputOrder = 41, RootContext = "Provenance", RootContextType = "Provenance" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Query", OutputOrder = 42, RootContext = "Query", RootContextType = "Query" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Questionnaire", OutputOrder = 43, RootContext = "Questionnaire", RootContextType = "Questionnaire" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "RelatedPerson", OutputOrder = 44, RootContext = "RelatedPerson", RootContextType = "RelatedPerson" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "SecurityEvent", OutputOrder = 45, RootContext = "SecurityEvent", RootContextType = "SecurityEvent" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Specimen", OutputOrder = 46, RootContext = "Specimen", RootContextType = "Specimen" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Substance", OutputOrder = 47, RootContext = "Substance", RootContextType = "Substance" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Supply", OutputOrder = 48, RootContext = "Supply", RootContextType = "Supply" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "ValueSet", OutputOrder = 49, RootContext = "ValueSet", RootContextType = "ValueSet" },
                new TemplateType() { ImplementationGuideTypeId = 4, Name = "Extension", OutputOrder = 50, RootContext = "Extension", RootContextType = "Extension" }
            );
        }

        private void SeedFHIRDSTU2(Trifolia.DB.TrifoliaDatabase context)
        {
            context.TemplateTypes.AddOrUpdate(tt => new { tt.ImplementationGuideTypeId, tt.Name },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "AllergyIntolerance", OutputOrder = 1, RootContext = "AllergyIntolerance", RootContextType = "AllergyIntolerance" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Appointment", OutputOrder = 2, RootContext = "Appointment", RootContextType = "Appointment" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "AppointmentResponse", OutputOrder = 3, RootContext = "AppointmentResponse", RootContextType = "AppointmentResponse" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "AuditEvent", OutputOrder = 4, RootContext = "AuditEvent", RootContextType = "AuditEvent" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Basic", OutputOrder = 5, RootContext = "Basic", RootContextType = "Basic" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Binary", OutputOrder = 6, RootContext = "Binary", RootContextType = "Binary" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "BodySite", OutputOrder = 7, RootContext = "BodySite", RootContextType = "BodySite" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Bundle", OutputOrder = 8, RootContext = "Bundle", RootContextType = "Bundle" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "CarePlan", OutputOrder = 9, RootContext = "CarePlan", RootContextType = "CarePlan" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Claim", OutputOrder = 10, RootContext = "Claim", RootContextType = "Claim" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ClaimResponse", OutputOrder = 11, RootContext = "ClaimResponse", RootContextType = "ClaimResponse" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ClinicalImpression", OutputOrder = 12, RootContext = "ClinicalImpression", RootContextType = "ClinicalImpression" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Communication", OutputOrder = 13, RootContext = "Communication", RootContextType = "Communication" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "CommunicationRequest", OutputOrder = 14, RootContext = "CommunicationRequest", RootContextType = "CommunicationRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Composition", OutputOrder = 15, RootContext = "Composition", RootContextType = "Composition" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ConceptMap", OutputOrder = 16, RootContext = "ConceptMap", RootContextType = "ConceptMap" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Condition", OutputOrder = 17, RootContext = "Condition", RootContextType = "Condition" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Conformance", OutputOrder = 18, RootContext = "Conformance", RootContextType = "Conformance" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DetectedIssue", OutputOrder = 19, RootContext = "DetectedIssue", RootContextType = "DetectedIssue" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Coverage", OutputOrder = 20, RootContext = "Coverage", RootContextType = "Coverage" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DataElement", OutputOrder = 21, RootContext = "DataElement", RootContextType = "DataElement" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Device", OutputOrder = 22, RootContext = "Device", RootContextType = "Device" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DeviceComponent", OutputOrder = 23, RootContext = "DeviceComponent", RootContextType = "DeviceComponent" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DeviceMetric", OutputOrder = 24, RootContext = "DeviceMetric", RootContextType = "DeviceMetric" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DeviceUseRequest", OutputOrder = 25, RootContext = "DeviceUseRequest", RootContextType = "DeviceUseRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DeviceUseStatement", OutputOrder = 26, RootContext = "DeviceUseStatement", RootContextType = "DeviceUseStatement" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DiagnosticOrder", OutputOrder = 27, RootContext = "DiagnosticOrder", RootContextType = "DiagnosticOrder" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DiagnosticReport", OutputOrder = 28, RootContext = "DiagnosticReport", RootContextType = "DiagnosticReport" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DocumentManifest", OutputOrder = 29, RootContext = "DocumentManifest", RootContextType = "DocumentManifest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "DocumentReference", OutputOrder = 30, RootContext = "DocumentReference", RootContextType = "DocumentReference" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "EligibilityRequest", OutputOrder = 31, RootContext = "EligibilityRequest", RootContextType = "EligibilityRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "EligibilityResponse", OutputOrder = 32, RootContext = "EligibilityResponse", RootContextType = "EligibilityResponse" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Encounter", OutputOrder = 33, RootContext = "Encounter", RootContextType = "Encounter" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "EnrollmentRequest", OutputOrder = 34, RootContext = "EnrollmentRequest", RootContextType = "EnrollmentRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "EnrollmentResponse", OutputOrder = 35, RootContext = "EnrollmentResponse", RootContextType = "EnrollmentResponse" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "EpisodeOfCare", OutputOrder = 36, RootContext = "EpisodeOfCare", RootContextType = "EpisodeOfCare" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ExplanationOfBenefit", OutputOrder = 37, RootContext = "ExplanationOfBenefit", RootContextType = "ExplanationOfBenefit" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "FamilyMemberHistory", OutputOrder = 38, RootContext = "FamilyMemberHistory", RootContextType = "FamilyMemberHistory" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Flag", OutputOrder = 39, RootContext = "Flag", RootContextType = "Flag" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Goal", OutputOrder = 40, RootContext = "Goal", RootContextType = "Goal" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Group", OutputOrder = 41, RootContext = "Group", RootContextType = "Group" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "HealthcareService", OutputOrder = 42, RootContext = "HealthcareService", RootContextType = "HealthcareService" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ImagingObjectSelection", OutputOrder = 43, RootContext = "ImagingObjectSelection", RootContextType = "ImagingObjectSelection" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ImagingStudy", OutputOrder = 44, RootContext = "ImagingStudy", RootContextType = "ImagingStudy" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Immunization", OutputOrder = 45, RootContext = "Immunization", RootContextType = "Immunization" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ImmunizationRecommendation", OutputOrder = 46, RootContext = "ImmunizationRecommendation", RootContextType = "ImmunizationRecommendation" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ImplementationGuide", OutputOrder = 47, RootContext = "ImplementationGuide", RootContextType = "ImplementationGuide" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "List", OutputOrder = 48, RootContext = "List", RootContextType = "List" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Location", OutputOrder = 49, RootContext = "Location", RootContextType = "Location" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Media", OutputOrder = 50, RootContext = "Media", RootContextType = "Media" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Medication", OutputOrder = 51, RootContext = "Medication", RootContextType = "Medication" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "MedicationAdministration", OutputOrder = 52, RootContext = "MedicationAdministration", RootContextType = "MedicationAdministration" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "MedicationDispense", OutputOrder = 53, RootContext = "MedicationDispense", RootContextType = "MedicationDispense" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "MedicationOrder", OutputOrder = 54, RootContext = "MedicationOrder", RootContextType = "MedicationOrder" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "MedicationStatement", OutputOrder = 55, RootContext = "MedicationStatement", RootContextType = "MedicationStatement" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "MessageHeader", OutputOrder = 56, RootContext = "MessageHeader", RootContextType = "MessageHeader" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "NamingSystem", OutputOrder = 57, RootContext = "NamingSystem", RootContextType = "NamingSystem" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "NutritionOrder", OutputOrder = 58, RootContext = "NutritionOrder", RootContextType = "NutritionOrder" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Observation", OutputOrder = 59, RootContext = "Observation", RootContextType = "Observation" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "OperationDefinition", OutputOrder = 60, RootContext = "OperationDefinition", RootContextType = "OperationDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "OperationOutcome", OutputOrder = 61, RootContext = "OperationOutcome", RootContextType = "OperationOutcome" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Order", OutputOrder = 62, RootContext = "Order", RootContextType = "Order" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "OrderResponse", OutputOrder = 63, RootContext = "OrderResponse", RootContextType = "OrderResponse" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Organization", OutputOrder = 64, RootContext = "Organization", RootContextType = "Organization" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Parameters", OutputOrder = 65, RootContext = "Parameters", RootContextType = "Parameters" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Patient", OutputOrder = 66, RootContext = "Patient", RootContextType = "Patient" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "PaymentNotice", OutputOrder = 67, RootContext = "PaymentNotice", RootContextType = "PaymentNotice" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "PaymentReconciliation", OutputOrder = 68, RootContext = "PaymentReconciliation", RootContextType = "PaymentReconciliation" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Person", OutputOrder = 69, RootContext = "Person", RootContextType = "Person" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Practitioner", OutputOrder = 70, RootContext = "Practitioner", RootContextType = "Practitioner" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Procedure", OutputOrder = 71, RootContext = "Procedure", RootContextType = "Procedure" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ProcessRequest", OutputOrder = 72, RootContext = "ProcessRequest", RootContextType = "ProcessRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ProcessResponse", OutputOrder = 73, RootContext = "ProcessResponse", RootContextType = "ProcessResponse" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ProcedureRequest", OutputOrder = 74, RootContext = "ProcedureRequest", RootContextType = "ProcedureRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Provenance", OutputOrder = 75, RootContext = "Provenance", RootContextType = "Provenance" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Questionnaire", OutputOrder = 76, RootContext = "Questionnaire", RootContextType = "Questionnaire" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "QuestionnaireResponse", OutputOrder = 77, RootContext = "QuestionnaireResponse", RootContextType = "QuestionnaireResponse" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ReferralRequest", OutputOrder = 78, RootContext = "ReferralRequest", RootContextType = "ReferralRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "RelatedPerson", OutputOrder = 79, RootContext = "RelatedPerson", RootContextType = "RelatedPerson" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "RiskAssessment", OutputOrder = 80, RootContext = "RiskAssessment", RootContextType = "RiskAssessment" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Schedule", OutputOrder = 81, RootContext = "Schedule", RootContextType = "Schedule" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "SearchParameter", OutputOrder = 82, RootContext = "SearchParameter", RootContextType = "SearchParameter" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Slot", OutputOrder = 83, RootContext = "Slot", RootContextType = "Slot" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Specimen", OutputOrder = 84, RootContext = "Specimen", RootContextType = "Specimen" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "StructureDefinition", OutputOrder = 85, RootContext = "StructureDefinition", RootContextType = "StructureDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Subscription", OutputOrder = 86, RootContext = "Subscription", RootContextType = "Subscription" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Substance", OutputOrder = 87, RootContext = "Substance", RootContextType = "Substance" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "SupplyRequest", OutputOrder = 88, RootContext = "SupplyRequest", RootContextType = "SupplyRequest" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "SupplyDelivery", OutputOrder = 89, RootContext = "SupplyDelivery", RootContextType = "SupplyDelivery" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "TestScript", OutputOrder = 90, RootContext = "TestScript", RootContextType = "TestScript" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "ValueSet", OutputOrder = 91, RootContext = "ValueSet", RootContextType = "ValueSet" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "VisionPrescription", OutputOrder = 92, RootContext = "VisionPrescription", RootContextType = "VisionPrescription" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Extension", OutputOrder = 93, RootContext = "Extension", RootContextType = "Extension" },
                new TemplateType() { ImplementationGuideTypeId = 5, Name = "Contract", OutputOrder = 94, RootContext = "Contract", RootContextType = "Contract" }
            );
        }

        private void SeedFHIRSTU3(Trifolia.DB.TrifoliaDatabase context)
        {
            context.TemplateTypes.AddOrUpdate(tt => new { tt.ImplementationGuideTypeId, tt.Name },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Account", OutputOrder = 1, RootContext = "Account", RootContextType = "Account" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ActivityDefinition", OutputOrder = 2, RootContext = "ActivityDefinition", RootContextType = "ActivityDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "AllergyIntolerance", OutputOrder = 3, RootContext = "AllergyIntolerance", RootContextType = "AllergyIntolerance" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Appointment", OutputOrder = 4, RootContext = "Appointment", RootContextType = "Appointment" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "AppointmentResponse", OutputOrder = 5, RootContext = "AppointmentResponse", RootContextType = "AppointmentResponse" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "AuditEvent", OutputOrder = 6, RootContext = "AuditEvent", RootContextType = "AuditEvent" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Basic", OutputOrder = 7, RootContext = "Basic", RootContextType = "Basic" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Binary", OutputOrder = 8, RootContext = "Binary", RootContextType = "Binary" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "BodySite", OutputOrder = 9, RootContext = "BodySite", RootContextType = "BodySite" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Bundle", OutputOrder = 10, RootContext = "Bundle", RootContextType = "Bundle" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "CarePlan", OutputOrder = 11, RootContext = "CarePlan", RootContextType = "CarePlan" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "CareTeam", OutputOrder = 12, RootContext = "CareTeam", RootContextType = "CareTeam" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Claim", OutputOrder = 13, RootContext = "Claim", RootContextType = "Claim" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ClaimResponse", OutputOrder = 14, RootContext = "ClaimResponse", RootContextType = "ClaimResponse" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ClinicalImpression", OutputOrder = 15, RootContext = "ClinicalImpression", RootContextType = "ClinicalImpression" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "CodeSystem", OutputOrder = 16, RootContext = "CodeSystem", RootContextType = "CodeSystem" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Communication", OutputOrder = 17, RootContext = "Communication", RootContextType = "Communication" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "CommunicationRequest", OutputOrder = 18, RootContext = "CommunicationRequest", RootContextType = "CommunicationRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "CompartmentDefinition", OutputOrder = 19, RootContext = "CompartmentDefinition", RootContextType = "CompartmentDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Composition", OutputOrder = 20, RootContext = "Composition", RootContextType = "Composition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ConceptMap", OutputOrder = 21, RootContext = "ConceptMap", RootContextType = "ConceptMap" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Condition", OutputOrder = 22, RootContext = "Condition", RootContextType = "Condition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Consent", OutputOrder = 24, RootContext = "Consent", RootContextType = "Consent" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Contract", OutputOrder = 25, RootContext = "Contract", RootContextType = "Contract" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Coverage", OutputOrder = 26, RootContext = "Coverage", RootContextType = "Coverage" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DataElement", OutputOrder = 27, RootContext = "DataElement", RootContextType = "DataElement" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DetectedIssue", OutputOrder = 29, RootContext = "DetectedIssue", RootContextType = "DetectedIssue" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Device", OutputOrder = 30, RootContext = "Device", RootContextType = "Device" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DeviceComponent", OutputOrder = 31, RootContext = "DeviceComponent", RootContextType = "DeviceComponent" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DeviceMetric", OutputOrder = 32, RootContext = "DeviceMetric", RootContextType = "DeviceMetric" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DeviceUseRequest", OutputOrder = 33, RootContext = "DeviceUseRequest", RootContextType = "DeviceUseRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DeviceUseStatement", OutputOrder = 34, RootContext = "DeviceUseStatement", RootContextType = "DeviceUseStatement" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DiagnosticRequest", OutputOrder = 35, RootContext = "DiagnosticRequest", RootContextType = "DiagnosticRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DiagnosticReport", OutputOrder = 36, RootContext = "DiagnosticReport", RootContextType = "DiagnosticReport" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DocumentManifest", OutputOrder = 37, RootContext = "DocumentManifest", RootContextType = "DocumentManifest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "DocumentReference", OutputOrder = 38, RootContext = "DocumentReference", RootContextType = "DocumentReference" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "EligibilityRequest", OutputOrder = 39, RootContext = "EligibilityRequest", RootContextType = "EligibilityRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "EligibilityResponse", OutputOrder = 40, RootContext = "EligibilityResponse", RootContextType = "EligibilityResponse" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Encounter", OutputOrder = 41, RootContext = "Encounter", RootContextType = "Encounter" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Endpoint", OutputOrder = 42, RootContext = "Endpoint", RootContextType = "Endpoint" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "EnrollmentRequest", OutputOrder = 43, RootContext = "EnrollmentRequest", RootContextType = "EnrollmentRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "EnrollmentResponse", OutputOrder = 44, RootContext = "EnrollmentResponse", RootContextType = "EnrollmentResponse" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "EpisodeOfCare", OutputOrder = 45, RootContext = "EpisodeOfCare", RootContextType = "EpisodeOfCare" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ExpansionProfile", OutputOrder = 46, RootContext = "ExpansionProfile", RootContextType = "ExpansionProfile" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ExplanationOfBenefit", OutputOrder = 47, RootContext = "ExplanationOfBenefit", RootContextType = "ExplanationOfBenefit" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Extension", OutputOrder = 48, RootContext = "Extension", RootContextType = "Extension" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "FamilyMemberHistory", OutputOrder = 49, RootContext = "FamilyMemberHistory", RootContextType = "FamilyMemberHistory" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Flag", OutputOrder = 50, RootContext = "Flag", RootContextType = "Flag" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Goal", OutputOrder = 51, RootContext = "Goal", RootContextType = "Goal" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Group", OutputOrder = 52, RootContext = "Group", RootContextType = "Group" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "GuidanceResponse", OutputOrder = 53, RootContext = "GuidanceResponse", RootContextType = "GuidanceResponse" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "HealthcareService", OutputOrder = 54, RootContext = "HealthcareService", RootContextType = "HealthcareService" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ImagingManifest", OutputOrder = 55, RootContext = "ImagingManifest", RootContextType = "ImagingManifest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ImagingStudy", OutputOrder = 56, RootContext = "ImagingStudy", RootContextType = "ImagingStudy" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Immunization", OutputOrder = 57, RootContext = "Immunization", RootContextType = "Immunization" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ImmunizationRecommendation", OutputOrder = 58, RootContext = "ImmunizationRecommendation", RootContextType = "ImmunizationRecommendation" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ImplementationGuide", OutputOrder = 59, RootContext = "ImplementationGuide", RootContextType = "ImplementationGuide" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Library", OutputOrder = 60, RootContext = "Library", RootContextType = "Library" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Linkage", OutputOrder = 61, RootContext = "Linkage", RootContextType = "Linkage" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "List", OutputOrder = 62, RootContext = "List", RootContextType = "List" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Location", OutputOrder = 63, RootContext = "Location", RootContextType = "Location" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Measure", OutputOrder = 64, RootContext = "Measure", RootContextType = "Measure" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "MeasureReport", OutputOrder = 65, RootContext = "MeasureReport", RootContextType = "MeasureReport" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Media", OutputOrder = 66, RootContext = "Media", RootContextType = "Media" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Medication", OutputOrder = 67, RootContext = "Medication", RootContextType = "Medication" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "MedicationAdministration", OutputOrder = 68, RootContext = "MedicationAdministration", RootContextType = "MedicationAdministration" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "MedicationDispense", OutputOrder = 69, RootContext = "MedicationDispense", RootContextType = "MedicationDispense" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "MedicationStatement", OutputOrder = 71, RootContext = "MedicationStatement", RootContextType = "MedicationStatement" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "MessageHeader", OutputOrder = 72, RootContext = "MessageHeader", RootContextType = "MessageHeader" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "NamingSystem", OutputOrder = 73, RootContext = "NamingSystem", RootContextType = "NamingSystem" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "NutritionRequest", OutputOrder = 74, RootContext = "NutritionRequest", RootContextType = "NutritionRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Observation", OutputOrder = 75, RootContext = "Observation", RootContextType = "Observation" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "OperationDefinition", OutputOrder = 76, RootContext = "OperationDefinition", RootContextType = "OperationDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "OperationOutcome", OutputOrder = 77, RootContext = "OperationOutcome", RootContextType = "OperationOutcome" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Organization", OutputOrder = 78, RootContext = "Organization", RootContextType = "Organization" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Parameters", OutputOrder = 79, RootContext = "Parameters", RootContextType = "Parameters" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Patient", OutputOrder = 80, RootContext = "Patient", RootContextType = "Patient" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "PaymentNotice", OutputOrder = 81, RootContext = "PaymentNotice", RootContextType = "PaymentNotice" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "PaymentReconciliation", OutputOrder = 82, RootContext = "PaymentReconciliation", RootContextType = "PaymentReconciliation" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Person", OutputOrder = 83, RootContext = "Person", RootContextType = "Person" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "PlanDefinition", OutputOrder = 84, RootContext = "PlanDefinition", RootContextType = "PlanDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Practitioner", OutputOrder = 85, RootContext = "Practitioner", RootContextType = "Practitioner" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "PractitionerRole", OutputOrder = 86, RootContext = "PractitionerRole", RootContextType = "PractitionerRole" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Procedure", OutputOrder = 87, RootContext = "Procedure", RootContextType = "Procedure" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ProcedureRequest", OutputOrder = 88, RootContext = "ProcedureRequest", RootContextType = "ProcedureRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ProcessRequest", OutputOrder = 89, RootContext = "ProcessRequest", RootContextType = "ProcessRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ProcessResponse", OutputOrder = 90, RootContext = "ProcessResponse", RootContextType = "ProcessResponse" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Provenance", OutputOrder = 91, RootContext = "Provenance", RootContextType = "Provenance" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Questionnaire", OutputOrder = 92, RootContext = "Questionnaire", RootContextType = "Questionnaire" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "QuestionnaireResponse", OutputOrder = 93, RootContext = "QuestionnaireResponse", RootContextType = "QuestionnaireResponse" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ReferralRequest", OutputOrder = 94, RootContext = "ReferralRequest", RootContextType = "ReferralRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "RelatedPerson", OutputOrder = 95, RootContext = "RelatedPerson", RootContextType = "RelatedPerson" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "RiskAssessment", OutputOrder = 96, RootContext = "RiskAssessment", RootContextType = "RiskAssessment" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Schedule", OutputOrder = 97, RootContext = "Schedule", RootContextType = "Schedule" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "SearchParameter", OutputOrder = 98, RootContext = "SearchParameter", RootContextType = "SearchParameter" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Sequence", OutputOrder = 99, RootContext = "Sequence", RootContextType = "Sequence" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Slot", OutputOrder = 100, RootContext = "Slot", RootContextType = "Slot" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Specimen", OutputOrder = 101, RootContext = "Specimen", RootContextType = "Specimen" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "StructureDefinition", OutputOrder = 102, RootContext = "StructureDefinition", RootContextType = "StructureDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "StructureMap", OutputOrder = 103, RootContext = "StructureMap", RootContextType = "StructureMap" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Subscription", OutputOrder = 104, RootContext = "Subscription", RootContextType = "Subscription" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Substance", OutputOrder = 105, RootContext = "Substance", RootContextType = "Substance" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "SupplyDelivery", OutputOrder = 106, RootContext = "SupplyDelivery", RootContextType = "SupplyDelivery" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "SupplyRequest", OutputOrder = 107, RootContext = "SupplyRequest", RootContextType = "SupplyRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "Task", OutputOrder = 108, RootContext = "Task", RootContextType = "Task" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "TestScript", OutputOrder = 109, RootContext = "TestScript", RootContextType = "TestScript" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ValueSet", OutputOrder = 110, RootContext = "ValueSet", RootContextType = "ValueSet" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "VisionPrescription", OutputOrder = 111, RootContext = "VisionPrescription", RootContextType = "VisionPrescription" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "CapabilityStatement", OutputOrder = 112, RootContext = "CapabilityStatement", RootContextType = "CapabilityStatement" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "MedicationRequest", OutputOrder = 113, RootContext = "MedicationRequest", RootContextType = "MedicationRequest" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "MessageDefinition", OutputOrder = 114, RootContext = "MessageDefinition", RootContextType = "MessageDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "RequestGroup", OutputOrder = 115, RootContext = "RequestGroup", RootContextType = "RequestGroup" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ResearchStudy", OutputOrder = 116, RootContext = "ResearchStudy", RootContextType = "ResearchStudy" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ResearchSubject", OutputOrder = 117, RootContext = "ResearchSubject", RootContextType = "ResearchSubject" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "ServiceDefinition", OutputOrder = 118, RootContext = "ServiceDefinition", RootContextType = "ServiceDefinition" },
                new TemplateType() { ImplementationGuideTypeId = 6, Name = "TestReport", OutputOrder = 119, RootContext = "TestReport", RootContextType = "TestReport" }
            );
        }

        protected override void Seed(Trifolia.DB.TrifoliaDatabase context)
        {
            context.ImplementationGuideTypes.AddOrUpdate(igt => igt.Id,
                new ImplementationGuideType()
                {
                    Id = 1,
                    Name = "CDA",
                    SchemaLocation = "cda.xsd",
                    SchemaPrefix = "cda",
                    SchemaURI = "urn:hl7-org:v3"
                },
                new ImplementationGuideType()
                {
                    Id = 2,
                    Name = "eMeasure",
                    SchemaLocation = "schemas\\EMeasure.xsd",
                    SchemaPrefix = "ems",
                    SchemaURI = "urn:hl7-org:v3"
                },
                new ImplementationGuideType()
                {
                    Id = 3,
                    Name = "HQMF R2",
                    SchemaLocation = "schemas\\EMeasure.xsd",
                    SchemaPrefix = "hqmf",
                    SchemaURI = "urn:hl7-org:v3"
                },
                new ImplementationGuideType()
                {
                    Id = 4,
                    Name = "FHIR DSTU1",
                    SchemaLocation = "fhir-all.xsd",
                    SchemaPrefix = "fhir",
                    SchemaURI = "http://hl7.org/fhir"
                },
                new ImplementationGuideType()
                {
                    Id = 5,
                    Name = "FHIR DSTU2",
                    SchemaLocation = "fhir-all.xsd",
                    SchemaPrefix = "fhir",
                    SchemaURI = "http://hl7.org/fhir"
                },
                new ImplementationGuideType()
                {
                    Id = 6,
                    Name = "FHIR DSTU3",
                    SchemaLocation = "fhir-all.xsd",
                    SchemaPrefix = "fhir",
                    SchemaURI = "http://hl7.org/fhir"
                }
            );

            this.SeedCDA(context);
            this.SeedEMeasure(context);
            this.SeedFHIRDSTU1(context);
            this.SeedFHIRDSTU2(context);
            this.SeedFHIRSTU3(context);

            context.AppSecurables.AddOrUpdate(apps => apps.Id, this.appSecurables);
            context.Roles.AddOrUpdate(r => r.Name, this.roles);

            List<RoleAppSecurable> roleAppSecurables = new List<RoleAppSecurable>();

            // Administrators: has all securales
            foreach (var appSecurable in appSecurables)
                this.AssignSecurableToRole(context, appSecurable.Name, "Administrators");

            // IG Admins
            this.AssignSecurableToRole(context, "TemplateList", "IG Admins");
            this.AssignSecurableToRole(context, "IGManagementList", "IG Admins");
            this.AssignSecurableToRole(context, "ImplementationGuideEdit", "IG Admins");
            this.AssignSecurableToRole(context, "ImplementationGuideEditBookmarks", "IG Admins");
            this.AssignSecurableToRole(context, "ImplementationGuideNotes", "IG Admins");
            this.AssignSecurableToRole(context, "ImplementationGuidePrimitives", "IG Admins");
            this.AssignSecurableToRole(context, "ImplementationGuideAuditTrail", "IG Admins");
            this.AssignSecurableToRole(context, "ValueSetList", "IG Admins");
            this.AssignSecurableToRole(context, "CodeSystemList", "IG Admins");
            this.AssignSecurableToRole(context, "ExportWordDocuments", "IG Admins");
            this.AssignSecurableToRole(context, "ExportVocabulary", "IG Admins");
            this.AssignSecurableToRole(context, "ExportSchematron", "IG Admins");
            this.AssignSecurableToRole(context, "PublishSettings", "IG Admins");
            this.AssignSecurableToRole(context, "ExportXML", "IG Admins");
            this.AssignSecurableToRole(context, "Import", "IG Admins");
            this.AssignSecurableToRole(context, "TemplateEdit", "IG Admins");
            this.AssignSecurableToRole(context, "ValueSetEdit", "IG Admins");
            this.AssignSecurableToRole(context, "CodeSystemEdit", "IG Admins");
            this.AssignSecurableToRole(context, "TemplateCopy", "IG Admins");
            this.AssignSecurableToRole(context, "TemplateDelete", "IG Admins");
            this.AssignSecurableToRole(context, "IGFileManagement", "IG Admins");
            this.AssignSecurableToRole(context, "IGFileView", "IG Admins");
            this.AssignSecurableToRole(context, "LandingPage", "IG Admins");
            this.AssignSecurableToRole(context, "TemplateMove", "IG Admins");
            this.AssignSecurableToRole(context, "WebIG", "IG Admins");

            // Template Authors
            this.AssignSecurableToRole(context, "TemplateList", "Template Authors");
            this.AssignSecurableToRole(context, "IGManagementList", "Template Authors");
            this.AssignSecurableToRole(context, "ImplementationGuideNotes", "Template Authors");
            this.AssignSecurableToRole(context, "ImplementationGuidePrimitives", "Template Authors");
            this.AssignSecurableToRole(context, "ImplementationGuideAuditTrail", "Template Authors");
            this.AssignSecurableToRole(context, "ValueSetList", "Template Authors");
            this.AssignSecurableToRole(context, "ValueSetEdit", "Template Authors");
            this.AssignSecurableToRole(context, "CodeSystemList", "Template Authors");
            this.AssignSecurableToRole(context, "CodeSystemEdit", "Template Authors");
            this.AssignSecurableToRole(context, "ExportWordDocuments", "Template Authors");
            this.AssignSecurableToRole(context, "ExportVocabulary", "Template Authors");
            this.AssignSecurableToRole(context, "ExportSchematron", "Template Authors");
            this.AssignSecurableToRole(context, "TemplateEdit", "Template Authors");
            this.AssignSecurableToRole(context, "TemplateCopy", "Template Authors");
            this.AssignSecurableToRole(context, "TemplateDelete", "Template Authors");
            this.AssignSecurableToRole(context, "ReportTemplateReview", "Template Authors");
            this.AssignSecurableToRole(context, "ReportTemplateCompliance", "Template Authors");
            this.AssignSecurableToRole(context, "PublishSettings", "Template Authors");
            this.AssignSecurableToRole(context, "ExportXML", "Template Authors");
            this.AssignSecurableToRole(context, "TemplateMove", "Template Authors");
            this.AssignSecurableToRole(context, "WebIG", "Template Authors");
            this.AssignSecurableToRole(context, "Import", "Template Authors");

            // Terminology Admins
            this.AssignSecurableToRole(context, "TerminologyOverride", "Terminology Admins");
            this.AssignSecurableToRole(context, "ValueSetEdit", "Terminology Admins");
            this.AssignSecurableToRole(context, "ValueSetList", "Terminology Admins");
            this.AssignSecurableToRole(context, "CodeSystemEdit", "Terminology Admins");
            this.AssignSecurableToRole(context, "CodeSystemList", "Terminology Admins");

            // Users
            this.AssignSecurableToRole(context, "TemplateList", "Users");
            this.AssignSecurableToRole(context, "IGManagementList", "Users");
            this.AssignSecurableToRole(context, "ValueSetList", "Users");
            this.AssignSecurableToRole(context, "CodeSystemList", "Users");
            this.AssignSecurableToRole(context, "ExportWordDocuments", "Users");
            this.AssignSecurableToRole(context, "ExportXML", "Users");

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
