using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Account
{
    public class UserProfile
    {
        public string userName { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public bool okayToContact { get; set; }
        public string organization { get; set; }
        public string organizationType { get; set; }
        public string authToken { get; set; }
        public string openIdConfigUrl { get; set; }
    }
}