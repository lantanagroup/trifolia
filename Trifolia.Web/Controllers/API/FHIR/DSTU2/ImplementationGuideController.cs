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
using Trifolia.Shared.FHIR;

using fhir_dstu2.Hl7.Fhir.Model;
using FhirImplementationGuide = fhir_dstu2.Hl7.Fhir.Model.ImplementationGuide;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using Trifolia.Export.FHIR.DSTU2;
using System.Web;
using Trifolia.Config;
using Trifolia.Logging;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    /// <summary>
    /// FHIR DSTU2 ImplementationGuide API Controller
    /// </summary>
    [DSTU2Config]
    [RoutePrefix("api/FHIR2")]
    public class FHIR2ImplementationGuideController : ApiController
    {
        private const string VERSION_NAME = "DSTU2";

        private IObjectRepository tdb;
        private ImplementationGuideType implementationGuideType;

        #region Construct/Dispose

        public FHIR2ImplementationGuideController(IObjectRepository tdb)
        {
            this.tdb = tdb;
            this.implementationGuideType = DSTU2Helper.GetImplementationGuideType(this.tdb, true);
        }

        public FHIR2ImplementationGuideController()
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

        /// <summary>
        /// Get the specified implementation guide
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <param name="format">The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.ImplementationGuide</returns>
        [HttpGet]
        [Route("ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public HttpResponseMessage GetImplementationGuide(
            int implementationGuideId,
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] fhir_dstu2.Hl7.Fhir.Rest.SummaryType? summary = null)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, implementationGuide.ImplementationGuideType);
            ImplementationGuideExporter exporter = new ImplementationGuideExporter(this.tdb, schema, uri.Scheme, uri.Authority);
            FhirImplementationGuide response = exporter.Convert(implementationGuide, summary);
            return Shared.GetResponseMessage(this.Request, format, response);
        }

        /// <summary>
        /// Gets implementation guides. Can specify search information, such as the name of the implementation guide and the id of the implementation guide.
        /// </summary>
        /// <param name="format">The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">The type of summary to respond with.</param>
        /// <param name="include">Indicate what additional information should be included with the implementation guide (such as "ImplementationGuide:resource")</param>
        /// <param name="implementationGuideId">Specify the id of the implementation guide to search for.</param>
        /// <param name="name">Specifies the name of the implementation guide to search for. Implementation guides whose name *contains* this value will be returned.</param>
        /// <returns>Hl7.Fhir.Model.Bundle</returns>
        [HttpGet]
        [Route("ImplementationGuide")]
        [Route("ImplementationGuide/_search")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public HttpResponseMessage GetImplementationGuides(
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] fhir_dstu2.Hl7.Fhir.Rest.SummaryType? summary = null,
            [FromUri(Name = "_include")] string include = null,
            [FromUri(Name = "_id")] int? implementationGuideId = null,
            [FromUri(Name = "name")] string name = null)
        {
            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, this.implementationGuideType);
            ImplementationGuideExporter exporter = new ImplementationGuideExporter(this.tdb, schema, uri.Scheme, uri.Authority);
            var bundle = exporter.GetImplementationGuides(summary, include, implementationGuideId, name);
            return Shared.GetResponseMessage(this.Request, format, bundle);
        }

        /// <summary>
        /// Creates a new implementation guide
        /// </summary>
        /// <param name="implementationGuide">The implementation guide body</param>
        /// <param name="format">The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <returns></returns>
        /// <remarks>Not implemented</remarks>
        [HttpPost]
        [Route("ImplementationGuide")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage CreateImplementationGuide(
            [FromBody] FhirImplementationGuide implementationGuide,
            [FromUri(Name = "_format")] string format = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the specified implementation guide
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide to update</param>
        /// <param name="implementationGuide">The implementation guide body</param>
        /// <returns></returns>
        /// <remarks>Not implemented</remarks>
        [HttpPut]
        [Route("ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage UpdateImplementationGuide(
            [FromUri] int implementationGuideId,
            [FromBody] FhirImplementationGuide implementationGuide)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the specified implementation guide
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <returns></returns>
        /// <remarks>Not implemented</remarks>
        [HttpDelete]
        [Route("ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage DeleteImplementationGuide([FromUri] int implementationGuideId)
        {
            throw new NotImplementedException();
        }
    }
}
