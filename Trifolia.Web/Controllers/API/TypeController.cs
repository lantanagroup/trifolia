using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Xml;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Web.Models.Type;

namespace Trifolia.Web.Controllers.API
{
    [SecurableAction]
    public class TypeController : ApiController
    {
        private const string XsNamespace = "http://www.w3.org/2001/XMLSchema";
        private IObjectRepository tdb;

        #region Constructors

        public TypeController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        public TypeController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        [HttpGet, Route("api/Type"), SecurableAction(SecurableNames.ADMIN)]
        public IEnumerable<ListItemModel> GetImplementationGuideTypes()
        {
            return (from igt in this.tdb.ImplementationGuideTypes
                    select new ListItemModel()
                    {
                        Id = igt.Id,
                        Name = igt.Name,
                        Schema = igt.SchemaLocation,
                        TemplateCount = igt.Templates.Count
                    });
        }

        [HttpDelete, Route("api/Type/{implementationGuideTypeId}"), SecurableAction(SecurableNames.ADMIN)]
        public void DeleteImplementationGuideType(int implementationGuideTypeId)
        {
            var implementationGuideType = this.tdb.ImplementationGuideTypes.Single(y => y.Id == implementationGuideTypeId);

            if (implementationGuideType.Templates.Count > 0)
                throw new Exception("Cannot delete implementation guide type that is associated with templates.");

            implementationGuideType.DataTypes.ToList().ForEach(y => {
                this.tdb.ImplementationGuideTypeDataTypes.DeleteObject(y);
            });
            implementationGuideType.TemplateTypes.ToList().ForEach(y => {
                this.tdb.TemplateTypes.DeleteObject(y); 
            });
            this.tdb.ImplementationGuideTypes.DeleteObject(implementationGuideType);

            string directory = Shared.Helper.GetSchemasDirectory(implementationGuideType.Name);
            Directory.Delete(directory, true);

            this.tdb.SaveChanges();
        }

        [HttpGet, Route("api/Type/{implementationGuideTypeId}"), SecurableAction(SecurableNames.ADMIN)]
        public TypeModel GetImplementationGuideType(int implementationGuideTypeId)
        {
            var implementationGuideType = this.tdb.ImplementationGuideTypes.Single(y => y.Id == implementationGuideTypeId);
            TypeModel model = new TypeModel()
            {
                Id = implementationGuideType.Id,
                Name = implementationGuideType.Name,
                SchemaLocation = implementationGuideType.SchemaLocation,
                SchemaPrefix = implementationGuideType.SchemaPrefix,
                SchemaUri = implementationGuideType.SchemaURI
            };

            model.TemplateTypes = (from tt in this.tdb.TemplateTypes
                                   where tt.ImplementationGuideTypeId == implementationGuideTypeId
                                   select new TypeModel.TemplateTypeModel()
                                   {
                                       Id = tt.Id,
                                       Name = tt.Name,
                                       Context = tt.RootContext,
                                       ContextType = tt.RootContextType,
                                       OutputOrder = tt.OutputOrder,
                                       TemplateCount = tt.Templates.Count
                                   });

            var schema = implementationGuideType.GetSimpleSchema();
            model.ComplexTypes = (from ct in schema.ComplexTypes
                                  where implementationGuideType.DataTypes.Count(y => y.DataTypeName == ct.Name) == 0
                                  select ct.Name);

            model.DataTypes = (from dt in implementationGuideType.DataTypes
                               select dt.DataTypeName);

            return model;
        }

        [HttpPost, Route("api/Type/Zip"), SecurableAction(SecurableNames.ADMIN)]
        public IEnumerable<string> GetZipSchemas(byte[] content)
        {
            List<string> results = new List<string>();

            using (MemoryStream ms = new MemoryStream(content))
            {
                ZipFile zipFile = ZipFile.Read(ms);

                foreach (Ionic.Zip.ZipEntry cEntry in zipFile.Entries)
                {
                    if (cEntry.IsDirectory)
                        continue;

                    FileInfo cEntryFileInfo = new FileInfo(cEntry.FileName);

                    if (cEntryFileInfo.Extension == ".xsd")
                    {
                        string fileName = cEntry.FileName.Replace("/", "\\");
                        results.Add(fileName);
                    }
                }
            }

            return results;
        }

