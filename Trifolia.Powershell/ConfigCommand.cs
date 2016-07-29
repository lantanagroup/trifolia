using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Trifolia.Authorization;
using System.Security.Principal;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Set, "TrifoliaConfig")]
    public class ConfigCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The location of the configuration file to use for Trifolia powershell commands")]
        public string ConfigLocation { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The username to authenticate calls with")]
        public string Username { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The organization that the user belongs to")]
        public string Organization { get; set; }

        protected override void ProcessRecord()
        {
            AppConfig.ConfigLocation = this.ConfigLocation;

            TrifoliaApiIdentity identity = new TrifoliaApiIdentity(this.Username, this.Organization);
            GenericPrincipal principal = new GenericPrincipal(identity, null);
            Thread.CurrentPrincipal = principal;
        }
    }
}
