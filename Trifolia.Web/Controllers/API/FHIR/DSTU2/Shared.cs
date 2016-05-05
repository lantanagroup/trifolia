extern alias fhir_dstu2;

using fhir_dstu2.Hl7.Fhir.Model;
using fhir_dstu2.Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using Trifolia.DB;
using Trifolia.Web.Formatters.FHIR.DSTU2;
using Trifolia.Shared;
using System.IO;

using ImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    public class DSTU2ConfigAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Clear();
            controllerSettings.Formatters.Add(new GeneralFHIRMediaTypeFormatter());
        }
    }

    public class Shared
    {
        public const string DEFAULT_USER_NAME = "admin";
        public const string DEFAULT_ORG_NAME = "LCG";

        public static FHIRUrlStructure GetStructureFromReferenceUrl(string reference)
        {
            Regex regex = new Regex("((http|https):\\/\\/([A-Za-z0-9\\\\\\/\\.\\:\\%\\$])*)?(Account|AllergyIntolerance|Appointment|AppointmentResponse|AuditEvent|Basic|Binary|BodySite|Bundle|CarePlan|Claim|ClaimResponse|ClinicalImpression|Communication|CommunicationRequest|Composition|ConceptMap|Condition|Conformance|Contract|Coverage|DataElement|DetectedIssue|Device|DeviceComponent|DeviceMetric|DeviceUseRequest|DeviceUseStatement|DiagnosticOrder|DiagnosticReport|DocumentManifest|DocumentReference|EligibilityRequest|EligibilityResponse|Encounter|EnrollmentRequest|EnrollmentResponse|EpisodeOfCare|ExplanationOfBenefit|FamilyMemberHistory|Flag|Goal|Group|HealthcareService|ImagingObjectSelection|ImagingStudy|Immunization|ImmunizationRecommendation|ImplementationGuide|List|Location|Media|Medication|MedicationAdministration|MedicationDispense|MedicationOrder|MedicationStatement|MessageHeader|NamingSystem|NutritionOrder|Observation|OperationDefinition|OperationOutcome|Order|OrderResponse|Organization|Patient|PaymentNotice|PaymentReconciliation|Person|Practitioner|Procedure|ProcedureRequest|ProcessRequest|ProcessResponse|Provenance|Questionnaire|QuestionnaireResponse|ReferralRequest|RelatedPerson|RiskAssessment|Schedule|SearchParameter|Slot|Specimen|StructureDefinition|Subscription|Substance|SupplyDelivery|SupplyRequest|TestScript|ValueSet|VisionPrescription)\\/([A-Za-z0-9\\-\\.]{1,64})(\\/_history\\/([A-Za-z0-9\\-\\.]{1,64}))?");

            if (!regex.IsMatch(reference))
                throw new Exception("Unexpected format for resource reference found: " + reference);

            Match match = regex.Match(reference);
            FHIRUrlStructure urlStruct = new FHIRUrlStructure()
            {
                FullUrl = reference,
                Base = match.Groups[1].Value,
                Identifier = match.Groups[5].Value,
                ResourceType = match.Groups[4].Value,
                Version = match.Groups[7].Value
            };

            return urlStruct;
        }

        public static Template FindTemplate(HttpRequestMessage request, IWebClient webClient, IObjectRepository tdb, ResourceReference reference)
        {
            if (string.IsNullOrEmpty(reference.Reference))
                throw new Exception("Expected ResourceReference to have a value");

            var urlStruct = GetStructureFromReferenceUrl(reference.Reference);

            if (urlStruct.ResourceType != "StructureDefinition")
                throw new Exception("Cannot find a template for a non-StructureDefinition reference: " + urlStruct.FullUrl);

            // Full/absolute or relative reference to resource on this server
            if (urlStruct.IsThisServer(request))
            {
                int templateId = -1;

                if (!Int32.TryParse(urlStruct.Identifier, out templateId))
                    throw new Exception("Unexpected identifier for StructureDefinition: " + urlStruct.FullUrl);

                Template template = tdb.Templates.SingleOrDefault(y => y.Id == templateId);

                if (template == null)
                    throw new Exception("Could not find template for id: " + templateId);

                return template;
            }
            else if (!string.IsNullOrEmpty(urlStruct.Base))
            {
                var resourceStream = webClient.OpenRead(urlStruct.FullUrl);

                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    var resourceContent = reader.ReadToEnd();
                    StructureDefinition resource = (StructureDefinition) FhirParser.ParseResourceFromJson(resourceContent);

                    FHIR2StructureDefinitionController sdController = new FHIR2StructureDefinitionController(tdb);
                    var template = sdController.CreateTemplate(resource);
                    return template;
                }
            }
            else
            {
                throw new Exception("Cannot find resource reference " + reference.Reference);
            }
        }

        public static string FormatIdentifier(string identifier)
        {
            if (identifier.StartsWith("http") || identifier.StartsWith("urn"))
                return identifier;

            return string.Format("urn:oid:{0}", identifier);
        }

        public static HttpResponseMessage GetResponseMessage(HttpRequestMessage request, string format, object ret, int statusCode = 200, Dictionary<string, string> headers = null)
        {
            MediaTypeFormatter formatter = new GeneralFHIRMediaTypeFormatter();
            HttpResponseMessage message = new HttpResponseMessage();
            message.Content = new ObjectContent(ret.GetType(), ret, formatter, format);
            message.StatusCode = (HttpStatusCode)statusCode;

            if (headers != null)
            {
                foreach (var headerKey in headers.Keys)
                {
                    message.Headers.Add(headerKey, headers[headerKey]);
                }
            }

            return message;
        }
    }

    public static class TemplateConstraintExtensions
    {
        public static string GetElementPath(this TemplateConstraint constraint, string resourceType)
        {
            string elementPath = "";

            // Element path
            var current = constraint;
            while (current != null)
            {
                if (!string.IsNullOrEmpty(elementPath))
                    elementPath = "." + elementPath;

                elementPath = current.Context.Replace("@", "") + elementPath;
                current = current.ParentConstraint;
            }

            return (string.IsNullOrEmpty(resourceType) ? "Resource" : resourceType) + "." + elementPath;
        }
    }

    public static class TemplateExtensions
    {
        public static string GetFullUrl(this Template template, HttpRequestMessage request)
        {
            var url = string.Format("{0}://{1}/api/FHIR2/StructureDefinition/{2}",
                request.RequestUri.Scheme,
                request.RequestUri.Authority,
                template.Id);
            return url;
        }

        public static string GetId(this Template template)
        {
            if (!string.IsNullOrEmpty(template.Oid) && (template.Oid.StartsWith("http://") || template.Oid.StartsWith("https://")))
                return template.Oid.Substring(template.Oid.LastIndexOf('/') + 1);

            return template.Id.ToString();
        }
    }

    public static class ImplementationGuideExtensions
    {
        public static string GetFullUrl(this ImplementationGuide implementationGuide, HttpRequestMessage request)
        {
            var url = string.Format("{0}://{1}/api/FHIR2/ImplementationGuide/{2}",
                request.RequestUri.Scheme,
                request.RequestUri.Authority,
                implementationGuide.Id);
            return url;
        }
    }

    /// <summary>
    /// Enumeration that indicates what type of summary to respond with
    /// </summary>
    public enum SummaryType
    {
        /// <summary>
        /// true
        /// </summary>
        [EnumMember(Value = "true")]
        True,

        /// <summary>
        /// false
        /// </summary>
        [EnumMember(Value = "false")]
        False,

        /// <summary>
        /// text
        /// </summary>
        [EnumMember(Value = "text")]
        Text,

        /// <summary>
        /// data
        /// </summary>
        [EnumMember(Value = "data")]
        Data
    }

    public class FHIRUrlStructure
    {
        public string FullUrl { get; set; }
        public string Base { get; set; }
        public string ResourceType { get; set; }
        public string Identifier { get; set; }
        public string Version { get; set; }

        public bool IsThisServer(HttpRequestMessage request)
        {
            if (string.IsNullOrEmpty(this.Base))
                return true;

            string thisHost = string.Format("{0}://{1}",
                    request.RequestUri.Scheme,
                    request.RequestUri.Authority);

            return this.Base.StartsWith(thisHost);
        }
    }
}