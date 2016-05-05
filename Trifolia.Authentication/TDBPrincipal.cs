using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace TemplateDatabase.Authentication
{
    public class TDBPrincipal : IPrincipal
    {
        public const string ROLE_USERS = "Users";
        public const string ROLE_IG_ADMINS = "IG Admins";
        public const string ROLE_SCHEMA_ADMINS = "Schema Admins";
        public const string ROLE_TEMPLATE_AUTHORS = "Template Authors";
        public const string ROLE_ADMINISTRATORS = "Administrators";

        private TDBIdentity identity;

        public IIdentity Identity
        {
            get { return identity; }
        }

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }

        public TDBPrincipal(string application, string name, string username)
        {
            this.identity = new TDBIdentity(application, name, username);
        }
    }
}
