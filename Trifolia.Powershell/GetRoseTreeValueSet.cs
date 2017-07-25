using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Trifolia.DB;
using Trifolia.Shared.Plugins;
using Trifolia.Shared;
using Trifolia.Import.Terminology.External;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Get, "RoseTreeValueSet")]
    public class GetRoseTreeValueSet : BaseCommand
    {
        [Parameter(
            HelpMessage = "The OID of the value set"
        )]
        public string Oid { get; set; }

        protected override void ProcessRecord()
        {
            HL7RIMValueSetImportProcessor<ImportValueSet, ImportValueSetMember> processor = new HL7RIMValueSetImportProcessor<ImportValueSet, ImportValueSetMember>();

            if (!string.IsNullOrEmpty(this.Oid))
            {
                this.WriteVerbose("Finding value set by oid");
                ImportValueSet valueSet = processor.FindValueSet(this.tdb, this.Oid);

                this.WriteVerbose("Found value set");
                this.WriteObject(valueSet);
            }
            else
            {
                this.WriteVerbose("Finding all value sets");
                List<ImportValueSet> valueSets = processor.FindValueSets(this.tdb);

                this.WriteVerbose("Done");
                this.WriteObject(valueSets);
            }
        }
    }
}
