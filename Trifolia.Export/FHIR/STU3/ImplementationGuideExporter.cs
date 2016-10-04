extern alias fhir_stu3;
using fhir_stu3.Hl7.Fhir.Model;
using fhir_stu3.Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;
using Trifolia.Config;
using Trifolia.Logging;
using FhirImplementationGuide = fhir_stu3.Hl7.Fhir.Model.ImplementationGuide;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using SummaryType = fhir_stu3.Hl7.Fhir.Rest.SummaryType;
using fhir_stu3.Hl7.Fhir.Serialization;

namespace Trifolia.Export.FHIR.STU3
{
    public class ImplementationGuideExporter
    {
        private IObjectRepository tdb;
        private string scheme;
        private string authority;
        private SimpleSchema schema;
        private ImplementationGuideType implementationGuideType;

        /// <summary>
        /// Initializes a new instance of ImplementationGuideExporter
        /// </summary>
        /// <param name="tdb">Reference to the database</param>
        /// <param name="scheme">The server url's scheme</param>
        /// <param name="authority">The server url's authority</param>
        public ImplementationGuideExporter(IObjectRepository tdb, SimpleSchema schema, string scheme, string authority)
        {
            this.tdb = tdb;
            this.scheme = scheme;
            this.authority = authority;
            this.schema = schema;
            this.implementationGuideType = STU3Helper.GetImplementationGuideType(this.tdb, true);
        }

        private string GetFullUrl(ImplementationGuide implementationGuide)
        {
            var url = string.Format("{0}://{1}/api/FHIR2/ImplementationGuide/{2}",
                this.scheme,
                this.authority,
                implementationGuide.Id);
            return url;
        }

        public string GetFullUrl(Template template)
        {
            var url = string.Format("{0}://{1}/api/FHIR2/StructureDefinition/{2}",
                this.scheme,
                this.authority,
                template.Id);
            return url;
        }

        private static string GetResourceName(Resource resource, string defaultName = "")
        {
            string name = !string.IsNullOrEmpty(defaultName) ? defaultName.Replace(" ", "_") : "";

            switch (resource.ResourceType)
            {
                case ResourceType.Organization:
                    return ((fhir_stu3.Hl7.Fhir.Model.Organization)resource).Name;
                case ResourceType.Conformance:
                    return ((fhir_stu3.Hl7.Fhir.Model.Conformance)resource).Name;
                case ResourceType.SearchParameter:
                    return ((fhir_stu3.Hl7.Fhir.Model.SearchParameter)resource).Name;
            }

            return name;
        }

        public FhirImplementationGuide Convert(ImplementationGuide ig, SummaryType? summaryType = null, bool includeVocabulary = true)
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

            if (summaryType == null || summaryType == SummaryType.Data)
            {
                // Package
                FhirImplementationGuide.PackageComponent package = new FhirImplementationGuide.PackageComponent();
                package.Name = "Profiles in this Implementation Guide";
                fhirImplementationGuide.Package.Add(package);

                // Page: Create a page for the implementation guide. This is required by the fhir ig publisher
                fhirImplementationGuide.Page = new FhirImplementationGuide.PageComponent();
                fhirImplementationGuide.Page.Kind = FhirImplementationGuide.GuidePageKind.Page;
                fhirImplementationGuide.Page.Title = ig.GetDisplayName();
                fhirImplementationGuide.Page.Source = string.Format("{0}://{1}/IG/View/{2}", this.scheme, this.authority, ig.Id);

                // Add profiles to the implementation guide
                List<Template> templates = ig.GetRecursiveTemplates(this.tdb, inferred: false);
                var profileResources = (from t in templates
                                        select new FhirImplementationGuide.ResourceComponent()
                                        {
                                            Example = false,
                                            Source = new ResourceReference()
                                            {
                                                Reference = string.Format("StructureDefinition/{0}", t.FhirId()),
                                                Display = t.Name
                                            }
                                        });
                package.Resource.AddRange(profileResources);

                if (includeVocabulary)
                {
                    // Add value sets to the implementation guide
                    var valueSets = (from t in templates
                                     join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                                     where tc.ValueSet != null
                                     select tc.ValueSet).Distinct().ToList();
                    var valueSetResources = (from vs in valueSets
                                             select new FhirImplementationGuide.ResourceComponent()
                                             {
                                                 Example = false,
                                                 Source = new ResourceReference()
                                                 {
                                                     Reference = string.Format("ValueSet/{0}", vs.Id.ToString()),
                                                     Display = vs.Name
                                                 }
                                             });
                    package.Resource.AddRange(valueSetResources);
                }

                // Add each of the individual FHIR resources added as files to the IG
                foreach (var file in ig.Files)
                {
                    var fileData = file.GetLatestData();
                    Resource resource = null;

                    try
                    {
                        string fileContent = System.Text.Encoding.UTF8.GetString(fileData.Data);

                        if (file.MimeType == "application/xml" || file.MimeType == "text/xml")
                            resource = FhirParser.ParseResourceFromXml(fileContent);
                        else if (file.MimeType == "application/json")
                            resource = FhirParser.ParseResourceFromJson(fileContent);
                    }
                    catch
                    {
                    }

                    if (resource == null || string.IsNullOrEmpty(resource.Id))
                        continue;

                    var packageFile = new FhirImplementationGuide.ResourceComponent()
                    {
                        Example = false,
                        Source = new ResourceReference()
                        {
                            Reference = string.Format("{0}/{1}", resource.ResourceType, resource.Id),
                            Display = GetResourceName(resource, file.FileName)
                        }
                    };

                    package.Resource.Add(packageFile);
                }

                // Add each of the samples generated for the template/profile
                var templateExamples = (from t in templates
                                        join ts in this.tdb.TemplateSamples on t.Id equals ts.TemplateId
                                        select new { Template = t, Sample = ts });

                foreach (var templateExample in templateExamples)
                {
                    Resource resource = null;

                    try
                    {
                        resource = FhirParser.ParseResourceFromXml(templateExample.Sample.XmlSample);
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (resource == null)
                            resource = FhirParser.ParseResourceFromJson(templateExample.Sample.XmlSample);
                    }
                    catch
                    {
                    }

                    if (resource == null || string.IsNullOrEmpty(resource.Id))
                        continue;

                    var packageExample = new FhirImplementationGuide.ResourceComponent()
                    {
                        Example = true,
                        Source = new ResourceReference()
                        {
                            Reference = string.Format("{0}/{1}", resource.ResourceType, resource.Id),
                            Display = GetResourceName(resource, templateExample.Sample.Name)
                        }
                    };

                    package.Resource.Add(packageExample);
                }
            }

            return fhirImplementationGuide;
        }

        public Bundle GetImplementationGuides(SummaryType? summary = null, string include = null, int? implementationGuideId = null, string name = null)
        {
            // TODO: Should not be using constant string for IG type name to find templates... Not sure how else to identify FHIR DSTU1 templates though
            var implementationGuides = this.tdb.ImplementationGuides.Where(y => y.ImplementationGuideTypeId == this.implementationGuideType.Id);

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
                bundle.AddResourceEntry(fhirImplementationGuide, this.GetFullUrl(ig));

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

                    StructureDefinitionExporter strucDefExporter = new StructureDefinitionExporter(this.tdb, this.scheme, this.authority);
                    foreach (var template in templates)
                    {
                        var templateSchema = this.schema.GetSchemaFromContext(template.PrimaryContextType);
                        var strucDef = strucDefExporter.Convert(template, schema);
                        bundle.AddResourceEntry(strucDef, this.GetFullUrl(template));
                    }
                }
            }

            return bundle;
        }
    }
}
