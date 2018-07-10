extern alias fhir_dstu2;
using System;
using System.Collections.Generic;
using Trifolia.DB;
using Trifolia.Shared.Validation;
using Trifolia.Shared;

namespace Trifolia.Plugins.FHIR
{
    [ImplementationGuideTypePlugin(Constants.IGTypeNames.FHIR_DSTU2)]
    public class DSTU2Plugin : DefaultPlugin, IIGTypePlugin
    {
        public List<String> GetFhirTypes(string elementPath)
        {
            throw new NotSupportedException();
        }

        public string GetFHIRResourceInstanceJson(string content)
        {
            throw new NotImplementedException();
        }

        public string GetFHIRResourceInstanceXml(string content)
        {
            throw new NotImplementedException();
        }

        public IValidator GetValidator(IObjectRepository tdb)
        {
            return new Validation.RIMValidator(tdb);
        }
    }
}
