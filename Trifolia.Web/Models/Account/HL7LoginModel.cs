using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Account
{
    public class HL7LoginModel
    {
        public HL7LoginModel()
        {
            
        }

        public string userid { get; set; }
        public int timestampUTCEpoch { get; set; }
        public string apiKey { get; set; }
        public string requestHash { get; set; }
        public string fullName { get; set; }
        public string roles { get; set; }
        public string ReturnUrl { get; set; }
    }
}