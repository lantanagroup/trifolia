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
        [TestMethod, TestCategory("FHIR2")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void TestConvertExtension()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();
            mockRepo.InitializeFHIR2Repository();
            mockRepo.InitializeLCG();

            var ig = mockRepo.FindOrAddImplementationGuide(MockObjectRepository.DEFAULT_FHIR_DSTU2_IG_TYPE_NAME, "Test IG");
            var template = mockRepo.GenerateTemplate("http://test.com/fhir/test", "Composition", "Test Composition", ig, "Composition", "Composition");
            template.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test.com/extension",
                Type = "String",
                Value = "Test string extension value"
            });
            template.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test2.com/extension",
                Type = "Date",
                Value = "2016234234234"     // Invalid date format, but should still be parsed by FHIR library
            });
            template.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test3.com/extension",
                Type = "Coding",
                Value = "xyz-123|display|urn:oid:2.16.113"
            });
            template.Extensions.Add(new Trifolia.DB.TemplateExtension()
            {
                Identifier = "http://test4.com/extension",
                Type = "CodeableConcept",
                Value = "xyz-123|display|urn:oid:2.16.113"
            });
            template.Extensions.Add(new Trifolia.DB.TemplateExtension()         // Extension has invalid value... It is skipped
            {
                Identifier = "http://test5.com/extension",
                Type = "CodeableConcept",
                Value = "test"
            });
            template.Extensions.Add(new Trifolia.DB.TemplateExtension()         // Extension has invalid value... It is skipped
            {
                Identifier = "http://test6.com/extension",
                Type = "Boolean",
                Value = "test"
            });
            template.Extensions.Add(new Trifolia.DB.TemplateExtension()         // Extension has invalid value... It is skipped
            {
                Identifier = "http://test7.com/extension",
                Type = "Integer",
                Value = "test"
            });

            StructureDefinitionExporter exporter = new StructureDefinitionExporter(mockRepo, "http", "test.com");
            SimpleSchema schema = SimpleSchema.CreateSimpleSchema(
                Trifolia.Shared.Helper.GetIGSimplifiedSchemaLocation(
                    new ImplementationGuideType()
                    {
                        Name = MockObjectRepository.DEFAULT_FHIR_DSTU2_IG_TYPE_NAME,
                        SchemaLocation = "fhir-all.xsd"
                    }));
            
            StructureDefinition strucDef = exporter.Convert(template, schema);

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

        [TestMethod, TestCategory("FHIR2")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void TestSuccessfulCreate()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();
            mockRepo.InitializeFHIR2Repository();
            mockRepo.InitializeLCG();

            var dafPatientJson = Helper.GetSampleContents("Trifolia.Test.DocSamples.daf-patient_struc_def.json");
            StructureDefinition strucDef = (StructureDefinition)FhirParser.ParseResourceFromJson(dafPatientJson);

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://localhost:8080/api/FHIR2/StructureDefinition");

            HttpRequest contextRequest = new HttpRequest(null, "http://localhost:8080/api/FHIR2/StructureDefinition", null);
            HttpResponse contextResponse = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(contextRequest, contextResponse);
            
            FHIR2StructureDefinitionController controller = new FHIR2StructureDefinitionController(mockRepo, request);
            var response = controller.CreateStructureDefinition(strucDef);
            var result = AssertHelper.IsType<TrifoliaApiController.CustomHeadersWithContentResult<StructureDefinition>>(response);

            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(result.CustomHeaders["Location"]);
            Assert.AreEqual(result.CustomHeaders["Location"], "http://localhost:8080/api/FHIR2/StructureDefinition/1");

            Assert.IsNotNull(result.Content);
            Assert.AreEqual(strucDef.Name, result.Content.Name);
        }

        [TestMethod, TestCategory("FHIR2")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void TestFailedCreate_Id()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();
            mockRepo.InitializeFHIR2Repository();
            mockRepo.InitializeLCG();

            var dafPatientJson = Helper.GetSampleContents("Trifolia.Test.DocSamples.daf-patient_struc_def.json");
            StructureDefinition strucDef = (StructureDefinition)FhirParser.ParseResourceFromJson(dafPatientJson);
            strucDef.Id = "daf-patient";

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://localhost:8080/api/FHIR2/StructureDefinition");

            HttpRequest contextRequest = new HttpRequest(null, "http://localhost:8080/api/FHIR2/StructureDefinition", null);
            HttpResponse contextResponse = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(contextRequest, contextResponse);

            FHIR2StructureDefinitionController controller = new FHIR2StructureDefinitionController(mockRepo, request);
            var response = controller.CreateStructureDefinition(strucDef);
            var result = AssertHelper.IsType<NegotiatedContentResult<OperationOutcome>>(response);

            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            Assert.IsNotNull(result.Content);
            Assert.AreEqual(1, result.Content.Issue.Count);
        }

        [TestMethod, TestCategory("FHIR2")]
        [DeploymentItem("Schemas\\", "Schemas\\")]
        public void TestGetTemplates()
        {
            MockObjectRepository mockRepo = new MockObjectRepository();
            mockRepo.InitializeFHIR2Repository();
            mockRepo.InitializeLCGAndLogin();

            var ig = mockRepo.FindOrAddImplementationGuide("FHIR DSTU2", "Test IG");
            var template = mockRepo.GenerateTemplate("http://test.com/profile1", "Composition", "Test Composition", ig);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost:8080/api/FHIR2/StructureDefinition")
            };
            HttpRequest contextRequest = new HttpRequest(null, "http://localhost:8080/api/FHIR2/StructureDefinition", null);
            HttpResponse contextResponse = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(contextRequest, contextResponse);

            FHIR2StructureDefinitionController controller = new FHIR2StructureDefinitionController(mockRepo, request);
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
