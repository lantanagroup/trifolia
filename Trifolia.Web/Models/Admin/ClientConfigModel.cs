using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Admin
{
    public class ClientConfigModel
    {
        public ClientConfigModel()
        {
            this.FhirIgTypes = new List<FhirIgType>();
        }

        public List<FhirIgType> FhirIgTypes { get; set; }

        public class FhirIgType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string BaseUrl { get; set; }
        }
    }
}