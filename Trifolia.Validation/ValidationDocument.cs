using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.ValidationService
{
    public class ValidationDocument
    {
        #region Properties

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string contentType;

        public string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }
        private string mimeType;

        public string MimeType
        {
            get { return mimeType; }
            set { mimeType = value; }
        }
        private byte[] content;

        public byte[] Content
        {
            get { return content; }
            set { content = value; }
        }

        #endregion
    }
}