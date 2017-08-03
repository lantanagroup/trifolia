using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Import.VSAC;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Get, "VSACValueSet")]
    public class GetVSACValueSetCommand : BaseCommand
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The OID for the value set (NOT including \"urn:oid:\", just the OID itself)"
        )]
        public string Oid { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The username to authenticate with VSAC"
        )]
        public string VSACUsername { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The password to authenticate with VSAC"
        )]
        public string VSACPassword { get; set; }

        protected override void ProcessRecord()
        {
            VSACImporter importer = new VSACImporter(this.tdb);
            importer.Authenticate(this.VSACUsername, this.VSACPassword);

            if (importer.ImportValueSet(this.Oid))
                this.WriteObject("Successfully imported value set");
            else
                this.WriteObject("No value set imported");
        }
    }
}
