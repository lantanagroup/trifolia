extern alias fhir_dstu2;
using fhir_dstu2.Hl7.Fhir.Model;
using fhir_dstu2.Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Trifolia.Web.Formatters.FHIR.DSTU2
{
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
            return type.IsSubclassOf(typeof(Resource));
        }

        public override bool CanWriteType(Type type)
        {
            return type.IsSubclassOf(typeof(Resource));
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
            /*
            if (value is ResourceEntry)
            {
                ResourceEntry re = (ResourceEntry)value;

                if (re.SelfLink != null)
                    content.Headers.ContentLocation = re.SelfLink;
                if (re.LastUpdated != null)
                    content.Headers.LastModified = re.LastUpdated;
            }
             */

            return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }
    }
}