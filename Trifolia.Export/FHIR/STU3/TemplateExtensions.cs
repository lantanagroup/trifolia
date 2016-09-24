using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;

namespace Trifolia.Export.FHIR.STU3
{
    public static class TemplateExtensions
    {
        public static string FhirId(this Template template)
        {
            if (string.IsNullOrEmpty(template.Bookmark))
                return string.Empty;

            return template.Bookmark.Replace("_", "-");
        }

        public static string FhirUrl(this Template template)
        {
            if (string.IsNullOrEmpty(template.Oid))
                return string.Empty;

            string urlEnd = "/" + template.Bookmark;

            if (template.Oid.LastIndexOf(urlEnd) == template.Oid.Length - urlEnd.Length)
                return template.Oid.Substring(0, template.Oid.LastIndexOf(template.Bookmark)) + template.FhirId();

            return template.Oid;
        }
    }
}
