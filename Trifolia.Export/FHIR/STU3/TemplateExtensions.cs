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
            // Method placeholder in case pre-processing needs to be performed on the bookmark before returning it as the FHRI id
            return template.Bookmark;
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
