extern alias fhir_dstu2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Shared;

using fhir_dstu2.Hl7.Fhir.Model;
using FhirImplementationGuide = fhir_dstu2.Hl7.Fhir.Model.ImplementationGuide;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using Trifolia.Plugins.FHIR.DSTU2;
using System.Web;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    [DSTU2Config]
    public class FHIR2ImplementationGuideController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public FHIR2ImplementationGuideController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public FHIR2ImplementationGuideController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        private string BaseProfilePath
        {
            get
            {
                return HttpContext.Current.Server.MapPath("~/App_Data/FHIR/DSTU2/");
            }
        }

        #endregion

        /// <summary>
        /// Get the specified implementation guide in FHIR DSTU2 format
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <param name="format">Optional. The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.ImplementationGuide</returns>
        [HttpGet]
        [Route("api/FHIR2/ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public HttpResponseMessage GetImplementationGuide(
            int implementationGuideId,
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] fhir_dstu2.Hl7.Fhir.Rest.SummaryType? summary = null)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, implementationGuide.ImplementationGuideType);
            ImplementationGuideExporter exporter = new ImplementationGuideExporter(this.tdb, schema, this.BaseProfilePath, this.Request.RequestUri.Scheme, this.Request.RequestUri.Authority);
            FhirImplementationGuide response = exporter.Convert(implementationGuide, summary);
            return Shared.GetResponseMessage(this.Request, format, response);
        }

        /// <summary>
        /// Gets implementation guides in FHIR DSTU2 format. Can specify search information, such as the name of the implementation guide and the id of the implementation guide.
        /// </summary>
        /// <param name="format">Optional. The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <param name="include">Indicate what additional information should be included with the implementation guide (such as "ImplementationGuide:resource")</param>
        /// <param name="implementationGuideId">Specify the id of the implementation guide to search for.</param>
        /// <param name="name">Specifies the name of the implementation guide to search for. Implementation guides whose name *contains* this value will be returned.</param>
        /// <returns>Hl7.Fhir.Model.Bundle</returns>
        [HttpGet]
        [Route("api/FHIR2/ImplementationGuide")]
        [Route("api/FHIR2/ImplementationGuide/_search")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public HttpResponseMessage GetImplementationGuides(
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] fhir_dstu2.Hl7.Fhir.Rest.SummaryType? summary = null,
            [FromUri(Name = "_include")] string include = null,
            [FromUri(Name = "_id")] int? implementationGuideId = null,
            [FromUri(Name = "name")] string name = null)
        {
            var dstu2IgType = this.tdb.ImplementationGuideTypes.Single(y => y.Name == ImplementationGuideType.FHIR_DSTU2_NAME);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, dstu2IgType);
            ImplementationGuideExporter exporter = new ImplementationGuideExporter(this.tdb, schema, this.BaseProfilePath, this.Request.RequestUri.Scheme, this.Request.RequestUri.Authority);
            var bundle = exporter.GetImplementationGuides(summary, include, implementationGuideId, name);
            return Shared.GetResponseMessage(this.Request, format, bundle);
        }

        [HttpPost]
        [Route("api/FHIR2/ImplementationGuide")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage CreateImplementationGuide(
            [FromBody] FhirImplementationGuide implementationGuide,
            [FromUri(Name = "_format")] string format = null)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("api/FHIR2/ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage UpdateImplementationGuide(
            [FromUri] int implementationGuideId,
            [FromBody] FhirImplementationGuide implementationGuide)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [Route("api/FHIR2/ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage DeleteImplementationGuide([FromUri] int implementationGuideId)
        {
            throw new NotImplementedException();
        }
    }
}
