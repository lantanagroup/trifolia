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

            //Collect all methods in script
            Assembly assembly = Assembly.GetExecutingAssembly();

            //Find all FHIR API IGControllers
            var FHIRClasses = assembly.GetTypes()
                .Where(t => t.IsClass && t.GetCustomAttributes(typeof(FHIRInfo)).Any())
                .ToArray();

            foreach (var FHIRClass in FHIRClasses)
            {
                //Get the attributes of the FHIR IGController being examined
                var attributes = FHIRClass.GetCustomAttributes();

                //Collect the version attribute of that IGController
                var versionAttribute = (FHIRInfo)attributes.Single(a => a.GetType() == typeof(FHIRInfo));

                //Find the igType in the database
                ImplementationGuideType igType = this.tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name.ToLower() == versionAttribute.IGType.ToLower());

                //If doesn't exist, throw an error but continue
                if (igType == null)
                {
                    Log.For(this).Error("Implementation guide type defined in web.config not found in database: " + versionAttribute.IGType);
                    continue;
                }

                //Get the route attribute
                var routeCheck = attributes.Where(a => a.GetType() == typeof(RoutePrefixAttribute));

                //Make sure there's exactly one route attribute (shouldn't be possible but doesn't hurt to check)
                if (routeCheck.Count() > 1 || routeCheck.Count() == 0)
                {
                    throw new Exception("There are " + routeCheck.Count().ToString() + " route prefixes when there should be 1 for FHIR " + versionAttribute.Version);
                }

                //Get the route attribute of the IGController
                var routeAttribute = (RoutePrefixAttribute)attributes.Single(a => a.GetType() == typeof(RoutePrefixAttribute));
                
                var fhirIgType = new ClientConfigModel.FhirIgType()
                {
                    Id = igType.Id,
                    Name = igType.Name,
                    Version = versionAttribute.Version,
                    BaseUrl = "/" + routeAttribute.Prefix + "/"
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
