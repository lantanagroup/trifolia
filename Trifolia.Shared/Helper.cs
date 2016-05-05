using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public class Helper
    {
        public static string GetSchemasDirectory(string igTypeName)
        {
            string basePath = ".\\";

            if (!string.IsNullOrEmpty(Properties.Settings.Default.IGTypeSchemaLocation))
            {
                if (Properties.Settings.Default.IGTypeSchemaLocation.StartsWith("~"))
                    basePath = HttpContext.Current.ApplicationInstance.Server.MapPath(Properties.Settings.Default.IGTypeSchemaLocation);
                else
                    basePath = new FileInfo(Properties.Settings.Default.IGTypeSchemaLocation).FullName;
            }

            return GetSchemasDirectory(basePath, igTypeName);
        }

        public static string GetSchemasDirectory(string basePath, string igTypeName)
        {
            string igTypeSchemasPath = Path.Combine(basePath, igTypeName.Replace(" ", "_"));

            if (!Directory.Exists(igTypeSchemasPath))
                Directory.CreateDirectory(igTypeSchemasPath);

            return igTypeSchemasPath;
        }

        public static XmlSchema GetIGSchema(ImplementationGuideType igType)
        {
            string schemaPath = GetSchemasDirectory(igType.Name);
            Dictionary<string, string> namespaces = null;
            return GetSchema(Path.Combine(schemaPath, igType.SchemaLocation), out namespaces);
        }

        public static XmlSchema GetSchema(string location, out Dictionary<string, string> namespaces)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(location);

            namespaces = new Dictionary<string, string>();

            foreach (XmlAttribute cAttribute in doc.DocumentElement.Attributes)
            {
                if (cAttribute.NamespaceURI == "http://www.w3.org/2000/xmlns/" && !namespaces.ContainsKey(cAttribute.Value))
                    namespaces.Add(cAttribute.Value, cAttribute.LocalName);
            }

            return GetSchema(location);
        }

        public static XmlSchema GetSchema(string location)
        {
            using (XmlTextReader reader = new XmlTextReader(location))
            {
                XmlSchemaSet schemas = new XmlSchemaSet();

                XmlSchema mainSchema = XmlSchema.Read(reader, null);
                schemas.Add(mainSchema);

                schemas.Compile();

                return mainSchema;
            }
        }

        //this has been refactored to an instance method, because it's untestable (unmockable) with unit tests. the static method is 
        //still here to provide backwards compatibility. This is a prime example as to why static is generally a poor design.
        [Obsolete("Static methods are untestable and therefore an indication of poor design.")]
        public static string GetIGSimplifiedSchemaLocation(ImplementationGuideType igType)
        {
            var instance = new Helper();

            return instance.GetIGSimplifiedSchemaLocation2(igType);
        }

        public virtual string GetIGSimplifiedSchemaLocation2(ImplementationGuideType igType)
        {
            string schemaPath = GetSchemasDirectory(igType.Name);
            string fullSchemaPath = Path.Combine(schemaPath, igType.SchemaLocation);

            return fullSchemaPath;
        }

        public static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return name
                .Replace("@", "")
                .Replace(":", "")
                .Replace("-", "")
                .Replace(".", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Replace("\t", "")
                .Replace(" ", "");
        }

        public static bool DetermineMinMax(string cardinality, string conformance, out int min, out int max)
        {
            min = max = -1;

            if (!string.IsNullOrEmpty(cardinality))
            {
                string[] cardSplit = cardinality.Replace("..", ".").Split('.');

                if (cardSplit.Length != 2)
                    return false;

                min = Int32.Parse(cardSplit[0]);
                    
                if (cardSplit[1] == "*")
                    max = Int32.MaxValue;
                else
                    max = Int32.Parse(cardSplit[1]);

                return true;
            }
            else if (!string.IsNullOrEmpty(conformance))
            {
                switch (conformance)
                {
                    case "SHALL":
                        min = 1;
                        max = 1;
                        break;
                    case "MAY":
                    case "SHOULD":
                    case "SHOULD NOT":
                        min = 0;
                        max = 1;
                        break;
                    case "SHALL NOT":
                        min = 0;
                        max = 0;
                        break;
                    default:
                        return false;
                }

                return true;
            }

            return false;
        }

        public static bool DetermineMinMax(TemplateConstraint constraint, out int min, out int max)
        {
            string cardinality = constraint.Cardinality;
            string conformance = constraint.Conformance;

            return DetermineMinMax(cardinality, conformance, out min, out max);
        }

        public static string GetDataTypeName(string ns, string dataTypeName)
        {
            // Only change the dataTypeName for CDA schemas
            if (ns != "urn:hl7-org:v3")
                return dataTypeName;

            if (dataTypeName != null && dataTypeName.Contains("."))
                return dataTypeName.Substring(dataTypeName.LastIndexOf(".") + 1);
            
            return dataTypeName;
        }

        public const string SpecialCharactersRegexPattern = @"[^\w\s]";

        public static string GetCleanName(string name, int? maxCharacters=null)
        {
            string newName = name
                .Replace("\t", "_")
                .Replace(" ", "_");

            // Replace special characters in the bookmark with nothing
            Regex specialCharactersRegex = new Regex(SpecialCharactersRegexPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            newName = specialCharactersRegex.Replace(newName, "");

            if (maxCharacters != null && newName.Length > maxCharacters.Value)
                newName = newName.Substring(0, maxCharacters.Value);

            return newName;
        }

        private static void LoadSchemaIncludes(XmlSchemaSet destinationSet, string basePath, XmlSchema schema)
        {
            foreach (XmlSchemaInclude cInclude in schema.Includes)
            {
                using (XmlReader reader = XmlReader.Create(Path.Combine(basePath, cInclude.SchemaLocation), new XmlReaderSettings()))
                {
                    XmlSchema nextSchema = XmlSchema.Read(reader, null);

                    try
                    {
                        destinationSet.Add(nextSchema);
                    }
                    catch { } // For duplicate schema inclusions

                    LoadSchemaIncludes(destinationSet, basePath, nextSchema);
                }
            }
        }
    }
}
