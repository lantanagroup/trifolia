extern alias fhir_stu3;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fhir_stu3.Hl7.Fhir.Model;
using fhir_stu3.Hl7.Fhir.Serialization;

namespace Trifolia.Shared.FHIR.Profiles.STU3
{
    public static class ProfileHelper
    {
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

                AppDomain.CurrentDomain.SetData("fhir_stu3_profile_bundle", profileBundle);
                return profileBundle;
            }
        }

        public static Bundle GetProfileBundle()
        {
            object profileBundleObj = AppDomain.CurrentDomain.GetData("fhir_stu3_profile_bundle");

            if (profileBundleObj == null)
                return LoadProfileBundle();

            return (Bundle)profileBundleObj;
        }
    }
}
