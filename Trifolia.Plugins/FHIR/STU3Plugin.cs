extern alias fhir_stu3;
using System;
using System.Collections.Generic;
using Trifolia.DB;
using Trifolia.Shared.FHIR.Profiles.STU3;
using Trifolia.Shared.Validation;

namespace Trifolia.Plugins.FHIR
{
    [ImplementationGuideTypePlugin("FHIR STU3")]
    public class STU3Plugin : DefaultPlugin, IIGTypePlugin
    {
        public List<String> GetFhirTypes(string elementPath)
        {
            // TODO: Extract the resourceType from elementPath
            string resourceType = elementPath.Substring(0, elementPath.IndexOf('.'));
            var strucDef = ProfileHelper.GetProfile(resourceType);
            List<String> fhirTypes = new List<String>();

            // TODO: element type is not a reference, return empty array

            foreach (var element in strucDef.Snapshot.Element)
            {
                if (element.Path != elementPath)
                    continue;

                foreach(var type in element.Type)
                {
                    if (type.Code != "Reference")
                    {
                        throw new NotSupportedException("Not a reference");
                    }
                    else
                    {
                        String profile = type.TargetProfile;
                        String primaryContext = profile.Substring(profile.LastIndexOf("/") + 1);
                        fhirTypes.Add(primaryContext);
                    }
                }
            }

            return fhirTypes;
        }

        private fhir_stu3.Hl7.Fhir.Model.Resource GetFHIRResource(string content)
        {
            var parserSettings = new fhir_stu3.Hl7.Fhir.Serialization.ParserSettings();
            parserSettings.AcceptUnknownMembers = true;
            parserSettings.AllowUnrecognizedEnums = true;
            parserSettings.DisallowXsiAttributesOnRoot = false;

            try
            {
                var xmlParser = new fhir_stu3.Hl7.Fhir.Serialization.FhirXmlParser(parserSettings);
                return xmlParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(content);
            }
            catch
            {
            }

            try
            {
                var jsonParser = new fhir_stu3.Hl7.Fhir.Serialization.FhirJsonParser(parserSettings);
                return jsonParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(content);
            }
            catch
            {
            }

            return null;
        }

        public string GetFHIRResourceInstanceJson(string content)
        {
            var resource = GetFHIRResource(content);

            if (resource != null)
                return fhir_stu3.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToJson(resource);

            return null;
        }

        public string GetFHIRResourceInstanceXml(string content)
        {
            var resource = GetFHIRResource(content);

            if (resource != null)
                return fhir_stu3.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToXml(resource);

            return null;
        }

        public IValidator GetValidator(IObjectRepository tdb)
        {
            return new Validation.FHIR.STU3Validator(tdb);
        }
    }
}
