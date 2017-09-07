using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Trifolia.Config;
using Trifolia.Logging;

namespace Trifolia.Shared
{
    public class UmlsHelper
    {
        private const string TGT_URL = "https://vsac.nlm.nih.gov/vsac/ws/Ticket";
        private const string TGT_BODY_FORMAT = "username={0}&password={1}";

        /// <summary>
        /// Authenticates the user with the VSAC using the credentials specified.
        /// </summary>
        /// <param name="username">The VSAC username</param>
        /// <param name="password">The VSAC password</param>
        /// <returns>True if authenticated, otherwise false.</returns>
        public static string Authenticate(string username, string password)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(TGT_URL);
            string body = string.Format(TGT_BODY_FORMAT, username, password);
            byte[] rawBody = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.ContentType = "text/plain";
            webRequest.ContentLength = rawBody.Length;

            using (var sw = webRequest.GetRequestStream())
            {
                sw.Write(rawBody, 0, rawBody.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (WebException wex)
            {
                Log.For(typeof(UmlsHelper)).Error("Error authenticating with UMLS", wex);
            }

            return null;
        }

        public static bool ValidateCredentials(string username, string password)
        {
            string ticketGrantingTicket = Authenticate(username, password);
            return !string.IsNullOrEmpty(ticketGrantingTicket);
        }

        public static bool ValidateLicense(string username, string password)
        {
            string licenseCode = AppSettings.UmlsLicenseCode;
            string[] query = new string[] {
                "user=" + Uri.EscapeDataString(username),
                "password=" + Uri.EscapeDataString(password),
                "licenseCode=" + Uri.EscapeDataString(licenseCode)
            };
            string url = AppSettings.UmlsValidateUrl + "?" + string.Join("&", query);
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "x-www-form-urlencoded";

            var response = (HttpWebResponse) webRequest.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                var responseContent = sr.ReadToEnd();
                bool isValid = false;

                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(responseContent);

                    if (doc.DocumentElement.Name != "Result" || !Boolean.TryParse(doc.DocumentElement.InnerText, out isValid))
                    {
                        Log.For(typeof(UmlsHelper)).Error("Unexpected response from UMLS validation service: {0}", responseContent);
                        return false;
                    }

                    return isValid;
                }
                catch (Exception ex)
                {
                    Log.For(typeof(UmlsHelper)).Error("Error validation UMLS license for {0}", ex, username);
                    return false;
                }
            }
        }
    }
}
