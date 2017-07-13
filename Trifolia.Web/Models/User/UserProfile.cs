using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Account
{
    public class UserProfile
    {
        public string UserName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool OkayToContact { get; set; }
        public string Organization { get; set; }
        public string OrganizationType { get; set; }
        public string AuthToken { get; set; }
        public string OpenIdConfigUrl { get; set; }
        public string UmlsUsername { get; set; }
        public string UmlsPassword { get; set; }
    }
}