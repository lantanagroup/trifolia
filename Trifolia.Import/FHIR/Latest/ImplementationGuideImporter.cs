extern alias fhir_latest;
using fhir_latest.Hl7.Fhir.Model;
using fhir_latest.Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.FHIR;
using Trifolia.Config;
using Trifolia.Logging;
using FhirImplementationGuide = fhir_latest.Hl7.Fhir.Model.ImplementationGuide;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using SummaryType = fhir_latest.Hl7.Fhir.Rest.SummaryType;

namespace Trifolia.Import.FHIR.Latest
{
    public class ImplementationGuideImporter
    {
        private IObjectRepository tdb;
        private string scheme;
        private string authority;
        private SimpleSchema schema;
        private ImplementationGuideType implementationGuideType;

        /// <summary>
        /// Initializes a new instance of ImplementationGuideImporter
        /// </summary>
        /// <param name="tdb">Reference to the database</param>
        /// <param name="scheme">The server url's scheme</param>
        /// <param name="authority">The server url's authority</param>
        public ImplementationGuideImporter(IObjectRepository tdb, SimpleSchema schema, string scheme, string authority)
        {
            this.tdb = tdb;
            this.scheme = scheme;
            this.authority = authority;
            this.schema = schema;
            this.implementationGuideType = LatestHelper.GetImplementationGuideType(this.tdb, true);
        }

        public ImplementationGuide Convert(FhirImplementationGuide fhirImplementationGuide, ImplementationGuide implementationGuide)
        {
            if (implementationGuide == null)
                implementationGuide = new ImplementationGuide()
                {
                    ImplementationGuideType = this.implementationGuideType
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
    }
}
