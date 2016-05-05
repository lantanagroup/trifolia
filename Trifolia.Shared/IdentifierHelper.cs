using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.Shared
{
    public static class IdentifierHelper
    {
        private const string OID_REGEX = "^urn:oid:(.+)";
        private const string II_REGEX = "^urn:hl7ii:(.+)?:(.+)?";
        private const string URI_REGEX = "^uri:(.+)";

        /// <summary>
        /// Determines if the identifier for the template is an "urn:oid:" identifier
        /// </summary>
        public static bool IsIdentifierOID(string templateIdentifier)
        {
            if (templateIdentifier == null)
                return false;

            Regex regex = new Regex(OID_REGEX, RegexOptions.Multiline);
            return regex.IsMatch(templateIdentifier);
        }

        public static bool GetIdentifierOID(string templateIdentifier, out string oid)
        {
            Regex regex = new Regex(OID_REGEX, RegexOptions.Multiline);

            if (regex.IsMatch(templateIdentifier))
            {
                Match match = regex.Match(templateIdentifier);
                oid = match.Groups[1].Value;
                return true;
            }

            oid = null;
            return false;
        }

        /// <summary>
        /// Determines if the identifier for the template is an "urn:hl7ii" (instance identifier) identifier
        /// </summary>
        public static bool IsIdentifierII(string templateIdentifier)
        {
            if (templateIdentifier == null)
                return false;

            Regex regex = new Regex(II_REGEX, RegexOptions.Multiline);
            return regex.IsMatch(templateIdentifier);
        }

        public static bool GetIdentifierII(string templateIdentifier, out string root, out string extension)
        {
            Regex regex = new Regex(II_REGEX, RegexOptions.Multiline);

            if (regex.IsMatch(templateIdentifier))
            {
                Match match = regex.Match(templateIdentifier);
                root = match.Groups[1].Value;
                extension = match.Groups[2].Value;
                return true;
            }

            root = extension = null;
            return false;
        }

        /// <summary>
        /// Determines if the identifier for the template is a "uri:" identifier
        /// </summary>
        public static bool IsIdentifierURI(string templateIdentifier)
        {
            if (templateIdentifier == null)
                return false;

            Regex regex = new Regex(URI_REGEX, RegexOptions.Multiline);
            return regex.IsMatch(templateIdentifier);
        }

        public static bool GetIdentifierURI(string templateIdentifier, out string uri)
        {
            Regex regex = new Regex(URI_REGEX, RegexOptions.Multiline);

            if (regex.IsMatch(templateIdentifier))
            {
                Match match = regex.Match(templateIdentifier);
                uri = match.Groups[1].Value;
                return true;
            }

            uri = null;
            return false;
        }
    }
}
