extern alias fhir_dstu2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Shared;

using fhir_dstu2.Hl7.Fhir.Model;
using FhirImplementationGuide = fhir_dstu2.Hl7.Fhir.Model.ImplementationGuide;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;

namespace Trifolia.Web.Controllers.API.FHIR.DSTU2
{
    [DSTU2Config]
    public class FHIR2ImplementationGuideController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public FHIR2ImplementationGuideController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public FHIR2ImplementationGuideController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        private FhirImplementationGuide Convert(ImplementationGuide ig, SummaryType? summaryType = null)
        {
            var fhirImplementationGuide = new FhirImplementationGuide()
            {
                Id = ig.Id.ToString(),
                Name = ig.Name,
                Url = string.Format("ImplementationGuide/{0}", ig.Id)
            };

            // Status
            if (ig.PublishStatus == PublishStatus.GetPublishedStatus(this.tdb))
                fhirImplementationGuide.Status = ConformanceResourceStatus.Active;
            else if (ig.PublishStatus == PublishStatus.GetRetiredStatus(this.tdb) || ig.PublishStatus == PublishStatus.GetDeprecatedStatus(this.tdb))
                fhirImplementationGuide.Status = ConformanceResourceStatus.Retired;
            else
                fhirImplementationGuide.Status = ConformanceResourceStatus.Draft;

            // Package
            FhirImplementationGuide.ImplementationGuidePackageComponent package = new FhirImplementationGuide.ImplementationGuidePackageComponent();
            package.Name = "Profiles in this Implementation Guide";
            fhirImplementationGuide.Package.Add(package);

            if (summaryType == null || summaryType == SummaryType.Data)
            {
                List<Template> templates = ig.GetRecursiveTemplates(this.tdb, inferred: false);
                var packageResources = (from t in templates
                                        select new FhirImplementationGuide.ImplementationGuidePackageResourceComponent()
                                        {
                                            Purpose = FhirImplementationGuide.GuideResourcePurpose.Profile,
                                            Source = new ResourceReference()
                                            {
                                                Reference = string.Format("StructureDefinition/{0}", t.Id),
                                                Display = t.Name
                                            }
                                        });

                package.Resource.AddRange(packageResources);
            }

            // Page
            fhirImplementationGuide.Page = new FhirImplementationGuide.ImplementationGuidePageComponent();
            fhirImplementationGuide.Page.Kind = FhirImplementationGuide.GuidePageKind.Page;
            fhirImplementationGuide.Page.Name = ig.GetDisplayName();
            fhirImplementationGuide.Page.Source = string.Format("{0}://{1}/IG/View/{2}", this.Request.RequestUri.Scheme, this.Request.RequestUri.Authority, ig.Id);

            return fhirImplementationGuide;
        }

        private ImplementationGuide Convert(FhirImplementationGuide fhirImplementationGuide, ImplementationGuide implementationGuide)
        {
            if (implementationGuide == null)
                implementationGuide = new ImplementationGuide()
                {
                    ImplementationGuideType = this.tdb.ImplementationGuideTypes.Single(y => y.Name == ImplementationGuideType.FHIR_DSTU2_NAME)
                };

            if (implementationGuide.Name != fhirImplementationGuide.Name)
                implementationGuide.Name = fhirImplementationGuide.Name;

            if (fhirImplementationGuide.Package != null)
            {
                foreach (var package in fhirImplementationGuide.Package)
                {
                    foreach (var resource in package.Resource)
                    {
                        if (resource.Source is ResourceReference)
                        {
                            
                        }
                        else
                        {
                            throw new Exception("Only resource references are supported by ImplementationGuide.resource");
                        }
                    }
                }
            }

            return implementationGuide;
        }

