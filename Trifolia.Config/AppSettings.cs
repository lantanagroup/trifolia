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

        public static string HL7ApiKey
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7ApiKey"];
            }
        }

        public static string HL7SharedKey
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7SharedKey"];
            }
        }

        public static bool LogInformation
        {
            get
            {
                return GetBoolean("LogInformation");
            }
        }

        public static string CardinalityZeroToOne
        {
            get
            {
                return ConfigurationManager.AppSettings["CardinalityZeroToOne"];
            }
        }

        public static string CardinalityOneToOne
        {
            get
            {
                return ConfigurationManager.AppSettings["CardinalityOneToOne"];
            }
        }

        public static string CardinalityAtLeastOne
        {
            get
            {
                return ConfigurationManager.AppSettings["CardinalityAtLeastOne"];
            }
        }

        public static string CardinalityZeroOrMore
        {
            get
            {
                return ConfigurationManager.AppSettings["CardinalityZeroOrMore"];
            }
        }

        public static string CardinalityZero
        {
            get
            {
                return ConfigurationManager.AppSettings["CardinalityZero"];
            }
        }

        public static string IGTypeSchemaLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["IGTypeSchemaLocation"];
            }
        }

        public static string PhinVadsServiceUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["PhinVadsServiceUrl"];
            }
        }

        public static string DefaultJiraEndpoint
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultJiraEndpoint"];
            }
        }

        public static string DefaultJiraTaskType
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultJiraTaskType"];
            }
        }

        public static string DefaultJiraProject
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultJiraProject"];
            }
        }

        public static string DefaultJiraUsername
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultJiraUsername"];
            }
        }

        public static string DefaultJiraPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultJiraPassword"];
            }
        }

        public static string HL7LoginUrlFormat
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7LoginUrlFormat"];
            }
        }

        public static string HL7RoseTreeLocation
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7RoseTreeLocation"];
            }
        }

        public static string MailHost
        {
            get
            {
                return ConfigurationManager.AppSettings["MailHost"];
            }
        }

        public static string MailUser
        {
            get
            {
                return ConfigurationManager.AppSettings["MailUser"];
            }
        }

        public static string MailPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["MailPassword"];
            }
        }

        public static string MailFromAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["MailFromAddress"];
            }
        }

        public static int MailPort
        {
            get
            {
                return GetInteger("MailPort");
            }
        }

        public static bool MailEnableSSL
        {
            get
            {
                return GetBoolean("MailEnableSSL");
            }
        }

        public static string RecaptchaVerifyUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["RecaptchaVerifyUrl"];
            }
        }

        public static string RecaptchaVerifyMethod
        {
            get
            {
                return ConfigurationManager.AppSettings["RecaptchaVerifyMethod"];
            }
        }

        public static string RecaptchaSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["RecaptchaSecret"];
            }
        }

        public static string RecaptchaFormFieldName
        {
            get
            {
                return ConfigurationManager.AppSettings["RecaptchaFormFieldName"];
            }
        }

        public static bool RecaptchaAllowBypass
        {
            get
            {
                return GetBoolean("RecaptchaAllowBypass");
            }
        }

        public static string SupportEmailTo
        {
            get
            {
                return ConfigurationManager.AppSettings["SupportEmailTo"];
            }
        }

        public static string RedirectURL
        {
            get 
            {
                return ConfigurationManager.AppSettings["RedirectURL"];
            }
        }

        public static bool EnableJiraSupport
        {
            get
            {
                return GetBoolean("EnableJiraSupport");
            }
        }

        public static string HL7DisclaimerUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7DisclaimerUrl"];
            }
        }

        public static string HL7MemberRole
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7MemberRole"];
            }
        }

        public static string HL7OrganizationName
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7OrganizationName"];
            }
        }

        public static string HL7StaffRole
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7StaffRole"];
            }
        }

        public static string HL7BoardRole
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7BoardRole"];
            }
        }

        public static string HL7CoChairRole
        {
            get
            {
                return ConfigurationManager.AppSettings["HL7CoChairRole"];
            }
        }

        public static string SharedSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["SharedSecret"];
            }
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
            get
            {
                return ConfigurationManager.AppSettings["DefaultBaseUrl"];
            }
        }
    }
}
