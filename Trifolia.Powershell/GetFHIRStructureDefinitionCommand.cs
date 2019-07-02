extern alias fhir_r4;
using fhir_r4.Hl7.Fhir.Model;
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
    [Cmdlet(VerbsCommon.Get, "FHIRStructureDefinition")]
    public class GetFHIRStructureDefinitionCommand : BaseCommand
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The ID of the template/profile to retrieve as StructureDefinition"
        )]
        public int TemplateId { get; set; }

        [Parameter(HelpMessage = "Whether or not to return the StructureDefinition as JSON")]
        public bool ReturnJson { get; set; }

        [Parameter(HelpMessage = "The file name to save the StructureDefinition to. Otherwise, returns the content.")]
        public string FileName { get; set; }

        [Parameter(HelpMessage = "The base URL of the implementation guide")]
        public string BaseImplementationGuideUrl { get; set; }

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
            Template template = this.tdb.Templates.SingleOrDefault(y => y.Id == this.TemplateId);

            this.WriteDebug("Getting/converting template " + template.Id.ToString() + " with name " + template.Name);

            StructureDefinitionExporter exporter = new StructureDefinitionExporter(this.tdb, null, null);
            var fhirSchema = template.ImplementationGuideType.GetSimpleSchema();
            var templateSchema = fhirSchema.GetSchemaFromContext(template.PrimaryContextType);
            var exported = exporter.Convert(template, templateSchema);
            bool isCDA = fhirSchema.Namespaces.ToList().Exists(y => y.Key == "urn:hl7-org:v3");

            if (!string.IsNullOrEmpty(this.BaseImplementationGuideUrl))
                exported.Url = this.BaseImplementationGuideUrl + (this.BaseImplementationGuideUrl.EndsWith("/") ? "" : "/") + "StructureDefinition/" + exported.Id;

            if (isCDA)
            {
                if (template.ImpliedTemplate == null)
                    exported.BaseDefinition = "http://hl7.org/fhir/cda/StructureDefinition/" + template.PrimaryContextType;
                else
                {
                    string impliedOid, impliedVersion;

                    if (template.ImpliedTemplate.GetIdentifierII(out impliedOid, out impliedVersion))
                        exported.BaseDefinition = this.BaseImplementationGuideUrl + (this.BaseImplementationGuideUrl.EndsWith("/") ? "" : "/") + "StructureDefinition/" + impliedOid;
                    else if (template.ImpliedTemplate.GetIdentifierOID(out impliedOid))
                        exported.BaseDefinition = this.BaseImplementationGuideUrl + (this.BaseImplementationGuideUrl.EndsWith("/") ? "" : "/") + "StructureDefinition/" + impliedOid;
                    else if (template.ImpliedTemplate.GetIdentifierURL(out impliedOid))
                        exported.BaseDefinition = impliedOid;
                }

                foreach (var element in exported.Differential.Element)
                {
                    if (element.Binding != null && element.Binding.ValueSet != null)
                    {
                        if (element.Binding.ValueSet.StartsWith("urn:oid:"))
                            element.Binding.ValueSet = "https://cts.nlm.nih.gov/fhir/ValueSet/" + element.Binding.ValueSet.Substring("urn:oid:".Length);
                        else if (element.Binding.ValueSet.StartsWith("urn:hl7ii:"))
                            element.Binding.ValueSet = "https://cts.nlm.nih.gov/fhir/ValueSet/" + element.Binding.ValueSet.Substring("urn:hl7ii:".Length, element.Binding.ValueSet.LastIndexOf(":") - "urn:hl7ii:".Length);

                        this.WriteWarning(element.Binding.ValueSet);
                    }

                    foreach (var type in element.Type)
                    {
                        if (isCDA)
                            type.Code = "http://hl7.org/fhir/cda/StructureDefinition/" + type.Code;

                        string root, extension;

                        var profiles = type.Profile.ToList();

                        for (var i = 0; i < profiles.Count; i++)
                        {
                            if (!profiles[i].StartsWith("urn:"))
                                continue;

                            Template fakeTemplate = new Template();
                            fakeTemplate.Oid = profiles[i];

                            if (fakeTemplate.GetIdentifierOID(out root))
                                profiles[i] = this.BaseImplementationGuideUrl + (this.BaseImplementationGuideUrl.EndsWith("/") ? "" : "/") + "StructureDefinition/" + root;
                            else if (fakeTemplate.GetIdentifierII(out root, out extension))
                                profiles[i] = this.BaseImplementationGuideUrl + (this.BaseImplementationGuideUrl.EndsWith("/") ? "" : "/") + "StructureDefinition/" + root;
                            else if (fakeTemplate.GetIdentifierURL(out root))
                                profiles[i] = root;
                        }

                        type.Profile = profiles;

                        var targetProfiles = type.TargetProfile.ToList();

                        for (var i = 0; i < targetProfiles.Count; i++)
                        {
                            if (!targetProfiles[i].StartsWith("urn:"))
                                continue;

                            Template fakeTemplate = new Template();
                            fakeTemplate.Oid = targetProfiles[i];

                            if (fakeTemplate.GetIdentifierOID(out root))
                                targetProfiles[i] = this.BaseImplementationGuideUrl + (this.BaseImplementationGuideUrl.EndsWith("/") ? "" : "/") + "StructureDefinition/" + root;
                            else if (fakeTemplate.GetIdentifierII(out root, out extension))
                                targetProfiles[i] = this.BaseImplementationGuideUrl + (this.BaseImplementationGuideUrl.EndsWith("/") ? "" : "/") + "StructureDefinition/" + root;
                            else if (fakeTemplate.GetIdentifierURL(out root))
                                targetProfiles[i] = root;
                        }

                        type.TargetProfile = targetProfiles;
                    }
                }
            }

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
