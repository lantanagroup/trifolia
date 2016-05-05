using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Account
{
    public class LoginModel
    {
        public LoginModel()
        {
            this.Organizations = new Dictionary<int, string>();
        }

        public Dictionary<int, string> Organizations { get; set; }
        public string HL7LoginLink { get; set; }
        public string Message { get; set; }

        public int? OrganizationId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
        public bool RecaptchaAllowBypass { get; set; }
    }
}