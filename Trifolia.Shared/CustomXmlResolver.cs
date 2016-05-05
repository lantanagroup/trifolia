using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Trifolia.Shared
{
    [Serializable]
    public class CustomXmlResolver : XmlResolver
    {
        private Dictionary<string, byte[]> storedFiles = new Dictionary<string, byte[]>();

        public void AddStoredFile(string fileName, byte[] data)
        {
            storedFiles.Add(fileName, data);
        }

        public override System.Net.ICredentials Credentials
        {
            set { throw new NotImplementedException(); }
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            string fileName = string.Empty;

            if (!Uri.IsWellFormedUriString(absoluteUri.AbsoluteUri, UriKind.RelativeOrAbsolute))
            {
                FileInfo fileNameInfo = new FileInfo(absoluteUri.AbsoluteUri);
                fileName = fileNameInfo.Name;
            }
            else
            {
                int index = absoluteUri.AbsoluteUri.LastIndexOf('/');
                if (index > 0)
                    fileName = absoluteUri.AbsoluteUri.Substring(index + 1);
            }
            

            if (storedFiles.ContainsKey(fileName))
            {
                MemoryStream ms = new MemoryStream(storedFiles[fileName]);
                return ms;
            }

            throw new Exception("'" + fileName + "' is not loaded.");
        }
    }
}