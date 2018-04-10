using System.IO;
using System.Linq;
using System.Management.Automation;
using Trifolia.DB;
using Trifolia.Export.Schematron;
using Trifolia.Shared;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Get, "TrifoliaSchematron")]
    public class SchematronCommand : BaseCommand
    {
        [Parameter(
            Mandatory = false,
            HelpMessage = "The file name/location to output the export to. If not specified, will be output to console"
        )]
        public string OutputFileName { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The id of the implementation guide to export"
        )]
        public int ImplementationGuideId { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "The name of the file that should be used to reference vocabulary in the schematron (ex: voc.xml)"
        )]
        public string VocabFileName { get; set; }

        public SchematronCommand()
        {
            this.VocabFileName = "voc.xml";
        }

        protected override void ProcessRecord()
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == this.ImplementationGuideId);
            SimpleSchema schema = ig.ImplementationGuideType.GetSimpleSchema();
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, ig.Id);
            var templates = ig.GetRecursiveTemplates(this.tdb);

            string schematron = SchematronGenerator.Generate(this.tdb, ig, true, VocabularyOutputType.Default, this.VocabFileName, templates);

            if (!string.IsNullOrEmpty(this.OutputFileName))
            {
                File.WriteAllText(this.OutputFileName, schematron);
            }
            else
            {
                this.WriteObject(schematron);
            }
        }
    }
}
