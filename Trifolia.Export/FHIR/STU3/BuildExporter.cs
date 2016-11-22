extern alias fhir_stu3;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Trifolia.DB;
using Trifolia.Logging;
using Ionic.Zip;
using Newtonsoft.Json;
using Trifolia.Shared;
using fhir_stu3.Hl7.Fhir.Serialization;
using System.Text.RegularExpressions;
using Trifolia.Config;
using System.Net;

namespace Trifolia.Export.FHIR.STU3
{
    public class BuildExporter
    {
        private const string STU3_FHIR_BUILD_PACKAGE = "Trifolia.Export.FHIR.STU3.package.zip";
        private const string IG_PUBLISHER_JAR_NAME = "org.hl7.fhir.igpublisher.jar";

        private IObjectRepository tdb;
        private ImplementationGuide ig;
        private ZipFile zip;
        private SimpleSchema schema;
        private Models.Control control;
        private string igName;
        private string controlFileName;
        private IEnumerable<Template> templates;

        private static Regex RemoveSpecialCharactersRegex = new Regex(@"[^\u0000-\u007F]+", RegexOptions.Compiled);

        public Models.Control Control
        {
            get { return this.control; }
        }

        public bool JsonFormat { get; set; }

        #region Constructors

        public BuildExporter(IObjectRepository tdb, int implementationGuideId, IEnumerable<Template> templates = null, bool jsonFormat = false)
        {
            this.tdb = tdb;

            this.ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);
            this.templates = templates ?? this.ig.GetRecursiveTemplates(this.tdb);

            this.schema = this.ig.ImplementationGuideType.GetSimpleSchema();
            this.igName = this.ig.Name.Replace(" ", "_");
            this.controlFileName = this.igName + ".json";
            this.JsonFormat = jsonFormat;
        }

        public BuildExporter(int implementationGuideId)
            : this(DBContext.Create(), implementationGuideId)
        {
        }

        #endregion

        private ZipFile GetPackage()
        {
            if (!string.IsNullOrEmpty(AppSettings.FhirSTU3Package) && File.Exists(AppSettings.FhirSTU3Package))
                return ZipFile.Read(AppSettings.FhirSTU3Package);

            return ZipFile.Read(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(STU3_FHIR_BUILD_PACKAGE));
        }

        public byte[] Export(bool includeVocabulary = true)
        {
            this.control = new Models.Control();
            this.control.canonicalBase = this.ig.Identifier;

            if (!string.IsNullOrEmpty(this.control.canonicalBase) && this.control.canonicalBase.LastIndexOf("/") == this.control.canonicalBase.Length - 1)
                this.control.canonicalBase = this.control.canonicalBase.Substring(0, this.control.canonicalBase.Length - 1);

            this.zip = GetPackage();

            this.AddImplementationGuide(includeVocabulary);

            this.AddProfiles();

            if (includeVocabulary)
                this.AddValueSets();

            this.AddOverviewPage();

            this.AddAuthorPage();

            this.AddDescriptionPage();

            this.AddCodeSystemsPage();

            this.AddValueSetsPage();

            this.AddExtensionsPage();

            this.AddResourceInstances();

            this.AddExamples();

            this.AddControl();

            this.AddBatch();

            this.UpdateBuildXml();

            try
            {
                string tempFileName = Path.GetTempFileName();
                this.zip.Save(tempFileName);
                byte[] retBytes = File.ReadAllBytes(tempFileName);
                File.Delete(tempFileName);

                return retBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving/reading zip package for generated FHIR build", ex);
            }
        }

