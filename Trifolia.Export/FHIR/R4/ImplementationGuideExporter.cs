extern alias fhir_r4;
using fhir_r4.Hl7.Fhir.Model;
using fhir_r4.Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;
using Trifolia.Config;
using Trifolia.Logging;
using FhirImplementationGuide = fhir_r4.Hl7.Fhir.Model.ImplementationGuide;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using SummaryType = fhir_r4.Hl7.Fhir.Rest.SummaryType;
using fhir_r4.Hl7.Fhir.Serialization;

namespace Trifolia.Export.FHIR.R4
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
            var url = string.Format("{0}://{1}/api/FHIR3/ImplementationGuide/{2}",
                this.scheme,
                this.authority,
                implementationGuide.Id);
            return url;
        }

        public string GetFullUrl(Template template)
        {
            var url = string.Format("{0}://{1}/api/FHIR3/StructureDefinition/{2}",
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
                    return ((fhir_r4.Hl7.Fhir.Model.Organization)resource).Name;
                case ResourceType.CapabilityStatement:
                    return ((fhir_r4.Hl7.Fhir.Model.CapabilityStatement)resource).Name;
                case ResourceType.SearchParameter:
                    return ((fhir_r4.Hl7.Fhir.Model.SearchParameter)resource).Name;
                case ResourceType.Group:
                    return ((fhir_r4.Hl7.Fhir.Model.Group)resource).Name;
                case ResourceType.Location:
                    return ((fhir_r4.Hl7.Fhir.Model.Location)resource).Name;
                case ResourceType.Account:
                    return ((fhir_r4.Hl7.Fhir.Model.Account)resource).Name;
            }

            return name;
        }

        public FhirImplementationGuide Convert(ImplementationGuide ig, SummaryType? summaryType = null, bool includeVocabulary = true)
        {
            var parserSettings = new fhir_r4.Hl7.Fhir.Serialization.ParserSettings();
            parserSettings.AcceptUnknownMembers = true;
            parserSettings.AllowUnrecognizedEnums = true;
            parserSettings.DisallowXsiAttributesOnRoot = false;
            var fhirXmlParser = new FhirXmlParser(parserSettings);
            var fhirJsonParser = new FhirJsonParser(parserSettings);

            string url = string.Format("ImplementationGuide/{0}", ig.Id);

            if (!string.IsNullOrEmpty(ig.Identifier) && (ig.Identifier.StartsWith("http://") || ig.Identifier.StartsWith("https://")))
                url = ig.Identifier.TrimEnd('/') + "/" + url.TrimStart('/');

            var fhirImplementationGuide = new FhirImplementationGuide()
            {
                Id = ig.Id.ToString(),
                Name = ig.Name,
                Url = url
            };

            // Status
            if (ig.PublishStatus == PublishStatus.GetPublishedStatus(this.tdb))
                fhirImplementationGuide.Status = PublicationStatus.Active;
            else if (ig.PublishStatus == PublishStatus.GetRetiredStatus(this.tdb) || ig.PublishStatus == PublishStatus.GetDeprecatedStatus(this.tdb))
                fhirImplementationGuide.Status = PublicationStatus.Retired;
            else
                fhirImplementationGuide.Status = PublicationStatus.Draft;

            if (summaryType == null || summaryType == SummaryType.Data)
            {
                FhirImplementationGuide.DefinitionComponent definitionComponent = new FhirImplementationGuide.DefinitionComponent();
                
                // Page: Create a page for the implementation guide. This is required by the fhir ig publisher
                definitionComponent.Page = new FhirImplementationGuide.PageComponent();
                definitionComponent.Page.Generation = FhirImplementationGuide.GuidePageGeneration.Html;
                definitionComponent.Page.Title = ig.GetDisplayName();
                definitionComponent.Page.Name = new FhirUrl(string.Format("{0}://{1}/IG/View/{2}", this.scheme, this.authority, ig.Id));

                // Add profiles to the implementation guide
                List <Template> templates = ig.GetRecursiveTemplates(this.tdb, inferred: false);
                var profileResources = (from t in templates.OrderBy(y => y.ImpliedTemplateId)
                                        select new FhirImplementationGuide.ResourceComponent()
                                        {
                                            Example = new FhirBoolean(false),
                                            Reference = new ResourceReference()
                                            {
                                                Reference = string.Format("StructureDefinition/{0}", t.FhirId()),
                                                Display = t.Name
                                            }
                                        });
                definitionComponent.Resource.AddRange(profileResources);

                if (includeVocabulary)
                {
                    // Add value sets to the implementation guide
                    var valueSetsIds = (from t in templates
                                        join tc in this.tdb.TemplateConstraints.AsNoTracking() on t.Id equals tc.TemplateId
                                        where tc.ValueSetId != null
                                        select tc.ValueSetId)
                                       .Distinct()
                                       .ToList();
                    var valueSets = (from vs in this.tdb.ValueSets
                                     join vsi in valueSetsIds on vs.Id equals vsi
                                     select vs).ToList();
                    var valueSetResources = (from vs in valueSets
                                             where vs.GetIdentifier() != null && !vs.GetIdentifier().StartsWith("http://hl7.org/fhir/ValueSet/")    // Ignore value sets in the base spec
                                             select new FhirImplementationGuide.ResourceComponent()
                                             {
                                                 Example = new FhirBoolean(false),
                                                 Reference = new ResourceReference()
                                                 {
                                                     Reference = string.Format("ValueSet/{0}", vs.GetFhirId()),
                                                     Display = vs.Name
                                                 }
                                             });
                    definitionComponent.Resource.AddRange(valueSetResources);
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
                            resource = fhirXmlParser.Parse<Resource>(fileContent);
                        else if (file.MimeType == "application/json")
                            resource = fhirJsonParser.Parse<Resource>(fileContent);
                    }
                    catch
                    {
                    }

                    if (resource == null || string.IsNullOrEmpty(resource.Id))
                        continue;

                    var packageFile = new FhirImplementationGuide.ResourceComponent()
                    {
                        Example = new FhirBoolean(false),
                        Reference = new ResourceReference()
                        {
                            Reference = string.Format("{0}/{1}", resource.ResourceType, resource.Id),
                            Display = GetResourceName(resource, file.FileName)
                        }
                    };

                    definitionComponent.Resource.Add(packageFile);
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
                        resource = fhirXmlParser.Parse<Resource>(templateExample.Sample.XmlSample);
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (resource == null)
                            resource = fhirJsonParser.Parse<Resource>(templateExample.Sample.XmlSample);
                    }
                    catch
                    {
                    }

                    if (resource == null || string.IsNullOrEmpty(resource.Id))
                        continue;

                    var packageExample = new FhirImplementationGuide.ResourceComponent()
                    {
                        Example = new FhirBoolean(true),
                        Reference = new ResourceReference()
                        {
                            Reference = string.Format("{0}/{1}", resource.ResourceType, resource.Id),
                            Display = GetResourceName(resource, templateExample.Sample.Name)
                        }
                    };

                    definitionComponent.Resource.Add(packageExample);
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
