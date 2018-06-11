using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using System.Diagnostics;
using Trifolia.Authorization;
using Trifolia.Config;
using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Web.Models;
using System.IO;
using Trifolia.DB;
using Newtonsoft.Json;

namespace Trifolia.Web.Controllers.API
{
    [RoutePrefix("api/Support")]
    public class SupportController : ApiController
    {
        private const string TRIFOLIA_SUPPORT_LABEL = "trifolia_support";
        private const string REMOTE_FIELD_LABELS = "labels";

        private IObjectRepository tdb;

        #region Construct/Dispose

        public SupportController() : this(DBContext.Create())
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public SupportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        [HttpPost]
        public string SubmitSupportRequest(SupportRequestModel model)
        {
            var supportType = GetPreferredSupportType();

            if (supportType == SupportTypes.JIRA)
                return SubmitJiraSupportRequest(model);
            else if (supportType == SupportTypes.EMAIL)
                return EmailSupportRequest(model);
            else
            {
                Log.For(this).Warn("User attempted to send support request via incorrect method (" + supportType.ToString() + ")");
                throw new Exception("Cannot submit support request (support method is invalid)");
            }
        }

        private string SubmitJiraSupportRequest(SupportRequestModel model)
        {
            var currentUser = CheckPoint.Instance.IsAuthenticated ? CheckPoint.Instance.GetUser(this.tdb) : null;
            string reporter = string.Empty;

            if (currentUser != null)
                reporter = currentUser.Email;
            else if (!string.IsNullOrEmpty(model.Email))
                reporter = model.Email;

            try
            {
                var priorityConfig = JiraSection.GetSection().Priorities[model.Priority];
                var typeConfig = JiraSection.GetSection().Types[model.Type];

                if (priorityConfig == null)
                    throw new Exception("Cannot find a configured JIRA priority mapping for " + model.Priority);

                if (typeConfig == null)
                    throw new Exception("Cannot find a configured JIRA type mapping for " + model.Type);

                string issueTypeId = typeConfig.Id;

                if (string.IsNullOrEmpty(issueTypeId))
                    issueTypeId = AppSettings.DefaultJiraTaskType;

                JIRAIssueData issueData = new JIRAIssueData();
                issueData.project = AppSettings.JiraProject;
                issueData.type = issueTypeId;
                issueData.summary = model.Summary;
                issueData.description = model.Details;
                issueData.reporter = reporter;
                issueData.priority = priorityConfig.Id;

                return this.SubmitJIRAIssue(issueData, TRIFOLIA_SUPPORT_LABEL);
            }
            catch (Exception submitException)
            {
                Log.For(this).Error("Failed to submit JIRA support request", submitException);
                throw new Exception("Could not submit JIRA support request. Please notify the administrator.");
            }
        }

        private string SubmitJIRAIssue(JIRAIssueData aIssue, string aIssueLabel)
        {
            bool validReporter = true;
            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(AppSettings.JiraUsername + ":" + AppSettings.JiraPassword));

            if (!string.IsNullOrEmpty(AppSettings.JiraUserEndpoint))
            {
                var userCheck = (HttpWebRequest)WebRequest.Create(AppSettings.JiraUserEndpoint + "?username=" + aIssue.reporter);
                userCheck.Method = "GET";
                userCheck.Headers.Add("Authorization", "Basic " + encoded);

                try
                {
                    var userCheckResponse = userCheck.GetResponse();

                    using (var streamReader = new StreamReader(userCheckResponse.GetResponseStream()))
                    {
                        var resultsJSON = streamReader.ReadToEnd();
                        var results = JsonConvert.DeserializeObject<List<JiraUserSearchResponse>>(resultsJSON);

                        if (results.Count != 1)
                        {
                            aIssue.description = string.Format("{0}\\n\\nSubmitted By: {1}", aIssue.description, aIssue.reporter);
                            validReporter = false;
                        }
                        else
                        {
                            aIssue.reporter = results[0].key;
                        }
                    }
                }
                catch (WebException wex)
                {
                    if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Log.For(this).Error("Request to JIRA returned Unauthorized");
                        throw new Exception("Error submitting support request to JIRA (System Configuration Error)");
                    }

                    aIssue.description = string.Format("{0}\\n\\nSubmitted By: {1}", aIssue.description, aIssue.reporter);
                    validReporter = false;
                }
            }
            else
            {
                aIssue.description = string.Format("{0}\\n\\nSubmitted By: {1}", aIssue.description, aIssue.reporter);
                validReporter = false;
            }

            var webRequest = (HttpWebRequest)WebRequest.Create(AppSettings.JiraIssueEndpoint);
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", "Basic " + encoded);

            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                JiraIssue jiraIssue = new JiraIssue();
                jiraIssue.fields.project.key = aIssue.project;
                jiraIssue.fields.summary = aIssue.summary;
                jiraIssue.fields.priority.id = aIssue.priority;
                jiraIssue.fields.description = aIssue.description;
                jiraIssue.fields.issuetype.id = aIssue.type;