        /// <summary>
        /// Adds individual FHIR Resource Instance files attached to the implementation guide being exported
        /// to the ZIP package
        /// </summary>
        /// <remarks>Uses the mime-type of the file to determine if the attached file is xml or json.</remarks>
        private void AddResourceInstances()
        {
            var parserSettings = new fhir_stu3.Hl7.Fhir.Serialization.ParserSettings();
            parserSettings.AcceptUnknownMembers = true;
            parserSettings.AllowUnrecognizedEnums = true;
            parserSettings.DisallowXsiAttributesOnRoot = false;
            var fhirXmlParser = new FhirXmlParser(parserSettings);
            var fhirJsonParser = new FhirJsonParser(parserSettings);

            // Check that each FHIR resource instance is valid and has the required fields
            foreach (var file in ig.Files)
            {
                var fileData = file.GetLatestData();
                fhir_stu3.Hl7.Fhir.Model.Resource resource = null;
                string resourceContent;
                string fileExtension = "";

                try
                {
                    string fileContent = System.Text.Encoding.UTF8.GetString(fileData.Data);

                    if (file.MimeType == "application/xml" || file.MimeType == "text/xml")
                    {
                        resource = fhirXmlParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                        fileExtension = "xml";
                    }
                    else if (file.MimeType == "application/json" || file.MimeType == "binary/octet-stream")
                    {
                        resource = fhirJsonParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                        fileExtension = "json";
                    }
                }
                catch
                {
                }

                if (resource == null || string.IsNullOrEmpty(resource.Id))
                    continue;

                try
                {
                    // Convert the resource to the desired format for output
                    if (this.JsonFormat)
                        resourceContent = FhirSerializer.SerializeResourceToJson(resource);
                    else
                        resourceContent = FhirSerializer.SerializeResourceToXml(resource);

                    fileExtension = this.JsonFormat ? "json" : "xml";
                    string fileName = string.Format("resources/{0}/{1}.{2}", resource.ResourceType.ToString(), resource.Id, fileExtension);

                    // Add the resource to the zip file
                    this.zip.AddEntry(fileName, resourceContent);
                }
                catch
                {
                    string fileName = string.Format("resources/{0}/{1}.{2}", resource.ResourceType.ToString(), resource.Id, fileExtension);
                    this.zip.AddEntry(fileName, fileData.Data);
                }
            }
        }

        /// <summary>
        /// Adds examples for the profiles to the ZIP package. Only adds valid samples to the zip package.
        /// </summary>
        /// <remarks>Attempts to use the fhir-net-api to parse the resource as Xml and then as Json. If the sample 
        /// cannot be parsed successfully, then it is skipped and not added to the ZIP package.</remarks>
        private void AddExamples()
        {
            // Validate that each of the samples associated with profiles has the required fields
            var templateExamples = (from t in this.templates
                                    join ts in this.tdb.TemplateSamples on t.Id equals ts.TemplateId
                                    select new { Template = t, Sample = ts });
            var parserSettings = new fhir_stu3.Hl7.Fhir.Serialization.ParserSettings();
            parserSettings.AcceptUnknownMembers = true;
            parserSettings.AllowUnrecognizedEnums = true;
            parserSettings.DisallowXsiAttributesOnRoot = false;
            var fhirXmlParser = new FhirXmlParser(parserSettings);
            var fhirJsonParser = new FhirJsonParser(parserSettings);

            foreach (var templateExample in templateExamples)
            {
                fhir_stu3.Hl7.Fhir.Model.Resource resource = null;
                string fileExtension = "";

                try
                {
                    resource = fhirXmlParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(templateExample.Sample.XmlSample);
                    fileExtension = "xml";
                }
                catch
                {
                }

                try
                {
                    if (resource == null)
                    {
                        resource = fhirJsonParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(templateExample.Sample.XmlSample);
                        fileExtension = "json";
                    }
                }
                catch
                {
                }

                if (resource == null || string.IsNullOrEmpty(resource.Id))
                    continue;

                string fileName = string.Format("resources/{0}/{1}.{2}", resource.ResourceType.ToString(), resource.Id, fileExtension);
                this.zip.AddEntry(fileName, templateExample.Sample.XmlSample);
            }
        }

        private void AddValueSetsPage()
        {
            string valueSetsContent = string.Empty;

            var valueSets = (from t in this.templates
                             join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                             where tc.ValueSet != null
                             select tc.ValueSet).Distinct();

            if (valueSets.Any())
            {
                valueSetsContent += "<p>This guide references the following value sets.</p>\n" +
                    "<table class=\"codes\"><thead><tr><td><b>Name</b></td><td><b>Definition</b></td></tr></thead><tbody>";
            }
            else
            {
                valueSetsContent += "<p>This guide does not reference any value sets.</p>";
            }

            foreach (var valueSet in valueSets)
            {
                string definition = !string.IsNullOrEmpty(valueSet.Description) ? string.Format("<u>{0}</u><br/>\n{1}", valueSet.Oid, valueSet.Description) : valueSet.Oid;
                valueSetsContent += string.Format("<tr><td><a href=\"ValueSet-{0}.html\">{1}</a></td><td>{2}</td></tr>", valueSet.GetFhirId(), valueSet.Name, definition);
            }

            if (valueSets.Any())
                valueSetsContent += "</tbody></table>";

            this.zip.AddEntry("pages/_includes/valuesets.html", RemoveSpecialCharacters(valueSetsContent));
        }

