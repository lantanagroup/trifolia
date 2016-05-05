extern alias fhir_dstu1;

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

using Newtonsoft.Json;

using fhir_dstu1.Hl7.Fhir.Model;
using fhir_dstu1.Hl7.Fhir.Rest;
using fhir_dstu1.Hl7.Fhir.Serialization;

namespace Trifolia.Web.Formatters.FHIR.DSTU1
{
    public class JSONFHIRMediaTypeFormatter : FhirMediaTypeFormatter
    {
        public JSONFHIRMediaTypeFormatter()
            : base()
        {
            foreach (var mediaType in ContentType.JSON_CONTENT_HEADERS)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew<object>(() => 
            {
                try
                {
                    var body = base.ReadBodyFromStream(readStream, content);

                    if (type == typeof(Profile))
                    {
                        return FhirParser.ParseResourceFromJson(body);
                    }
                    else if (type == typeof(ResourceEntry))
                    {
                        Resource resource = FhirParser.ParseResourceFromJson(body);
                        ResourceEntry entry = ResourceEntry.Create(resource);
                        return entry;
                    }
                    else if (type == typeof(Bundle))
                    {
                        return FhirParser.ParseBundleFromJson(body);
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

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                StreamWriter writer = new StreamWriter(writeStream);
                JsonWriter jsonwriter = new JsonTextWriter(writer);

                if (type == typeof(Profile))
                {
                    Profile entry = (Profile)value;
                    FhirSerializer.SerializeResource(entry, jsonwriter);
                }
                else if (type == typeof(ResourceEntry))
                {
                    ResourceEntry entry = (ResourceEntry)value;
                    FhirSerializer.SerializeResource(entry.Resource, jsonwriter);
                }
                else if (type == typeof(Bundle))
                {
                    FhirSerializer.SerializeBundle((Bundle)value, jsonwriter);
                }

                writer.Flush();
            });
        }
    }

    public class FhirMediaTypeFormatter : JsonMediaTypeFormatter
    {
        private JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();

        public FhirMediaTypeFormatter()
            : base()
        {
            foreach (var mediaType in ContentType.JSON_CONTENT_HEADERS)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(Profile) || type == typeof(Bundle);
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(Profile) || type == typeof(Bundle);
        }

        protected string ReadBodyFromStream(Stream readStream, HttpContent content)
        {
            var charset = content.Headers.ContentType.CharSet ?? Encoding.UTF8.HeaderName;
            var encoding = Encoding.GetEncoding(charset);

            if (encoding != Encoding.UTF8)
                throw new Exception("FHIR supports UTF-8 encoding exclusively, not " + encoding.WebName);

            StreamReader sr = new StreamReader(readStream, Encoding.UTF8, true);
            return sr.ReadToEnd();
        }

        public override System.Threading.Tasks.Task WriteToStreamAsync(Type type, object value, System.IO.Stream writeStream, HttpContent content, System.Net.TransportContext transportContext)
        {
            if (value is ResourceEntry)
            {
                ResourceEntry re = (ResourceEntry)value;

                if (re.SelfLink != null)
                    content.Headers.ContentLocation = re.SelfLink;
                if (re.LastUpdated != null)
                    content.Headers.LastModified = re.LastUpdated;
            }

            return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }
    }
}