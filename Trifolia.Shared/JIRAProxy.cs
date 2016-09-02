using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Trifolia.Config;

namespace Trifolia.Shared
{
    public class JIRAProxy
    {
        #region Private Constants

        private const string TRIFOLIA_SUPPORT_LABEL = "trifolia_support";
        private const string TRIFOLIA_BETA_LABEL    = "trifolia_beta";
        private const string REMOTE_FIELD_LABELS    = "labels";

        #endregion

        #region Public Methods

        /// <summary>
        /// Submits a support ticket using default JIRA settings
        /// </summary>
        /// <param name="aUserName"></param>
        /// <param name="aSupportSummaryText"></param>
        /// <param name="aSupportDetailsText"></param>
        /// <param name="aSupportPriority"></param>
        public string SubmitSupportTicket(string aUserName, string aSupportSummaryText, string aSupportDetailsText, string aSupportPriority, string aIssueType = null)
        {
            if (aIssueType == null)
                aIssueType = AppSettings.DefaultJiraTaskType;

            JIRAIssueData issueData = new JIRAIssueData();
            issueData.project = AppSettings.DefaultJiraProject;
            issueData.type = aIssueType;
            issueData.summary = aSupportSummaryText;
            issueData.description = aSupportDetailsText;
            issueData.reporter = aUserName;
            issueData.priority = aSupportPriority;

            return this.SubmitJIRAIssue(issueData, TRIFOLIA_SUPPORT_LABEL);
        }

        #endregion

        #region Private Methods

        private string SubmitJIRAIssue(JIRAIssueData aIssue, string aIssueLabel)
        {
            bool validReporter = true;
            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(AppSettings.DefaultJiraUsername + ":" + AppSettings.DefaultJiraPassword));

            var userCheck = (HttpWebRequest)WebRequest.Create("https://jira.lantanagroup.com/rest/api/2/user" + "?username=" + aIssue.reporter);
            userCheck.Method = "GET";
            userCheck.Headers.Add("Authorization", "Basic " + encoded);

            try
            {
                var userCheckResponse = userCheck.GetResponse();

                using (var streamReader = new StreamReader(userCheckResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                aIssue.description = string.Format("{0}\\n\\nSubmitted By: {1}", aIssue.description, aIssue.reporter);
                validReporter = false;
            }

            var webRequest = (HttpWebRequest)WebRequest.Create("https://jira.lantanagroup.com/rest/api/2/issue");
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", "Basic " + encoded);
             
            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                string jsonIssue = "{" +
                                        "\"fields\":{" +
                                            "\"project\":{" +
                                                "\"key\":\"" + aIssue.project + "\"" +
                                            "}," +
                                            "\"summary\":\"" + aIssue.summary + "\"," +
                                            "\"priority\":{" +
                                                "\"id\":\"" + aIssue.priority + "\"" +
                                            "}," +
                                            "\"description\":\"" + aIssue.description + "\"," +
                                            "\"issuetype\":{" +
                                                "\"id\":\"" + aIssue.type + "\"" +
                                            "}";
                if (validReporter) jsonIssue += "," +
                                             "\"reporter\":{" +
                                                "\"name\":\"" + aIssue.reporter + "\"" +
                                             "}";
                jsonIssue += "}}";
                streamWriter.Write(jsonIssue);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var response = webRequest.GetResponse();

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var key = result.Substring(result.IndexOf("\"key\":") + 7, result.IndexOf(",\"self\":") - result.IndexOf("\"key\":") - 8);
                return key;
            }
        }

        #endregion
    }

    public class JIRAIssueData
    {
        public string project;
        public string type;
        public string summary;
        public string description;
        public string reporter;
        public string priority;
    }
}