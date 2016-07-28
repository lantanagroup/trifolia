extern alias fhir_dstu2;

using fhir_dstu2.Hl7.Fhir.Model;
using fhir_dstu2.Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using Trifolia.DB;
using Trifolia.Web.Formatters.FHIR.DSTU2;
using Trifolia.Shared;
using System.IO;

using ImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    public class DSTU2ConfigAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Clear();
            controllerSettings.Formatters.Add(new GeneralFHIRMediaTypeFormatter());
        }
    }

    public class Shared
    {
        public static HttpResponseMessage GetResponseMessage(HttpRequestMessage request, string format, object ret, int statusCode = 200, Dictionary<string, string> headers = null)
        {
            MediaTypeFormatter formatter = new GeneralFHIRMediaTypeFormatter();
            HttpResponseMessage message = new HttpResponseMessage();
            message.Content = new ObjectContent(ret.GetType(), ret, formatter, format);
            message.StatusCode = (HttpStatusCode)statusCode;

            if (headers != null)
            {
                foreach (var headerKey in headers.Keys)
                {
                    message.Headers.Add(headerKey, headers[headerKey]);
                }
            }

            return message;
        }
    }

    public static class TemplateExtensions
    {
        public static string GetFullUrl(this Template template, HttpRequestMessage request)
        {
            var url = string.Format("{0}://{1}/api/FHIR2/StructureDefinition/{2}",
                request.RequestUri.Scheme,
                request.RequestUri.Authority,
                template.Id);
            return url;
        }

        public static string GetId(this Template template)
        {
            if (!string.IsNullOrEmpty(template.Oid) && (template.Oid.StartsWith("http://") || template.Oid.StartsWith("https://")))
                return template.Oid.Substring(template.Oid.LastIndexOf('/') + 1);

            return template.Id.ToString();
        }
    }

    public static class ImplementationGuideExtensions
    {
        public static string GetFullUrl(this ImplementationGuide implementationGuide, HttpRequestMessage request)
        {
            return implementationGuide.GetFullUrl(request.RequestUri.Scheme, request.RequestUri.Authority);
        }

        public static string GetFullUrl(this ImplementationGuide implementationGuide, string scheme, string authority)
        {
            var url = string.Format("{0}://{1}/api/FHIR2/ImplementationGuide/{2}",
                scheme,
                authority,
                implementationGuide.Id);
            return url;
        }
    }

    public class FHIRUrlStructure
    {
        public string FullUrl { get; set; }
        public string Base { get; set; }
        public string ResourceType { get; set; }
        public string Identifier { get; set; }
        public string Version { get; set; }

        public bool IsThisServer(HttpRequestMessage request)
        {
            if (string.IsNullOrEmpty(this.Base))
                return true;

            string thisHost = string.Format("{0}://{1}",
                    request.RequestUri.Scheme,
                    request.RequestUri.Authority);

            return this.Base.StartsWith(thisHost);
        }
    }
}