using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Hosting;
using System.Web.Http.Description;
using System.Resources;
using System.Dynamic;
using System.Globalization;
using System.Collections;
using Newtonsoft.Json;

namespace Trifolia.Web.Controllers.API
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalizationController : ApiController
    {
        private string GetResourcesJSObject()
        {
            var resourceManager = Trifolia.Web.App_GlobalResources.TrifoliaLang.ResourceManager;

            var ret = new
            {
                // Each assembly that has different resources here
                Web = new ExpandoObject()
            };

            var webResources = ret.Web as IDictionary<string, object>;

            foreach (DictionaryEntry resource in resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true))
            {
                string resourceKey = resource.Key.ToString();
                webResources.Add(resourceKey, resourceManager.GetString(resourceKey, Thread.CurrentThread.CurrentCulture));
            }

            return JsonConvert.SerializeObject(ret);
        }

        [HttpGet, Route("api/Localization")]
        public HttpResponseMessage GetResourcesScript()
        {
            string resources = GetResourcesJSObject();
            var ret = string.Format("var Trifolia = {0};", resources);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(ret);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript");
            return result;
        }
    }
}
