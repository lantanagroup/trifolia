using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Authentication.Models
{
    public class OAuth2UserInfo
    {
        // These are properties that are included in almost all OpenID userinfo requests
        public string name { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string email { get; set; }
        public string sub { get; set; }
        public string picture { get; set; }
        public string preferred_username { get; set; }

        // These are extra properties that may not always be returned depending on the identity provider
        // These particular properties are returned from auth0.com
        public bool email_verified { get; set; }
        public string clientID { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_time { get; set; }
        public string user_id { get; set; }
        public string nickname { get; set; }
        public string link { get; set; }
        public List<string> emails { get; set; }
        public Phones phones { get; set; }

        internal string phone
        {
            get
            {
                if (this.phones == null)
                    return null;

                if (!string.IsNullOrEmpty(this.phones.business))
                    return this.phones.business;
                else if (!string.IsNullOrEmpty(this.phones.mobile))
                    return this.phones.mobile;
                else if (!string.IsNullOrEmpty(this.phones.personal))
                    return this.phones.personal;

                return null;
            }
        }

        public class Phones
        {
            public string personal { get; set; }
            public string business { get; set; }
            public string mobile { get; set; }
        }
    }
}
