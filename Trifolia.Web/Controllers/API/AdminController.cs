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
using System.Reflection;
using Trifolia.Web.Controllers.API.FHIR;

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

                //Collect all methods in script
                Assembly assembly = Assembly.GetExecutingAssembly();

                //Find all FHIR API IGControllers
                var FHIRClasses = assembly.GetTypes()
                    .Where(t => t.IsClass && t.GetCustomAttributes(typeof(FHIRVersion)).Any())
                    .ToArray();

                String route = "";
                foreach(var FHIRClass in FHIRClasses)
                {
                    //Get the attributes of the FHIR IGController being examined
                    var attributes = FHIRClass.GetCustomAttributes();

                    //Collect the version attribute of that IGController
                    var versionAttribute = (FHIRVersion)attributes.Single(a => a.GetType() == typeof(FHIRVersion));

                    //If the IGController being examined doesn't have a matching FHIR Version, move to the next
                    if(versionAttribute.Version != fit.Version) continue;

                    //Get the route attribute of the IGController. Always only going to be one attribute that matches
                    var routeAttribute = (RoutePrefixAttribute)attributes.Single(a => a.GetType() == typeof(RoutePrefixAttribute));

                    //Save it here and stop searching
                    route = routeAttribute.Prefix;
                    break;
                }

                var fhirIgType = new ClientConfigModel.FhirIgType()
                {
                    Id = igType.Id,
                    Name = igType.Name,
                    Version = fit.Version,
                    BaseUrl = route
                };

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
