extern alias fhir_stu3;

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Linq;

using Trifolia.DB;
using Trifolia.Config;

using FhirConformance = fhir_stu3.Hl7.Fhir.Model.Conformance;

namespace Trifolia.Web.Controllers.API.FHIR.STU3
{
    [STU3Config]
    [RoutePrefix("api/FHIR3")]
    public class FHIR3ConformanceController : ApiController
    {
        private IObjectRepository tdb;
        private FhirConformance.ResourceInteractionComponent searchInteraction;
        private FhirConformance.ResourceInteractionComponent readInteraction;
        private FhirConformance.ResourceInteractionComponent updateInteraction;
        private FhirConformance.ResourceInteractionComponent createInteraction;

        #region Constructors

        public FHIR3ConformanceController(IObjectRepository tdb)
        {
            this.tdb = tdb;

            this.searchInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.SearchType
            };
            this.readInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.Read
            };
            this.createInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.Create
            };
            this.updateInteraction = new FhirConformance.ResourceInteractionComponent()
            {
                Code = FhirConformance.TypeRestfulInteraction.Update
            };
        }

        public FHIR3ConformanceController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        private FhirConformance.ResourceComponent GetImplementationGuideResourceComponent()
        {
            var interactions = new List<FhirConformance.ResourceInteractionComponent>();
            interactions.Add(this.searchInteraction);
            interactions.Add(this.readInteraction);
            interactions.Add(this.updateInteraction);
            interactions.Add(this.createInteraction);

            var restImplementationGuide = new FhirConformance.ResourceComponent()
            {
                Type = fhir_stu3.Hl7.Fhir.Model.ResourceType.ImplementationGuide,
                Interaction = interactions
            };

            return restImplementationGuide;
        }

        private FhirConformance.ResourceComponent GetStructureDefinitionResourceComponent()
        {
            var interactions = new List<FhirConformance.ResourceInteractionComponent>();
            interactions.Add(this.searchInteraction);
            interactions.Add(this.readInteraction);

            var restStructureDefinition = new FhirConformance.ResourceComponent()
            {
                Type = fhir_stu3.Hl7.Fhir.Model.ResourceType.StructureDefinition,
                Interaction = interactions
            };

            return restStructureDefinition;
        }

        private FhirConformance.ResourceComponent GetValueSetResourceComponent()
        {
            var interactions = new List<FhirConformance.ResourceInteractionComponent>();
            interactions.Add(this.searchInteraction);
            interactions.Add(this.readInteraction);
            interactions.Add(this.updateInteraction);
            interactions.Add(this.createInteraction);

            var restValueSet = new FhirConformance.ResourceComponent()
            {
                Type = fhir_stu3.Hl7.Fhir.Model.ResourceType.ValueSet,
                Interaction = interactions
            };

            return restValueSet;
        }

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
                Url = string.Format("{0}://{1}/api/FHIR3/", this.Request.RequestUri.Scheme, this.Request.RequestUri.Authority)
            };

            var restComponent = new FhirConformance.RestComponent();
            conformance.Rest.Add(restComponent);

            restComponent.Resource.Add(GetImplementationGuideResourceComponent());
            restComponent.Resource.Add(GetStructureDefinitionResourceComponent());
            restComponent.Resource.Add(GetValueSetResourceComponent());

            return Shared.GetResponseMessage(this.Request, format, conformance);
        }
    }
}