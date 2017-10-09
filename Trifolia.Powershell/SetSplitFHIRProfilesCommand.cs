using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Trifolia.DB;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Set, "SplitFHIRProfiles")]
    public class SetSplitFHIRProfilesCommand : BaseCommand
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The path to the profiles-resources.xml file.")]
        public string ProfilesXmlPath { get; set; }

        [Parameter(
            Mandatory = true,
            HelpMessage = "The directory to store the individual profile XML files extracted from the ProfilesXmlPath file.")]
        public string OutputDirectory { get; set; }

        protected override void ProcessRecord()
        {
            string profilesXmlPath = Path.Combine(this.SessionState.Path.CurrentLocation.Path, this.ProfilesXmlPath);

            using (StreamReader sr = new StreamReader(profilesXmlPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(sr);

                XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("fhir", "http://hl7.org/fhir");

                var nodes = doc.SelectNodes("/fhir:Bundle/fhir:entry/fhir:resource/fhir:StructureDefinition[fhir:kind/@value='resource']", nsManager);

                foreach (XmlElement element in nodes)
                {
                    string fileName = string.Format("profile-{0}.xml", element.SelectSingleNode("fhir:id/@value", nsManager).Value);
                    string outputPath = Path.Combine(this.SessionState.Path.CurrentLocation.Path, this.OutputDirectory, fileName);

                    this.WriteVerbose("Creating " + fileName);

                    XmlDocument newDoc = new XmlDocument();
                    newDoc.LoadXml(element.OuterXml);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        XmlTextWriter xmlWriter = new XmlTextWriter(ms, Encoding.UTF8);
                        xmlWriter.Formatting = Formatting.Indented;

                        newDoc.WriteContentTo(xmlWriter);
                        xmlWriter.Flush();
                        ms.Flush();
                        ms.Position = 0;

                        StreamReader newDocReader = new StreamReader(ms);
                        string output = newDocReader.ReadToEnd();

                        File.WriteAllText(outputPath, output);
                    }
                }
            }
        }
    }
}