        [HttpPost, Route("api/Type/Save"), SecurableAction(SecurableNames.ADMIN)]
        public TypeModel SaveType(TypeModel model)
        {
            using (var transaction = this.tdb.BeginTransaction())
            {
                ImplementationGuideType igType = new ImplementationGuideType();

                if (model.Id != null)
                    igType = this.tdb.ImplementationGuideTypes.Single(y => y.Id == model.Id);
                else
                    this.tdb.ImplementationGuideTypes.AddObject(igType);

                if (igType.Name != model.Name)
                {
                    if (!string.IsNullOrEmpty(igType.Name))
                    {
                        string currentDirectory = Shared.Helper.GetSchemasDirectory(igType.Name);
                        string newDirectory = Shared.Helper.GetSchemasDirectory(model.Name);
                        MoveSchemaFiles(currentDirectory, newDirectory);
                    }

                    igType.Name = model.Name;
                }

                if (igType.SchemaLocation != model.SchemaLocation)
                    igType.SchemaLocation = model.SchemaLocation;

                if (igType.SchemaPrefix != model.SchemaPrefix)
                    igType.SchemaPrefix = model.SchemaPrefix;

                if (igType.SchemaURI != model.SchemaUri)
                    igType.SchemaURI = model.SchemaUri;

                // Update template types
                foreach (var templateTypeModel in model.DeletedTemplateTypes)
                {
                    var templateType = igType.TemplateTypes.Single(y => y.Id == templateTypeModel.Id);
                    this.tdb.TemplateTypes.DeleteObject(templateType);
                }

                foreach (var templateTypeModel in model.TemplateTypes)
                {
                    SaveTemplateType(igType, templateTypeModel);
                }

                // Update data types
                var newDataTypes = model.DataTypes.Where(y => igType.DataTypes.Count(x => x.DataTypeName == y) == 0).ToList();
                var deleteDataTypes = igType.DataTypes.Where(y => model.DataTypes.Count(x => x == y.DataTypeName) == 0).ToList();

                foreach (var current in newDataTypes)
                {
                    var newDataType = new ImplementationGuideTypeDataType()
                    {
                        ImplementationGuideType = igType,
                        DataTypeName = current
                    };
                    this.tdb.ImplementationGuideTypeDataTypes.AddObject(newDataType);
                }

                foreach (var current in deleteDataTypes)
                {
                    current.green_constraint.ToList().ForEach(y => { y.ImplementationGuideTypeDataType = null; });
                    this.tdb.ImplementationGuideTypeDataTypes.DeleteObject(current);
                }

                this.tdb.SaveChanges();

                try
                {
                    // If a new schema file is attached, save it
                    if (!string.IsNullOrEmpty(model.SchemaFileContentType))
                    {
                        switch (model.SchemaFileContentType)
                        {
                            case "application/xml":
                                SaveSchemaFile(igType, model.SchemaFile);
                                break;
                            case "application/zip":
                            case "application/x-zip-compressed":
                                SaveSchemaZip(igType, model.SchemaFile);
                                break;
                            default:
                                throw new Exception("Unexpected file format " + model.SchemaFileContentType + " for new schema.");
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }

                return GetImplementationGuideType(igType.Id);
            }
        }

        /// <summary>
        /// Gets all choices available within the specified implementation guide type's schema
        /// This returns the model that should be used to POST the schema choices back to the server for changing the
        /// schema choice names.
        /// </summary>
        /// <param name="implementationGuideTypeId">The ID of the implementation guide type whose schema should be searched for choices</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;Trifoliw.Web.Models.Type.SchemaChoiceModel&gt;</returns>
        [HttpGet, Route("api/Type/{implementationGuideTypeId}/SchemaChoice"), SecurableAction(SecurableNames.ADMIN)]
        public IEnumerable<SchemaChoiceModel> GetSchemaChoices(int implementationGuideTypeId)
        {
            var igType = this.tdb.ImplementationGuideTypes.Single(y => y.Id == implementationGuideTypeId);
            var schema = Shared.Helper.GetIGSchema(igType);

            string schemaLocation = Shared.Helper.GetIGSimplifiedSchemaLocation(igType);

            return GetChoices(schemaLocation);
        }

        private IEnumerable<SchemaChoiceModel> GetChoices(string schemaLocation, List<string> loadedSchemas = null)
        {
            if (loadedSchemas == null)
                loadedSchemas = new List<string>();
            else if (loadedSchemas.Contains(schemaLocation) || schemaLocation.EndsWith("xhtml.xsd") || schemaLocation.EndsWith("NarrativeBlock.xsd"))
                return new List<SchemaChoiceModel>();

            string schemaDirectory = Path.GetDirectoryName(schemaLocation);
            XmlDocument doc = new XmlDocument();
            doc.Load(schemaLocation);

            loadedSchemas.Add(schemaLocation);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("xs", XsNamespace);

            string targetNs = doc.DocumentElement.Attributes["xmlns"] != null ? doc.DocumentElement.Attributes["xmlns"].Value : string.Empty;

            List<SchemaChoiceModel> choiceModels = new List<SchemaChoiceModel>();

            // Loop through each complex type in the schema
            foreach (XmlElement complexTypeWithChoice in doc.SelectNodes("//xs:complexType[@name][descendant::xs:choice]", nsManager))
            {
                // Loop through each choice within the complex type
                foreach (XmlElement choice in complexTypeWithChoice.SelectNodes("//xs:choice", nsManager))
                {
                    List<string> optionNames = new List<string>();
                    List<string[]> optionNameWords = new List<string[]>();
                    var xpath = GetXpath(choice, nsManager);

                    // Make sure we aren't duplicating the choice by searching the returned choices for similar choices with the same xpath and source location
                    if (choiceModels.Exists(y => y.XPath == xpath && y.SourceUri == schemaLocation))
                        continue;

                    // Determine what the name of the options (child elements) are within the choice
                    foreach (XmlElement option in choice.SelectNodes("./descendant::xs:element[@name or @ref]", nsManager))
                    {
                        string name = option.Attributes["name"] != null ? option.Attributes["name"].Value : option.Attributes["ref"].Value;
                        optionNameWords.Add(Shared.Helper.GetWords(name));
                        optionNames.Add(name);
                    }

                    // See if we already have a name defined for the choice
                    var appInfoNode = choice.SelectSingleNode("xs:annotation/xs:appinfo[@source='" + SimpleSchema.SchemaChoiceAppInfoUri + "']", nsManager);

                    choiceModels.Add(new SchemaChoiceModel()
                    {
                        ComplexTypeName = complexTypeWithChoice.Attributes["name"].Value,
                        CalculatedName = Shared.Helper.GetChoiceCommonName(optionNameWords, targetNs),
                        XPath = xpath,
                        SourceUri = schemaLocation,
                        ChildrenElements = optionNames,
                        Documentation = GetChoiceDocumentation(choice, nsManager),
                        DefinedName = appInfoNode != null ? appInfoNode.InnerText : string.Empty
                    });
                }
            }

            foreach (XmlElement otherSchemaNode in doc.SelectNodes("//xs:include[@schemaLocation] | //xs:import[@schemaLocation]", nsManager))
            {
                string otherSchemaLocation = otherSchemaNode.Attributes["schemaLocation"].Value;

                if (!otherSchemaLocation.Contains(':'))
                {
                    otherSchemaLocation = Path.Combine(schemaDirectory, otherSchemaLocation);

                    if (!File.Exists(otherSchemaLocation))
                    {
                        otherSchemaLocation = Path.Combine(schemaDirectory, Path.GetFileName(otherSchemaNode.Attributes["schemaLocation"].Value));

                        if (!File.Exists(otherSchemaLocation))
                            continue;
                    }
                }

                choiceModels.AddRange(GetChoices(otherSchemaLocation, loadedSchemas));
            }

            return choiceModels;
        }

        /// <summary>
        /// Saves all choices to the specified schema
        /// </summary>
        /// <param name="implementationGuideTypeId">The implementation guide type that the choices should be saved for</param>
        [HttpPost, Route("api/Type/{implementationGuideTypeId}/SchemaChoice"), SecurableAction(SecurableNames.ADMIN)]
        public void SaveSchemaChoices(int implementationGuideTypeId, SchemaChoiceModel[] choiceModels)
        {
            ImplementationGuideType igType = this.tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Id == implementationGuideTypeId);
            Dictionary<string, XmlDocument> documents = new Dictionary<string, XmlDocument>();

            var schemaDirectory = Shared.Helper.GetSchemasDirectory(igType.Name);

            // Go through each choice model passed
            foreach (var choiceModel in choiceModels)
            {
                XmlDocument document = documents.ContainsKey(choiceModel.SourceUri) ? documents[choiceModel.SourceUri] : null;

                // Load the document in a temporary cache of documents so that we don't have to reload it for every choice
                if (document == null)
                {
                    // Make sure we are only making changes to files within the schema directory
                    if (!choiceModel.SourceUri.StartsWith(schemaDirectory))
                        throw new Exception("Invalid source URI specified... Not within the schema directory");

                    // Make sure we are only making changes to schemas... Don't want to have a security problem and modify an .exe or .dll
                    if (!choiceModel.SourceUri.EndsWith(".xsd"))
                        throw new Exception("Invalid source URI specified... Not an XSD");

                    document = new XmlDocument();
                    document.Load(choiceModel.SourceUri);
                    documents.Add(choiceModel.SourceUri, document);
                }

                XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("xs", XsNamespace);

                // Find the choice using the XPath provided by the choice model
                var choiceNode = document.SelectSingleNode(choiceModel.XPath, nsManager);

                if (choiceNode == null)
                    throw new Exception("Could not find XPath " + choiceModel.XPath);

                var annotationNode = choiceNode.SelectSingleNode("xs:annotation", nsManager);

                // If there is no annotation on the choice and we don't have a name defined for the choice by the model, just continue
                if (string.IsNullOrEmpty(choiceModel.DefinedName) && annotationNode == null)
                    continue;

                // Create the annotation node if it doesn't exist, make it the first child of the choice
                if (annotationNode == null)
                {
                    annotationNode = document.CreateElement("xs", "annotation", XsNamespace);

                    if (choiceNode.ChildNodes.Count > 0)
                        choiceNode.InsertBefore(annotationNode, choiceNode.ChildNodes[0]);
                    else
                        choiceNode.AppendChild(annotationNode);
                }

                var appInfoNode = annotationNode.SelectSingleNode("xs:appinfo[@source='" + SimpleSchema.SchemaChoiceAppInfoUri + "']", nsManager);

                // If there is no appinfo on the choice's annotation and we don't have a name defined for the choice by the model, just continue
                if (string.IsNullOrEmpty(choiceModel.DefinedName) && appInfoNode == null)
                    continue;

                // Create the appinfo node in the annotation if it doesn't exist
                if (appInfoNode == null)
                {
                    appInfoNode = document.CreateElement("xs", "appinfo", XsNamespace);

                    var sourceAtt = document.CreateAttribute("source");
                    sourceAtt.Value = SimpleSchema.SchemaChoiceAppInfoUri;

                    appInfoNode.Attributes.Append(sourceAtt);
                    annotationNode.AppendChild(appInfoNode);
                }

                // Set the value/innerText of the appInfo node to the name defined by the choice model
                appInfoNode.InnerText = choiceModel.DefinedName;
            }

            // Save each of the schema's files
            foreach (string documentLocation in documents.Keys)
            {
                documents[documentLocation].Save(documentLocation);
            }

            // Clear the application's cache of the parsed simple schema so that it loads with the correct choice names next time
            if (HttpContext.Current.Application[igType.Name] != null)
                HttpContext.Current.Application.Remove(igType.Name);
        }

        private string GetChoiceDocumentation(XmlElement choiceElement, XmlNamespaceManager nsManager)
        {
            XmlElement annotation = (XmlElement)choiceElement.SelectSingleNode("xs:annotation", nsManager);

            if (annotation == null)
                return string.Empty;

            XmlElement documentation = (XmlElement)annotation.SelectSingleNode("xs:documentation", nsManager);

            if (documentation == null)
                return string.Empty;

            return documentation.InnerText;
        }

        private string GetXpath(XmlElement element, XmlNamespaceManager nsManager)
        {
            string xpath = element.Name + "[" + GetXpathIndex(element, nsManager).ToString() + "]";
            var current = element.ParentNode;

            while (current != null && current is XmlElement)
            {
                xpath = ((XmlElement)current).Name + "[" + GetXpathIndex((XmlElement)current, nsManager).ToString() + "]/" + xpath;
                current = current.ParentNode;
            }

            return "/" + xpath;
        }

        private int GetXpathIndex(XmlElement element, XmlNamespaceManager nsManager)
        {
            var siblings = element.ParentNode.SelectNodes(string.Format("{0}:{1}", element.GetPrefixOfNamespace(element.NamespaceURI), element.LocalName), nsManager);

            for (var i = 0; i < siblings.Count; i++)
            {
                if (siblings[i] == element)
                    return i + 1;
            }

            return -1;
        }

        private void SaveSchemaZip(ImplementationGuideType igType, byte[] zip)
        {
            string directory = Shared.Helper.GetSchemasDirectory(igType.Name);
            string tempDirectory = directory + "TEMP";

            using (MemoryStream zipFileStream = new MemoryStream(zip))
            {
                ZipFile zipFile = ZipFile.Read(zipFileStream);
                zipFile.ExtractAll(tempDirectory);
            }

            MoveSchemaFiles(tempDirectory, directory);
        }

        private void SaveSchemaFile(ImplementationGuideType igType, byte[] schema)
        {
            string directory = Shared.Helper.GetSchemasDirectory(igType.Name);
            string tempDirectory = directory + "TEMP";
            string fullTempPath = Path.Combine(tempDirectory, igType.SchemaLocation);

            // Write file to temporary location
            Directory.CreateDirectory(tempDirectory);
            File.WriteAllBytes(fullTempPath, schema);

            MoveSchemaFiles(tempDirectory, directory);
        }

        private void MoveSchemaFiles(string tempDirectory, string directory)
        {
            string versionDirectory = directory + "_" + DateTime.Now.Ticks;

            // Move files to removal location
            if (Directory.Exists(directory))
                Directory.Move(directory, versionDirectory);

            // Move files from temporary location to permanent location
            Directory.Move(tempDirectory, directory);
        }

        private void SaveTemplateType(ImplementationGuideType igType, TypeModel.TemplateTypeModel model)
        {
            TemplateType templateType = new TemplateType();

            if (model.Id != null)
            {
                templateType = this.tdb.TemplateTypes.Single(y => y.Id == model.Id);
            }
            else
            {
                templateType.ImplementationGuideType = igType;
                this.tdb.TemplateTypes.AddObject(templateType);
            }

            if (templateType.Name != model.Name)
                templateType.Name = model.Name;

            if (templateType.OutputOrder != model.OutputOrder)
                templateType.OutputOrder = model.OutputOrder;

            if (templateType.RootContext != model.Context)
                templateType.RootContext = model.Context;

            if (templateType.RootContextType != model.ContextType)
                templateType.RootContextType = model.ContextType;
        }
    }
}
