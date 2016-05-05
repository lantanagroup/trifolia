using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Principal;
using System.Threading;
using System.Web.Security;
using System.Web;

using Trifolia.Shared;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Test
{
    internal class Helper
    {
        internal const string AUTH_INTERNAL = "LCG";
        internal const string AUTH_EXTERNAL = "HL7";

        public static string GetSampleContents(string location)
        {
            using (StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(location)))
            {
                return sr.ReadToEnd();
            }
        }

        public static byte[] GetSampleContentBytes(string location)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(location))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static void AuthLogin(IObjectRepository repo, string userName, string organizationName)
        {
            HttpRequest request = new HttpRequest("", "http://tempuri.org", "");
            HttpResponse response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(request, response);

            var principal = AuthLogin(repo, HttpContext.Current, userName, organizationName);

            Thread.CurrentPrincipal = principal;
            DBContext.Instance = repo;
        }

        public static GenericPrincipal AuthLogin(IObjectRepository repo, HttpContext context, string userName, string organizationName)
        {
            /*
            string userData = string.Format("Organization={0}", organizationName);
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, userName, DateTime.Now, DateTime.Now.AddHours(2), true, userData);
            string encAuthTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encAuthTicket);
            context.Request.Cookies.Add(faCookie);
            */

            TrifoliaApiIdentity identity = new TrifoliaApiIdentity(userName, organizationName);
            GenericPrincipal principal = new GenericPrincipal(identity, null);

            context.User = principal;

            return principal;
        }
    }
}
