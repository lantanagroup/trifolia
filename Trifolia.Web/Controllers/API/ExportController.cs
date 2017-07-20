extern alias fhir_dstu1;
extern alias fhir_dstu2;
extern alias fhir_stu3;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Xml;

using NativeExporter = Trifolia.Export.Native.TemplateExporter;
using DecorExporter = Trifolia.Export.DECOR.TemplateExporter;
using Trifolia.Logging;
using Trifolia.Export.Schematron;
using Trifolia.Generation.IG;
using Trifolia.Generation.Green;
using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Web.Models.Export;
using Trifolia.Shared;
using Trifolia.Terminology;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using Ionic.Zip;
using Trifolia.Shared.Plugins;
using System.Web;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Attributes;

namespace Trifolia.Web.Controllers.API
{
    public class ExportController : ApiController
    {
        private IObjectRepository tdb;
        private const string MSWordExportSettingsPropertyName = "MSWordExportSettingsJson";
        private const string DOCX_MIME_TYPE = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        private const string XML_MIME_TYPE = "application/xml";
        private const string JSON_MIME_TYPE = "application/json";
        private const string ZIP_MIME_TYPE = "application/zip";
        private const string XLSX_MIME_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        #region Construct/Dispose

        public ExportController()
            : this(DBContext.Create())
        {
        }

        public ExportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        public HttpResponseMessage Export(ExportSettingsModel model)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == model.ImplementationGuideId);

            if (model.TemplateIds == null)
                model.TemplateIds = ig.GetRecursiveTemplates(this.tdb, categories: model.SelectedCategories.ToArray()).Select(y => y.Id).ToArray();

