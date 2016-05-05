using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace Trifolia.Web.Controllers.API
{
    public class TrifoliaApiController : ApiController
    {
        protected override System.Web.Http.Results.NegotiatedContentResult<T> Content<T>(System.Net.HttpStatusCode statusCode, T value)
        {
            return base.Content<T>(statusCode, value);
        }

        protected NegotiatedContentResult<T> Content<T>(HttpStatusCode statusCode, T value, Dictionary<string, string> headers)
        {
            return new CustomHeadersWithContentResult<T>(statusCode, value, this, headers);
        }

        public class CustomHeadersWithContentResult<T> : NegotiatedContentResult<T>
        {
            public CustomHeadersWithContentResult(HttpStatusCode status, T content, ApiController controller)
                : base(status, content, controller)
            {
                this.CustomHeaders = new Dictionary<string, string>();
            }

            public CustomHeadersWithContentResult(HttpStatusCode status, T content, ApiController controller, Dictionary<string, string> customHeaders)
                : base(status, content, controller)
            {
                this.CustomHeaders = customHeaders;
            }

            public Dictionary<string, string> CustomHeaders { get; set; }

            public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                HttpResponseMessage response = await base.ExecuteAsync(cancellationToken);

                foreach (var headerName in this.CustomHeaders.Keys)
                {
                    if (headerName == "Location")
                    {
                        response.Headers.Location = new Uri(this.CustomHeaders[headerName]);
                        continue;
                    }

                    if (response.Headers.Contains(headerName))
                        response.Headers.Remove(headerName);

                    response.Headers.Add(headerName, this.CustomHeaders[headerName]);
                }

                return response;
            }
        }
    }
}