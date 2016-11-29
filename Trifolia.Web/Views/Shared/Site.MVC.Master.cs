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
        public List<string> Securables { get; set; }
        public string DatabaseLabel { get; set; }
        public string VersionLabel { get; set; }
        
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

            // Only make the HL7 logo visible to users who are authenticated by the HL7
            //this.HL7LogoImage.Visible = CheckPoint.Instance.OrganizationName == "HL7";
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
