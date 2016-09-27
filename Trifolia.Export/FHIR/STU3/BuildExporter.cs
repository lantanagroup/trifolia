extern alias fhir_stu3;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Trifolia.DB;
using Ionic.Zip;
using Newtonsoft.Json;
using Trifolia.Shared;
using fhir_stu3.Hl7.Fhir.Serialization;
using System.Text.RegularExpressions;

namespace Trifolia.Export.FHIR.STU3
{
    public class BuildExporter
    {
        private const string STU3_FHIR_BUILD_PACKAGE = "Trifolia.Export.FHIR.STU3.package.zip";

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
            get
            {
                return this.control;
            }
        }

        #region Constructors

        public BuildExporter(IObjectRepository tdb, int implementationGuideId, IEnumerable<Template> templates = null)
        {
            this.tdb = tdb;

            this.ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);
            this.templates = templates == null ? this.ig.GetRecursiveTemplates(this.tdb) : templates;

            this.schema = this.ig.ImplementationGuideType.GetSimpleSchema();
            this.igName = this.ig.Name.Replace(" ", "_");
            this.controlFileName = this.igName + ".json";
        }

        public BuildExporter(int implementationGuideId)
            : this(DBContext.Create(), implementationGuideId)
        {

        }

        #endregion

        public byte[] Export(bool includeVocabulary = true)
        {
            this.control = new Models.Control();
            this.control.canonicalBase = this.ig.Identifier;

            if (!string.IsNullOrEmpty(this.control.canonicalBase) && this.control.canonicalBase.LastIndexOf("/") == this.control.canonicalBase.Length - 1)
                this.control.canonicalBase = this.control.canonicalBase.Substring(0, this.control.canonicalBase.Length - 1);

            this.zip = ZipFile.Read(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(STU3_FHIR_BUILD_PACKAGE));

            this.AddImplementationGuide(includeVocabulary);

            this.AddTemplates();

            if (includeVocabulary)
                this.AddValueSets();

            this.AddVolume1Pages();

            this.AddAuthorPage();

            this.AddDescriptionPage();

            this.AddCodeSystemsPage();

            this.AddValueSetsPage();

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

        private void AddValueSetsPage()
        {
            string valueSetsContent = string.Empty;

            var valueSets = (from t in this.templates
                             join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                             where tc.ValueSet != null
                             select tc.ValueSet).Distinct();

            if (valueSets.Any())
            {
                valueSetsContent += "<h3>Value Sets</h3>\n" +
                    "<p>This guide references the following value sets.</p>\n" +
                    "<table class=\"codes\"><thead><tr><td><b>Name</b></td><td><b>Definition</b></td></tr></thead><tbody>";
            }

            foreach (var valueSet in valueSets)
            {
                string definition = !string.IsNullOrEmpty(valueSet.Description) ? string.Format("<u>{0}</u><br/>\n{1}", valueSet.Oid, valueSet.Description) : valueSet.Oid;
                valueSetsContent += string.Format("<tr><td>{0}</td><td>{1}</td></tr>", valueSet.Name, definition);
            }

            if (!string.IsNullOrEmpty(valueSetsContent))
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

            if (codeSystems.Count() > 0)
            {
                codeSystemsContent += "<h3>Code Systems</h3>\n" + 
                    "<p>This guide references the following code systems.</p>\n" +
                    "<table class=\"codes\"><thead><tr><td><b>Name</b></td><td><b>Definition</b></td></tr></thead><tbody>";
            }

            foreach (var codeSystem in codeSystems)
            {
                string definition = !string.IsNullOrEmpty(codeSystem.Description) ? string.Format("<u>{0}</u><br/>\n{1}", codeSystem.Oid, codeSystem.Description) : codeSystem.Oid;
                codeSystemsContent += string.Format("<tr><td>{0}</td><td>{1}</td></tr>", codeSystem.Name, definition);
            }

            if (!string.IsNullOrEmpty(codeSystemsContent))
                codeSystemsContent += "</tbody></table>";

            this.zip.AddEntry("pages/_includes/codesystems.html", RemoveSpecialCharacters(codeSystemsContent));
        }

        private void AddDescriptionPage()
        {
            string descriptionContent = this.ig.WebDescription != null ? this.ig.WebDescription : string.Empty;
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

        private void AddVolume1Pages()
        {
            string volume1Content = "";

            foreach (var section in this.ig.Sections)
            {
                volume1Content += string.Format("<div><h{0}>{1}</h{0}>{2}</div>\n", section.Level + 2, section.Heading, section.Content);
            }
            
            this.zip.AddEntry("pages/_includes/volume1.html", RemoveSpecialCharacters(volume1Content));
        }

        private void AddValueSets()
        {
            var valueSets = (from t in templates
                             join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                             where tc.ValueSet != null
                             select tc.ValueSet).Distinct();
            ValueSetExporter exporter = new ValueSetExporter(this.tdb);

            foreach (var valueSet in valueSets)
            {
                var fhirValueSet = exporter.Convert(valueSet);
                fhirValueSet.Id = valueSet.Id.ToString();
                var fhirValueSetContent = FhirSerializer.SerializeResourceToXml(fhirValueSet);
                var fhirValueSetFileName = string.Format("resources/valueset/{0}.xml", valueSet.Id);

                this.zip.AddEntry(fhirValueSetFileName, fhirValueSetContent);
            }
        }

        private void AddImplementationGuide(bool includeVocabulary)
        {
            ImplementationGuideExporter igExporter = new ImplementationGuideExporter(this.tdb, this.schema, "http", "test.com");
            var fhirIg = igExporter.Convert(this.ig, includeVocabulary: includeVocabulary);

            var fhirIgContent = FhirSerializer.SerializeResourceToXml(fhirIg);
            string fhirIgFileName = string.Format("resources/implementationguide/ImplementationGuide_{0}.xml", ig.Id);

            this.control.source = "implementationguide/ImplementationGuide_" + ig.Id.ToString() + ".xml";

            this.zip.AddEntry(fhirIgFileName, fhirIgContent);
        }

        private void AddTemplates()
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
                var strucDefContent = FhirSerializer.SerializeResourceToXml(strucDef);
                string strucDefFileName = "resources/structuredefinition/" + template.FhirId() + ".xml";
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
