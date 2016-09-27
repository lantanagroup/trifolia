extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.Config;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Import.FHIR.STU3;
using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;

namespace Trifolia.Web.Controllers.API.FHIR.STU3
{
    [STU3Config]
    [RoutePrefix("api/FHIR3")]
    public class FHIR3StructureDefinitionController : TrifoliaApiController
    {
        private IObjectRepository tdb;
        private ImplementationGuideType implementationGuideType;

        #region Constructors

        public FHIR3StructureDefinitionController(IObjectRepository tdb, HttpRequestMessage request = null)
        {
            Log.For(this).Trace("Instantiating controller");

            this.tdb = tdb;

            // NOTE: This is for unit testing only
            if (request != null)
                this.Request = request;

            this.implementationGuideType = STU3Helper.GetImplementationGuideType(this.tdb, true);

            Log.For(this).Trace("Done instantiating controller");
        }

        public FHIR3StructureDefinitionController()
            : this(DBContext.Create())
        {

        }

        #endregion

        /// <summary>
        /// Gets a specific profile and converts it to a StructureDefinition resource.
        /// </summary>
        /// <param name="templateId">The id of the profile/template to get</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.StructureDefinition</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_LIST">Only users with the ability to list templates/profiles can execute this operation</permission>
        [HttpGet]
        [Route("StructureDefinition/{templateId}")]
        [SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public IHttpActionResult GetTemplate(
            [FromUri] string templateId,
            [FromUri(Name = "_summary")] SummaryType? summary = null)
        {
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Oid == this.Request.RequestUri.AbsoluteUri || y.Id.ToString() == templateId);

            // Return an operation outcome indicating that the profile was not found
            if (template == null)
            {
                OperationOutcome oo = new OperationOutcome();
                oo.Issue.Add(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Fatal,
                    Diagnostics = "Profile was not found"
                });

                return Content<OperationOutcome>(HttpStatusCode.NotFound, oo);
            }

            // Redirect the user to the Trifolia web interface if an acceptable format is text/html, and no format or summary was specified
            if (Request.Headers.Accept.Any(y => y.MediaType == "text/html") && summary == null)
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Location", Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/TemplateManagement/View/Id/" + template.Id);

