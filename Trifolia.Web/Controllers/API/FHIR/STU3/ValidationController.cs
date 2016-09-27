extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Model;
using fhir_stu3.Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Trifolia.DB;

namespace Trifolia.Web.Controllers.API.FHIR.STU3
{
    [STU3Config]
    [RoutePrefix("api/FHIR3")]
    public class ValidationController : ApiController
    {
        private IObjectRepository tdb;

        public ValidationController()
            : this(DBContext.Create())
        {

        }

        public ValidationController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

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