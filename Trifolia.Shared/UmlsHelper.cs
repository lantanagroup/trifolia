using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Trifolia.Config;

namespace Trifolia.Shared
{
    public class UmlsHelper
    {
        public static string GetTicketGrantingTicket(string apiKey)
        {
            string url = AppSettings.UMLSTicketGrantingTicketURL;
            HttpWebRequest tgtRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            tgtRequest.Method = "POST";
            tgtRequest.ContentType = "application/x-www-form-urlencoded";
            tgtRequest.Accept = "application/xml";

            try
            {
                using (StreamWriter sw = new StreamWriter(tgtRequest.GetRequestStream()))
                {
                    sw.Write("apikey=" + apiKey);
                }

                HttpWebResponse tgtResponse = (HttpWebResponse)tgtRequest.GetResponse();

                if (tgtResponse.StatusCode != HttpStatusCode.Created)
                    return null;

                string location = tgtResponse.GetResponseHeader("Location");

                if (string.IsNullOrEmpty(location) || location.IndexOf("TGT") < 0)
                    return null;

                return location.Substring(location.IndexOf("TGT"));
            }
            catch
            {
                return null;
            }
        }

        public static string GetServiceTicket(string tgt)
        {
            HttpWebRequest serviceTicketRequest = (HttpWebRequest) HttpWebRequest.Create("https://utslogin.nlm.nih.gov/cas/v1/tickets/" + tgt);
            serviceTicketRequest.Method = "POST";
            serviceTicketRequest.ContentType = "application/x-www-form-urlencoded";

            try
            {
                using (StreamWriter sw = new StreamWriter(serviceTicketRequest.GetRequestStream()))
                {
                    sw.Write("service=http://umlsks.nlm.nih.gov");
                }

                HttpWebResponse stResponse = (HttpWebResponse)serviceTicketRequest.GetResponse();

                using (StreamReader sr = new StreamReader(stResponse.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool ValidateLicense(string apiKey)
        {
            HttpClient client = new HttpClient();
            String authenticationString = $"apikey:{apiKey}";
            String base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://cts.nlm.nih.gov/fhir/ValueSet/2.16.840.1.113762.1.4.1096.82");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);

            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                String responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logging.Log.For(typeof(UmlsHelper)).Error(String.Format("API Key is not valid: {0}", apiKey));
                    return false;
                }

                Logging.Log.For(typeof(UmlsHelper)).Info(String.Format("Validated API Key {0}", apiKey));
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log.For(typeof(UmlsHelper)).Error(String.Format("Error validating API Key: {0}", ex.Message));
                return false;
            }
        }
    }
}
