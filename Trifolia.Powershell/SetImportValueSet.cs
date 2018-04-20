using System.Management.Automation;
using Trifolia.Import.Terminology.External;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Set, "ImportValueSet")]
    public class SetImportValueSet : BaseCommand
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The ImportValueSet object to persist"
        )]
        public ImportValueSet ValueSet { get; set; }

        protected override void ProcessRecord()
        {
            HL7RIMValueSetImportProcessor<ImportValueSet, ImportValueSetMember> processor = new HL7RIMValueSetImportProcessor<ImportValueSet, ImportValueSetMember>();
            processor.SaveValueSet(this.tdb, this.ValueSet);
            this.WriteVerbose("Loaded Value Set into EF object model");
            this.tdb.SaveChanges();
            this.WriteVerbose("Saved EF object model");
        }
    }
}
