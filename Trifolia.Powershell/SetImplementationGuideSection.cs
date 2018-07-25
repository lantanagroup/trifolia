using System.Management.Automation;
using Trifolia.DB;
using System.IO;
using System;
using System.Linq;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Set, "ImplementationGuideSection")]
    public class SetImplementationGuideSectionCommand : BaseCommand
    {
        [Parameter(
            HelpMessage = "A directory containing /<IG_ID>/<SECTION_NAME>.txt structure of files that should be imported into Trifolia (overwriting existing section text)"
        )]
        public string RootDirectory { get; set; }

        protected override void ProcessRecord()
        {
            if (!string.IsNullOrEmpty(this.RootDirectory))
            {
                var igDirectories = Directory.GetDirectories(this.RootDirectory);

                foreach (var igDirectory in igDirectories)
                {
                    FileInfo igDirectoryInfo = new FileInfo(igDirectory);
                    int implementationGuideId = Int32.Parse(igDirectoryInfo.Name);

                    ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

                    var sectionFiles = Directory.GetFiles(igDirectory, "*.txt");

                    foreach (var sectionFile in sectionFiles)
                    {
                        FileInfo sectionFileInfo = new FileInfo(sectionFile);
                        string sectionName = sectionFileInfo.Name.Substring(0, sectionFileInfo.Name.Length - sectionFileInfo.Extension.Length);

                        ImplementationGuideSection section = ig.Sections.Single(y => y.Heading.Trim().ToLower() == sectionName.Trim().ToLower());
                        section.Content = File.ReadAllText(sectionFile);

                        this.WriteVerbose("Updating implementation guide " + ig.Id + " section " + section.Id + " (" + section.Heading + ")");
                    }
                }

                this.tdb.SaveChanges();
            }
        }
    }
}
