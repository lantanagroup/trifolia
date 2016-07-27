extern alias fhir_dstu1;
extern alias fhir_dstu2;

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
using Trifolia.Generation.Schematron;
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

namespace Trifolia.Web.Controllers.API
{
    public class ExportController : ApiController
    {
        private IObjectRepository tdb;
        private const string MSWordExportSettingsPropertyName = "MSWordExportSettingsJson";

        #region CTOR

        public ExportController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        public ExportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        #region XML

        [HttpGet, Route("api/Export/{implementationGuideId}/XML"), SecurableAction(SecurableNames.EXPORT_XML)]
        public XMLModel Xml(int implementationGuideId, bool dy = false)
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
                throw ex;
            }
        }

        [HttpPost, Route("api/Export/XML"), SecurableAction(SecurableNames.EXPORT_XML)]
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
            string export = string.Empty;
            bool returnJson = Request.Headers.Accept.Count(y => y.MediaType == "application/json") == 1;
            bool includeVocabulary = model.IncludeVocabulary == true;
            var igTypePlugin = IGTypePluginFactory.GetPlugin(ig.ImplementationGuideType);
            var schema = SimplifiedSchemaContext.GetSimplifiedSchema(HttpContext.Current.Application, ig.ImplementationGuideType);

            if (model.XmlType == XMLSettingsModel.ExportTypes.Proprietary)
            {
                export = igTypePlugin.Export(tdb, schema, ExportFormats.Proprietary, igSettings, model.SelectedCategories, templates, includeVocabulary, returnJson);
            }
            else if (model.XmlType == XMLSettingsModel.ExportTypes.DSTU)
            {
                export = igTypePlugin.Export(tdb, schema, ExportFormats.TemplatesDSTU, igSettings, model.SelectedCategories, templates, includeVocabulary, returnJson);
            }
            else if (model.XmlType == XMLSettingsModel.ExportTypes.FHIR)
            {
                igTypePlugin.Export(this.tdb, schema, ExportFormats.FHIR, igSettings, model.SelectedCategories, templates, includeVocabulary, returnJson);
            }
            else if (model.XmlType == XMLSettingsModel.ExportTypes.JSON)
            {
                ImplementationGuideController ctrl = new ImplementationGuideController(this.tdb);
                var dataModel = ctrl.GetViewData(model.ImplementationGuideId, null, null, true);

                // Serialize the data to JSON
                var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                export = jsonSerializer.Serialize(dataModel);

                // Set the filename to JSON
                fileName = string.Format("{0}.json", ig.GetDisplayName(true));
            }

            byte[] data = System.Text.ASCIIEncoding.UTF8.GetBytes(export);
            return GetExportResponse(fileName, data);
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
                    contentType = "text/xml";
                    string vocXml = service.GetImplementationGuideVocabulary(model.ImplementationGuideId, model.MaximumMembers, (int)model.ExportFormat, model.Encoding);
                    Encoding encoding = Encoding.GetEncoding(model.Encoding);
                    data = encoding.GetBytes(vocXml);
                    break;
                case VocabularySettingsModel.ExportFormatTypes.Excel:
                    fileType = "xlsx";
                    contentType = "application/octet-stream";
                    data = service.GetImplementationGuideVocabularySpreadsheet(model.ImplementationGuideId, model.MaximumMembers);
                    break;
            }

            string fileName = string.Format("{0}.{1}", ig.GetDisplayName(true), fileType);

            return GetExportResponse(fileName, data);
        }

        [HttpGet, Route("api/Export/{implementationGuideId}/ValueSet"), SecurableAction()]
        public List<VocabularyItemModel> GetValueSets(int implementationGuideId, int? exportFormat = 0)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            switch (exportFormat)
            {
                case 0:
                case 1:
                case 2:
                    return ConvertVocabularyItems(ig.GetValueSets(this.tdb, true));
                default:
                    return ConvertVocabularyItems(ig.GetValueSets(this.tdb));
            }
        }

        private List<VocabularyItemModel> ConvertVocabularyItems(List<ImplementationGuideValueSet> valueSets)
        {
            List<VocabularyItemModel> ret = new List<VocabularyItemModel>();

            foreach (var cValueSet in valueSets)
            {
                ret.Add(
                    new VocabularyItemModel()
                    {
                        Id = cValueSet.ValueSet.Id,
                        Name = cValueSet.ValueSet.Name,
                        Oid = cValueSet.ValueSet.Oid,
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

            return GetExportResponse(fileName, data);
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

            generator.BuildImplementationGuide(lConfig);

            byte[] data = generator.GetDocument();

            return GetExportResponse(fileName, data);
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

                    return GetExportResponse(packageFileName, data);
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

        private HttpResponseMessage GetExportResponse(string fileName, byte[] data) 
        {
            Array.ForEach(Path.GetInvalidFileNameChars(),
                c => fileName = fileName.Replace(c.ToString(), String.Empty));
            fileName = fileName
                .Replace("—", string.Empty)
                .Replace("®", string.Empty)
                .Replace(",", string.Empty);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(data));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            return response;
        }

        #endregion
    }
}
