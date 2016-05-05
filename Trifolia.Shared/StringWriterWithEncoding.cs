using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Trifolia.Shared
{
    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
}
