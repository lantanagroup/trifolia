extern alias fhir_stu3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.Validation;
using fhir_stu3.Hl7.Fhir.Serialization;

namespace Trifolia.Plugins.Validation.FHIR
{
    public class STU3Validator : BaseValidator
    {
        private FhirXmlParser fhirXmlParser;
        private FhirJsonParser fhirJsonParser;

        public STU3Validator(IObjectRepository tdb)
            : base(tdb)
        {
            var parserSettings = new fhir_stu3.Hl7.Fhir.Serialization.ParserSettings();
            parserSettings.AcceptUnknownMembers = true;
            parserSettings.AllowUnrecognizedEnums = true;
            parserSettings.DisallowXsiAttributesOnRoot = false;

            this.fhirXmlParser = new FhirXmlParser(parserSettings);
            this.fhirJsonParser = new FhirJsonParser(parserSettings);
        }

        protected override ValidationResults ValidateImplementationGuide(ImplementationGuide implementationGuide, SimpleSchema igSchema)
        {
            ValidationResults results = base.ValidateImplementationGuide(implementationGuide, igSchema);

            // Check that the implementation guide has a base identifier/url
            if (string.IsNullOrEmpty(implementationGuide.Identifier))
                results.Messages.Add("Implementation guide does not have a base identifier/url.");

            // Check that each FHIR resource instance is valid and has the required fields
            foreach (var file in implementationGuide.Files)
            {
                var fileData = file.GetLatestData();
                fhir_stu3.Hl7.Fhir.Model.Resource resource = null;
                string fileContent = System.Text.Encoding.UTF8.GetString(fileData.Data);

                try
                {
                    resource = this.fhirXmlParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                }
                catch (Exception ex)
                {
                    if (file.MimeType == "application/xml" || file.MimeType == "text/xml")
                        results.Messages.Add("FHIR Resource instance \"" + file.FileName + "\" cannot be parsed as valid XML: " + ex.Message);
                }

                try
                {
                    if (resource == null)
                        resource = this.fhirJsonParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                }
                catch (Exception ex)
                {
                    if (file.MimeType == "binary/octet-stream" || file.MimeType == "application/json")
                        results.Messages.Add("FHIR Resource instance \"" + file.FileName + "\" cannot be parsed as valid JSON: " + ex.Message);
                }

                if (resource != null && string.IsNullOrEmpty(resource.Id))
                {
                    string msg = string.Format("FHIR resource instance \"" + file.FileName + "\" does not have an \"id\" property.");
                    results.Messages.Add(msg);
                }
            }

            List<string> resourceIds = new List<string>();

            // Go through each of the FHIR Resource Instance files associated with the implementation guide and get the resource type and id for each one
            foreach (var file in implementationGuide.Files.Where(y => y.ContentType == ImplementationGuideFile.ContentTypeFHIRResourceInstance))
            {
                fhir_stu3.Hl7.Fhir.Model.Resource resource = null;
                string fileContent = Encoding.UTF8.GetString(file.GetLatestData().Data);

                try
                {
                    if (file.MimeType.Contains("json"))
                    {
                        fhir_stu3.Hl7.Fhir.Serialization.FhirJsonParser parser = new FhirJsonParser();
                        resource = parser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                    }
                    else if (file.MimeType.Contains("xml"))
                    {
                        fhir_stu3.Hl7.Fhir.Serialization.FhirXmlParser parser = new FhirXmlParser();
                        resource = parser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                    }
                }
                catch (Exception ex)
                {
                    // Do nothing? Skip?

                }

                if (resource != null)
                {
                    // Ex: Questionnaire/ssi-questionnaire
                    resourceIds.Add(resource.ResourceType + "/" + resource.Id);
                }
            }

            // Go through each of the profile samples, and do the same thing. Loop through each template in the IG, and Template.TemplateSamples
            var allTemplateSamples = implementationGuide.ChildTemplates.SelectMany(y => y.TemplateSamples);

            foreach (var templateSample in allTemplateSamples)
            {
                fhir_stu3.Hl7.Fhir.Model.Resource resource = null;
                string fileContent = templateSample.XmlSample;

                try
                {
                    fhir_stu3.Hl7.Fhir.Serialization.FhirXmlParser parser = new FhirXmlParser();
                    resource = parser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                }
                catch (Exception ex)
                {
                    // Do nothing

                }
                if (resource != null)
                {
                    resourceIds.Add(resource.ResourceType + "/" + resource.Id);
                }
            }

            // Go through each of the profiles and determine their StructureDefinition/<id> as well, add to list
            foreach (var template in implementationGuide.ChildTemplates)
            {
                String resourceId = template.Oid.Substring(template.Oid.IndexOf("StructureDefinition/") + 20);
                resourceIds.Add(template.PrimaryContextType + "/" + resourceId);
            }

            // Check the list of resourceIds for duplicates (suggest using some form of linq query - search online)
            var duplicateCheck = resourceIds.GroupBy(x => x)
                                            .Where(g => g.Count() > 1)
                                            .Select(d => d.Key).ToList();

            // For each duplicate, create a resources.Messages error. Flag results.RestrictDownload if any duplicates, cause the exports will error out.
            if (duplicateCheck.Count != 0)
            {
                results.RestrictDownload = true;
                foreach (var duplicate in duplicateCheck)
                {
                    results.Messages.Add("Export Restricted: Entry with type and id " + duplicate.ToString() + " appears more than once within this implementation guide as an IG sample file or as a sample associated with a profile");
                }
            }

            return results;
        }

