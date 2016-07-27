using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Plugins.FHIR.DSTU2
{
    public class Shared
    {
        public const string DEFAULT_IG_NAME = "Unowned FHIR DSTU2 Profiles";
        public const string DEFAULT_USER_NAME = "admin";
        public const string DEFAULT_ORG_NAME = "LCG";
        public const string STRUCDEF_NEW_IDENTIFIER_FORMAT = "https://trifolia.lantanagroup.com/Generated/{0}";

        public static string FormatIdentifier(string identifier)
        {
            if (identifier.StartsWith("http") || identifier.StartsWith("urn"))
                return identifier;

            return string.Format("urn:oid:{0}", identifier);
        }
    }

    public static class TemplateConstraintExtensions
    {
        public static string GetElementPath(this TemplateConstraint constraint, string resourceType)
        {
            string elementPath = "";

            // Element path
            var current = constraint;
            while (current != null)
            {
                if (!string.IsNullOrEmpty(elementPath))
                    elementPath = "." + elementPath;

                elementPath = current.Context.Replace("@", "") + elementPath;
                current = current.ParentConstraint;
            }

            return (string.IsNullOrEmpty(resourceType) ? "Resource" : resourceType) + "." + elementPath;
        }
    }
}
