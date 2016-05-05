extern alias fhir_dstu2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.Web.Controllers.API.FHIR.DSTU2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Trifolia.Test;
using fhir_dstu2.Hl7.Fhir.Model;
using fhir_dstu2.Hl7.Fhir.Serialization;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2.Test
{
    [TestClass()]
    public class SharedTest
    {
        [TestMethod()]
        public void FindTemplate_Local_AbsoluateUrl_Test()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://test:100/");
            MockObjectRepository repo = new MockObjectRepository();

            repo.InitializeFHIR2Repository();

            var ig = repo.FindOrAddImplementationGuide(repo.FindImplementationGuideType(MockObjectRepository.DEFAULT_FHIR_DSTU2_IG_TYPE_NAME), "Test IG");
            var template = repo.GenerateTemplate("urn:oid:1.2.3.4", repo.FindTemplateType(MockObjectRepository.DEFAULT_FHIR_DSTU2_IG_TYPE_NAME, "Composition"), "Test Template", ig);

            ResourceReference reference = new ResourceReference();
            reference.Reference = "http://test:100/api/FHIR2/StructureDefinition/" + template.Id;

            MockWebClient webClient = new MockWebClient();
            var foundTemplate = Shared.FindTemplate(request, webClient, repo, reference);

            Assert.IsNotNull(foundTemplate);
            Assert.AreEqual(template.Id, foundTemplate.Id);
            Assert.AreEqual(template.Name, foundTemplate.Name);
            Assert.AreEqual(0, webClient.RequestCount);
        }

        [TestMethod()]
        public void FindTemplate_Local_RelativeUrl_Test()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://test:100/");
            MockObjectRepository repo = new MockObjectRepository();

            repo.InitializeFHIR2Repository();

            var ig = repo.FindOrAddImplementationGuide(repo.FindImplementationGuideType(MockObjectRepository.DEFAULT_FHIR_DSTU2_IG_TYPE_NAME), "Test IG");
            var template = repo.GenerateTemplate("urn:oid:1.2.3.4", repo.FindTemplateType(MockObjectRepository.DEFAULT_FHIR_DSTU2_IG_TYPE_NAME, "Composition"), "Test Template", ig);

            ResourceReference reference = new ResourceReference();
            reference.Reference = "StructureDefinition/" + template.Id;

            MockWebClient webClient = new MockWebClient();
            var foundTemplate = Shared.FindTemplate(request, webClient, repo, reference);

            Assert.IsNotNull(foundTemplate);
            Assert.AreEqual(template.Id, foundTemplate.Id);
            Assert.AreEqual(template.Name, foundTemplate.Name);
            Assert.AreEqual(0, webClient.RequestCount);
        }

        [TestMethod()]
        public void FindTemplate_Remote_Test()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("http://test:100/");
            MockObjectRepository repo = new MockObjectRepository();

            repo.InitializeFHIR2Repository();
            repo.InitializeLCG();

            ResourceReference reference = new ResourceReference();
            reference.Reference = "http://test.com/FHIR/StructureDefinition/test";

            MockWebClient webClient = new MockWebClient();
            var dafPatientJson = Trifolia.Test.Helper.GetSampleContents("Trifolia.Test.DocSamples.daf-patient_struc_def.json");
            StructureDefinition strucDef = (StructureDefinition)FhirParser.ParseResourceFromJson(dafPatientJson);
            webClient.ExpectContent("http://test.com/FHIR/StructureDefinition/test", dafPatientJson);
            var foundTemplate = Shared.FindTemplate(request, webClient, repo, reference);

            Assert.IsNotNull(foundTemplate);
            Assert.AreEqual(1, foundTemplate.Id);
            Assert.AreEqual(strucDef.Name, foundTemplate.Name);
        }
    }
}
