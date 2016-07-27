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
            this.FhirIgTypes = new Dictionary<string, int>();
        }

        public Dictionary<string, int> FhirIgTypes { get; set; }
    }
}