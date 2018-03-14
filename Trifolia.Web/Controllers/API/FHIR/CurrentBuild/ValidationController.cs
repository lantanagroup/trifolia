extern alias fhir_latest;
using fhir_latest.Hl7.Fhir.Model;
using fhir_latest.Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Trifolia.DB;

namespace Trifolia.Web.Controllers.API.FHIR.CurrentBuild
{
    [LatestConfigAttribute]
    [RoutePrefix("api/FHIRLatest")]
    public class ValidationController : ApiController
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public ValidationController()
            : this(DBContext.Create())
        {

        }

        public ValidationController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        /*
        [HttpPost]
        [Route("{resourceType}/$validate")]
        [Route("{resourceType}/{templateId}/$validate")]
        public OperationOutcome Validate(Resource resource, string resourceType, int? templateId = null)
        {
            OperationOutcome oo = new OperationOutcome();

            var spec = SpecificationFactory.Create("http://hl7.org/fhir/Profile/" + resourceType);

            return oo;
        }
         */
    }
}