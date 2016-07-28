extern alias fhir_dstu2;

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

using Trifolia.DB;

using FhirConformance = fhir_dstu2.Hl7.Fhir.Model.Conformance;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    [DSTU2Config]
    [RoutePrefix("api/FHIR2")]
    public class FHIR2ConformanceController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public FHIR2ConformanceController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public FHIR2ConformanceController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        /// <summary>
        /// Gets conformance information about the FHIR DSTU2 server.
        /// </summary>
        /// <param name="format">The format that the response should be returned in</param>
        /// <returns>Resource&lt;Conformance&gt;</returns>
        [HttpGet, Route("Conformance")]
        public HttpResponseMessage GetConformance([FromUri(Name = "_format")] string format = null)
        {
            FhirConformance conformance = new FhirConformance()
            {
                Url = string.Format("{0}://{1}/api/FHIR2/", this.Request.RequestUri.Scheme, this.Request.RequestUri.Authority)
            };

            var restComponent = new FhirConformance.ConformanceRestComponent();
            conformance.Rest.Add(restComponent);

            var searchInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.SearchType
            };
            var readInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.Read
            };
            var createInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.Create
            };
            var updateInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.Update
            };

            var restImplementationGuide = new FhirConformance.ConformanceRestResourceComponent()
            {
                Type = "ImplementationGuide",
                Interaction = new List<FhirConformance.ResourceInteractionComponent>(new FhirConformance.ResourceInteractionComponent[] { searchInteraction, readInteraction, updateInteraction, createInteraction })
            };
            restComponent.Resource.Add(restImplementationGuide);

            var restStructureDefinition = new FhirConformance.ConformanceRestResourceComponent()
            {
                Type = "StructureDefinition",
                Interaction = new List<FhirConformance.ResourceInteractionComponent>(new FhirConformance.ResourceInteractionComponent[] { searchInteraction, readInteraction })
            };
            restComponent.Resource.Add(restStructureDefinition);

            var restValueSet = new FhirConformance.ConformanceRestResourceComponent()
            {
                Type = "ValueSet",
                Interaction = new List<FhirConformance.ResourceInteractionComponent>(new FhirConformance.ResourceInteractionComponent[] { searchInteraction, readInteraction, updateInteraction, createInteraction })
            };
            restComponent.Resource.Add(restValueSet);

            return Shared.GetResponseMessage(this.Request, format, conformance);
        }
    }
}