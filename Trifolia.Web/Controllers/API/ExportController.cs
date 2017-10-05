extern alias fhir_dstu1;
extern alias fhir_dstu2;
extern alias fhir_stu3;
using Ionic.Zip;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Export.FHIR.STU3;
using Trifolia.Export.HTML;
using Trifolia.Export.Schematron;
using Trifolia.Export.Terminology;
using Trifolia.Generation.Green;
using Trifolia.Generation.IG;
using Trifolia.Logging;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using Trifolia.Web.Models.Export;
using ImplementationGuide = Trifolia.DB.ImplementationGuide;
using NativeExporter = Trifolia.Export.Native.TemplateExporter;

namespace Trifolia.Web.Controllers.API
{
    [RoutePrefix("api/Export")]
    public class ExportController : ApiController
    {
        private IObjectRepository tdb;
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

        private ExportSettingsModel GetLegacyMSWordExportSettings(IGSettingsManager igSettings, string newSettingsProperty)
        {
            string exportSettingsJson = igSettings.GetSetting(IGSettingsManager.SettingProperty.MSWordExportSettingsJson);
            ExportSettingsModel exportSettings;

            // If we have an old export settings format, convert it to the new one.
            if (!string.IsNullOrEmpty(exportSettingsJson))
            {
                MSWordSettingsModel oldExportSettings = JsonConvert.DeserializeObject<MSWordSettingsModel>(exportSettingsJson);

                exportSettings = new ExportSettingsModel();
                exportSettings.ExportFormat = ExportFormats.Microsoft_Word_DOCX;
                exportSettings.DocumentTables = oldExportSettings.DocumentTables;
                exportSettings.IncludeVocabulary = oldExportSettings.GenerateValuesets;
                exportSettings.ImplementationGuideId = oldExportSettings.ImplementationGuideId;
                exportSettings.IncludeChangeList = oldExportSettings.IncludeChangeList;
                exportSettings.IncludeNotes = oldExportSettings.IncludeNotes;
                exportSettings.IncludeTemplateStatus = oldExportSettings.IncludeTemplateStatus;
                exportSettings.IncludeXmlSample = oldExportSettings.IncludeXmlSample;
                exportSettings.IncludeInferred = oldExportSettings.Inferred;
                exportSettings.MaximumValueSetMembers = oldExportSettings.MaximumValuesetMembers;
                exportSettings.ParentTemplateIds = oldExportSettings.ParentTemplateIds != null ? oldExportSettings.ParentTemplateIds : new List<int>();
                exportSettings.SelectedCategories = oldExportSettings.SelectedCategories;
                exportSettings.TemplateIds = oldExportSettings.TemplateIds != null ? oldExportSettings.TemplateIds : new List<int>();
                exportSettings.TemplateSortOrder = oldExportSettings.TemplateSortOrder;
                exportSettings.TemplateTables = oldExportSettings.TemplateTables;
                exportSettings.ValueSetAppendix = oldExportSettings.ValuesetAppendix;
                exportSettings.ValueSetMaxMembers = oldExportSettings.ValueSetMaxMembers != null ? oldExportSettings.ValueSetMaxMembers : new List<int>();
                exportSettings.ValueSetOid = oldExportSettings.ValueSetOid;

                // Save the converted/new settings format
                igSettings.SaveSetting(newSettingsProperty, JsonConvert.SerializeObject(exportSettings));

                // Remove the old settings format
                igSettings.SaveSetting(IGSettingsManager.SettingProperty.MSWordExportSettingsJson, null);

                return exportSettings;
            }

            return null;
        }

        [HttpGet, Route("Settings")]
        public ExportSettingsModel GetExportSettings(int implementationGuideId, ExportFormats format)
        {
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
            string settingsProperty = "ExportSettings_" + format.ToString();

            if (format == ExportFormats.Microsoft_Word_DOCX)
            {
                var legacyExportSettings = this.GetLegacyMSWordExportSettings(igSettings, settingsProperty);

                if (legacyExportSettings != null)
                    return legacyExportSettings;
            }

            string exportSettingsJson = igSettings.GetSetting(settingsProperty);

            if (!string.IsNullOrEmpty(exportSettingsJson))
            {
                var settings = JsonConvert.DeserializeObject<ExportSettingsModel>(exportSettingsJson);
                settings.ImplementationGuideId = implementationGuideId;
                settings.ExportFormat = format;
                return settings;
            }

            return new ExportSettingsModel()
            {
                ImplementationGuideId = implementationGuideId,
                ExportFormat = format
            };
        }

