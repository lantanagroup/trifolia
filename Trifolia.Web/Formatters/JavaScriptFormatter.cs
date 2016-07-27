using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Trifolia.Web.Formatters
{
    public class JavaScriptFormatter : MediaTypeFormatter
    {
        public JavaScriptFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/javascript"));
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(string);
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                StreamWriter writer = new StreamWriter(writeStream);
                writer.Write(value);
                writer.Flush();
            });
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew(() =>
            {
                StreamReader reader = new StreamReader(readStream);
                return (object)reader.ReadToEnd();
            });
        }
    }
}