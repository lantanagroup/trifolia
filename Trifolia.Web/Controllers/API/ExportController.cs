extern alias fhir_dstu1;
extern alias fhir_dstu2;
extern alias fhir_stu3;
using Ionic.Zip;
using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Attributes;
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

                default:
                    throw new NotSupportedException();
            }

            return GetExportResponse(fileName, contentType, export);
        }

        private Encoding GetExportEncoding(ExportSettingsModel model)
        {
            switch (model.Encoding)
            {
                case ExportSettingsModel.EncodingOptions.UTF8:
                    return Encoding.UTF8;
                case ExportSettingsModel.EncodingOptions.UNICODE:
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
