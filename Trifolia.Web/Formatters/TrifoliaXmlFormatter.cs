using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using Trifolia.DB.Exceptions;

namespace Trifolia.Web.Formatters
{
    public class TrifoliaXmlFormatter : BufferedMediaTypeFormatter
    {
        public TrifoliaXmlFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content, System.Threading.CancellationToken cancellationToken)
        {
            base.WriteToStream(type, value, writeStream, content, cancellationToken);
        }

        public override object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, System.Threading.CancellationToken cancellationToken)
        {
            return base.ReadFromStream(type, readStream, content, formatterLogger, cancellationToken);
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(Trifolia.Shared.ImportExport.Model.Trifolia);
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(Trifolia.Shared.ImportExport.Model.Trifolia);
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            var model = value as Trifolia.Shared.ImportExport.Model.Trifolia;

            using (var writer = new StreamWriter(writeStream))
            {
                writer.Write(model.Serialize());
            }
        }

        public override object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            try
            {
                return Trifolia.Shared.ImportExport.Model.Trifolia.Deserialize(readStream);
            }
            catch (Exception ex)
            {
                throw new TrifoliaModelException("Only files exported with type \"Trifolia XML\" can be imported into Trifolia at this time", ex);
            }
        }
    }
}