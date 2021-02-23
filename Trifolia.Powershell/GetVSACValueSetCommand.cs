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
            HelpMessage = "The API Key to authenticate with UMLS/VSAC"
        )]
        public string UMLSApiKey { get; set; }

        protected override void ProcessRecord()
        {
            VSACImporter importer = new VSACImporter(this.tdb);
            importer.Authenticate(this.UMLSApiKey);

            if (importer.ImportValueSet(this.Oid))
                this.WriteObject("Successfully imported value set");
            else
                this.WriteObject("No value set imported");
        }
    }
}
