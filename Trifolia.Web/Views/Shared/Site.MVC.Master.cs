using System;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Permissions;
using System.Security.Principal;
using System.DirectoryServices;

using Trifolia.Authentication;
using Trifolia.Config;
using Trifolia.Shared;
using Trifolia.Logging;
using Trifolia.Authorization;
using Trifolia.DB;

namespace Trifolia.Web.Views.Shared
{
    public partial class SiteMVCMaster : System.Web.Mvc.ViewMasterPage
    {
        private const string GoogleAnalyticsScriptFormat = @"
<!-- Global Site Tag (gtag.js) - Google Analytics -->
<script async src=""https://www.googletagmanager.com/gtag/js?id={0}""></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag() {{ dataLayer.push(arguments) }};
  gtag('js', new Date());
  gtag('config', '{0}');
</script>
";
        public List<string> Securables { get; set; }
        public string DatabaseLabel { get; set; }
        public string VersionLabel { get; set; }
        public string GoogleAnalyticsScript { get; set; }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                var user = CheckPoint.Instance.GetUser(tdb);

                if (user != null)
                {
                    this.Securables = (from ur in user.Roles
                                       join ras in tdb.RoleAppSecurables on ur.RoleId equals ras.RoleId
                                       select ras.AppSecurable.Name).ToList();

                    if (CheckPoint.Instance.IsDataAdmin)
                        this.DatabaseLabel = string.Format("{0}/{1}", tdb.Connection.DataSource, tdb.Connection.Database);
                }
            }

            this.VersionLabel = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.GoogleAnalyticsScript = !string.IsNullOrEmpty(AppSettings.GoogleAnalyticsGtag) ? string.Format(GoogleAnalyticsScriptFormat, AppSettings.GoogleAnalyticsGtag) : string.Empty;
        }

        private bool HasSecurables(string[] securables)
        {
            if (this.Securables == null)
                return false;

            foreach (var securable in securables)
            {
                if (this.Securables.Contains(securable))
                    return true;
            }

            return false;
        }
    }
}
