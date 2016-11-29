using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Trifolia.Authorization;
using System.Security.Principal;
using System.Net;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Set, "TrifoliaPasswordReset")]
    public class PasswordResetCommand : BaseCommand
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The connection name for the Auth0 account (ex: \"Username-Password-Authentication\")")]
        public string Auth0Connection { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The account name in auth0")]
        public string Auth0Account { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The client id of the application in Auth0")]
        public string Auth0ClientId { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Email addresses to send a password reset request for")]
        public string[] Emails { get; set; }

        protected override void ProcessRecord()
        {
            string url = string.Format("https://{0}.auth0.com/dbconnections/change_password", this.Auth0Account);
            List<EmailResponse> responses = new List<EmailResponse>();

            foreach (var email in this.Emails)
            {
                WebRequest request = HttpWebRequest.Create(url);
                request.ContentType = "application/json";
                request.Method = "POST";

                string body = string.Format("{{ \"client_id\": \"{0}\", \"email\": \"{1}\", \"connection\": \"{2}\" }}",
                    this.Auth0ClientId,
                    email,
                    this.Auth0Connection);

                using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(body);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                responses.Add(new EmailResponse(email, responseString));
            }

            this.WriteObject(responses);
        }

        public class EmailResponse
        {
            public EmailResponse(string email, string response)
            {
                this.Email = email;
                this.Response = response;
            }

            public string Email { get; set; }
            public string Response { get; set; }
        }
    }
}