        [HttpPost, Route("Settings")]
        public void SaveExportSettings([FromBody] ExportSettingsModel model, [FromUri] int implementationGuideId, [FromUri] ExportFormats format)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new UnauthorizedAccessException("You do not have permissions to save default settings for this implementation guide.");

            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
            string settingsJson = JsonConvert.SerializeObject(model);
            string settingsProperty = "ExportSettings_" + format.ToString();

            igSettings.SaveSetting(settingsProperty, settingsJson);
        }

        [HttpPost]
        public HttpResponseMessage Export(ExportSettingsModel model)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == model.ImplementationGuideId);

            if (model.TemplateIds == null)
                model.TemplateIds = ig.GetRecursiveTemplates(this.tdb, categories: model.SelectedCategories.ToArray()).Select(y => y.Id).ToList();

            List<Template> templates = this.tdb.Templates.Where(y => model.TemplateIds.Contains(y.Id)).ToList();
            SimpleSchema schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, ig.ImplementationGuideType);
            IIGTypePlugin igTypePlugin = IGTypePluginFactory.GetPlugin(ig.ImplementationGuideType);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, model.ImplementationGuideId);
            bool isCDA = ig.ImplementationGuideType.SchemaURI == "urn:hl7-org:v3";
            string fileName;
            byte[] export;
            string contentType = null;

            switch (model.ExportFormat)
            {
                case ExportFormats.FHIR_Build_Package:
                case ExportFormats.FHIR_Bundle:
                case ExportFormats.Native_XML:
                case ExportFormats.Templates_DSTU_XML:
                    string fileExtension = model.ReturnJson ? "json" : "xml";
                    contentType = model.ReturnJson ? JSON_MIME_TYPE : XML_MIME_TYPE;

                    if (model.ExportFormat == ExportFormats.FHIR_Build_Package)
                    {
                        fileExtension = "zip";
                        contentType = ZIP_MIME_TYPE;
                    }

                    fileName = string.Format("{0}.{1}", ig.GetDisplayName(true), fileExtension);
                    export = igTypePlugin.Export(this.tdb, schema, model.ExportFormat, igSettings, model.SelectedCategories, templates, model.IncludeVocabulary, model.ReturnJson);
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
                        c.GenerateTemplateConstraintTable = model.TemplateTables == TemplateTableOptions.ConstraintOverview || model.TemplateTables == TemplateTableOptions.Both;
                        c.GenerateTemplateContextTable = model.TemplateTables == TemplateTableOptions.Context || model.TemplateTables == TemplateTableOptions.Both;
                        c.GenerateDocTemplateListTable = model.DocumentTables == DocumentTableOptions.List || model.DocumentTables == DocumentTableOptions.Both;
                        c.GenerateDocContainmentTable = model.DocumentTables == DocumentTableOptions.Containment || model.DocumentTables == DocumentTableOptions.Both;
                        c.AlphaHierarchicalOrder = model.TemplateSortOrder == TemplateSortOrderOptions.AlphaHierarchically;
                        c.DefaultValueSetMaxMembers = model.ValueSetTables ? model.MaximumValueSetMembers : 0;
                        c.GenerateValueSetAppendix = model.ValueSetAppendix;
                        c.IncludeXmlSamples = model.IncludeXmlSample;
                        c.IncludeChangeList = model.IncludeChangeList;
                        c.IncludeTemplateStatus = model.IncludeTemplateStatus;
                        c.IncludeNotes = model.IncludeNotes;
                        c.SelectedCategories = model.SelectedCategories;
                    });

                    if (model.ValueSetOid != null && model.ValueSetOid.Count > 0)
                    {
                        Dictionary<string, int> valueSetMemberMaximums = new Dictionary<string, int>();

                        for (int i = 0; i < model.ValueSetOid.Count; i++)
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
                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddEntry(fileName, export);

                            NativeTerminologyExporter nativeTermExporter = new NativeTerminologyExporter(this.tdb);
                            byte[] vocData = nativeTermExporter.GetExport(ig.Id, this.GetExportEncoding(model));
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

                case ExportFormats.Vocabulary_XLSX:
                    fileName = string.Format("{0}.xlsx", ig.GetDisplayName(true));
                    contentType = XLSX_MIME_TYPE;

                    ExcelExporter excelExporter = new ExcelExporter(this.tdb);
                    export = excelExporter.GetSpreadsheet(ig.Id, model.MaximumValueSetMembers);

                    break;
                case ExportFormats.Vocbulary_Single_SVS_XML:
                    fileName = string.Format("{0}_SingleSVS.xml", ig.GetDisplayName(true));
                    contentType = XML_MIME_TYPE;

                    SingleSVSExporter ssvsExporter = new SingleSVSExporter(this.tdb);
                    export = ssvsExporter.GetExport(ig.Id, model.MaximumValueSetMembers, this.GetExportEncoding(model));

                    break;
                case ExportFormats.Vocabulary_Multiple_SVS_XML:
                    fileName = string.Format("{0}_MultipleSVS.xml", ig.GetDisplayName(true));
                    contentType = XML_MIME_TYPE;

                    MultipleSVSExporter msvsExporter = new MultipleSVSExporter(this.tdb);
                    export = msvsExporter.GetExport(ig.Id, model.MaximumValueSetMembers, this.GetExportEncoding(model));

                    break;
                case ExportFormats.Vocabulary_Native_XML:
                    fileName = string.Format("{0}_MultipleSVS.xml", ig.GetDisplayName(true));
                    contentType = XML_MIME_TYPE;

                    NativeTerminologyExporter nativeExporter = new NativeTerminologyExporter(this.tdb);
                    export = nativeExporter.GetExport(ig.Id, model.MaximumValueSetMembers, this.GetExportEncoding(model));

                    break;
                case ExportFormats.Web_HTML:
                    // Get the data from the API controller
                    HtmlExporter exporter = new HtmlExporter(this.tdb);
                    var templateIds = templates.Select(y => y.Id).ToArray();
                    var htmlDataModel = exporter.GetExportData(ig.Id, null, templateIds, model.IncludeInferred);

                    IGController igController = new IGController(this.tdb);
                    var downloadPackage = igController.GetDownloadPackage(ig, JsonConvert.SerializeObject(htmlDataModel));

                    export = downloadPackage.Content;
                    fileName = downloadPackage.FileName;
                    contentType = ZIP_MIME_TYPE;
                    break;
                case ExportFormats.Vocbulary_Bundle_FHIR_XML:
                    ValueSetExporter vsExporter = new ValueSetExporter(this.tdb);
                    var bundle = vsExporter.GetImplementationGuideValueSets(ig);

                    if (model.ReturnJson)
                    {
                        export = fhir_stu3.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToJsonBytes(bundle);
                        fileName = string.Format("{0}_fhir_voc.json", ig.GetDisplayName(true));
                        contentType = JSON_MIME_TYPE;
                    }
                    else
                    {
                        export = fhir_stu3.Hl7.Fhir.Serialization.FhirSerializer.SerializeResourceToXmlBytes(bundle);
                        fileName = string.Format("{0}_fhir_voc.xml", ig.GetDisplayName(true));
                        contentType = XML_MIME_TYPE;
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }

            return GetExportResponse(fileName, contentType, export);
        }

        private Encoding GetExportEncoding(ExportSettingsModel model)
        {
            switch (model.Encoding)
            {
                case EncodingOptions.UTF8:
                    return Encoding.UTF8;
                case EncodingOptions.UNICODE:
                    return Encoding.Unicode;
                default:
                    throw new ArgumentException("Unexpected encoding " + model.Encoding.ToString());
            }
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
    }
}
