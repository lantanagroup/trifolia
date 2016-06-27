using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            RemoteIssue lRemoteIssue = new RemoteIssue();
            lRemoteIssue.project = AppSettings.DefaultJiraProject;
            lRemoteIssue.type = aIssueType;
            lRemoteIssue.summary = aSupportSummaryText;
            lRemoteIssue.description = aSupportDetailsText;
            lRemoteIssue.created = DateTime.Now;
            lRemoteIssue.reporter = aUserName;
            lRemoteIssue.priority = aSupportPriority;

            return this.SubmitJIRAIssue(lRemoteIssue, TRIFOLIA_SUPPORT_LABEL);
        }

        #endregion

        #region Private Methods

        private string SubmitJIRAIssue(RemoteIssue aRemoteIssue, string aIssueLabel)
        {
            using (JiraSoapServiceClient client = new JiraSoapServiceClient())
            {
                string jiraSession = client.login(AppSettings.DefaultJiraUsername, AppSettings.DefaultJiraPassword);
                RemoteUser reporter = client.getUser(jiraSession, aRemoteIssue.reporter);

                if (reporter == null)
                {
                    reporter = client.getUser(jiraSession, AppSettings.DefaultJiraUsername);
                    aRemoteIssue.description = string.Format("{0}\n\nSubmitted By: {1}", aRemoteIssue.description, aRemoteIssue.reporter);
                    aRemoteIssue.reporter = reporter.name;
                }

                // Define additional fields to set
                List<RemoteFieldValue> actionParams = new List<RemoteFieldValue>();

                RemoteFieldValue labels
                    = new RemoteFieldValue { id = REMOTE_FIELD_LABELS, values = new string[] { aIssueLabel } };

                actionParams.Add(labels);

                RemoteIssue lNewIssue = client.createIssue(jiraSession, aRemoteIssue);
                client.updateIssue(jiraSession, lNewIssue.key, actionParams.ToArray());

                return lNewIssue.key;
            }
        }

        #endregion
    }
}