using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Trifolia.Web.Controllers.API.FHIR
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FHIRVersion : Attribute
    {
        private String version;

        public FHIRVersion(String version)
        {
            this.version = version;
        }

        public String Version
        {
            get { return this.version; }
        }
    }
}