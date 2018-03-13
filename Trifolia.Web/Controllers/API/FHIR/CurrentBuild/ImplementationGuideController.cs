extern alias fhir_latest;
using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Export.FHIR.Latest;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;
using FhirImplementationGuide = fhir_latest.Hl7.Fhir.Model.ImplementationGuide;
using SummaryType = fhir_latest.Hl7.Fhir.Rest.SummaryType;

namespace Trifolia.Web.Controllers.API.FHIR.CurrentBuild
{
    [LatestConfigAttribute]
    [RoutePrefix("api/FHIRLatest")]
    public class FHIRLatestImplementationGuideController : ApiController
    {
        private IObjectRepository tdb;
        private ImplementationGuideType implementationGuideType;

        #region Construct/Dispose

        public FHIRLatestImplementationGuideController(IObjectRepository tdb)
        {
            this.tdb = tdb;
            this.implementationGuideType = STU3Helper.GetImplementationGuideType(this.tdb, true);
        }

        public FHIRLatestImplementationGuideController()
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
        /// Get the specified implementation guide in FHIR DSTU2 format
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <param name="format">Optional. The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.ImplementationGuide</returns>
        [HttpGet]
        [Route("ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public HttpResponseMessage GetImplementationGuide(
            int implementationGuideId,
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] fhir_latest.Hl7.Fhir.Rest.SummaryType? summary = null)
        {
            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, implementationGuide.ImplementationGuideType);
            ImplementationGuideExporter exporter = new ImplementationGuideExporter(this.tdb, schema, uri.Scheme, uri.Authority);
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
        [Route("ImplementationGuide")]
        [Route("ImplementationGuide/_search")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public HttpResponseMessage GetImplementationGuides(
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] fhir_latest.Hl7.Fhir.Rest.SummaryType? summary = null,
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

        [HttpPost]
        [Route("ImplementationGuide")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage CreateImplementationGuide(
            [FromBody] FhirImplementationGuide fhirImplementationGuide,
            [FromUri(Name = "_format")] string format = null)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Name.ToLower() == fhirImplementationGuide.Name.ToLower());

            if (ig != null)
                throw new Exception("The implementation guide already exists. Use PUT to update the implementation guide instead.");

            ig = new ImplementationGuide()
            {
                Name = fhirImplementationGuide.Name
            };

            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage UpdateImplementationGuide(
            [FromUri] int implementationGuideId,
            [FromBody] FhirImplementationGuide implementationGuide)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);

            if (ig == null)
                throw new Exception("The implementation guide does not exist");

            if (!CheckPoint.Instance.GrantEditImplementationGuide(ig.Id))
                throw new UnauthorizedAccessException("You do not have permissions to edit this implementation guide");

            throw new NotImplementedException();
        }

        [HttpDelete]
        [Route("ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage DeleteImplementationGuide([FromUri] int implementationGuideId)
        {
            throw new NotImplementedException();
        }
    }
}
