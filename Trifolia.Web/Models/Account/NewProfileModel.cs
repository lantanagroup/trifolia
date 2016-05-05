using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Account
{
    [Serializable]
    public class NewProfileModel
    {
        public string RedirectUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool OkayToContact { get; set; }
        public string Organization { get; set; }
        public string OrganizationType { get; set; }
    }
}