            List<Template> templates = this.tdb.Templates.Where(y => model.TemplateIds.Contains(y.Id)).ToList();
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, ig.ImplementationGuideType);
            IIGTypePlugin igTypePlugin = IGTypePluginFactory.GetPlugin(ig.ImplementationGuideType);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, model.ImplementationGuideId);
            string fileName;
            byte[] export;
            string contentType = null;

            switch (model.ExportFormat)
            {
                case ExportFormats.FHIR_Build_Package:
                case ExportFormats.FHIR_Bundle:
                case ExportFormats.Native_XML:
                case ExportFormats.Templates_DSTU_XML:
                    fileName = string.Format("{0}.{1}", ig.GetDisplayName(true), model.ReturnJson ? "json" : "xml");
                    export = igTypePlugin.Export(this.tdb, schema, model.ExportFormat, igSettings, model.SelectedCategories, templates, model.IncludeVocabulary, model.ReturnJson);
                    contentType = model.ReturnJson ? JSON_MIME_TYPE : XML_MIME_TYPE;
                    break;

                case ExportFormats.Snapshot_JSON:
                    ImplementationGuideController ctrl = new ImplementationGuideController(this.tdb);
                    var dataModel = ctrl.GetViewData(model.ImplementationGuideId, null, null, true);

                    // Serialize the data to JSON
                    var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                    export = System.Text.Encoding.UTF8.GetBytes(jsonSerializer.Serialize(dataModel));

                    // Set the filename to JSON
                    fileName = string.Format("{0}.json", ig.GetDisplayName(true));
                    contentType = JSON_MIME_TYPE;
                    break;

                case ExportFormats.Microsoft_Word_DOCX:
                    ImplementationGuideGenerator generator = new ImplementationGuideGenerator(this.tdb, model.ImplementationGuideId, model.TemplateIds);
                    fileName = string.Format("{0}.docx", ig.GetDisplayName(true));

                    ExportSettings lConfig = new ExportSettings();
                    lConfig.Use(c =>
                    {
                        c.GenerateTemplateConstraintTable = model.TemplateTables == ExportSettingsModel.TemplateTableOptions.Overview || model.TemplateTables == ExportSettingsModel.TemplateTableOptions.Both;
                        c.GenerateTemplateContextTable = model.TemplateTables == ExportSettingsModel.TemplateTableOptions.Context || model.TemplateTables == ExportSettingsModel.TemplateTableOptions.Both;
                        c.GenerateDocTemplateListTable = model.DocumentTables == ExportSettingsModel.DocumentTableOptions.List || model.DocumentTables == ExportSettingsModel.DocumentTableOptions.Both;
                        c.GenerateDocContainmentTable = model.DocumentTables == ExportSettingsModel.DocumentTableOptions.Containment || model.DocumentTables == ExportSettingsModel.DocumentTableOptions.Both;
                        c.AlphaHierarchicalOrder = model.TemplateSortOrder == ExportSettingsModel.TemplateSortOrderOptions.AlphaHierarchical;
                        c.DefaultValueSetMaxMembers = model.ValueSetTables ? model.MaximumValueSetMembers : 0;
                        c.GenerateValueSetAppendix = model.ValueSetAppendix;
                        c.IncludeXmlSamples = model.IncludeXmlSample;
                        c.IncludeChangeList = model.IncludeChangeList;
                        c.IncludeTemplateStatus = model.IncludeTemplateStatus;
                        c.IncludeNotes = model.IncludeNotes;
                        c.SelectedCategories = model.SelectedCategories;
                    });

                    if (model.ValueSetOid != null && model.ValueSetOid.Length > 0)
                    {
                        Dictionary<string, int> valueSetMemberMaximums = new Dictionary<string, int>();

                        for (int i = 0; i < model.ValueSetOid.Length; i++)
                        {
                            if (valueSetMemberMaximums.ContainsKey(model.ValueSetOid[i]))
                                continue;

                            valueSetMemberMaximums.Add(model.ValueSetOid[i], model.ValueSetMaxMembers[i]);
                        }

                        lConfig.ValueSetMaxMembers = valueSetMemberMaximums;
                    }

                    generator.BuildImplementationGuide(lConfig, ig.ImplementationGuideType.GetPlugin());
                    export = generator.GetDocument();
                    contentType = DOCX_MIME_TYPE;
                    break;

                case ExportFormats.Schematron_SCH:
                    string schematronResult = SchematronGenerator.Generate(tdb, ig, model.IncludeCustomSchematron, VocabularyOutputType.Default, model.VocabularyFileName, templates, model.SelectedCategories, model.DefaultSchematron);
                    export = ASCIIEncoding.UTF8.GetBytes(schematronResult);
                    fileName = string.Format("{0}.sch", ig.GetDisplayName(true));
                    contentType = XML_MIME_TYPE;

                    if (model.IncludeVocabulary)
                    {
                        VocabularyService service = new VocabularyService(tdb, ig.ImplementationGuideType.SchemaURI == "urn:hl7-org:v3");

                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddEntry(fileName, export);

                            string encoding = model.Encoding == ExportSettingsModel.EncodingOptions.UTF8 ? "UTF-8" : "Unicode";

                            //Assuming default settings for vocabulary file
                            string vocXml = service.GetImplementationGuideVocabulary(model.ImplementationGuideId, 0, (int)VocabularyOutputType.Default, encoding);

                            byte[] vocData = ASCIIEncoding.UTF8.GetBytes(vocXml);
                            string vocFileName = string.Format("{0}", model.VocabularyFileName);

                            //Ensuring the extension is present in case input doesn't have it
                            if (vocFileName.IndexOf(".xml") == -1)
                                vocFileName += ".xml";

                            zip.AddEntry(vocFileName, vocData);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                zip.Save(ms);

                                fileName = string.Format("{0}.zip", ig.GetDisplayName(true));
                                contentType = ZIP_MIME_TYPE;
                                export = ms.ToArray();
                            }
                        }
                    }
                    break;

                    /*
                case ExportFormats.Vocabulary_XLSX:
                case ExportFormats.Vocbulary_Single_SVS_XML:
                case ExportFormats.Vocabulary_Multiple_SVS_XML:
                case ExportFormats.Vocabulary_Native_XML:
                    VocabularyService service = new VocabularyService(this.tdb, ig.ImplementationGuideType.SchemaURI == "urn:hl7-org:v3");

                    switch (model.ExportFormat)
                    {
                        case VocabularySettingsModel.ExportFormatTypes.Standard:
                        case VocabularySettingsModel.ExportFormatTypes.SVS:
                        case VocabularySettingsModel.ExportFormatTypes.FHIR:
                            fileType = "xml";
                            contentType = XML_MIME_TYPE;
                            string vocXml = service.GetImplementationGuideVocabulary(model.ImplementationGuideId, model.MaximumMembers, (int)model.ExportFormat, model.Encoding);
                            Encoding encoding = Encoding.GetEncoding(model.Encoding);
                            data = encoding.GetBytes(vocXml);
                            break;
                        case VocabularySettingsModel.ExportFormatTypes.Excel:
                            fileType = "xlsx";
                            contentType = XLSX_MIME_TYPE;
                            data = service.GetImplementationGuideVocabularySpreadsheet(model.ImplementationGuideId, model.MaximumValueSetMembers);
                            break;
                    }

                    fileName = string.Format("{0}.{1}", ig.GetDisplayName(true), fileType);
                    break;
                    */

                default:
                    throw new NotSupportedException();
            }

            return GetExportResponse(fileName, contentType, export);
        }

        #region XML

        [HttpPost, Route("api/Export/XML/Validate"), SecurableAction(SecurableNames.EXPORT_XML)]
        public IEnumerable<string> ValidateXml(XMLSettingsModel model)
        {
            List<string> messages = new List<string>();
            ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == model.ImplementationGuideId);
            var templates = (from t in this.tdb.Templates
                             join tid in model.TemplateIds on t.Id equals tid
                             select t);

            var parserSettings = new fhir_stu3.Hl7.Fhir.Serialization.ParserSettings();
            parserSettings.AcceptUnknownMembers = true;
            parserSettings.AllowUnrecognizedEnums = true;
            parserSettings.DisallowXsiAttributesOnRoot = false;
            var fhirXmlParser = new fhir_stu3.Hl7.Fhir.Serialization.FhirXmlParser(parserSettings);
            var fhirJsonParser = new fhir_stu3.Hl7.Fhir.Serialization.FhirJsonParser(parserSettings);

            if (ig == null)
            {
                messages.Add("Could not find implementation guide with id " + model.ImplementationGuideId);
                return messages;
            }

            switch (model.XmlType)
            {
                case XMLSettingsModel.ExportTypes.FHIRBuild:
                case XMLSettingsModel.ExportTypes.FHIRBuildJSON:
                    // Check that the implementation guide has a base identifier/url
                    if (string.IsNullOrEmpty(ig.Identifier))
                        messages.Add("Implementation guide does not have a base identifier/url.");

                    // Check that each FHIR resource instance is valid and has the required fields
                    foreach (var file in ig.Files)
                    {
                        var fileData = file.GetLatestData();
                        fhir_stu3.Hl7.Fhir.Model.Resource resource = null;
                        string fileContent = System.Text.Encoding.UTF8.GetString(fileData.Data);

                        try
                        {
                            resource = fhirXmlParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                        }
                        catch (Exception ex)
                        {
                            if (file.MimeType == "application/xml" || file.MimeType == "text/xml")
                                messages.Add("FHIR Resource instance \"" + file.FileName + "\" cannot be parsed as valid XML: " + ex.Message);
                        }

                        try
                        {
                            if (resource == null)
                                resource = fhirJsonParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(fileContent);
                        }
                        catch (Exception ex)
                        {
                            if (file.MimeType == "binary/octet-stream" || file.MimeType == "application/json")
                                messages.Add("FHIR Resource instance \"" + file.FileName + "\" cannot be parsed as valid JSON: " + ex.Message);
                        }

                        if (resource != null && string.IsNullOrEmpty(resource.Id))
                        {
                            string msg = string.Format("FHIR resource instance \"" + file.FileName + "\" does not have an \"id\" property.");
                            messages.Add(msg);
                        }
                    }

                    // Validate that each of the samples associated with profiles has the required fields
                    var templateExamples = (from t in templates
                                        join ts in this.tdb.TemplateSamples on t.Id equals ts.TemplateId
                                        select new { Template = t, Sample = ts });

                    foreach (var templateExample in templateExamples)
                    {
                        fhir_stu3.Hl7.Fhir.Model.Resource resource = null;

                        try
                        {
                            resource = fhirXmlParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(templateExample.Sample.XmlSample);
                        }
                        catch
                        {
                        }

                        try
                        {
                            if (resource == null)
                                resource = fhirJsonParser.Parse<fhir_stu3.Hl7.Fhir.Model.Resource>(templateExample.Sample.XmlSample);
                        }
                        catch
                        {
                        }

                        if (resource == null)
                        {
                            string msg = string.Format("Profile sample \"" + templateExample.Sample.Name + "\" cannot be parsed as a valid XML or JSON resource.");
                            messages.Add(msg);
                        }
                        else if (string.IsNullOrEmpty(resource.Id))
                        {
                            string msg = string.Format("Profile sample \"" + templateExample.Sample.Name + "\" does not have an \"id\" property.");
                            messages.Add(msg);
                        }
                    }

                    break;
            }

            return messages;
        }

        [HttpGet, Route("api/Export/{implementationGuideId}/XML"), SecurableAction(SecurableNames.EXPORT_XML)]
        public XMLModel Xml(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to this implementation guide.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);

            XMLModel model = new XMLModel()
            {
                ImplementationGuideId = implementationGuideId,
                Name = ig.GetDisplayName(),
                CancelUrl = GetCancelUrl(implementationGuideId),
                ImplementationGuideType = ig.ImplementationGuideType.Name
            };

            // Get categories from the IG's settings
            string categories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
            if (!string.IsNullOrEmpty(categories))
            {
                string[] categoriesSplit = categories.Split(',');

                foreach (string category in categoriesSplit)
                {
                    model.Categories.Add(category.Replace("###", ","));
                }
            }

            return model;
        }

        [HttpPost, Route("api/Export/Trifolia"), SecurableAction(SecurableNames.EXPORT_XML)]
        public Trifolia.Shared.ImportExport.Model.Trifolia ExportTrifoliaModel(XMLSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (model.ImplementationGuideId == 0)
                throw new ArgumentNullException("model.ImplementationGuideId");

            ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == model.ImplementationGuideId);

            if (ig == null)
                throw new Exception("Implementation guide with id " + model.ImplementationGuideId + " was not found");

            List<Template> templates = this.tdb.Templates.Where(y => model.TemplateIds.Contains(y.Id)).ToList();
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, model.ImplementationGuideId);
            string fileName = string.Format("{0}.xml", ig.GetDisplayName(true));
            string export = string.Empty;

            try
            {
                NativeExporter exporter = new NativeExporter(this.tdb, templates, igSettings, categories: model.SelectedCategories);
                return exporter.GenerateExport();
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Error creating Trifolia export", ex);
                throw;
            }
        }

        /// <summary>
        /// Exports data from Trifolia using the export format specified in the export model.
        /// </summary>
        /// <param name="model">Settings/information on what XML format should be generated.</param>
        /// <returns>HttpResponseMessage</returns>
        /// <remarks>Compression is disabled for this Web API endpoint because there are problems with the FHIRBuild export being zipped/compressed, and then WebAPI tries to re-compress the content and fails.</remarks>
        [HttpPost, Route("api/Export/XML"), SecurableAction(SecurableNames.EXPORT_XML), Compression(Enabled = false)]
        public HttpResponseMessage ExportXML(XMLSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (model.ImplementationGuideId == 0)
                throw new ArgumentNullException("model.ImplementationGuideId");

            ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == model.ImplementationGuideId);

            if (ig == null)
                throw new Exception("Implementation guide with id " + model.ImplementationGuideId + " was not found");

            List<Template> templates = this.tdb.Templates.Where(y => model.TemplateIds.Contains(y.Id)).ToList();
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, model.ImplementationGuideId);
            string fileName = string.Format("{0}.xml", ig.GetDisplayName(true));
            string contentType = XML_MIME_TYPE;
            byte[] export = null;
            bool returnJson = Request.Headers.Accept.Count(y => y.MediaType == JSON_MIME_TYPE) == 1;
            bool includeVocabulary = model.IncludeVocabulary == true;
            var igTypePlugin = IGTypePluginFactory.GetPlugin(ig.ImplementationGuideType);
            var schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, ig.ImplementationGuideType);

            if (model.XmlType == XMLSettingsModel.ExportTypes.Proprietary)
            {
                export = igTypePlugin.Export(tdb, schema, ExportFormats.Native_XML, igSettings, model.SelectedCategories, templates, includeVocabulary, returnJson);
            }
            else if (model.XmlType == XMLSettingsModel.ExportTypes.DSTU)
            {
                export = igTypePlugin.Export(tdb, schema, ExportFormats.Templates_DSTU_XML, igSettings, model.SelectedCategories, templates, includeVocabulary, returnJson);
            }
            else if (model.XmlType == XMLSettingsModel.ExportTypes.FHIR)
            {
                export = igTypePlugin.Export(this.tdb, schema, ExportFormats.FHIR_Bundle, igSettings, model.SelectedCategories, templates, includeVocabulary, returnJson);
            }
            else if (model.XmlType == XMLSettingsModel.ExportTypes.FHIRBuild || model.XmlType == XMLSettingsModel.ExportTypes.FHIRBuildJSON)
            {
                if (model.XmlType == XMLSettingsModel.ExportTypes.FHIRBuildJSON)
                    returnJson = true;

                export = igTypePlugin.Export(this.tdb, schema, ExportFormats.FHIR_Build_Package, igSettings, model.SelectedCategories, templates, includeVocabulary, returnJson);
                contentType = ZIP_MIME_TYPE;
                fileName = string.Format("{0}.zip", ig.GetDisplayName(true));
            }
            else if (model.XmlType == XMLSettingsModel.ExportTypes.JSON)
            {
                ImplementationGuideController ctrl = new ImplementationGuideController(this.tdb);
                var dataModel = ctrl.GetViewData(model.ImplementationGuideId, null, null, true);

                // Serialize the data to JSON
                var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                export = System.Text.Encoding.UTF8.GetBytes(jsonSerializer.Serialize(dataModel));

                // Set the filename to JSON
                fileName = string.Format("{0}.json", ig.GetDisplayName(true));
                contentType = JSON_MIME_TYPE;
            }

            return GetExportResponse(fileName, contentType, export);
        }

        #endregion

        #region Vocabulary

        [HttpGet, Route("api/Export/{implementationGuideId}/Vocabulary"), SecurableAction(SecurableNames.EXPORT_VOCAB)]
        public VocabularyModel Vocabulary(int implementationGuideId)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            VocabularyModel model = new VocabularyModel()
            {
                ImplementationGuideId = implementationGuideId,
                Name = implementationGuide.GetDisplayName(),
                CancelUrl = GetCancelUrl(implementationGuideId)
            };

            return model;
        }

        [HttpPost, Route("api/Export/Vocabulary"), SecurableAction(SecurableNames.EXPORT_VOCAB)]
        public HttpResponseMessage ExportVocabulary(VocabularySettingsModel model)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == model.ImplementationGuideId);
            byte[] data = new byte[0];
            string fileType = string.Empty;
            string contentType = string.Empty;

            VocabularyService service = new VocabularyService(this.tdb, ig.ImplementationGuideType.SchemaURI == "urn:hl7-org:v3");

            switch (model.ExportFormat)
            {
                case VocabularySettingsModel.ExportFormatTypes.Standard:
                case VocabularySettingsModel.ExportFormatTypes.SVS:
                case VocabularySettingsModel.ExportFormatTypes.FHIR:
                    fileType = "xml";
                    contentType = XML_MIME_TYPE;
                    string vocXml = service.GetImplementationGuideVocabulary(model.ImplementationGuideId, model.MaximumMembers, (int)model.ExportFormat, model.Encoding);
                    Encoding encoding = Encoding.GetEncoding(model.Encoding);
                    data = encoding.GetBytes(vocXml);
                    break;
                case VocabularySettingsModel.ExportFormatTypes.Excel:
                    fileType = "xlsx";
                    contentType = XLSX_MIME_TYPE;
                    data = service.GetImplementationGuideVocabularySpreadsheet(model.ImplementationGuideId, model.MaximumMembers);
                    break;
            }

            string fileName = string.Format("{0}.{1}", ig.GetDisplayName(true), fileType);

            return GetExportResponse(fileName, contentType, data);
        }

        [HttpGet, Route("api/Export/{implementationGuideId}/ValueSet"), SecurableAction()]
        public List<VocabularyItemModel> GetValueSets(int implementationGuideId, int? exportFormat = 0)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            IIGTypePlugin igTypePlugin = ig.ImplementationGuideType.GetPlugin();

            switch (exportFormat)
            {
                case 0:
                case 1:
                case 2:
                    return ConvertVocabularyItems(ig.GetValueSets(this.tdb, true), igTypePlugin);
                default:
                    return ConvertVocabularyItems(ig.GetValueSets(this.tdb), igTypePlugin);
            }
        }

        private List<VocabularyItemModel> ConvertVocabularyItems(List<ImplementationGuideValueSet> valueSets, IIGTypePlugin igTypePlugin)
        {
            List<VocabularyItemModel> ret = new List<VocabularyItemModel>();

            foreach (var cValueSet in valueSets)
            {
                ret.Add(
                    new VocabularyItemModel()
                    {
                        Id = cValueSet.ValueSet.Id,
                        Name = cValueSet.ValueSet.Name,
                        Oid = cValueSet.ValueSet.GetIdentifier(igTypePlugin),
                        BindingDate = cValueSet.BindingDate != null ? cValueSet.BindingDate.Value.ToString("MM/dd/yyyy") : string.Empty
                    });
            }

            return ret;
        }

        #endregion

        #region Schematron

        [HttpGet, Route("api/Export/{implementationGuideId}/Schematron"), SecurableAction(SecurableNames.EXPORT_SCHEMATRON)]
        public SchematronModel Schematron(int implementationGuideId)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);

            SchematronModel model = new SchematronModel()
            {
                ImplementationGuideId = implementationGuideId,
                Name = implementationGuide.GetDisplayName(),
                CancelUrl = GetCancelUrl(implementationGuideId)
            };

            // Get categories from the IG's settings
            string categories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
            if (!string.IsNullOrEmpty(categories))
            {
                string[] categoriesSplit = categories.Split(',');

                foreach (string category in categoriesSplit)
                {
                    model.Categories.Add(category.Replace("###", ","));
                }
            }

            return model;
        }

        [HttpPost, Route("api/Export/Schematron"), SecurableAction(SecurableNames.EXPORT_SCHEMATRON)]
        public HttpResponseMessage ExportSchematron(SchematronSettingsModel model)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == model.ImplementationGuideId);
            VocabularyOutputType vocOutputType = GetVocOutputType(model.ValueSetOutputFormat);

            var templates = (from t in this.tdb.Templates
                             where model.TemplateIds.Contains(t.Id)
                             select t).ToList();
            string schematronResult = SchematronGenerator.Generate(tdb, ig, model.IncludeCustomSchematron, vocOutputType, model.VocabularyFileName, templates, model.SelectedCategories, model.DefaultSchematron);
            byte[] data = ASCIIEncoding.UTF8.GetBytes(schematronResult);
            string fileName = string.Format("{0}.sch", ig.GetDisplayName(true));

            if (model.IncludeVocabulary)
            {
                VocabularyService service = new VocabularyService(tdb, ig.ImplementationGuideType.SchemaURI == "urn:hl7-org:v3");

                using (ZipFile zip = new ZipFile())
                {
                    zip.AddEntry(fileName, data);

                    //Assuming default settings for vocabulary file
                    string vocXml = service.GetImplementationGuideVocabulary(model.ImplementationGuideId, 0, (int)vocOutputType, "UTF-8");

                    byte[] vocData = ASCIIEncoding.UTF8.GetBytes(vocXml);
                    string vocFileName = string.Format("{0}", model.VocabularyFileName);
                    //Ensuring the extension is present in case input doesn't have it
                    if (vocFileName.IndexOf(".xml") == -1) vocFileName += ".xml";
                    zip.AddEntry(vocFileName, vocData);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        zip.Save(ms);

                        string zipFileName = string.Format("{0}.zip", ig.GetDisplayName(true));
                        byte[] zipData = ms.ToArray();

                        return GetExportResponse(zipFileName, ZIP_MIME_TYPE, zipData);
                    }
                }
            }
            else
            {
                return GetExportResponse(fileName, XML_MIME_TYPE, data);
            }
        }

        #endregion

        #region MS Word

        /// <summary>
        /// Gets default settings for the specified implementation guide
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide being exported</param>
        /// <returns>Trifolia.Web.Models.Export.MSWordSettingsModel</returns>
        /// <permission cref="Trifolia.Authorization.SecurableNames.EXPORT_WORD">User must have access to the EXPORT_WORD securable</permission>
        [HttpGet, Route("api/Export/{implementationGuideId}/Settings"), SecurableAction(SecurableNames.EXPORT_WORD)]
        public MSWordSettingsModel GetExportSettings(int implementationGuideId)
        {
            IGSettingsManager settingsManager = new IGSettingsManager(this.tdb, implementationGuideId);
            var settingsJson = settingsManager.GetSetting(MSWordExportSettingsPropertyName);

            if (!string.IsNullOrEmpty(settingsJson))
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                MSWordSettingsModel settings = serializer.Deserialize<MSWordSettingsModel>(settingsJson);
                return settings;
            }

            return null;
        }

        /// <summary>
        /// Gets a model for displaying information about the implementation guide being exported
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide being exported</param>
        /// <returns>Trifolia.Web.Models.Export.MSWordModel</returns>
        [HttpGet, Route("api/Export/{implementationGuideId}/MSWord"), SecurableAction(SecurableNames.EXPORT_WORD)]
        public MSWordModel MSWord(int implementationGuideId)
        {
            var ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
            bool canEdit = CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_EDIT);
            bool grantEdit = CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId);

            MSWordModel model = new MSWordModel()
            {
                ImplementationGuideId = implementationGuideId,
                Name = ig.GetDisplayName(),
                CanEdit = canEdit && grantEdit
            };

            // Get categories from the IG's settings
            string categories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
            if (!string.IsNullOrEmpty(categories))
            {
                string[] categoriesSplit = categories.Split(',');

                foreach (string category in categoriesSplit)
                {
                    model.Categories.Add(category.Replace("###", ","));
                }
            }

            return model;
        }

        /// <summary>
        /// Exports an MS Word implementation guide with the specified settings.
        /// </summary>
        /// <param name="exportSettings">The settings for the export</param>
        /// <returns>A file-download of the .docx MS Word implementation guide</returns>
        [HttpPost, Route("api/Export/MSWord"), SecurableAction(SecurableNames.EXPORT_WORD)]
        public HttpResponseMessage ExportMSWord(MSWordSettingsModel exportSettings)
        {
            if (exportSettings == null)
                throw new ArgumentNullException("model");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == exportSettings.ImplementationGuideId);
            ImplementationGuideGenerator generator = new ImplementationGuideGenerator(this.tdb, exportSettings.ImplementationGuideId, exportSettings.TemplateIds);
            string fileName = string.Format("{0}.docx", ig.GetDisplayName(true));

            ExportSettings lConfig = new ExportSettings();
            lConfig.Use(c =>
            {
                c.GenerateTemplateConstraintTable = exportSettings.TemplateTables == TemplateTableOptions.ConstraintOverview || exportSettings.TemplateTables == TemplateTableOptions.Both;
                c.GenerateTemplateContextTable = exportSettings.TemplateTables == TemplateTableOptions.Context || exportSettings.TemplateTables == TemplateTableOptions.Both;
                c.GenerateDocTemplateListTable = exportSettings.DocumentTables == DocumentTableOptions.List || exportSettings.DocumentTables == DocumentTableOptions.Both;
                c.GenerateDocContainmentTable = exportSettings.DocumentTables == DocumentTableOptions.Containment || exportSettings.DocumentTables == DocumentTableOptions.Both;
                c.AlphaHierarchicalOrder = exportSettings.TemplateSortOrder == TemplateSortOrderOptions.AlphaHierarchically;
                c.DefaultValueSetMaxMembers = exportSettings.GenerateValuesets ? exportSettings.MaximumValuesetMembers : 0;
                c.GenerateValueSetAppendix = exportSettings.ValuesetAppendix;
                c.IncludeXmlSamples = exportSettings.IncludeXmlSample;
                c.IncludeChangeList = exportSettings.IncludeChangeList;
                c.IncludeTemplateStatus = exportSettings.IncludeTemplateStatus;
                c.IncludeNotes = exportSettings.IncludeNotes;
                c.SelectedCategories = exportSettings.SelectedCategories;
            });

            if (exportSettings.ValueSetOid != null && exportSettings.ValueSetOid.Count > 0)
            {
                Dictionary<string, int> valueSetMemberMaximums = new Dictionary<string, int>();

                for (int i = 0; i < exportSettings.ValueSetOid.Count; i++)
                {
                    if (valueSetMemberMaximums.ContainsKey(exportSettings.ValueSetOid[i]))
                        continue;

                    valueSetMemberMaximums.Add(exportSettings.ValueSetOid[i], exportSettings.ValueSetMaxMembers[i]);
                }

                lConfig.ValueSetMaxMembers = valueSetMemberMaximums;
            }

            // Save the export settings as the default settings
            if (exportSettings.SaveAsDefaultSettings)
            {
                IGSettingsManager settingsManager = new IGSettingsManager(this.tdb, exportSettings.ImplementationGuideId);
                var settingsJson = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(exportSettings);
                settingsManager.SaveSetting(MSWordExportSettingsPropertyName, settingsJson);
            }

            generator.BuildImplementationGuide(lConfig, ig.ImplementationGuideType.GetPlugin());

            byte[] data = generator.GetDocument();

            return GetExportResponse(fileName, DOCX_MIME_TYPE, data);
        }

        #endregion

        #region Green

        [HttpGet, Route("api/Export/{implementationGuideId}/Green"), SecurableAction(SecurableNames.EXPORT_GREEN)]
        public GreenModel Green(int implementationGuideId)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            GreenModel model = new GreenModel()
            {
                ImplementationGuideId = implementationGuideId,
                Name = ig.GetDisplayName()
            };

            List<TemplateType> rootTemplateTypes = ig.ImplementationGuideType.GetRootTemplateTypes();
            model.Templates = (from t in ig.ChildTemplates
                               join rtt in rootTemplateTypes on t.TemplateTypeId equals rtt.Id
                               where t.GreenTemplates.Count > 0
                               select LookupTemplate.ConvertTemplate(t)).ToList();

            return model;
        }

        [HttpPost, Route("api/Export/Green"), SecurableAction(SecurableNames.EXPORT_GREEN)]
        public HttpResponseMessage ExportGreen(GreenSettingsModel model)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(model.ImplementationGuideId))
                throw new AuthorizationException("You do not have permissions to this implementation guide.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == model.ImplementationGuideId);
            Template rootTemplate = this.tdb.Templates.Single(y => y.Id == model.RootTemplateId);

            // Generate the schema
            GreenSchemaPackage package = GreenSchemaGenerator.Generate(this.tdb, rootTemplate, model.SeparateDataTypes);

            using (ZipFile zip = new ZipFile())
            {
                byte[] schemaContentBytes = ASCIIEncoding.UTF8.GetBytes(package.GreenSchemaContent);
                zip.AddEntry(package.GreenSchemaFileName, schemaContentBytes);

                // If the datatypes were separated into another schema
                if (!string.IsNullOrEmpty(package.DataTypesFileName))
                {
                    byte[] dataTypesContentBytes = ASCIIEncoding.UTF8.GetBytes(package.DataTypesContent);
                    zip.AddEntry(package.DataTypesFileName, dataTypesContentBytes);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    zip.Save(ms);

                    string packageFileName = string.Format("{0}_green.zip", ig.GetDisplayName(true));
                    byte[] data = ms.ToArray();

                    return GetExportResponse(packageFileName, ZIP_MIME_TYPE, data);
                }
            }
        }

        #endregion

        #region Private Methods

        private string GetCancelUrl(int implementationGuideId)
        {
            return "/IGManagement/View/" + implementationGuideId;
        }

        private static VocabularyOutputType GetVocOutputType(SchematronSettingsModel.ValueSetOutputFormats format)
        {
            switch (format)
            {
                case SchematronSettingsModel.ValueSetOutputFormats.Standard:
                    return VocabularyOutputType.Default;
                case SchematronSettingsModel.ValueSetOutputFormats.SVS_Multiple:
                    return VocabularyOutputType.SVS;
                case SchematronSettingsModel.ValueSetOutputFormats.SVS_Single:
                    return VocabularyOutputType.SVS_SingleValueSet;
            }

            return VocabularyOutputType.Default;
        }

        private HttpResponseMessage GetExportResponse(string fileName, string contentType, byte[] data)
        {
            Array.ForEach(Path.GetInvalidFileNameChars(),
                c => fileName = fileName.Replace(c.ToString(), String.Empty));
            fileName = fileName
                .Replace("—", string.Empty)
                .Replace("®", string.Empty)
                .Replace(",", string.Empty);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(data);
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            return response;
        }

        #endregion
    }
}
