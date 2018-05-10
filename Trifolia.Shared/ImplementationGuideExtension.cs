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
        public static bool HasImportedValueSets(this ImplementationGuide implementationGuide, IObjectRepository tdb, ValueSetImportSources importSource)
        {
            var templates = implementationGuide.GetRecursiveTemplates(tdb);
            var valueSets = (from t in templates
                             join tc in tdb.TemplateConstraints.AsNoTracking() on t.Id equals tc.TemplateId
                             join vs in tdb.ValueSets.AsNoTracking() on tc.ValueSetId equals vs.Id
                             where vs.ImportSource == importSource
                             select vs.Id);
            return valueSets.Count() > 0;
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
