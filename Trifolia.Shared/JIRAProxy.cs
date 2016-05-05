using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void SubmitSupportTicket(string aUserName, string aSupportSummaryText, string aSupportDetailsText, string aSupportPriority, string aIssueType = null)
        {
            if (aIssueType == null)
                aIssueType = Properties.Settings.Default.DefaultJiraTaskType;

            RemoteIssue lRemoteIssue = new RemoteIssue();
            lRemoteIssue.project = Properties.Settings.Default.DefaultJiraProject;
            lRemoteIssue.type = aIssueType;
            lRemoteIssue.summary = aSupportSummaryText;
            lRemoteIssue.description = aSupportDetailsText;
            lRemoteIssue.created = DateTime.Now;
            lRemoteIssue.reporter = aUserName;
            lRemoteIssue.priority = aSupportPriority;

            this.SubmitJIRAIssue(lRemoteIssue, TRIFOLIA_SUPPORT_LABEL);
        }

        #endregion

        #region Private Methods

        private void SubmitJIRAIssue(RemoteIssue aRemoteIssue, string aIssueLabel)
        {
            using (JiraSoapServiceClient client = new JiraSoapServiceClient())
            {
                string jiraSession = client.login(Properties.Settings.Default.DefaultJiraUsername, Properties.Settings.Default.DefaultJiraPassword);
                RemoteUser reporter = client.getUser(jiraSession, aRemoteIssue.reporter);

                if (reporter == null)
                {
                    reporter = client.getUser(jiraSession, Properties.Settings.Default.DefaultJiraUsername);
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
            }
        }

        #endregion
    }
}