                OperationOutcome redirectOutcome = new OperationOutcome();
                redirectOutcome.Issue.Add(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Information,
                    Diagnostics = "Detecting browser-based request, redirecting to Trifolia web interface."
                });

                return Content<OperationOutcome>(HttpStatusCode.Redirect, redirectOutcome, headers);
            }

            if (template.TemplateType.ImplementationGuideType != this.implementationGuideType)
                throw new FormatException(App_GlobalResources.TrifoliaLang.TemplateNotFHIRDSTU2);

            if (!CheckPoint.Instance.GrantViewTemplate(template.Id))
                throw new UnauthorizedAccessException();

            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(this.tdb, uri.Scheme, uri.Authority);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current, template.ImplementationGuideType);
            schema = schema.GetSchemaFromContext(template.PrimaryContextType);

            StructureDefinition strucDef = exporter.Convert(template, schema, summary);
            return Content<StructureDefinition>(HttpStatusCode.OK, strucDef);
        }

        /// <summary>
        /// Searches for profiles within Trifolia and returns them as StructureDefinition resources within a Bundle.
        /// </summary>
        /// <param name="templateId">The id of a template/profile to search for and return in the bundle</param>
        /// <param name="name">The name of a template/profile to search for. Only templates/profiles that contain this specified name will be returned.</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.Bundle</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_LIST">Only users with the ability to list templates/profiles can execute this operation</permission>
        [HttpGet]
        [Route("StructureDefinition")]
        [Route("StructureDefinition/_search")]
        [SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public IHttpActionResult GetTemplates(
            [FromUri(Name = "_id")] int? templateId = null,
            [FromUri(Name = "name")] string name = null,
            [FromUri(Name = "_summary")] SummaryType? summary = null)
        {
            Log.For(this).Trace("Begin GetTemplates");

            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            var templates = this.tdb.Templates.Where(y => y.TemplateType.ImplementationGuideTypeId == this.implementationGuideType.Id);
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(this.tdb, uri.Scheme, uri.Authority);

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                Log.For(this).Info("User is a data administrator");

                User currentUser = CheckPoint.Instance.GetUser(this.tdb);
                templates = (from t in templates
                             join vtp in this.tdb.ViewTemplatePermissions on t.Id equals vtp.TemplateId
                             where vtp.UserId == currentUser.Id
                             select t);
            }

            if (templateId != null)
                templates = templates.Where(y => y.Id == templateId);

            if (!string.IsNullOrEmpty(name))
                templates = templates.Where(y => y.Name.ToLower().Contains(name.ToLower()));

            Bundle bundle = new Bundle()
            {
                Type = Bundle.BundleType.BatchResponse
            };

            Log.For(this).Trace("Creating bundle with each template/profile found in search.");

            try
            {
                foreach (var template in templates)
                {
                    SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current, template.ImplementationGuideType);
                    schema = schema.GetSchemaFromContext(template.PrimaryContextType);

                    bool isMatch = true;
                    StructureDefinition strucDef = exporter.Convert(template, schema, summary);
                    var fullUrl = string.Format("{0}://{1}/api/FHIR3/StructureDefinition/{2}",
                        this.Request.RequestUri.Scheme,
                        this.Request.RequestUri.Authority,
                        template.Id);

                    // Skip adding the structure definition to the response if a criteria rules it out
                    foreach (var queryParam in this.Request.GetQueryNameValuePairs())
                    {
                        if (queryParam.Key == "_id" || queryParam.Key == "name" || queryParam.Key == "_format" || queryParam.Key == "_summary")
                            continue;

                        if (queryParam.Key.Contains("."))
                            throw new NotSupportedException(App_GlobalResources.TrifoliaLang.FHIRSearchCriteriaNotSupported);

                        var propertyDef = strucDef.GetType().GetProperty(queryParam.Key, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (propertyDef == null)
                            continue;

                        var value = propertyDef.GetValue(strucDef);
                        var valueString = value != null ? value.ToString() : string.Empty;

                        if (valueString != queryParam.Value)
                            isMatch = false;
                    }

                    if (isMatch)
                        bundle.AddResourceEntry(strucDef, fullUrl);
                }
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Error adding one or more templates to the bundle", ex);
                throw ex;
            }

            Log.For(this).Trace("Returning bundle with " + bundle.Entry.Count + " entries");

            return Content<Bundle>(HttpStatusCode.OK, bundle);
        }

        public Template CreateTemplate(StructureDefinition strucDef)
        {
            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            StructureDefinitionImporter importer = new StructureDefinitionImporter(this.tdb, uri.Scheme, uri.Authority);
            Template template = importer.Convert(strucDef);
            this.tdb.Templates.AddObject(template);
            return template;
        }

        /// <summary>
        /// Creates a template/profile from the specified StructureDefinition
        /// </summary>
        /// <returns>Hl7.Fhir.Model.StructureDefinition</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_EDIT">Only users with the ability to edit templates/profiles can execute this operation</permission>
        [HttpPost]
        [Route("StructureDefinition")]
        [SecurableAction(SecurableNames.TEMPLATE_EDIT)]
        public IHttpActionResult CreateStructureDefinition(
            [FromBody] StructureDefinition strucDef)
        {
            if (!string.IsNullOrEmpty(strucDef.Id))
            {
                OperationOutcome error = new OperationOutcome();
                error.Issue.Add(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Diagnostics = App_GlobalResources.TrifoliaLang.CreateFHIR2TemplateFailedDuplicateId
                });
                return Content<OperationOutcome>(HttpStatusCode.BadRequest, error);
            }

            var foundProfile = this.tdb.Templates.SingleOrDefault(y => y.Oid == strucDef.Url);

            if (foundProfile != null)
            {
                OperationOutcome oo = new OperationOutcome()
                {
                    Issue = new List<OperationOutcome.IssueComponent> {
                        new OperationOutcome.IssueComponent()
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.Duplicate,
                            Diagnostics = "A StructureDefinition with the same url already exists. To update, use PUT."
                        }
                    }
                };

                return Content<OperationOutcome>(HttpStatusCode.Created, oo);
            }

            var template = CreateTemplate(strucDef);
            this.tdb.SaveChanges();

            string location = string.Format("{0}://{1}/api/FHIR3/StructureDefinition/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    template.Id);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Location", location);

            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(this.tdb, uri.Scheme, uri.Authority);
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current, template.ImplementationGuideType);
            schema = schema.GetSchemaFromContext(template.PrimaryContextType);

            StructureDefinition newStrucDef = exporter.Convert(template, schema);
            return Content<StructureDefinition>(HttpStatusCode.Created, newStrucDef, headers);
        }

        /// <summary>
        /// Updates an existing template/profile based on the specified StructureDefinition
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="strucDef"></param>
        /// <returns>Hl7.Fhir.Model.StructureDefinition</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_EDIT">Only users with the ability to edit templates/profiles can execute this operation</permission>
        [HttpPut]
        [Route("StructureDefinition/{templateId}")]
        [SecurableAction(SecurableNames.TEMPLATE_EDIT)]
        public IHttpActionResult UpdateStructureDefinition(
            [FromBody] StructureDefinition strucDef,
            [FromUri] int templateId)
        {
            Template existingTemplate = this.tdb.Templates.SingleOrDefault(y => y.Id == templateId);

            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new UnauthorizedAccessException();

            var uri = HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url : new Uri(AppSettings.DefaultBaseUrl);
            StructureDefinitionImporter importer = new StructureDefinitionImporter(this.tdb, uri.Scheme, uri.Authority);
            StructureDefinitionExporter exporter = new StructureDefinitionExporter(this.tdb, uri.Scheme, uri.Authority);

            Template updatedTemplate = importer.Convert(strucDef, existingTemplate);

            if (existingTemplate == null)
                this.tdb.Templates.AddObject(updatedTemplate);

            this.tdb.SaveChanges();

            string location = string.Format("{0}://{1}/api/FHIR3/StructureDefinition/{2}",
                    this.Request.RequestUri.Scheme,
                    this.Request.RequestUri.Authority,
                    updatedTemplate.Id);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Location", location);

            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current, updatedTemplate.ImplementationGuideType);
            schema = schema.GetSchemaFromContext(updatedTemplate.PrimaryContextType);

            StructureDefinition updatedStrucDef = exporter.Convert(updatedTemplate, schema);
            return Content<Resource>((existingTemplate == null ? HttpStatusCode.Created : HttpStatusCode.OK), updatedStrucDef, headers);
        }

        /// <summary>
        /// Deletes the specified profile/template. This is a permanent deletion, and cannot be restored via _history.
        /// </summary>
        /// <param name="templateId">The id of the profile/template to delete</param>
        /// <returns>Hl7.Fhir.Model.OperationOutcome</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.TEMPLATE_DELETE">Only users with the ability to delete templates/profiles can execute this operation</permission>
        [HttpDelete]
        [Route("StructureDefinition/{templateId}")]
        [SecurableAction(SecurableNames.TEMPLATE_DELETE)]
        public IHttpActionResult DeleteStructureDefinition(
            [FromUri] int templateId)
        {
            Template template = null;
            OperationOutcome outcome = new OperationOutcome();
            outcome.Id = "ok";

            try
            {
                template = this.tdb.Templates.Single(y => y.Id == templateId);
            }
            catch
            {
                OperationOutcome error = new OperationOutcome();
                error.Issue.Add(new OperationOutcome.IssueComponent()
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Diagnostics = App_GlobalResources.TrifoliaLang.TemplateNotFoundMessageFormat
                });
                return Content<OperationOutcome>(HttpStatusCode.BadRequest, error);
            }

            if (!CheckPoint.Instance.GrantEditTemplate(templateId))
                throw new UnauthorizedAccessException();

            template.Delete(this.tdb, null);

            this.tdb.SaveChanges();

            return Content(HttpStatusCode.NoContent, outcome);
        }
    }
}
