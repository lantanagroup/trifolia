using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

using Trifolia.Web.Filters;
using Trifolia.Web.Formatters;
using Trifolia.Web.Formatters.FHIR.DSTU2;

namespace Trifolia.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Insert(0, new TrifoliaXmlFormatter()); 

            GlobalConfiguration.Configuration.MessageHandlers.Insert(0,
                new ServerCompressionHandler(
                    new GZipCompressor(),
                    new DeflateCompressor()));

            //GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnsureInitialized();
        }
    }
}