        public override List<ValidationResult> ValidateTemplate(Template template, SimpleSchema igSchema, IEnumerable<Template> allContainedTemplates = null)
        {
            List<ValidationResult> results = base.ValidateTemplate(template, igSchema, allContainedTemplates);

            var noHttpValueSets = (from tc in template.ChildConstraints
                                   join vs in this.tdb.ValueSets on tc.ValueSetId equals vs.Id
                                   where vs.Identifiers.Count(y => y.Type == ValueSetIdentifierTypes.HTTP) == 0
                                   select new { ConstraintNumber = tc.Number, ValueSet = vs });

            foreach (var noHttpValueSet in noHttpValueSets)
            {
                string vsIdentifier = noHttpValueSet.ValueSet.GetIdentifier(ValueSetIdentifierTypes.HTTP);

                results.Add(new ValidationResult()
                {
                    ConstraintNumber = noHttpValueSet.ConstraintNumber,
                    Level = ValidationLevels.Error,
                    Message = string.Format("The constraint references a value set ({0}) that does not have an HTTP identifier", vsIdentifier),
                    TemplateId = template.Id,
                    TemplateName = template.Name
                });
            }

            foreach (var templateExample in template.TemplateSamples)
            {
                fhir_stu3.Hl7.Fhir.Model.Resource resource = null;

                try
                {
                    resource = fhirXmlParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(templateExample.XmlSample);
                }
                catch
                {
                }

                try
                {
                    if (resource == null)
                        resource = fhirJsonParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(templateExample.XmlSample);
                }
                catch
                {
                }

                if (resource == null)
                {
                    results.Add(new ValidationResult()
                    {
                        TemplateId = template.Id,
                        TemplateName = template.Name,
                        Level = ValidationLevels.Error,
                        Message = string.Format("Profile sample \"{0}\" cannot be parsed as a valid XML or JSON resource.", templateExample.Name)
                    });
                }
                else if (string.IsNullOrEmpty(resource.Id))
                {
                    results.Add(new ValidationResult()
                    {
                        TemplateId = template.Id,
                        TemplateName = template.Name,
                        Level = ValidationLevels.Error,
                        Message = string.Format("Profile sample \"{0}\" does not have an \"id\" property.", templateExample.Name)
                    });
                }
            }

            return results;
        }
    }
}
