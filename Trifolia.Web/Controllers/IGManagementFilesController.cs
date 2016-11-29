using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Ionic.Zip;

using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Web.Models.IGManagement;

namespace Trifolia.Web.Controllers
{
    public class IGManagementFilesController : Controller
    {
        #region Constructors

        private IObjectRepository tdb;

        public IGManagementFilesController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public IGManagementFilesController()
            : this(DBContext.Create())
        {

        }

        #endregion

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT)]
        public ActionResult Index(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have access to edit this implementation guide's files.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            
            FilesModel model = new FilesModel()
            {
                Id = ig.Id,
                Name = ig.NameWithVersion
            };

            return View("Files", model);
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT, SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW)]
        public JsonResult All(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have access to view this implementation guide's files.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            List<FileModel> files = new List<FileModel>();

            foreach (var file in ig.Files)
            {
                var latestVersionDate = file.Versions.Max(y => y.UpdatedDate);
                var latestVersion = file.Versions.Single(y => y.UpdatedDate == latestVersionDate);

                FileModel newFileModel = new FileModel()
                {
                    FileId = file.Id,
                    VersionId = latestVersion.Id,
                    Type = (FileModel.Types)Enum.Parse(typeof(FileModel.Types), file.ContentType, true),
                    Name = file.FileName,
                    Date = latestVersionDate.ToString("MM/dd/yyyy HH:mm:ss"),
                    Note = latestVersion.Note,
                    MimeType = file.MimeType,
                    Description = file.Description,
                    Url = file.Url
                };

                files.Add(newFileModel);
            }

            return Json(new { Files = files });
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT, SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW)]
        public JsonResult History(int fileId)
        {
            List<FileVersionModel> fileVersions = new List<FileVersionModel>();
            ImplementationGuideFile file = this.tdb.ImplementationGuideFiles.Single(y => y.Id == fileId);

            if (!CheckPoint.Instance.GrantViewImplementationGuide(file.ImplementationGuideId))
                throw new AuthorizationException("You do not have access to view this implementation guide's files.");

            foreach (var version in file.Versions.OrderByDescending(y => y.UpdatedDate))
            {
                FileVersionModel newFileVersionModel = new FileVersionModel()
                {
                    FileId = fileId,
                    VersionId = version.Id,
                    Date = version.UpdatedDate.ToString("MM/dd/yyyy HH:mm:ss"),
                    Note = version.Note
                };

                fileVersions.Add(newFileVersionModel);
            }

            return Json(fileVersions, JsonRequestBehavior.AllowGet);
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT, SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW)]
        public FileResult Download(int versionId)
        {
            ImplementationGuideFileData file = this.tdb.ImplementationGuideFileDatas.Single(y => y.Id == versionId);

            if (!CheckPoint.Instance.GrantViewImplementationGuide(file.ImplementationGuideFile.ImplementationGuideId))
                throw new AuthorizationException("You do not have access to view this implementation guide's files.");

            string fileName = file.ImplementationGuideFile.FileName;

            if (fileName.LastIndexOf(".") >= 0)
                fileName = fileName.Insert(fileName.LastIndexOf("."), "_" + file.UpdatedDate.ToString("yyyyMMddHHmmss"));

            return File(file.Data, file.ImplementationGuideFile.MimeType, fileName);
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT, SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW)]
        public FileResult DownloadAll(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have access to view this implementation guide's files.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            string fileName = string.Format("{0}_{1}.zip", ig.NameWithVersion, DateTime.Now.ToString("yyyyMMdd"));

            using (ZipFile zip = new ZipFile())
            {
                foreach (var file in ig.Files)
                {
                    var latestVersionDate = file.Versions.Max(y => y.UpdatedDate);
                    var latestVersion = file.Versions.Single(y => y.UpdatedDate == latestVersionDate);

                    zip.AddEntry(file.FileName, latestVersion.Data);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    zip.Save(ms);

                    return File(ms.ToArray(), "application/x-gzip", fileName);
                }
            }
        }
        
        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT)]
        public JsonResult Save(FilesModel model)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(model.Id))
                throw new AuthorizationException("You do not have access to edit this implementation guide's files.");

            foreach (FileModel fileModel in model.Files)
            {
                if (fileModel.IsRemoved)
                    RemoveFile(fileModel);
                else if (fileModel.FileId != null)
                    UpdateFile(fileModel);
                else
                    AddFile(model.Id, fileModel);

                this.tdb.SaveChanges();
            }

            return Json(new { });
        }

        private void RemoveFile(FileModel model)
        {
            ImplementationGuideFile foundFile = this.tdb.ImplementationGuideFiles.Single(y => y.Id == model.FileId);

            foreach (var fileData in foundFile.Versions.ToList())
            {
                this.tdb.ImplementationGuideFileDatas.DeleteObject(fileData);
            }

            this.tdb.ImplementationGuideFiles.DeleteObject(foundFile);
        }

        private void UpdateFile(FileModel model)
        {
            ImplementationGuideFile foundFile = this.tdb.ImplementationGuideFiles.Single(y => y.Id == model.FileId);

            foundFile.Description = model.Description;
            foundFile.Url = model.Url;

            if (model.Data != null && model.Data.Length > 0)
            {
                ImplementationGuideFileData version = CreateFileData(model);
                foundFile.Versions.Add(version);
            }
        }

        private void AddFile(int implementationGuideId, FileModel model)
        {
            if (model.Data == null || model.Data.Length == 0)
                throw new ArgumentNullException("Cannot add a file that does not have any data.");

            ImplementationGuideFile newFile = new ImplementationGuideFile()
            {
                ImplementationGuideId = implementationGuideId,
                ContentType = model.Type.ToString(),
                FileName = model.Name,
                MimeType = model.MimeType,
                Description = model.Description,
                Url = model.Url
            };

            ImplementationGuideFileData version = CreateFileData(model);
            newFile.Versions.Add(version);

            this.tdb.ImplementationGuideFiles.AddObject(newFile);
        }

        private ImplementationGuideFileData CreateFileData(FileModel model)
        {
            ImplementationGuideFileData data = new ImplementationGuideFileData()
            {
                Data = model.Data,
                UpdatedBy = CheckPoint.Instance.UserName,
                UpdatedDate = DateTime.Now,
                Note = model.Note
            };

            return data;
        }
    }
}
