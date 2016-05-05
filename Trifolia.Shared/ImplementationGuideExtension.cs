using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public static class ImplementationGuideExtension
    {
        public static string GetDisplayName(this ImplementationGuide implementationGuide, bool? fileNameSafe = false)
        {
            string name = implementationGuide.NameWithVersion;

            if (!string.IsNullOrEmpty(implementationGuide.DisplayName))
                name = implementationGuide.DisplayName;

            if (fileNameSafe == true)
            {
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    name = name.Replace(c, '_');
                }
            }

            return name;
        }

        public static string GetEditUrl(this ImplementationGuide implementationGuide, bool absoluteUrl = false)
        {
            var request = System.Web.HttpContext.Current.Request;
            string baseAddress = absoluteUrl ? string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority) : string.Empty;

            return string.Format("{0}/IGManagement/Edit/{1}", baseAddress, implementationGuide.Id);
        }

        public static string GetViewUrl(this ImplementationGuide implementationGuide, bool absoluteUrl = false)
        {
            var request = System.Web.HttpContext.Current.Request;
            string baseAddress = absoluteUrl ? string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority) : string.Empty;

            return string.Format("{0}/IGManagement/View/{1}", baseAddress, implementationGuide.Id);
        }
    }
}
