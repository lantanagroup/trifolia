using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.Shared;

namespace Trifolia.Test
{
    public class MockWebClient : IWebClient
    {
        private Dictionary<string, string> content = new Dictionary<string, string>();

        public int RequestCount { get; set; }

        public void ExpectContent(string uri, string content)
        {
            this.content.Add(uri, content);
        }

        public Stream OpenRead(string uri)
        {
            this.RequestCount++;

            if (!this.content.ContainsKey(uri))
                throw new Exception("Unexpected URI sent to MockWebClient: " + uri);

            MemoryStream ms = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(this.content[uri]));
            ms.Position = 0;

            return ms;
        }
    }
}
