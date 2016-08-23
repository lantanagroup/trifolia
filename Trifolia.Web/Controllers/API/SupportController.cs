using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.Config;
using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Web.Models;

namespace Trifolia.Web.Controllers.API
{
    public class SupportController : ApiController
    {
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

        [HttpPost, Route("api/Support")]
        public string SubmitSupportRequest(SupportRequestModel model)
        {
            if (CheckPoint.Instance.OrganizationName == "HL7" || !AppSettings.EnableJiraSupport)
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

                string lBody = string.Format("Issue Type: {0}\nIssue Priority: {1}\nDetails: {2}\nSubmitted By: {3} ({4})",
                    model.Type,
                    model.Priority,
                    model.Details,
                    CheckPoint.Instance.UserFullName,
                    CheckPoint.Instance.UserName);

                string lSubject = string.Format("Trifolia Support: {0}", model.Summary);

                try
                {
                    client.Send(
                        AppSettings.MailFromAddress,
                        AppSettings.SupportEmailTo,
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
                    lUserName = model.Name + "; " + model.Email;
                }

                try
                {
                    return lProxy.SubmitSupportTicket(lUserName, model.Summary, model.Details, priorityMap[model.Priority], _issueMap[model.Type]);
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