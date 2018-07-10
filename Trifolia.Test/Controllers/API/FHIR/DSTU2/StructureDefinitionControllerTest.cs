extern alias fhir_dstu2;
using fhir_dstu2.Hl7.Fhir.Model;
using fhir_dstu2.Hl7.Fhir.Serialization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Results;
using Trifolia.DB;
using Trifolia.Test;
using Trifolia.Web.Controllers.API;
using Trifolia.Web.Controllers.API.FHIR.DSTU2;
using Trifolia.Export.FHIR.DSTU2;
using Trifolia.Shared;

namespace Trifolia.Test.Controllers.API.FHIR.DSTU2
{
    [TestClass]
    public class StructureDefinitionControllerTest
    {
        private static MockObjectRepository mockRepo1;
        private static MockObjectRepository mockRepo2;
        private static Trifolia.DB.Template profile1;
        private static StructureDefinition dafPatientStrucDef1;
        private static StructureDefinition dafPatientStrucDef2;

        [ClassInitialize]
        public static void SetupData(TestContext context)
        {
            // Setup mockRepo1
            StructureDefinitionControllerTest.mockRepo1 = new MockObjectRepository();
            StructureDefinitionControllerTest.mockRepo1.InitializeFHIR2Repository();
            StructureDefinitionControllerTest.mockRepo1.InitializeLCGAndLogin();

            var ig = StructureDefinitionControllerTest.mockRepo1.FindOrCreateImplementationGuide(Constants.IGTypeNames.FHIR_DSTU2, "Test IG");
            StructureDefinitionControllerTest.profile1 = StructureDefinitionControllerTest.mockRepo1.CreateTemplate("http://test.com/fhir/test", "Composition", "Test Composition", ig, "Composition", "Composition");
            StructureDefinitionControllerTest.profile1.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test.com/extension",
                Type = "String",
                Value = "Test string extension value"
            });
            StructureDefinitionControllerTest.profile1.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test2.com/extension",
                Type = "Date",
                Value = "2016234234234"     // Invalid date format, but should still be parsed by FHIR library
            });
            StructureDefinitionControllerTest.profile1.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test3.com/extension",
                Type = "Coding",
                Value = "xyz-123|display|urn:oid:2.16.113"
            });
            StructureDefinitionControllerTest.profile1.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test4.com/extension",
                Type = "CodeableConcept",
                Value = "xyz-123|display|urn:oid:2.16.113"
            });
            StructureDefinitionControllerTest.profile1.Extensions.Add(new Trifolia.DB.TemplateExtension()         // Extension has invalid value... It is skipped
            {
                Identifier = "http://test5.com/extension",
                Type = "CodeableConcept",
                Value = "test"
            });
            StructureDefinitionControllerTest.profile1.Extensions.Add(new Trifolia.DB.TemplateExtension()         // Extension has invalid value... It is skipped
            {
                Identifier = "http://test6.com/extension",
                Type = "Boolean",
                Value = "test"
            });
            StructureDefinitionControllerTest.profile1.Extensions.Add(new Trifolia.DB.TemplateExtension()         // Extension has invalid value... It is skipped
            {
                Identifier = "http://test7.com/extension",
                Type = "Integer",
                Value = "test"
            });

            // Setup mockRepo2 for DSTU2_TestGetTemplates()
            StructureDefinitionControllerTest.mockRepo2 = new MockObjectRepository();
            StructureDefinitionControllerTest.mockRepo2.InitializeFHIR2Repository();
            StructureDefinitionControllerTest.mockRepo2.InitializeLCG();

            var ig2 = StructureDefinitionControllerTest.mockRepo2.FindOrCreateImplementationGuide(Constants.IGTypeNames.FHIR_DSTU2, "Test IG");
            StructureDefinitionControllerTest.mockRepo2.CreateTemplate("http://test.com/fhir/test", "Composition", "Test Composition", ig2, "Composition", "Composition");

            // Setup strucDef's
            var dafPatientJson = Helper.GetSampleContents("Trifolia.Test.DocSamples.daf-patient_struc_def.json");
            StructureDefinitionControllerTest.dafPatientStrucDef1 = (StructureDefinition)FhirParser.ParseResourceFromJson(dafPatientJson);
            StructureDefinitionControllerTest.dafPatientStrucDef1.Url = "http://hl7.org/fhir/StructureDefinition/daf-patient1";
            StructureDefinitionControllerTest.dafPatientStrucDef2 = (StructureDefinition)FhirParser.ParseResourceFromJson(dafPatientJson);
            StructureDefinitionControllerTest.dafPatientStrucDef2.Id = "daf-patient2";
            StructureDefinitionControllerTest.dafPatientStrucDef2.Url = "http://hl7.org/fhir/StructureDefinition/daf-patient2";
        }

        [TestMethod]
        [TestCategory("FHIR")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void DSTU2_TestConvertExtension()
        {
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(StructureDefinitionControllerTest.mockRepo1, "http", "test.com");
            SimpleSchema schema = SimpleSchema.CreateSimpleSchema(
                Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(
                    new ImplementationGuideType()
                    {
                        Name = Constants.IGTypeNames.FHIR_DSTU2,
                        SchemaLocation = "fhir-all.xsd"
                    }));
            
            StructureDefinition strucDef = exporter.Convert(StructureDefinitionControllerTest.profile1, schema);

            Assert.IsNotNull(strucDef);
            Assert.IsNotNull(strucDef.Extension);
            Assert.AreEqual(strucDef.Extension.Count, 4);

            // Extension 1
            Assert.AreEqual(strucDef.Extension[0].Url, "http://test.com/extension");
            Assert.IsInstanceOfType(strucDef.Extension[0].Value, typeof(FhirString));
            Assert.AreEqual(((FhirString)strucDef.Extension[0].Value).Value, "Test string extension value");

            // Extension 2
            Assert.AreEqual(strucDef.Extension[1].Url, "http://test2.com/extension");
            Assert.IsInstanceOfType(strucDef.Extension[1].Value, typeof(Date));
            Assert.AreEqual(((Date)strucDef.Extension[1].Value).Value, "2016234234234");

            // Extension 3
            Assert.AreEqual(strucDef.Extension[2].Url, "http://test3.com/extension");
            Assert.IsInstanceOfType(strucDef.Extension[2].Value, typeof(Coding));
            Assert.AreEqual(((Coding)strucDef.Extension[2].Value).Code, "xyz-123");
            Assert.AreEqual(((Coding)strucDef.Extension[2].Value).Display, "display");
            Assert.AreEqual(((Coding)strucDef.Extension[2].Value).System, "urn:oid:2.16.113");

            // Extension 4
            Assert.AreEqual(strucDef.Extension[3].Url, "http://test4.com/extension");
            Assert.IsInstanceOfType(strucDef.Extension[3].Value, typeof(CodeableConcept));
            Assert.AreEqual(((CodeableConcept)strucDef.Extension[3].Value).Coding.Count, 1);
            Assert.AreEqual(((CodeableConcept)strucDef.Extension[3].Value).Coding[0].Code, "xyz-123");
            Assert.AreEqual(((CodeableConcept)strucDef.Extension[3].Value).Coding[0].Display, "display");
            Assert.AreEqual(((CodeableConcept)strucDef.Extension[3].Value).Coding[0].System, "urn:oid:2.16.113");
        }

        [TestMethod]
        [TestCategory("FHIR")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void DSTU2_TestSuccessfulCreate()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://localhost:8080/api/FHIR2/StructureDefinition");

            HttpRequest contextRequest = new HttpRequest(null, "http://localhost:8080/api/FHIR2/StructureDefinition", null);
            HttpResponse contextResponse = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(contextRequest, contextResponse);
            
            FHIR2StructureDefinitionController controller = new FHIR2StructureDefinitionController(StructureDefinitionControllerTest.mockRepo1, request);
            var response = controller.CreateStructureDefinition(StructureDefinitionControllerTest.dafPatientStrucDef1);
            var result = AssertHelper.IsType<TrifoliaApiController.CustomHeadersWithContentResult<StructureDefinition>>(response);

            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(result.CustomHeaders["Location"]);
            Assert.AreEqual(result.CustomHeaders["Location"], "http://localhost:8080/api/FHIR2/StructureDefinition/2");     // "2" because a profile already exists in the DB from SetupData()

            Assert.IsNotNull(result.Content);
            Assert.AreEqual(StructureDefinitionControllerTest.dafPatientStrucDef1.Name, result.Content.Name);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void DSTU2_TestFailedCreate_Id()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://localhost:8080/api/FHIR2/StructureDefinition");

            HttpRequest contextRequest = new HttpRequest(null, "http://localhost:8080/api/FHIR2/StructureDefinition", null);
            HttpResponse contextResponse = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(contextRequest, contextResponse);

            FHIR2StructureDefinitionController controller = new FHIR2StructureDefinitionController(StructureDefinitionControllerTest.mockRepo1, request);
            var response = controller.CreateStructureDefinition(StructureDefinitionControllerTest.dafPatientStrucDef2);
            var result = AssertHelper.IsType<NegotiatedContentResult<OperationOutcome>>(response);

            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            Assert.IsNotNull(result.Content);
            Assert.AreEqual(1, result.Content.Issue.Count);
        }

        [TestMethod]
        [TestCategory("FHIR")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void DSTU2_TestGetTemplates()
        {
            // Login for the user. This is important to do here (outside of setup) so that it associates the thread
            // running the GetTemplates() method with authentication, since login is session/thread-specific
            StructureDefinitionControllerTest.mockRepo2.Login();

            var ig = StructureDefinitionControllerTest.mockRepo2.FindOrCreateImplementationGuide(Constants.IGTypeNames.FHIR_DSTU2, "Test IG");
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost:8080/api/FHIR2/StructureDefinition")
            };
            HttpRequest contextRequest = new HttpRequest(null, "http://localhost:8080/api/FHIR2/StructureDefinition", null);
            HttpResponse contextResponse = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(contextRequest, contextResponse);

            FHIR2StructureDefinitionController controller = new FHIR2StructureDefinitionController(StructureDefinitionControllerTest.mockRepo2, request);

            // Do the work: GetTemplates()
            // This responds with an HTTP response, which needs to be converted into the actual Bundle result
            var response = controller.GetTemplates();

            var result = AssertHelper.IsType<NegotiatedContentResult<Bundle>>(response);

            Assert.IsNotNull(result.Content);
            Assert.AreEqual(1, result.Content.Entry.Count);

            var firstEntry = result.Content.Entry.First();
            Assert.AreEqual("http://localhost:8080/api/FHIR2/StructureDefinition/1", firstEntry.FullUrl);
            Assert.IsNotNull(firstEntry.Resource);

            var firstStrucDef = AssertHelper.IsType<StructureDefinition>(firstEntry.Resource);
            Assert.IsNull(firstStrucDef.Id);
        }
    }
}
