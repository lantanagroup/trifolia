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

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Get, "TrifoliaExport")]
    public class ExportCommand : BaseCommand
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The format of the export"
        )]
        public ExportFormats Format { get; set; }

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
            HelpMessage = "Include vocabulary in the export"
        )]
        public bool IncludeVocab { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Output the export as JSON"
        )]
        public bool ReturnJson { get; set; }

        protected override void ProcessRecord()
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == this.ImplementationGuideId);
            SimpleSchema schema = ig.ImplementationGuideType.GetSimpleSchema();
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, ig.Id);
            var templates = ig.GetRecursiveTemplates(this.tdb);
            byte[] export = ig.ImplementationGuideType.GetPlugin().Export(this.tdb, schema, this.Format, igSettings, null, templates, this.IncludeVocab, this.ReturnJson);

            if (!string.IsNullOrEmpty(this.OutputFileName))
            {
                File.WriteAllBytes(this.OutputFileName, export);
            }
            else
            {
                this.WriteObject(export);
            }
        }
    }
}
