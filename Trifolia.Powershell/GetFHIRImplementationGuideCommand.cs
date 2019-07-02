extern alias fhir_r4;

using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml;
using Trifolia.DB;
using Trifolia.Export.FHIR.R4;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Powershell
{
    [Cmdlet(VerbsCommon.Get, "FHIRImplementationGuide")]
    public class GetFHIRImplementationGuideCommand : BaseCommand
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The ID of the template/profile to retrieve as StructureDefinition"
        )]
        public int ImplementationGuideId { get; set; }

        [Parameter(HelpMessage = "Whether or not to return the StructureDefinition as JSON")]
        public bool ReturnJson { get; set; }

        [Parameter(HelpMessage = "The file name to save the StructureDefinition to. Otherwise, returns the content.")]
        public string FileName { get; set; }

        public static string PrintXML(string xml)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                // Handle the exception
            }

            mStream.Close();
            writer.Close();

            return result;
        }

        protected override void ProcessRecord()
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == this.ImplementationGuideId);

            this.WriteDebug("Getting/converting implementation guide" + ig.Id.ToString() + " with name " + ig.Name);

            ImplementationGuideExporter exporter = new ImplementationGuideExporter(this.tdb, ig.ImplementationGuideType.GetSimpleSchema(), null, null);
            var exported = exporter.Convert(ig);

            string xml = PrintXML(fhir_r4.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToXml(exported));

            if (!string.IsNullOrEmpty(this.FileName))
            {
                this.WriteDebug("Writing to file name " + this.FileName);
                File.WriteAllText(this.FileName, xml);
            }
            else
                this.WriteObject(xml);
        }
    }
}
