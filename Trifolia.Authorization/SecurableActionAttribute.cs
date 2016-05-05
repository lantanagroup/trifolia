extern alias fhir_dstu1;
extern alias fhir_dstu2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Net.Http.Headers;

using Trifolia.DB;
using System.Web.Http.Results;
using System.Security.Principal;

using FHIR1OperationOutcome = fhir_dstu1.Hl7.Fhir.Model.OperationOutcome;
using FHIR2OperationOutcome = fhir_dstu2.Hl7.Fhir.Model.OperationOutcome;

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
            if (!CheckPoint.Instance.IsAuthenticated && !CheckPoint.Instance.IsTrustedSharedSecret)
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
                using (IObjectRepository tdb = new TemplateDatabaseDataSource())
                {
                    var authorizationDataBytes = System.Convert.FromBase64String(context.Request.Headers.Authorization.Parameter);
                    var authorizationData = System.Text.Encoding.UTF8.GetString(authorizationDataBytes);
                    string[] authSplit = authorizationData.Split('|');

                    if (authSplit.Length != 5)
                    {
                        context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                        return Task.FromResult(0);
                    }

                    string userName = authSplit[0];
                    string organizationName = authSplit[1];
                    User user = tdb.Users.SingleOrDefault(y => y.UserName == userName && y.Organization.Name == organizationName);
                    long timestamp = 0;

                    long.TryParse(authSplit[2], out timestamp);
                    var timestampDate = new DateTime(1970, 1, 1).AddMilliseconds(timestamp);
                    var salt = authSplit[3];
                    var requestHashBytes = System.Convert.FromBase64String(authSplit[4]);
                    var requestHash = System.Text.Encoding.UTF8.GetString(requestHashBytes);

                    if (user == null || timestampDate > DateTime.UtcNow.AddMinutes(ApiKeyTimeout) || timestampDate < DateTime.UtcNow.AddMinutes(ApiKeyTimeout * -1))
                    {
                        context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                        return Task.FromResult(0);
                    }

                    var cryptoProvider = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                    var actualHashData = user.UserName + "|" + user.Organization.Name + "|" + timestamp + "|" + salt + "|" + user.ApiKey;
                    var actualHashDataBytes = System.Text.Encoding.UTF8.GetBytes(actualHashData);
                    var actualHashBytes = cryptoProvider.ComputeHash(actualHashDataBytes);
                    var actualHash = System.Text.Encoding.UTF8.GetString(actualHashBytes);

                    if (actualHash != requestHash)
                    {
                        context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                        return Task.FromResult(0);
                    }

                    var identity = new TrifoliaApiIdentity(user.UserName, user.Organization.Name);
                    var currentPrincipal = new GenericPrincipal(identity, null);
                    context.Principal = currentPrincipal;
                    Thread.CurrentPrincipal = currentPrincipal;
                }
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, System.Threading.CancellationToken cancellationToken)
        {
            ResultWithChallenge.ResponseType responseType = ResultWithChallenge.ResponseType.Default;

            if (context.Request.RequestUri.AbsolutePath.StartsWith("/api/FHIR2"))
                responseType = ResultWithChallenge.ResponseType.FHIR2;
            else if (context.Request.RequestUri.AbsolutePath.StartsWith("/api/FHIR"))
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
                errorMessage.StatusCode = HttpStatusCode.Unauthorized;
                errorMessage.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(authenticationScheme));
                errorString = ex.Message;
                errorStackTrace = ex.StackTrace;
            }
            catch (Exception ex)
            {
                errorMessage.StatusCode = HttpStatusCode.InternalServerError;
                errorString = ex.Message;
                errorStackTrace = ex.StackTrace;
            }

            switch (this.responseType)
            {
                case ResponseType.FHIR1:
                    FHIR1OperationOutcome oo1 = new FHIR1OperationOutcome();
                    List<string> locations1 = new List<string>();

                    if (!string.IsNullOrEmpty(errorStackTrace))
                        locations1.Add(errorStackTrace);

                    oo1.Issue.Add(new FHIR1OperationOutcome.OperationOutcomeIssueComponent()
                    {
                        Severity = FHIR1OperationOutcome.IssueSeverity.Fatal,
                        Details = errorString,
                        Location = locations1
                    });

                    errorMessage.Content = new StringContent(fhir_dstu1.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToJson(oo1));
                    break;
                case ResponseType.FHIR2:
                    FHIR2OperationOutcome oo2 = new FHIR2OperationOutcome();
                    List<string> locations2 = new List<string>();

                    if (!string.IsNullOrEmpty(errorStackTrace))
                        locations2.Add(errorStackTrace);

                    oo2.Issue.Add(new FHIR2OperationOutcome.OperationOutcomeIssueComponent()
                    {
                        Severity = FHIR2OperationOutcome.IssueSeverity.Fatal,
                        Diagnostics = errorString,
                        Code = FHIR2OperationOutcome.IssueType.Exception,
                        Location = locations2
                    });

                    errorMessage.Content = new StringContent(fhir_dstu2.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToJson(oo2));
                    break;
            }

            return errorMessage;
        }

        public enum ResponseType
        {
            Default,
            FHIR1,
            FHIR2
        }
    }

    [Serializable]
    public class TrifoliaApiIdentity : MarshalByRefObject, IIdentity
    {
        private string userName;
        private string organization;

        public TrifoliaApiIdentity()
        {

        }

        public TrifoliaApiIdentity(string userName, string organization)
        {
            this.userName = userName;
            this.organization = organization;
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

        public string OrganizationName
        {
            get { return this.organization; }
        }
    }
}