                if (validReporter)
                    jiraIssue.fields.reporter.name = aIssue.reporter;

                if (!string.IsNullOrEmpty(AppSettings.JiraLabels))
                {
                    string[] labels = AppSettings.JiraLabels.Split(',');
                    jiraIssue.fields.labels.AddRange(labels);
                }

                string requestJSON = JsonConvert.SerializeObject(jiraIssue);
                streamWriter.Write(requestJSON);
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

        private string EmailSupportRequest(SupportRequestModel model)
        {
            if (string.IsNullOrEmpty(AppSettings.MailFromAddress))
                throw new Exception("MailFromAddress is not configured.");

            if (string.IsNullOrEmpty(AppSettings.SupportEmailTo))
                throw new Exception("SupportEmailTo is not configured");

            var client = new SmtpClient(AppSettings.MailHost, AppSettings.MailPort)
            {
                Credentials = new NetworkCredential(AppSettings.MailUser, AppSettings.MailPassword),
                Port = AppSettings.MailPort,
                EnableSsl = AppSettings.MailEnableSSL
            };

            var currentUser = CheckPoint.Instance.IsAuthenticated ? CheckPoint.Instance.GetUser(this.tdb) : null;
            string byName = currentUser != null ? string.Format("{0} {1}", currentUser.FirstName, currentUser.LastName) : model.Name;
            string byEmail = currentUser != null ? currentUser.Email : model.Email;

            string lBody = string.Format("Issue Type: {0}\nIssue Priority: {1}\nDetails: {2}\nSubmitted By: {3} ({4})",
                model.Type,
                model.Priority,
                model.Details,
                byName,
                byEmail);

            string lSubject = string.Format("Trifolia Support: {0}", model.Summary);

            try
            {
                string emailTo = !string.IsNullOrEmpty(AppSettings.DebugMailTo) ? AppSettings.DebugMailTo : AppSettings.SupportEmailTo;
                client.Send(
                    AppSettings.MailFromAddress,
                    emailTo,
                    lSubject,
                    lBody);

                return "Email sent";
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Error sending support request email", ex);
                throw ex;
            }
        }

        [HttpGet, Route("Config")]
        public dynamic SupportConfig()
        {
            SupportTypes supportType = GetPreferredSupportType();
            string method = "NONE";

            if (supportType == SupportTypes.JIRA || supportType == SupportTypes.EMAIL)
                method = "POPUP";
            else if (supportType == SupportTypes.URL)
                method = "URL";

            return new
            {
                Method = method,
                RedirectUrl = supportType == SupportTypes.URL ? AppSettings.RedirectURL : string.Empty
            };
        }

        private SupportTypes GetPreferredSupportType()
        {
            var authData = CheckPoint.Instance.GetAuthenticatedData();
            SupportTypes supportType = SupportTypes.DEFAULT;

            if (authData.ContainsKey(CheckPoint.AUTH_DATA_SUPPORT_METHOD) && !string.IsNullOrEmpty(authData[CheckPoint.AUTH_DATA_SUPPORT_METHOD]))
                Enum.TryParse<SupportTypes>(authData[CheckPoint.AUTH_DATA_SUPPORT_METHOD], out supportType);

            // If the user's account doesn't specify a support method, and Trifolia is configured with a default support method, use it
            if (supportType == SupportTypes.DEFAULT && !string.IsNullOrEmpty(AppSettings.DefaultSupportMethod))
                Enum.TryParse<SupportTypes>(AppSettings.DefaultSupportMethod, out supportType);

            // If the user is configured with a specific support type, make sure it is supported by config
            // Otherwise, choose a default support type based on config
            if (supportType != SupportTypes.DEFAULT)
            {
                if (supportType == SupportTypes.JIRA && !AppSettings.EnableJiraSupport)
                    supportType = SupportTypes.EMAIL;

                if (supportType == SupportTypes.EMAIL && string.IsNullOrEmpty(AppSettings.SupportEmailTo))
                    supportType = SupportTypes.URL;

                if (supportType == SupportTypes.URL && string.IsNullOrEmpty(AppSettings.RedirectURL))
                    supportType = SupportTypes.DEFAULT;
            }
            else
            {
                if (AppSettings.EnableJiraSupport)
                    supportType = SupportTypes.JIRA;
                else if (!string.IsNullOrEmpty(AppSettings.SupportEmailTo))
                    supportType = SupportTypes.EMAIL;
                else if (!string.IsNullOrEmpty(AppSettings.RedirectURL))
                    supportType = SupportTypes.URL;
            }

            return supportType;
        }
    }
}
