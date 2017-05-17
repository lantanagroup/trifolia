extern alias fhir_stu3;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fhir_stu3.Hl7.Fhir.Model;
using fhir_stu3.Hl7.Fhir.Serialization;
using System.Runtime;

namespace Trifolia.Shared.FHIR.Profiles.STU3
{
    public static class ProfileHelper
    {
        private const string resourceLocationFormat = "Trifolia.Shared.FHIR.Profiles.STU3.profile-{0}.xml";
        private const string appDomainProfileBundleKey = "fhir_stu3_profile_bundle";

        /// <summary>
        /// Gets the specified resource type's profile
        /// </summary>
        /// <remarks>All profiles are stored in resources embedded within the Trifolia.Shared assembly. This method parses the 
        /// profile embedded in the assembly and returns the resulting StructureDefinition.</remarks>
        /// <param name="resourceType">The resource type whose profile is to be retrieved</param>
        public static StructureDefinition GetProfile(string resourceType)
        {
            string resourceLocation = string.Format(resourceLocationFormat, resourceType);
            var resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation);

            if (resourceStream == null)
                return null;

            using (StreamReader profileReader = new StreamReader(resourceStream))
            {
                var parserSettings = new ParserSettings();
                parserSettings.AcceptUnknownMembers = true;
                parserSettings.AllowUnrecognizedEnums = true;
                parserSettings.DisallowXsiAttributesOnRoot = false;

                var parser = new FhirXmlParser(parserSettings);
                StructureDefinition profile = parser.Parse<StructureDefinition>(profileReader.ReadToEnd());

                return profile;
            }
        }

        [Obsolete]
        public static Bundle LoadProfileBundle()
        {
            string resourceLocation = "Trifolia.Shared.FHIR.Profiles.STU3.profiles-resources.xml";
            var resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation);

            using (StreamReader profileReader = new StreamReader(resourceStream))
            {
                var parserSettings = new ParserSettings();
                parserSettings.AcceptUnknownMembers = true;
                parserSettings.AllowUnrecognizedEnums = true;
                parserSettings.DisallowXsiAttributesOnRoot = false;

                var parser = new FhirXmlParser(parserSettings);
                Bundle profileBundle = parser.Parse<Bundle>(profileReader.ReadToEnd());

                AppDomain.CurrentDomain.SetData(appDomainProfileBundleKey, profileBundle);
                
                GC.Collect();

                return profileBundle;
            }
        }

        [Obsolete]
        public static Bundle GetProfileBundle()
        {
            object profileBundleObj = AppDomain.CurrentDomain.GetData(appDomainProfileBundleKey);

            if (profileBundleObj == null)
                profileBundleObj = ProfileHelper.LoadProfileBundle();

            return (Bundle)profileBundleObj;
        }
    }
}
