using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

using Trifolia.Web.Filters;
using Trifolia.Web.Formatters.FHIR.DSTU2;

namespace Trifolia.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.MessageHandlers.Insert(0,
                new ServerCompressionHandler(
                    new GZipCompressor(),
                    new DeflateCompressor()));

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
