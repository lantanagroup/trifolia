using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;

namespace Trifolia.Web.Controllers.API
{
    public class ImportController : ApiController
    {
        [HttpPost, Route("api/Import/Trifolia")]
        public void ImportTrifoliaModel(ImportModel model)
        {
            Console.WriteLine("test");
        }
    }
}
