using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Logging;
using Trifolia.Web.Models.Admin;

namespace Trifolia.Web.Controllers.API
{
    [RoutePrefix("api/Admin")]
    public class AdminController : ApiController
    {
        private IObjectRepository tdb;

        #region Construct/Dipose

        public AdminController() : this(DBContext.Create())
        {

        }

        public AdminController(IObjectRepository tdb)
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

        /// <summary>
        /// Gets configuration information used by the client-side application
        /// </summary>
        [HttpGet, Route("Config")]
        public IHttpActionResult GetClientConfig(bool isRef = false)
        {
            var clientConfig = new ClientConfigModel();

            foreach (IGTypeFhirElement fit in IGTypeSection.GetSection().FhirIgTypes)
            {
                ImplementationGuideType igType = this.tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name.ToLower() == fit.ImplementationGuideTypeName.ToLower());

                if (igType == null)
                {
                    Log.For(this).Error("Implementation guide type defined in web.config not found in database: " + fit.ImplementationGuideTypeName);
                    continue;
                }

                var fhirIgType = new ClientConfigModel.FhirIgType()
                {
                    Id = igType.Id,
                    Name = igType.Name,
                    Version = fit.Version
                };

                switch (fhirIgType.Version)
                {
                    case "DSTU1":
                        fhirIgType.BaseUrl = "/api/FHIR1/";
                        break;
                    case "DSTU2":
                        fhirIgType.BaseUrl = "/api/FHIR2/";
                        break;
                    case "STU3":
                        fhirIgType.BaseUrl = "/api/FHIR3/";
                        break;
                }

                clientConfig.FhirIgTypes.Add(fhirIgType);
            }

            if (isRef)
            {
                var clientConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(clientConfig);
                return Content<string>(HttpStatusCode.OK, "var trifoliaConfig = " + clientConfigJson + ";", new Formatters.JavaScriptFormatter(), "text/javascript");
            }

            return Content<ClientConfigModel>(HttpStatusCode.OK, clientConfig);
        }
    }
}
