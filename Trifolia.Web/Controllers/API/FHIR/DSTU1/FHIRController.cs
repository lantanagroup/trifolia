extern alias fhir_dstu1;

using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;

using fhir_dstu1.Hl7.Fhir.Model;
using fhir_dstu1.Hl7.Fhir.Serialization;

using Trifolia.Shared;
using Trifolia.DB;
using TDBOrganization = Trifolia.DB.Organization;
using Trifolia.Web.Formatters.FHIR.DSTU1;
using Trifolia.Authorization;
using Trifolia.Export.FHIR.DSTU1;
using Trifolia.Import.FHIR.DSTU1;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU1
{
    [DSTU1Config]
    public class FHIRController : ApiController
    {
        private const string TEMPLATE_AUTHOR_USERNAME = "admin";
        private const string FHIR_IG_TYPE_NAME = "FHIR DSTU1";
        private const string DEFAULT_IG_NAME = "FHIR Import";

        private IObjectRepository tdb;

        #region Constructors

        public FHIRController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public FHIRController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        [HttpGet, 
        Route("api/FHIR1/Profile"),
        Route("api/FHIR1/Profile/_search"),
        SecurableAction()]
        public HttpResponseMessage GetProfiles(string _format = "")
        {
            string fhirTemplatesExportString = string.Empty;
            ImplementationGuideType igType = GetFHIRIGType();
            User currentUser = CheckPoint.Instance.GetUser(this.tdb);
            var fhirTemplatesQuery = this.tdb.Templates.Where(y => y.ImplementationGuideTypeId == igType.Id);
            List<Template> fhirTemplates;

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                fhirTemplates = (from vtp in this.tdb.ViewTemplatePermissions
                                 join t in fhirTemplatesQuery on vtp.TemplateId equals t.Id
                                 where vtp.UserId == currentUser.Id
                                 select t)
                                 .ToList();
            }
            else
            {
                fhirTemplates = fhirTemplatesQuery.ToList();
            }

            try
            {
                fhirTemplatesExportString = FHIRExporter.GenerateExport(this.tdb, fhirTemplates, new IGSettingsManager(this.tdb));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate bundle export of FHIR templates: " + ex.Message, ex);
            }

            try
            {
                Bundle fhirTemplatesBundle = FhirParser.ParseBundleFromXml(fhirTemplatesExportString);
                return GetResponseMessage(_format, fhirTemplatesBundle);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse exported bundle for return: " + ex.Message, ex);
            }
        }

        [HttpGet,
        Route("api/FHIR1/Profile/{templateOid}"),
        SecurableAction()]
        public HttpResponseMessage GetProfile(string templateOid, string _format = "")
        {
            string fhirTemplatesExportString = string.Empty;
            ImplementationGuideType igType = GetFHIRIGType();
            User currentUser = CheckPoint.Instance.GetUser();
            int templateId = 0;

            Int32.TryParse(templateOid, out templateId);

            var fhirTemplatesQuery = this.tdb.Templates.Where(y => (y.Id == templateId || y.Oid == templateOid) && y.ImplementationGuideTypeId == igType.Id);
            List<Template> fhirTemplates;

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                fhirTemplates = (from vtp in this.tdb.ViewTemplatePermissions
                                 join t in fhirTemplatesQuery on vtp.TemplateId equals t.Id
                                 where vtp.UserId == currentUser.Id
                                 select t)
                                 .ToList();
            }
            else
            {
                fhirTemplates = fhirTemplatesQuery.ToList();
            }

            if (fhirTemplates.Count == 0)
                throw new Exception("Could not find specified FHIR profile/template.");

            try
            {
                fhirTemplatesExportString = FHIRExporter.GenerateExport(this.tdb, fhirTemplates, new IGSettingsManager(this.tdb));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate bundle export of FHIR templates: " + ex.Message, ex);
            }

            try
            {
                Bundle fhirTemplatesBundle = FhirParser.ParseBundleFromXml(fhirTemplatesExportString);
                return GetResponseMessage(_format, fhirTemplatesBundle);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse exported bundle for return: " + ex.Message, ex);
            }
        }

        [HttpPost,
        Route("api/FHIR1/Profile"),
        SecurableAction()]
        public HttpResponseMessage ImportNewProfiles(Bundle profiles)
        {
            return ImportProfile(profiles, false);
        }

        [HttpPut,
        Route("api/FHIR1/Profile"),
        SecurableAction()]
        public HttpResponseMessage ImportExistingProfiles(Bundle profiles)
        {
            // Authorization to individual templates/profiles is handled in the TemplateImporter class
            return ImportProfile(profiles, true);
        }

        private HttpResponseMessage ImportProfile(Bundle profiles, bool shouldUpdate)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlWriter writer = null;

                try
                {
                    writer = XmlWriter.Create(sw);
                    FhirSerializer.SerializeBundle(profiles, writer);
                }
                catch (Exception)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.ExpectationFailed);
                    response.Content = new StringContent("Failed to serialize request content");
                    return response;
                }

                writer.Flush();
                string bundleXml = sw.ToString();

                try
                {
                    string defaultImplementationGuideName = DEFAULT_IG_NAME;
                    ImplementationGuideType igType = this.tdb.ImplementationGuideTypes.Single(y => y.Name == FHIR_IG_TYPE_NAME);
                    ImplementationGuide defaultImplementationGuide = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Name.ToLower() == defaultImplementationGuideName.ToLower());
                    TDBOrganization defaultOrganization = this.tdb.Organizations.First();
                    User defaultAuthorUser = this.tdb.Users.Single(y => y.UserName == TEMPLATE_AUTHOR_USERNAME);

                    // Create a default implementation guide for imports if one doesn't already exist
                    if (defaultImplementationGuide == null)
                    {
                        defaultImplementationGuide = new ImplementationGuide()
                        {
                            Name = defaultImplementationGuideName,
                            Organization = defaultOrganization,
                            ImplementationGuideType = igType
                        };
                        this.tdb.ImplementationGuides.AddObject(defaultImplementationGuide);
                    }

                    FHIRImporter importer = new FHIRImporter(this.tdb, shouldUpdate);
                    importer.DefaultImplementationGuide = defaultImplementationGuide;
                    importer.DefaultAuthorUser = defaultAuthorUser;
                    importer.Import(bundleXml);

                    this.tdb.SaveChanges();
                }
                catch (AuthorizationException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    response.Content = new StringContent(ex.Message);
                    throw new HttpResponseException(response);
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private HttpResponseMessage GetResponseMessage(string format, object ret)
        {
            MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            var mediaTypes = Request.Headers.Accept.ToArray();

            if (string.IsNullOrEmpty(format) && mediaTypes.Count() == 1)
                format = mediaTypes[0].MediaType;

            switch (format)
            {
                case "application/json+fhir":
                    formatter = new JSONFHIRMediaTypeFormatter();
                    break;
                case "application/xml+fhir":
                    formatter = new XMLFHIRMediaTypeFormatter();
                    break;
                default:
                    format = "application/json";
                    break;
            }

            HttpResponseMessage message = new HttpResponseMessage();
            message.Content = new ObjectContent(ret.GetType(), ret, formatter, format);
            return message;
        }

        private ImplementationGuideType GetFHIRIGType()
        {
            ImplementationGuideType igType = this.tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name == FHIR_IG_TYPE_NAME);

            if (igType == null)
                throw new Exception("The FHIR schema has not yet been loaded on this server");

            return igType;
        }
    }
}
