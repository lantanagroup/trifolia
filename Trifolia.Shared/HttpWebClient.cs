using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Trifolia.Shared
{
    public class HttpWebClient : IWebClient
    {
        public Stream OpenRead(string uri)
        {
            WebClient webClient = new WebClient();
            return webClient.OpenRead(uri);
        }
    }
}
