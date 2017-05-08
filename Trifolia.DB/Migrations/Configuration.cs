namespace Trifolia.DB.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal sealed class Configuration : DbMigrationsConfiguration<Trifolia.DB.TrifoliaDatabase>
    {
        private AppSecurable[] appSecurables;
        private Role[] roles;

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

            this.appSecurables = new AppSecurable[]
            {
                new AppSecurable() { Name = "TemplateList", DisplayName = "List Templates", Description = "NULL" },
                new AppSecurable() { Name = "IGManagementList", DisplayName = "List Implementation Guides", Description = "NULL" },
                new AppSecurable() { Name = "LandingPage", DisplayName = "Landing Page", Description = "NULL" },
                new AppSecurable() { Name = "ImplementationGuideEdit", DisplayName = "Edit Implementation Guide", Description = "NULL" },
                new AppSecurable() { Name = "ImplementationGuideEditBookmarks", DisplayName = "Edit Implementation Guide Bookmarks", Description = "" },
                new AppSecurable() { Name = "ImplementationGuideNotes", DisplayName = "View Implementation Guide Notes", Description = "NULL" },
                new AppSecurable() { Name = "ImplementationGuidePrimitives", DisplayName = "View Implementation Guide Primitives", Description = "NULL" },
                new AppSecurable() { Name = "ImplementationGuideAuditTrail", DisplayName = "View Implementation Guide Audit Trail", Description = "NULL" },
                new AppSecurable() { Name = "ValueSetList", DisplayName = "List Value Sets", Description = "NULL" },
                new AppSecurable() { Name = "ValueSetEdit", DisplayName = "Edit Value Sets", Description = "NULL" },
                new AppSecurable() { Name = "CodeSystemList", DisplayName = "List Code Systems", Description = "NULL" },
                new AppSecurable() { Name = "CodeSystemEdit", DisplayName = "Edit Code Systems", Description = "NULL" },
                new AppSecurable() { Name = "ExportWordDocuments", DisplayName = "Export MS Word Documents", Description = "NULL" },
                new AppSecurable() { Name = "ExportVocabulary", DisplayName = "Export Vocabulary", Description = "NULL" },
                new AppSecurable() { Name = "ExportSchematron", DisplayName = "Export Schematron", Description = "NULL" },
                new AppSecurable() { Name = "TemplateEdit", DisplayName = "Edit Templates", Description = "NULL" },
                new AppSecurable() { Name = "TemplateCopy", DisplayName = "Copy Templates", Description = "NULL" },
                new AppSecurable() { Name = "TemplateDelete", DisplayName = "Delete Templates", Description = "NULL" },
                new AppSecurable() { Name = "Admin", DisplayName = "Admin Functions", Description = "NULL" },
                new AppSecurable() { Name = "ReportTemplateReview", DisplayName = "View Template Review Report", Description = "NULL" },
                new AppSecurable() { Name = "ReportTemplateCompliance", DisplayName = "View Template Compliance Report", Description = "NULL" },
                new AppSecurable() { Name = "OrganizationList", DisplayName = "Organization List", Description = "NULL" },
                new AppSecurable() { Name = "OrganizationDetails", DisplayName = "Organization Details", Description = "NULL" },
                new AppSecurable() { Name = "PublishSettings", DisplayName = "Publish Settings", Description = "NULL" },
                new AppSecurable() { Name = "ExportXML", DisplayName = "Export Templates XML", Description = "Ability to export templates to an XML format" },
                new AppSecurable() { Name = "IGFileManagement", DisplayName = "IG File Management", Description = "Allows users to upload/update/remove files associated with an implementation guide." },
                new AppSecurable() { Name = "IGFileView", DisplayName = "IG File View", Description = "Allows users view files associated with implementation guides they have permission to access." },
                new AppSecurable() { Name = "ExportGreen", DisplayName = "Export green artifacts", Description = "NULL" },
                new AppSecurable() { Name = "TerminologyOverride", DisplayName = "Terminology Override", Description = "Ability to override locked value sets and code systems." },
                new AppSecurable() { Name = "GreenModel", DisplayName = "Green Modeling", Description = "Allows user to create, edit and delete green models for templates." },
                new AppSecurable() { Name = "TemplateMove", DisplayName = "Move Templates", Description = "Ability to move a template from one implementation guide to another" },
                new AppSecurable() { Name = "WebIG", DisplayName = "Web-based IG", Description = "Ability to view an implementation guide's web-based IG" },
                new AppSecurable() { Name = "Import", DisplayName = "Import", Description = "The ability to import implementation guides and templates into Trifolia" }
            };

            this.roles = new Role[] {
                new Role() { Name = "Administrators", IsDefault = false, IsAdmin = true },
                new Role() { Name = "Template Authors", IsDefault = true, IsAdmin = false },
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
            List<string> resourceTypes = new List<string>() { "Extension" };
            List<TemplateType> templateTypes = new List<TemplateType>();

            using (StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Trifolia.DB.Migrations.FHIR_STU3_Resources.txt")))
            {
                string content = sr.ReadToEnd();
                List<string> resourceTypeLines = (from c in content.Split('\n')
                                                  orderby c
                                                  select c.Replace("\r", "").Trim())
                                                  .ToList();
                resourceTypes.AddRange(resourceTypeLines);
            }

            for (int i = 0; i < resourceTypes.Count; i++)
            {
                var templateType = new TemplateType()
                {
                    ImplementationGuideTypeId = 6,
                    Name = resourceTypes[i],
                    OutputOrder = i + 1,
                    RootContext = resourceTypes[i],
                    RootContextType = resourceTypes[i]
                };

                context.TemplateTypes.AddOrUpdate(tt => new { tt.ImplementationGuideTypeId, tt.Name }, templateType);
            }

            var removeTemplateTypes = (from tt in context.TemplateTypes
                                       where tt.ImplementationGuideTypeId == 6 && !resourceTypes.Contains(tt.Name)
                                       select tt).ToList();

            foreach (var removeTemplateType in removeTemplateTypes)
            {
                if (removeTemplateType.Templates.Count > 0)
                {
                    Console.WriteLine("Can't remove " + removeTemplateType.Name + " from FHIR STU3 because it is associated with templates");
                    continue;
                }

                context.TemplateTypes.Remove(removeTemplateType);
            }
        }

        protected override void Seed(Trifolia.DB.TrifoliaDatabase context)
        {
            context.PublishStatuses.AddOrUpdate(ps => ps.Id,
                new PublishStatus()
                {
                    Id = 1,
                    Status = "Draft"
                },
                new PublishStatus()
                {
                    Id = 2,
                    Status = "Ballot"
                },
                new PublishStatus()
                {
                    Id = 3,
                    Status = "Published"
                },
                new PublishStatus()
                {
                    Id = 4,
                    Status = "Deprecated"
                },
                new PublishStatus()
                {
                    Id = 5,
                    Status = "Retired"
                });

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
                    Name = "FHIR STU3",
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

            context.AppSecurables.AddOrUpdate(apps => apps.Name, this.appSecurables);
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
