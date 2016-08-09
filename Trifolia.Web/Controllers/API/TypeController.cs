using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Trifolia.DB;
using Trifolia.Web.Models.Type;
using Trifolia.Shared;
using Trifolia.Authorization;

using Ionic.Zip;

namespace Trifolia.Web.Controllers.API
{
    [SecurableAction]
    public class TypeController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public TypeController()
            : this(DBContext.Create())
        {

        }

        public TypeController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        #endregion

        [HttpGet, Route("api/Type")]
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

        [HttpDelete, Route("api/Type/{implementationGuideTypeId}")]
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

        [HttpGet, Route("api/Type/{implementationGuideTypeId}")]
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

        [HttpPost, Route("api/Type/Zip")]
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

        [HttpPost, Route("api/Type/Save")]
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
