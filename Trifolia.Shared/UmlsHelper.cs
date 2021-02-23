using System;
using System.IO;
using System.Net;
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
            string tgt = GetTicketGrantingTicket(apiKey);
            if (string.IsNullOrEmpty(tgt)) return false;
            string serviceTicket = GetServiceTicket(tgt);
            return !string.IsNullOrEmpty(serviceTicket);
        }
    }
}
