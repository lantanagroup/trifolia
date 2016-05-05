using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Trifolia.Shared;
using Trifolia.Logging;
using Trifolia.Authorization;

namespace Trifolia.Web.Controllers
{
    //[Securable]
    public class SupportController : Controller
    {
        #region Issue Priority Map

        Dictionary<string, string> priorityMap = new Dictionary<string, string>(){
            {"None", "6"},
            {"Blocker", "1"},
            {"Critical", "2"},
            {"Major", "3"}, 
            {"Minor", "4"}, 
            {"Trivial", "5"},
            {"Medium", "7"}
        };

        Dictionary<string, string> _issueMap = new Dictionary<string, string>()
        {
            {"Question", "7"},
            {"Bug/Defect", "8"},
            {"Feature Request", "10"}
        };

        #endregion

        //
        // GET: /Support/

        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        public void SubmitSupportRequest(string SupportName, string SupportEmail, string SupportSummary, string SupportType,
            string SupportPriority, string SupportDetails)
        {
            if (CheckPoint.Instance.OrganizationName == "HL7" || !Properties.Settings.Default.EnableJiraSupport)
            {
                string lSmtpServer = Properties.Settings.Default.MailHost;

                var client = new SmtpClient(lSmtpServer, 587)
                {
                    Credentials = new NetworkCredential(Properties.Settings.Default.MailUser, Properties.Settings.Default.MailPassword),
                    Port = Properties.Settings.Default.MailPort,
                    EnableSsl = Properties.Settings.Default.MailEnableSSL
                };

                string lBody = string.Format("Issue Type: {0}\nIssue Priority: {1}\nDetails: {2}\nSubmitted By: {3} ({4})", 
                    SupportType, 
                    SupportPriority, 
                    SupportDetails, 
                    CheckPoint.Instance.UserFullName,
                    CheckPoint.Instance.UserName);

                string lSubject = string.Format("Trifolia Support: {0}", SupportSummary);

                client.Send(
                    ConfigurationManager.AppSettings[Properties.Settings.Default.MailFromAddress], 
                    ConfigurationManager.AppSettings[Properties.Settings.Default.SupportEmailTo], 
                    lSubject, 
                    lBody);
            }
            else
            {
                JIRAProxy lProxy = new JIRAProxy();
                string lUserName = string.Empty;

                if (this.User.Identity.IsAuthenticated)
                {
                    lUserName = this.User.Identity.Name;
                }
                else
                {
                    lUserName = SupportName + "; " + SupportEmail;
                }

                try
                {
                    lProxy.SubmitSupportTicket(lUserName, SupportSummary, SupportDetails, priorityMap[SupportPriority], _issueMap[SupportType]);
                }
                catch (Exception submitException)
                {
                    Log.For(this).Error("Failed to submit JIRA beta user application", submitException);
                    throw new Exception("Could not submit beta user application.  Please notify the administrator");
                }
            }
        }
    }
}