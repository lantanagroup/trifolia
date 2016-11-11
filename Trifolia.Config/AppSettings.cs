using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Trifolia.Config
{
    public static class AppSettings
    {
        private static bool GetBoolean(string appSettingName)
        {
            var value = ConfigurationManager.AppSettings[appSettingName];
            var boolValue = false;
            Boolean.TryParse(value, out boolValue);
            return boolValue;
        }

        private static int GetInteger(string appSettingName)
        {
            var value = ConfigurationManager.AppSettings[appSettingName];
            int intValue;
            Int32.TryParse(value, out intValue);
            return intValue;
        }

        public static string DebugMailTo
        {
            get { return ConfigurationManager.AppSettings["DebugMailTo"]; }
        }

        public static string MailChimpReleaseAnnouncementList
        {
            get { return ConfigurationManager.AppSettings["MailChimpReleaseAnnouncementList"]; }
        }

        public static string MailChimpBaseUrl
        {
            get { return ConfigurationManager.AppSettings["MailChimpBaseUrl"]; }
        }

        public static string MailChimpApiKey
        {
            get { return ConfigurationManager.AppSettings["MailChimpApiKey"]; }
        }

        public static string LatestFhirIGPublisherLocation
        {
            get { return ConfigurationManager.AppSettings["LatestFhirIGPublisherLocation"]; }
        }

        public static string FhirIGPublisherDownload
        {
            get { return ConfigurationManager.AppSettings["FhirIGPublisherDownload"]; }
        }

        public static string FhirSTU3Package
        {
            get { return ConfigurationManager.AppSettings["FhirSTU3Package"]; }
        }

        public static string OpenIdConfigUrl
        {
            get { return ConfigurationManager.AppSettings["OpenIdConfigUrl"]; }
        }

        public static string OAuth2UserInfoEndpoint
        {
            get { return ConfigurationManager.AppSettings["OAuth2UserInfoEndpoint"]; }
        }

        public static string OAuth2AuthorizationEndpoint
        {
            get { return ConfigurationManager.AppSettings["OAuth2AuthorizationEndpoint"]; }
        }

        public static string OAuth2TokenEndpoint
        {
            get { return ConfigurationManager.AppSettings["OAuth2TokenEndpoint"]; }
        }

        public static string OAuth2ClientIdentifier
        {
            get { return ConfigurationManager.AppSettings["OAuth2ClientIdentifier"]; }
        }

        public static string OAuth2ClientSecret
        {
            get { return ConfigurationManager.AppSettings["OAuth2ClientSecret"]; }
        }

        public static bool LogInformation
        {
            get { return GetBoolean("LogInformation"); }
        }

        public static string CardinalityZeroToOne
        {
            get { return ConfigurationManager.AppSettings["CardinalityZeroToOne"]; }
        }

        public static string CardinalityOneToOne
        {
            get { return ConfigurationManager.AppSettings["CardinalityOneToOne"]; }
        }

        public static string CardinalityAtLeastOne
        {
            get { return ConfigurationManager.AppSettings["CardinalityAtLeastOne"]; }
        }

        public static string CardinalityZeroOrMore
        {
            get { return ConfigurationManager.AppSettings["CardinalityZeroOrMore"]; }
        }

        public static string CardinalityZero
        {
            get { return ConfigurationManager.AppSettings["CardinalityZero"]; }
        }

        public static string IGTypeSchemaLocation
        {
            get { return ConfigurationManager.AppSettings["IGTypeSchemaLocation"]; }
        }

        public static string PhinVadsServiceUrl
        {
            get { return ConfigurationManager.AppSettings["PhinVadsServiceUrl"]; }
        }

        public static string JiraLabels
        {
            get { return ConfigurationManager.AppSettings["JiraLabels"]; }
        }

        public static string JiraIssueEndpoint
        {
            get { return ConfigurationManager.AppSettings["JiraIssueEndpoint"]; }
        }

        public static string DefaultJiraTaskType
        {
            get { return ConfigurationManager.AppSettings["DefaultJiraTaskType"]; }
        }

        public static string JiraProject
        {
            get { return ConfigurationManager.AppSettings["JiraProject"]; }
        }

        public static string JiraUsername
        {
            get { return ConfigurationManager.AppSettings["JiraUsername"]; }
        }

        public static string JiraPassword
        {
            get { return ConfigurationManager.AppSettings["JiraPassword"]; }
        }

        public static string JiraUserEndpoint
        {
            get { return ConfigurationManager.AppSettings["JiraUserEndpoint"]; }
        }

        public static string HL7RoseTreeLocation
        {
            get { return ConfigurationManager.AppSettings["HL7RoseTreeLocation"]; }
        }

        public static string MailHost
        {
            get { return ConfigurationManager.AppSettings["MailHost"]; }
        }

        public static string MailUser
        {
            get { return ConfigurationManager.AppSettings["MailUser"]; }
        }

        public static string MailPassword
        {
            get { return ConfigurationManager.AppSettings["MailPassword"]; }
        }

        public static string MailFromAddress
        {
            get { return ConfigurationManager.AppSettings["MailFromAddress"]; }
        }

        public static int MailPort
        {
            get { return GetInteger("MailPort"); }
        }

        public static bool MailEnableSSL
        {
            get { return GetBoolean("MailEnableSSL"); }
        }

        public static string SupportEmailTo
        {
            get { return ConfigurationManager.AppSettings["SupportEmailTo"]; }
        }

        public static string RedirectURL
        {
            get { return ConfigurationManager.AppSettings["RedirectURL"]; }
        }

        public static bool EnableJiraSupport
        {
            get { return GetBoolean("EnableJiraSupport"); }
        }

        public static string SharedSecret
        {
            get { return ConfigurationManager.AppSettings["SharedSecret"]; }
        }

        public static string[] TrustedServers
        {
            get
            {
                var value = ConfigurationManager.AppSettings["TrustedServers"];

                if (string.IsNullOrEmpty(value))
                    return new string[] { };

                return value.Split(',');
            }
        }

        public static string DefaultBaseUrl
        {
            get { return ConfigurationManager.AppSettings["DefaultBaseUrl"]; }
        }

        public static string DatabaseConnectionString
        {
            get { return ConfigurationManager.AppSettings["DatabaseConnectionString"]; }
        }
    }
}
