using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Trifolia.Authorization;

namespace Trifolia.Web.Controllers.API
{
    public class SecurityController : ApiController
    {
        [HttpPost]
        [Route("api/Security/HasSecurables")]
        public bool HasSecurable([FromBody]string[] securables)
        {
            return CheckPoint.Instance.HasSecurables(securables);
        }
    }
}
