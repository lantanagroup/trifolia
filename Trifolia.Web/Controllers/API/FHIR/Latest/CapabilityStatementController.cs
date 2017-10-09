extern alias fhir_latest;

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Linq;

using Trifolia.DB;
using Trifolia.Config;

using CapabilityStatement = fhir_latest.Hl7.Fhir.Model.CapabilityStatement;

namespace Trifolia.Web.Controllers.API.FHIR.Latest
{
    [LatestConfigAttribute]
    [RoutePrefix("api/FHIRLatest")]
    public class FHIRLatestConformanceController : ApiController
    {
        private IObjectRepository tdb;
        private CapabilityStatement.ResourceInteractionComponent searchInteraction;
        private CapabilityStatement.ResourceInteractionComponent readInteraction;
        private CapabilityStatement.ResourceInteractionComponent updateInteraction;
        private CapabilityStatement.ResourceInteractionComponent createInteraction;

        #region Construct/Dispose

        public FHIRLatestConformanceController(IObjectRepository tdb)
        {
            this.tdb = tdb;

            this.searchInteraction = new CapabilityStatement.ResourceInteractionComponent()
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType
            };
            this.readInteraction = new CapabilityStatement.ResourceInteractionComponent()
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Read
            };
            this.createInteraction = new CapabilityStatement.ResourceInteractionComponent()
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Create
            };
            this.updateInteraction = new CapabilityStatement.ResourceInteractionComponent()
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Update
            };
        }

        public FHIRLatestConformanceController()
            : this(DBContext.Create())
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        private CapabilityStatement.ResourceComponent GetImplementationGuideResourceComponent()
        {
            var interactions = new List<CapabilityStatement.ResourceInteractionComponent>();
            interactions.Add(this.searchInteraction);
            interactions.Add(this.readInteraction);
            interactions.Add(this.updateInteraction);
            interactions.Add(this.createInteraction);

            var restImplementationGuide = new CapabilityStatement.ResourceComponent()
            {
                Type = fhir_latest.Hl7.Fhir.Model.ResourceType.ImplementationGuide,
                Interaction = interactions
            };

            return restImplementationGuide;
        }

        private CapabilityStatement.ResourceComponent GetStructureDefinitionResourceComponent()
        {
            var interactions = new List<CapabilityStatement.ResourceInteractionComponent>();
            interactions.Add(this.searchInteraction);
            interactions.Add(this.readInteraction);
            interactions.Add(this.createInteraction);
            interactions.Add(this.updateInteraction);

            var restStructureDefinition = new CapabilityStatement.ResourceComponent()
            {
                Type = fhir_latest.Hl7.Fhir.Model.ResourceType.StructureDefinition,
                Interaction = interactions
            };

            return restStructureDefinition;
        }

        private CapabilityStatement.ResourceComponent GetValueSetResourceComponent()
        {
            var interactions = new List<CapabilityStatement.ResourceInteractionComponent>();
            interactions.Add(this.searchInteraction);
            interactions.Add(this.readInteraction);
            interactions.Add(this.updateInteraction);
            interactions.Add(this.createInteraction);

            var restValueSet = new CapabilityStatement.ResourceComponent()
            {
                Type = fhir_latest.Hl7.Fhir.Model.ResourceType.ValueSet,
                Interaction = interactions
            };

            return restValueSet;
        }

        /// <summary>
        /// Gets conformance information about the FHIR DSTU2 server.
        /// </summary>
        /// <param name="format">The format that the response should be returned in</param>
        /// <returns>Resource&lt;Conformance&gt;</returns>
        [HttpGet]
        [Route("CapabilityStatement")]
        [Route("metadata")]
        public HttpResponseMessage GetConformance([FromUri(Name = "_format")] string format = null)
        {
            CapabilityStatement conformance = new CapabilityStatement()
            {
                Url = string.Format("{0}://{1}/api/FHIR3/", this.Request.RequestUri.Scheme, this.Request.RequestUri.Authority)
            };

            var restComponent = new CapabilityStatement.RestComponent();
            conformance.Rest.Add(restComponent);

            restComponent.Resource.Add(GetImplementationGuideResourceComponent());
            restComponent.Resource.Add(GetStructureDefinitionResourceComponent());
            restComponent.Resource.Add(GetValueSetResourceComponent());

            return Shared.GetResponseMessage(this.Request, format, conformance);
        }
    }
}