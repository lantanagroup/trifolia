extern alias fhir_stu3;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

using Newtonsoft.Json;

using fhir_stu3.Hl7.Fhir.Model;
using fhir_stu3.Hl7.Fhir.Rest;
using fhir_stu3.Hl7.Fhir.Serialization;
using System.Xml;
using Trifolia.Logging;

namespace Trifolia.Web.Formatters.FHIR.STU3
{
    public class GeneralFHIRMediaTypeFormatter : FhirMediaTypeFormatter
    {
        public GeneralFHIRMediaTypeFormatter()
            : base()
        {
            SupportedMediaTypes.Clear();

            foreach (var mediaType in ContentType.JSON_CONTENT_HEADERS)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));

            foreach (var mediaType in ContentType.XML_CONTENT_HEADERS)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(Resource) || type.IsSubclassOf(typeof(Resource));
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(Resource) || type.IsSubclassOf(typeof(Resource)) || type == typeof(HttpError);
        }

        public string RequestContentType
        {
            get
            {
                string contentType = HttpContext.Current != null ? HttpContext.Current.Request.ContentType : "application/json";

                if (!string.IsNullOrEmpty(contentType))
                {
                    if (contentType.IndexOf(";") > 0)
                        contentType = contentType.Substring(0, contentType.IndexOf(";"));
                }

                return contentType;
            }
        }

        public bool AcceptContainsXml
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Request.AcceptTypes == null || HttpContext.Current.Request.AcceptTypes.Length == 0)
                    return false;

                foreach (var acceptType in HttpContext.Current.Request.AcceptTypes)
                {
                    if (ContentType.XML_CONTENT_HEADERS.Contains(acceptType))
                        return true;
                }

                return false;
            }
        }

        public bool AcceptContainsJson
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Request.AcceptTypes == null || HttpContext.Current.Request.AcceptTypes.Length == 0)
                    return false;

                foreach (var acceptType in HttpContext.Current.Request.AcceptTypes)
                {
                    if (ContentType.JSON_CONTENT_HEADERS.Contains(acceptType))
                        return true;
                }

                return false;
            }
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);

            bool acceptXml = AcceptContainsXml;
            bool acceptJson = AcceptContainsJson;

            // If the Accept header was not specified by the client
            if (HttpContext.Current == null || HttpContext.Current.Request.AcceptTypes == null || HttpContext.Current.Request.AcceptTypes.Contains("*/*"))
            {
                // If the request's Content-Type was specified and it is either XML or JSON
                if (!string.IsNullOrEmpty(RequestContentType) && (ContentType.XML_CONTENT_HEADERS.Contains(RequestContentType) || ContentType.JSON_CONTENT_HEADERS.Contains(RequestContentType)))
                {
                    headers.ContentType = new MediaTypeHeaderValue(RequestContentType);
                    return;
                }
                // If the request's content-type was not specified
                else
                {
                    // Check if a _format parameter was specified, and if so, use that
                    if (HttpContext.Current != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Params["_format"]))
                    {
                        // Only set the content-type of the header if it is an acceptable format
                        if (ContentType.XML_CONTENT_HEADERS.Contains(HttpContext.Current.Request.Params["_format"]) || ContentType.JSON_CONTENT_HEADERS.Contains(HttpContext.Current.Request.Params["_format"]))
                        {
                            headers.ContentType = new MediaTypeHeaderValue(HttpContext.Current.Request.Params["_format"]);
                            return;
                        }
                    }
                }
            }
            else if (acceptXml && !acceptJson)
            {
                string acceptContentType = "application/json";

                if (HttpContext.Current.Request.AcceptTypes.Length == 1)
                    acceptContentType = HttpContext.Current.Request.AcceptTypes[0];

                headers.ContentType = new MediaTypeHeaderValue(acceptContentType);
                return;
            }

            headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            bool readXml = false;       // Default to reading JSON

            // If the request content-type was not specified, or the content-type specified is not JSON or XML
            if (string.IsNullOrEmpty(RequestContentType) || (!ContentType.JSON_CONTENT_HEADERS.Contains(RequestContentType) && !ContentType.XML_CONTENT_HEADERS.Contains(RequestContentType)))
            {
                // Check if a _format param was passed, and is an XML content type
                if (HttpContext.Current != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Params["_format"]) && ContentType.XML_CONTENT_HEADERS.Contains(HttpContext.Current.Request.Params["_format"]))
                    readXml = true;
            }
            // Otherwise check if the request's content-type is an XML content-type
            else if (ContentType.XML_CONTENT_HEADERS.Contains(RequestContentType))
            {
                readXml = true;
            }

            return System.Threading.Tasks.Task.Factory.StartNew<object>(() => 
            {
                try
                {
                    var body = base.ReadBodyFromStream(readStream, content);

                    if (type == typeof(Resource) || type.IsSubclassOf(typeof(Resource)))
                    {
                        var parserSettings = new fhir_stu3.Hl7.Fhir.Serialization.ParserSettings();
                        parserSettings.AcceptUnknownMembers = true;
                        parserSettings.AllowUnrecognizedEnums = true;
                        parserSettings.DisallowXsiAttributesOnRoot = false;

                        if (readXml)
                        {
                            var fhirXmlParser = new FhirXmlParser(parserSettings);
                            return fhirXmlParser.Parse<Resource>(body);
                        }
                        else
                        {
                            var fhirJsonParser = new FhirJsonParser(parserSettings);
                            return fhirJsonParser.Parse<Resource>(body);
                        }
                    }
                    else
                        throw new NotSupportedException(String.Format("Cannot read unsupported type {0} from body", type.Name));
                }
                catch (FormatException exc)
                {
                    throw new Exception("Body parsing failed: " + exc.Message);
                }
            });
        }

        public override System.Threading.Tasks.Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == typeof(HttpError))
            {
                HttpError error = (HttpError)value;
                OperationOutcome opOutcome = new OperationOutcome();
                List<string> locations = new List<string>();

                if (!string.IsNullOrEmpty(error.StackTrace))
                    locations.Add(error.StackTrace);

                opOutcome.Issue.Add(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Fatal,
                    Diagnostics = error.Message,
                    Code = OperationOutcome.IssueType.Exception,
                    Location = locations
                });

                string logMessage = string.Format("An error with FHIR STU3 ocurred: {0}\nError Detail: {1}\n{2}\n{3}", error.Message, error.MessageDetail, error.ExceptionMessage, error.StackTrace);
                Log.For(this).Error(logMessage);

                value = opOutcome;
                type = opOutcome.GetType();
            }

            // Return content-type was set by SetDefaultContentType(), just check if that determined that it should be XML
            bool returnXml = ContentType.XML_CONTENT_HEADERS.Contains(content.Headers.ContentType.MediaType);

            return System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                if (type == typeof(Resource) || type.IsSubclassOf(typeof(Resource)))
                {
                    if (returnXml)
                    {
                        XmlWriter writer = new XmlTextWriter(writeStream, Encoding.UTF8);
                        FhirSerializer.SerializeResource((Resource)value, writer);
                        writer.Flush();
                    }
                    else
                    {
                        StreamWriter writer = new StreamWriter(writeStream);
                        JsonWriter jsonwriter = new JsonTextWriter(writer);
                        Resource resource = (Resource)value;
                        FhirSerializer.SerializeResource(resource, jsonwriter);
                        writer.Flush();
                    }
                }
                else
                    throw new NotSupportedException(String.Format("Cannot write unsupported type {0} to body", type.Name));

            });
        }
    }
}