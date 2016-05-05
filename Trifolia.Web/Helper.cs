using System;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using Trifolia.Config;
using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Web
{
    public class Helper
    {
        public static T ToObjectFromJSON<T>(string jsonString)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Deserialize<T>(jsonString);
        }
        
        public static string GetValuesetScript()
        {
            string script = @"
<script language=""javascript"">
    var valuesets = [";

            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                foreach (ValueSet cValueset in tdb.ValueSets)
                {
                    string oid = cValueset.Oid.Replace("\r", "").Replace("\n", ""); ;
                    string name = cValueset.Name.Replace("\r", "").Replace("\n", "");
                    script += "\"" + oid + "\", \"" + name + "\", ";
                }

                script = script.Remove(script.Length - 2);
            }

            script += @"
    ];
</script>";

            return script;
        }

        public static string GetCodeSystemScript()
        {
            string script = @"
<script language=""javascript"">
    var codesystems = [";

            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                foreach (CodeSystem cCodeSystem in tdb.CodeSystems)
                {
                    string oid = cCodeSystem.Oid.Replace("\r", "").Replace("\n", "");
                    string name = cCodeSystem.Name.Replace("\r", "").Replace("\n", "");
                    script += "\"" + oid + "\", \"" + name + "\", ";
                }

                script = script.Remove(script.Length - 2);
            }

            script += @"
    ];
</script>";

            return script;
        }

        public static string GetContextScript()
        {
            string script = @"
<script language=""javascript"">
    var contexts = [";

            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                var contexts = (from tc in tdb.TemplateConstraints
                                select tc.Context).Distinct();

                foreach (string cContext in contexts)
                {
                    script += "\"" + cContext + "\", ";
                }

                script = script.Remove(script.Length - 2);
            }

            script += @"
    ];
</script>";

            return script;
        }

        public static string GetAbsoluteUrl(Page currentPage, string relativeUrl)
        {
            string resolvedPath = currentPage.ResolveUrl(relativeUrl);
            string host = currentPage.Request.Url.Host;

            string port = string.Empty;

            if (!currentPage.Request.Url.IsDefaultPort)
                port = ":" + currentPage.Request.Url.Port.ToString();

            if (currentPage.Request.IsSecureConnection)
                return string.Format("https://{0}{1}{2}", host, port, resolvedPath);

            return string.Format("http://{0}{1}{2}", host, port, resolvedPath);
        }
    }
}