        private void AddCodeSystemsPage()
        {
            string codeSystemsContent = string.Empty;
            var codeSystems = (from t in this.templates
                               join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                               where tc.CodeSystem != null
                               select tc.CodeSystem).Union(
                                from t in this.templates
                                join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                                join vs in this.tdb.ValueSets on tc.ValueSetId equals vs.Id
                                join vsm in this.tdb.ValueSetMembers on vs.Id equals vsm.ValueSetId
                                select vsm.CodeSystem)
                .Distinct();

            if (codeSystems.Any())
            {
                codeSystemsContent += "<p>This guide references the following code systems.</p>\n" +
                    "<table class=\"codes\"><thead><tr><td><b>Name</b></td><td><b>Definition</b></td></tr></thead><tbody>";
            }
            else
            {
                codeSystemsContent += "<p>This guide does not reference any code systems.</p>";
            }

            foreach (var codeSystem in codeSystems)
            {
                string definition = !string.IsNullOrEmpty(codeSystem.Description) ? string.Format("<u>{0}</u><br/>\n{1}", codeSystem.Oid, codeSystem.Description) : codeSystem.Oid;
                codeSystemsContent += string.Format("<tr><td>{0}</td><td>{1}</td></tr>", codeSystem.Name, definition);
            }

            if (codeSystems.Any())
                codeSystemsContent += "</tbody></table>";

            this.zip.AddEntry("pages/_includes/codesystems.html", RemoveSpecialCharacters(codeSystemsContent));
        }
        
        private void AddExtensionsPage()
        {
            string extensionsContent = string.Empty;
            var extensionTemplates = (from t in this.templates
                                      where t.TemplateType.RootContextType == "Extension"
                                      select t);

            if (extensionTemplates.Any())
            {
                extensionsContent += "<p>This guide references the following extensions.</p>\n" +
                    "<table class=\"codes\"><thead><tr><td><b>Name</b></td><td><b>Definition</b></td></tr></thead><tbody>";
            }
            else
            {
                extensionsContent += "<p>This guide does not reference any extensions</p>";
            }

            foreach (var extensionTemplate in extensionTemplates)
            {
                extensionsContent += string.Format("<tr><td>{0}</td><td>{1}</td></tr>", extensionTemplate.Name, extensionTemplate.Oid);
            }

            if (extensionTemplates.Any())
                extensionsContent += "</tbody></table>";

            this.zip.AddEntry("pages/_includes/extensions.html", RemoveSpecialCharacters(extensionsContent));
        }

        private void AddDescriptionPage()
        {
            string descriptionContent = this.ig.WebDescription ?? string.Empty;
            this.zip.AddEntry("pages/_includes/description.html", RemoveSpecialCharacters(descriptionContent));
        }

        private void AddAuthorPage()
        {
            var authors = this.templates.Select(y => y.Author).Distinct();
            string authorsContent = string.Empty;

            if (authors.Any())
            {
                authorsContent += "<h3>Authors</h3>\n";
                authorsContent += "<table class=\"grid\"><thead><tr><td><b>Author Name</b></td><td><b>Email</b></td></tr></thead><tbody>\n";
            }

            foreach (var author in authors)
            {
                authorsContent += string.Format("<tr><td>{0} {1}</td><td>{2}</td></tr>", author.FirstName, author.LastName, author.Email);
            }

            if (!string.IsNullOrEmpty(authorsContent))
                authorsContent += "</tbody></table>";

            this.zip.AddEntry("pages/_includes/authors.html", RemoveSpecialCharacters(authorsContent));
        }

        private void UpdateBuildXml()
        {
            var buildXmlEntry = this.zip["build.xml"];

            using (MemoryStream ms = new MemoryStream())
            {
                buildXmlEntry.Extract(ms);
                ms.Position = 0;

                var sr = new StreamReader(ms);
                var xml = sr.ReadToEnd();

                xml = xml.Replace("##ig_name##", this.igName);
                xml = xml.Replace("##specification##", this.control.paths.specification);

                this.zip.UpdateEntry("build.xml", xml);
            }
        }

