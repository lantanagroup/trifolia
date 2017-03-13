extern alias fhir_dstu1;
extern alias fhir_dstu2;
extern alias fhir_stu3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Trifolia.Authentication.Models;
using Trifolia.DB;
using Trifolia.DB.Exceptions;
using Trifolia.Logging;
using FHIR1OperationOutcome = fhir_dstu1.Hl7.Fhir.Model.OperationOutcome;
using FHIR2OperationOutcome = fhir_dstu2.Hl7.Fhir.Model.OperationOutcome;
using FHIR3OperationOutcome = fhir_stu3.Hl7.Fhir.Model.OperationOutcome;

namespace Trifolia.Authorization
{
    public class SecurableActionAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        private const int ApiKeyTimeout = 60;
        private string[] securables;

        public SecurableActionAttribute(params string[] securables)
        {
            this.securables = securables;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!CheckPoint.Instance.IsAuthenticated)
            {
                if (!CheckPoint.Instance.IsAuthenticated)
                    throw new AuthorizationException("Only logged-in users have permission to perform this operation");

                if (!CheckPoint.Instance.HasSecurables(securables))
                    throw new AuthorizationException("You do not have the securable required to perform this operation");
            }

            base.OnActionExecuting(actionContext);
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, System.Threading.CancellationToken cancellationToken)
        {
            var req = context.Request;

            if (context.Principal.Identity.IsAuthenticated)
                return Task.FromResult(0);

            if (context.Request.Headers.Authorization != null && context.Request.Headers.Authorization.Scheme == "Bearer")
            {
                using (IObjectRepository tdb = DBContext.Create())
                {
                    var userInfo = OAuth2UserInfo.GetUserInfo(context.Request.Headers.Authorization.Parameter);
                    var foundUser = tdb.Users.SingleOrDefault(y => y.UserName == userInfo.user_id);

                    if (foundUser != null)
                    {
                        var identity = new TrifoliaApiIdentity(foundUser.UserName);
                        var currentPrincipal = new GenericPrincipal(identity, null);
                        context.Principal = currentPrincipal;
                        Thread.CurrentPrincipal = currentPrincipal;
                    }
                }
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, System.Threading.CancellationToken cancellationToken)
        {
            ResultWithChallenge.ResponseType responseType = ResultWithChallenge.ResponseType.Default;
            string absolutePath = context.Request.RequestUri.AbsolutePath;
            
            if (absolutePath.StartsWith("/api/FHIR3"))
                responseType = ResultWithChallenge.ResponseType.FHIR3;
            else if (absolutePath.StartsWith("/api/FHIR2"))
                responseType = ResultWithChallenge.ResponseType.FHIR2;
            else if (absolutePath.StartsWith("/api/FHIR"))
                responseType = ResultWithChallenge.ResponseType.FHIR1;

            context.Result = new ResultWithChallenge(context.Result, responseType);
            return Task.FromResult(0);
        }
    }

    public class ResultWithChallenge : IHttpActionResult
    {
        private readonly string authenticationScheme = "Bearer";
        private readonly IHttpActionResult next;
        private ResponseType responseType;

        public ResultWithChallenge(IHttpActionResult next, ResponseType responseType)
        {
            this.next = next;
            this.responseType = responseType;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage errorMessage = new HttpResponseMessage();
            string errorString = string.Empty;
            string errorStackTrace = string.Empty;

            try
            {
                var response = await next.ExecuteAsync(cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(authenticationScheme));
                }

                return response;
            }
            catch (AuthorizationException ex)
            {
                Log.For(next).Error(ex.Message, ex);
                errorMessage.StatusCode = HttpStatusCode.Unauthorized;
                errorMessage.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(authenticationScheme));
                errorString = ex.Message;
                errorStackTrace = ex.StackTrace;
            }
            catch (TrifoliaModelException ex)
            {
                errorMessage.StatusCode = HttpStatusCode.BadRequest;
                errorString = ex.Message;
            }
            catch (Exception ex)
            {
                Log.For(next).Error(ex.Message, ex);
                errorMessage.StatusCode = HttpStatusCode.InternalServerError;
            }

            List<string> locations = new List<string>();

            if (!string.IsNullOrEmpty(errorStackTrace))
                locations.Add(errorStackTrace);

            switch (this.responseType)
            {
                case ResponseType.FHIR1:
                    FHIR1OperationOutcome oo1 = new FHIR1OperationOutcome();

                    oo1.Issue.Add(new FHIR1OperationOutcome.OperationOutcomeIssueComponent()
                    {
                        Severity = FHIR1OperationOutcome.IssueSeverity.Fatal,
                        Details = errorString,
                        Location = locations
                    });

                    errorMessage.Content = new StringContent(fhir_dstu1.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToJson(oo1));
                    break;
                case ResponseType.FHIR2:
                    FHIR2OperationOutcome oo2 = new FHIR2OperationOutcome();

                    oo2.Issue.Add(new FHIR2OperationOutcome.OperationOutcomeIssueComponent()
                    {
                        Severity = FHIR2OperationOutcome.IssueSeverity.Fatal,
                        Diagnostics = errorString,
                        Code = FHIR2OperationOutcome.IssueType.Exception,
                        Location = locations
                    });

                    errorMessage.Content = new StringContent(fhir_dstu2.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToJson(oo2));
                    break;
                case ResponseType.FHIR3:
                    FHIR3OperationOutcome oo3 = new FHIR3OperationOutcome();

                    oo3.Issue.Add(new FHIR3OperationOutcome.IssueComponent()
                    {
                        Severity = FHIR3OperationOutcome.IssueSeverity.Fatal,
                        Diagnostics = errorString,
                        Code = FHIR3OperationOutcome.IssueType.Exception,
                        Location = locations
                    });
                    
                    errorMessage.Content = new StringContent(fhir_stu3.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToJson(oo3));
                    break;
                default:
                    errorMessage.Content = new StringContent(errorString);
                    break;
            }

            return errorMessage;
        }

        public enum ResponseType
        {
            Default,
            FHIR1,
            FHIR2,
            FHIR3
        }
    }

    [Serializable]
    public class TrifoliaApiIdentity : MarshalByRefObject, IIdentity
    {
        private string userName;

        public TrifoliaApiIdentity()
        {

        }

        public TrifoliaApiIdentity(string userName)
        {
            this.userName = userName;
        }

        public string AuthenticationType
        {
            get { return "Bearer"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string Name
        {
            get { return this.userName; }
        }
    }
}
