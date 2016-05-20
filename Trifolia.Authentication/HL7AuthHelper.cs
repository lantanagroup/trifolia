using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web;

namespace Trifolia.Authentication
{
    public class HL7AuthHelper
    {
        public const string API_KEY = ***REMOVED***;
        public const string SHARED_KEY = ***REMOVED***;

        public static string GetEncrypted(string input, string key)
        {
            HMACSHA1 enc = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(key));

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(input);
            return enc.ComputeHash(bytes).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s).ToUpper();
        }

        public static bool ValidateTimestamp(int requestTimestamp)
        {
            DateTime nowRange = DateTime.UtcNow.AddMinutes(-5);
            TimeSpan t = (nowRange - new DateTime(1970, 1, 1));
            int timestamp = (int)t.TotalSeconds;

            return requestTimestamp > timestamp;
        }

        public static int GetTimestamp()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            int timestamp = (int)t.TotalSeconds;

            return timestamp;
        }

        public static string GetComplianceUrl(string destination, string username, string description)
        {
            int timestamp = GetTimestamp();
            string requestHash = string.Format("{0}|{1}|{2}|{3}",
                username,
                destination,
                timestamp,
                API_KEY);
            string requestHashEncrypted = GetEncrypted(requestHash, SHARED_KEY);

            return string.Format(
                "{0}?userid={1}&returnURL={2}&signingURL={2}&signingDescription={3}&requestHash={4}&timestampUTCEpoch={5}&apiKey={6}",
                System.Configuration.ConfigurationManager.AppSettings["HL7DisclaimerUrl"],
                username,
                HttpUtility.UrlEncode(destination),
                HttpUtility.UrlEncode(description),
                requestHashEncrypted,
                timestamp,
                API_KEY);
        }
    }
}