        /// <summary>
        /// Get the specified implementation guide in FHIR DSTU2 format
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <param name="format">Optional. The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <returns>Hl7.Fhir.Model.ImplementationGuide</returns>
        [HttpGet]
        [Route("api/FHIR2/ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public Resource GetImplementationGuide(
            int implementationGuideId,
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] SummaryType? summary = null)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            FhirImplementationGuide response = Convert(implementationGuide, summary);
            return response;
        }

        public Bundle GetImplementationGuides(SummaryType? summary = null, string include = null, int? implementationGuideId = null, string name = null)
        {
            // TODO: Should not be using constant string for IG type name to find templates... Not sure how else to identify FHIR DSTU1 templates though
            var implementationGuides = this.tdb.ImplementationGuides.Where(y => y.ImplementationGuideType.Name == ImplementationGuideType.FHIR_DSTU2_NAME);

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                User currentUser = CheckPoint.Instance.GetUser(this.tdb);
                implementationGuides = (from ig in implementationGuides
                                        join igp in this.tdb.ImplementationGuidePermissions on ig.Id equals igp.UserId
                                        where igp.UserId == currentUser.Id
                                        select ig);
            }

            if (implementationGuideId != null)
                implementationGuides = implementationGuides.Where(y => y.Id == implementationGuideId);

            if (!string.IsNullOrEmpty(name))
                implementationGuides = implementationGuides.Where(y => y.Name.ToLower().Contains(name.ToLower()));

            Bundle bundle = new Bundle()
            {
                Type = Bundle.BundleType.BatchResponse
            };

            foreach (var ig in implementationGuides)
            {
                FhirImplementationGuide fhirImplementationGuide = Convert(ig, summary);

                // Add the IG before the templates
                bundle.AddResourceEntry(fhirImplementationGuide, ig.GetFullUrl(this.Request));

                // TODO: Need to implement a more sophisticated approach to parsing "_include"
                if (!string.IsNullOrEmpty(include) && include == "ImplementationGuide:resource")
                {
                    List<Template> templates = ig.GetRecursiveTemplates(this.tdb, inferred: true);

                    /*
                    // TODO: Add additional query parameters for indicating parent template ids and categories?
                    IGSettingsManager igSettings = new IGSettingsManager(this.tdb, ig.Id);
                    string templateBundleXml = FHIRExporter.GenerateExport(this.tdb, templates, igSettings);
                    Bundle templateBundle = (Bundle)FhirParser.ParseResourceFromXml(templateBundleXml);

                    var templateEntries = templateBundle.Entry.Where(y => 
                        y.Resource.ResourceType != ResourceType.ImplementationGuide && 
                        y.Resource.Id != fhirImplementationGuide.Id);

                    // TODO: Make sure that if multiple IGs are returned, that there are distinct bundle entries
                    // (no duplicate template/profile entries that are referenced by two separate IGs)
                    bundle.Entry.AddRange(templateEntries);
                    */

                    FHIR2StructureDefinitionController strucDefController = new FHIR2StructureDefinitionController(this.tdb);
                    foreach (var template in templates)
                    {
                        var strucDef = strucDefController.Convert(template);
                        bundle.AddResourceEntry(strucDef, template.GetFullUrl(this.Request));
                    }
                }
            }

            return bundle;
        }

        /// <summary>
        /// Gets implementation guides in FHIR DSTU2 format. Can specify search information, such as the name of the implementation guide and the id of the implementation guide.
        /// </summary>
        /// <param name="format">Optional. The format to respond with (ex: "application/fhir+xml" or "application/fhir+json")</param>
        /// <param name="summary">Optional. The type of summary to respond with.</param>
        /// <param name="include">Indicate what additional information should be included with the implementation guide (such as "ImplementationGuide:resource")</param>
        /// <param name="implementationGuideId">Specify the id of the implementation guide to search for.</param>
        /// <param name="name">Specifies the name of the implementation guide to search for. Implementation guides whose name *contains* this value will be returned.</param>
        /// <returns>Hl7.Fhir.Model.Bundle</returns>
        [HttpGet]
        [Route("api/FHIR2/ImplementationGuide")]
        [Route("api/FHIR2/ImplementationGuide/_search")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public HttpResponseMessage GetImplementationGuides(
            [FromUri(Name = "_format")] string format = null,
            [FromUri(Name = "_summary")] SummaryType? summary = null,
            [FromUri(Name = "_include")] string include = null,
            [FromUri(Name = "_id")] int? implementationGuideId = null,
            [FromUri(Name = "name")] string name = null)
        {
            var bundle = GetImplementationGuides(summary, include, implementationGuideId, name);
            return Shared.GetResponseMessage(this.Request, format, bundle);
        }

        [HttpPost]
        [Route("api/FHIR2/ImplementationGuide")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage CreateImplementationGuide(
            [FromBody] FhirImplementationGuide implementationGuide,
            [FromUri(Name = "_format")] string format = null)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("api/FHIR2/ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage UpdateImplementationGuide(
            [FromUri] int implementationGuideId,
            [FromBody] FhirImplementationGuide implementationGuide)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [Route("api/FHIR2/ImplementationGuide/{implementationGuideId}")]
        [SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public HttpResponseMessage DeleteImplementationGuide([FromUri] int implementationGuideId)
        {
            throw new NotImplementedException();
        }
    }
}
