extern alias fhir_dstu2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;

using Trifolia.Shared;
using Trifolia.DB;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using ValueSet = Trifolia.DB.ValueSet;
using Trifolia.Web.Formatters.FHIR.DSTU2;
using Trifolia.Generation.XML.FHIR.DSTU2;

using fhir_dstu2.Hl7.Fhir.Model;
using FhirImplementationGuide = fhir_dstu2.Hl7.Fhir.Model.ImplementationGuide;
using FhirValueSet = fhir_dstu2.Hl7.Fhir.Model.ValueSet;
using FhirConformance = fhir_dstu2.Hl7.Fhir.Model.Conformance;
using fhir_dstu2.Hl7.Fhir.Serialization;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    [DSTU2Config]
    public class ConformanceController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public ConformanceController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public ConformanceController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        /// <summary>
        /// Gets conformance information about the FHIR DSTU2 server.
        /// </summary>
        /// <param name="format">The format that the response should be returned in</param>
        /// <seealso cref="http://test.com"/>
        /// <returns>Resource&lt;Conformance&gt;</returns>
        [HttpGet, Route("api/FHIR2/Conformance")]
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