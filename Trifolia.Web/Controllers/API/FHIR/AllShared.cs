using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Trifolia.Web.Controllers.API.FHIR
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FHIRVersion : Attribute
    {
        private String version;
        private String igType;

        public FHIRVersion(String version, String igType)
        {
            this.version = version;
            this.igType = igType;
        }

        public String Version
        {
            get { return this.version; }
        }

        public String IGType
        {
            get { return this.igType; }
        }

    }
}