        private void AddBatch()
        {
            string batchContent = "java -jar org.hl7.fhir.igpublisher.jar -ig %~dp0" + this.controlFileName;

            this.zip.AddEntry("RunIGPublisherCmd.bat", batchContent);
        }

        private void AddControl()
        {
            string controlContent = JsonConvert.SerializeObject(control, Formatting.Indented);

            this.zip.AddEntry(this.controlFileName, controlContent);
        }

        private void AddOverviewPage()
        {
            string volume1Content = "";

            foreach (var section in this.ig.Sections)
            {
                volume1Content += string.Format("<div><h{0}>{1}</h{0}>{2}</div>\n", section.Level + 2, section.Heading, section.Content);
            }

            this.zip.AddEntry("pages/_includes/overview.html", RemoveSpecialCharacters(volume1Content));
        }

        private void AddValueSets()
        {
            var valueSets = (from t in templates
                             join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                             where tc.ValueSet != null
                             select tc.ValueSet).Distinct();
            ValueSetExporter exporter = new ValueSetExporter(this.tdb);
            string fileExtension = this.JsonFormat ? "json" : "xml";

            foreach (var valueSet in valueSets)
            {
                var fhirValueSet = exporter.Convert(valueSet);
                var fhirValueSetContent = this.JsonFormat ? FhirSerializer.SerializeResourceToJson(fhirValueSet) : FhirSerializer.SerializeResourceToXml(fhirValueSet);
                var fhirValueSetFileName = string.Format("resources/valueset/{0}.{1}", valueSet.Id, fileExtension);

                // Add the value set file to the package
                this.zip.AddEntry(fhirValueSetFileName, fhirValueSetContent);

                // Add the value set to the control file so it knows what to do with it
                string resourceKey = "ValueSet/" + valueSet.GetFhirId();

                this.control.resources.Add(resourceKey, new Models.Control.ResourceReference()
                {
                    template_base = "instance-template-format.html",
                    reference_base = string.Format("ValueSet-{0}.html", valueSet.GetFhirId())
                });
            }
        }

        private void AddImplementationGuide(bool includeVocabulary)
        {
            ImplementationGuideExporter igExporter = new ImplementationGuideExporter(this.tdb, this.schema, "http", "test.com");
            var fhirIg = igExporter.Convert(this.ig, includeVocabulary: includeVocabulary);

            var fhirIgContent = this.JsonFormat ? FhirSerializer.SerializeResourceToJson(fhirIg) : FhirSerializer.SerializeResourceToXml(fhirIg);
            string fileExtension = this.JsonFormat ? "json" : "xml";
            string fhirIgFileName = string.Format("resources/ImplementationGuide/ImplementationGuide_{0}.{1}", ig.Id, fileExtension);

            this.control.source = "ImplementationGuide/ImplementationGuide_" + ig.Id.ToString() + ".xml";

            this.zip.AddEntry(fhirIgFileName, fhirIgContent);
        }

        private void AddProfiles()
        {
            List<TemplateType> templateTypes = this.templates.Select(y => y.TemplateType).Distinct().ToList();

            foreach (var templateType in templateTypes)
            {
                control.defaults.Add(templateType.RootContextType, new Models.Control.TemplateReference("instance-template-format-example.html", "instance-template-example.html"));
            }

            foreach (var template in this.templates)
            {
                string location = string.Format("StructureDefinition/{0}", template.FhirId());
                var templateSchema = this.schema.GetSchemaFromContext(template.PrimaryContextType);

                StructureDefinitionExporter sdExporter = new StructureDefinitionExporter(this.tdb, "http", "test.com");
                var strucDef = sdExporter.Convert(template, templateSchema);
                var strucDefContent = this.JsonFormat ? FhirSerializer.SerializeResourceToJson(strucDef) : FhirSerializer.SerializeResourceToXml(strucDef);
                string fileExtension = this.JsonFormat ? "json" : "xml";
                string strucDefFileName = "resources/StructureDefinition/" + template.FhirId() + "." + fileExtension;
                this.zip.AddEntry(strucDefFileName, strucDefContent);

                control.resources.Add(location, new Models.Control.ResourceReference("instance-template-sd-no-example.html", "StructureDefinition_" + template.FhirId() + ".html"));
            }
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return RemoveSpecialCharactersRegex.Replace(str, "");
        }
    }
}
