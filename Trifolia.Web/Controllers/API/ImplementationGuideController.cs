extern alias fhir_dstu1;
extern alias fhir_dstu2;
extern alias fhir_stu3;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Mail;
using System.Transactions;

using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Authorization;
using Trifolia.Web.Models;
using Trifolia.Web.Models.IGManagement;
using Trifolia.Web.Models.IG;
using Trifolia.Web.Models.PermissionManagement;
using Trifolia.Web.Models.Export;
using Trifolia.Web.Extensions;
using Trifolia.Logging;
using Trifolia.Config;
using Trifolia.Generation.IG;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Generation.Versioning;
using Trifolia.Shared.Plugins;
using Trifolia.Shared.Validation;

namespace Trifolia.Web.Controllers.API
{
    public class ImplementationGuideController : ApiController
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public ImplementationGuideController()
            : this(DBContext.Create())
        {
        }

        public ImplementationGuideController(IObjectRepository tdb)
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

        /// <summary>
        /// Returns a list of implementation guides that are editable by the current user
        /// </summary>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.IGManagement.IGListItem&gt;</returns>
        [HttpGet, Route("api/ImplementationGuide/Editable"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public IEnumerable<IGListItem> GetEditableImplementationGuides()
        {
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            var editableImplementationGuides = this.tdb.ImplementationGuides.AsEnumerable();

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                editableImplementationGuides = (from igp in this.tdb.ViewImplementationGuidePermissions
                                                join ig in this.tdb.ImplementationGuides on igp.ImplementationGuideId equals ig.Id
                                                where igp.Permission == "Edit" && igp.UserId == currentUser.Id
                                                select ig).AsEnumerable();
            }

            return (from ig in editableImplementationGuides
                    select new IGListItem()
                    {
                        Id = ig.Id,
                        Identifier = ig.Identifier,
                        Name = ig.NameWithVersion,
                        IsPublished = ig.IsPublished(),
                        Namespace = ig.ImplementationGuideType.SchemaURI
                    })
                    .OrderBy(y => y.Name);
        }

        /// <summary>
        /// Returns all implementation guides that the current usre has access to.
        /// Includes some additional information, such as a URL for the implementation guide to perform an operation based on the listMode sepecified, and whether the current user has access to edit the IG.
        /// </summary>
        /// <param name="listMode">What type of URL to return for the implementation guide</param>
        /// <returns>Trifolia.Web.Models.IGManagement.ListModel</returns>
        [HttpGet, Route("api/ImplementationGuide"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public ListModel GetImplementationGuides(IGListModes listMode = IGListModes.Default)
        {
            Log.For(this).Trace("BEGIN: Getting list model for List and ListPartial");

            ListModel model = new ListModel();

            model.HideOrganization = !CheckPoint.Instance.IsDataAdmin;

            List<ImplementationGuide> implementationGuides = null;
            List<int> editableImplementationGuides = null;
            var user = CheckPoint.Instance.GetUser(this.tdb);
            var userId = user != null ? (int?) user.Id : null;
            var canEdit = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_EDIT);
            var userIsInternal = CheckPoint.Instance.IsDataAdmin;

            if (userId == null || !model.HideOrganization)
            {
                implementationGuides = this.tdb.ImplementationGuides.ToList();
            }
            else
            {
                implementationGuides = (from p in this.tdb.ViewImplementationGuidePermissions
                                        join ig in this.tdb.ImplementationGuides on p.ImplementationGuideId equals ig.Id
                                        where p.UserId == userId && p.Permission == "View"
                                        select ig)
                                        .Distinct()
                                        .ToList();
                editableImplementationGuides = (from p in this.tdb.ViewImplementationGuidePermissions
                                                where p.UserId == userId && p.Permission == "Edit"
                                                select p.ImplementationGuideId)
                                                .Distinct()
                                                .ToList();
            }

            model.Items = (from ig in implementationGuides
                           select new ListModel.ImplementationGuideItem()
                           {
                               Id = ig.Id,
                               Identifier = ig.Identifier,
                               Organization = ig.Organization != null ? ig.Organization.Name : string.Empty,
                               PublishDate = ig.PublishDate,
                               Title = string.Format("{0} {1}", ig.Name, (ig.Version > 1 ? "V" + ig.Version.ToString() : string.Empty)),
                               DisplayName = ig.DisplayName,
                               Type = ig.ImplementationGuideType.Name,
                               TypeNamespace = ig.ImplementationGuideType.SchemaURI,
                               Status = ig.PublishStatus != null ? ig.PublishStatus.Status : string.Empty,
                               Url = GetUrlForImplementationGuide(listMode, ig.Id),
                               CanEdit = canEdit && (userIsInternal || editableImplementationGuides.Contains(ig.Id))
                           })
                           .OrderBy(y => y.Title)
                           .ToList();

            model.Organizations = (from ig in implementationGuides
                                   join o in this.tdb.Organizations on ig.OrganizationId equals o.Id
                                   select new FilterItem()
                                   {
                                       Id = o.Id,
                                       Name = o.Name
                                   }).Distinct(new FilterItem.Comparer()).ToList();

            model.Statuses = (from ig in implementationGuides
                              join ps in this.tdb.PublishStatuses on ig.PublishStatusId equals ps.Id
                              select new ListModel.Status()
                              {
                                  Id = ps.Id,
                                  Name = ps.Status
                              }).Distinct(new ListModel.Status.Comparer()).ToList();

            Log.For(this).Trace("END: Getting list model for List and ListPartial");

            return model;
        }

        /// <summary>
        /// Gets the template types defined for the implementation guide. Any customizations to the template type's name and description for the specified
        /// implementation guide is returned here.
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.IGManagement.TemplateTypeListItem&gt;</returns>
        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/TemplateType")]
        public IEnumerable<TemplateTypeListItem> GetTemplateTypes(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to view this imlementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            var templateTypes = ig.ImplementationGuideType.TemplateTypes.AsEnumerable();

            return (from tt in templateTypes
                    select new TemplateTypeListItem()
                    {
                        Id = tt.Id,
                        Name = tt.Name,
                        RootContext = tt.RootContext,
                        RootContextType = tt.RootContextType,
                        Abbreviation = tt.GetAbbreviation(),
                        ImplementationGuideType = tt.ImplementationGuideType.Name
                    });
        }

        /// <summary>
        /// Gets all template types for all IGs
        /// </summary>
        /// <returns>IEnumerable&lt;Trifolia.Web.Models.IGManagement.TemplateTypeListItem&gt;</returns>
        [HttpGet, Route("api/ImplementationGuide/All/TemplateType"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public IEnumerable<TemplateTypeListItem> GetTemplateTypes()
        {
            var templateTypes = this.tdb.TemplateTypes.AsEnumerable();

            return (from tt in templateTypes
                    select new TemplateTypeListItem()
                    {
                        Id = tt.Id,
                        Name = tt.Name,
                        RootContext = tt.RootContext,
                        RootContextType = tt.RootContextType,
                        Abbreviation = tt.GetAbbreviation(),
                        ImplementationGuideType = tt.ImplementationGuideType.Name
                    })
                    .OrderBy(y => y.FullName);
        }

        /// <summary>
        /// Gets all value sets for the given implementation guide.
        /// </summary>
        /// <param name="implementationGuideId">The ID of the implementation guide</param>
        /// <param name="onlyStatic">Defaults to true, if not specified. When true, returns ONLY value sets that are statically bound to constraints in the implementation guide. When false, returns all value sets regardless of the static binding.</param>
        /// <returns>A list of all value sets, including id, name, oid, and binding date</returns>
        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/ValueSet"), SecurableAction(SecurableNames.EXPORT_WORD)]
        public List<VocabularyItemModel> GetImplementationGuideValueSets(int implementationGuideId, bool onlyStatic = true)
        {
            var ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            var igTypePlugin = ig.ImplementationGuideType.GetPlugin();
            bool? isStatic = onlyStatic ? (bool?)true : null;
            var valueSets = ig.GetValueSets(this.tdb, isStatic);

            return (from vs in valueSets
                    select new VocabularyItemModel()
                    {
                        Id = vs.ValueSet.Id,
                        Name = vs.ValueSet.Name,
                        Oid = vs.ValueSet.GetIdentifier(igTypePlugin),
                        BindingDate = vs.BindingDate != null ? vs.BindingDate.Value.ToString("MM/dd/yyyy") : string.Empty
                    }).ToList();
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public ViewModel GetImplementationGuide(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to retrieve this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            ImplementationGuide nextVersionIg = this.tdb.ImplementationGuides.SingleOrDefault(y => y.PreviousVersionImplementationGuideId == ig.Id);
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, ig.Id);

            bool canEdit = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_EDIT);
            bool canBookmarks = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS);
            bool grantEdit = CheckPoint.Instance.GrantEditImplementationGuide(ig.Id);
            bool canEditTemplate = CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_EDIT);

            var viewModel = new ViewModel()
            {
                Id = ig.Id,
                Name = ig.Name + (ig.Version > 1 ? " V" + ig.Version.ToString() : string.Empty),
                DisplayName = ig.DisplayName,
                Organization = ig.Organization != null ? ig.Organization.Name : string.Empty,
                PublishDate = ig.PublishDate.HasValue ? ig.PublishDate.Value.ToLongDateString() : string.Empty,
                Type = ig.ImplementationGuideType != null ? ig.ImplementationGuideType.Name : string.Empty,
                ViewAuditTrail = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_AUDIT_TRAIL),
                ViewNotes = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_NOTES),
                ViewPrimitives = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_PRIMITIVES),
                ViewFiles = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW, SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT),
                ViewWebIG = CheckPoint.Instance.HasSecurables(SecurableNames.WEB_IG),
                ShowExportMSWord = CheckPoint.Instance.HasSecurables(SecurableNames.EXPORT_WORD),
                ShowExportSchematron = CheckPoint.Instance.HasSecurables(SecurableNames.EXPORT_SCHEMATRON),
                ShowExportVocabulary = CheckPoint.Instance.HasSecurables(SecurableNames.EXPORT_VOCAB),
                ShowExportGreen = CheckPoint.Instance.HasSecurables(SecurableNames.EXPORT_GREEN),
                ShowExportXML = CheckPoint.Instance.HasSecurables(SecurableNames.EXPORT_XML),
                ShowManageFiles = CheckPoint.Instance.HasSecurables(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT),
                ShowEditIG = grantEdit && canEdit,
                ShowEditTemplate = canEditTemplate,
                ShowEditBookmarks = grantEdit && canBookmarks,
                ShowPublish = grantEdit && canEdit,
                ShowNewVersion = grantEdit && canEdit,
                ShowDelete = grantEdit && canEdit,
                EnableNewVersion = nextVersionIg == null && ig.IsPublished(),
                Status = ig.PublishStatus != null ? ig.PublishStatus.Status : null
            };

            if (nextVersionIg != null && CheckPoint.Instance.GrantViewImplementationGuide(nextVersionIg.Id))
                viewModel.NextVersionImplementationGuideId = nextVersionIg != null ? new int?(nextVersionIg.Id) : null;

            if (ig.PreviousVersionImplementationGuideId.HasValue && CheckPoint.Instance.GrantViewImplementationGuide(ig.PreviousVersionImplementationGuideId.Value))
            {
                ImplementationGuide lPreviousVersion = this.tdb.ImplementationGuides.Single(previous => previous.Id == ig.PreviousVersionImplementationGuideId.Value);

                viewModel.HasPreviousVersion = true;
                viewModel.PreviousVersionImplementationGuideId = lPreviousVersion.Id;
                viewModel.PreviousVersionIgName = lPreviousVersion.Name;
            }
            
            // Build the list of web publications
            foreach (var file in ig.Files.Where(y => y.ContentType == "DataSnapshot"))
            {
                string url = string.Format("/IG/Web/{0}", file.Url);
                viewModel.WebPublications.Add(url);
            }

            List<Template> lTemplates = ig.GetRecursiveTemplates(tdb);

            var userId = CheckPoint.Instance.User.Id;
            var editableTemplates = this.tdb.ViewTemplatePermissions.Where(y => y.UserId == userId && y.Permission == "Edit").ToList();
            bool userIsInternal = CheckPoint.Instance.IsDataAdmin;
            bool userCanEdit = CheckPoint.Instance.HasSecurables(SecurableNames.TEMPLATE_EDIT);

            // Get Templates
            foreach (Template cTemplate in lTemplates)
            {
                var igTemplateType = igSettings.TemplateTypes.SingleOrDefault(y => y.TemplateTypeId == cTemplate.TemplateType.Id);

                ViewModel.TemplateItem newTemplateItem = new ViewModel.TemplateItem()
                {
                    Id = cTemplate.Id,
                    Name = cTemplate.Name,
                    Oid = cTemplate.Oid,
                    Type = igTemplateType != null ? igTemplateType.Name : cTemplate.TemplateType.Name,
                    Description = cTemplate.Description,
                    HasGreenModel = cTemplate.GreenTemplates.Any(),
                    Status = cTemplate.Status != null ? cTemplate.Status.Status : null,
                    CanEdit = userCanEdit && (userIsInternal || editableTemplates.Count(y => y.TemplateId == cTemplate.Id) > 0)
                };

                var newViewTemplateType = new ViewTemplateType()
                {
                    Name = igTemplateType != null ? igTemplateType.Name : cTemplate.TemplateType.Name,
                    Description = igTemplateType != null ? igTemplateType.DetailsText : null,
                    Order = igTemplateType != null ? igTemplateType.OutputOrder : -1
                };

                if (viewModel.TemplateTypes.Count(y => y.Name == newViewTemplateType.Name) == 0)
                    viewModel.TemplateTypes.Add(newViewTemplateType);

                viewModel.Templates.Add(newTemplateItem);
            }

            // Order the template types
            viewModel.TemplateTypes = viewModel.TemplateTypes.OrderBy(y => y.Order).ToList();

            return viewModel;
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/AuditTrail"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_AUDIT_TRAIL)]
        public IEnumerable<AuditEntryModel> GetImplementationGuideAuditTrail(int implementationGuideId, DateTime startDate, DateTime endDate)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to retrieve this implementation guide");

            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

            var auditTrails = (from viat in this.tdb.ViewIGAuditTrails
                               where viat.ImplementationGuideId == implementationGuideId &&
                                    viat.AuditDate >= startDate &&
                                    viat.AuditDate <= endDate
                               select new AuditEntryModel()
                               {
                                   AuditDate = viat.AuditDate,
                                   ImplementationGuideId = viat.ImplementationGuideId,
                                   ConformanceNumber = viat.ConformanceNumber,
                                   IP = viat.Ip,
                                   Note = viat.Note,
                                   TemplateName = viat.TemplateName,
                                   Username = viat.UserName,
                                   Type = viat.Type
                               });

            return auditTrails;
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Note"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_NOTES)]
        public IEnumerable<ImplementationGuideNoteModel> GetImplementationGuideNotes(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to retrieve this implementation guide");

            var notes = (from t in this.tdb.Templates
                         where t.OwningImplementationGuideId == implementationGuideId && !string.IsNullOrEmpty(t.Notes)
                         select new ImplementationGuideNoteModel()
                         {
                             TemplateId = t.Id,
                             TemplateName = t.Name,
                             OwningImplementationGuideId = t.OwningImplementationGuideId,
                             ConstraintNumber = null,
                             Type = "Template/Profile",
                             Note = t.Notes
                         }).Union(
                         from t in this.tdb.Templates
                         join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                         where t.OwningImplementationGuideId == implementationGuideId && !string.IsNullOrEmpty(tc.Notes)
                         select new ImplementationGuideNoteModel()
                         {
                             TemplateId = t.Id,
                             TemplateName = t.Name,
                             OwningImplementationGuideId = t.OwningImplementationGuideId,
                             ConstraintNumber = tc.Number,
                             Type = "Constraint",
                             Note = tc.Notes
                         });

            return notes;
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Category"), SecurableAction]
        public IEnumerable<string> GetImplementationGuideCategories(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new UnauthorizedAccessException();

            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
            List<string> categoriesList = new List<string>();

            // Get categories from the IG's settings
            string categories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
            if (!string.IsNullOrEmpty(categories))
            {
                string[] categoriesSplit = categories.Split(',');

                foreach (string category in categoriesSplit)
                {
                    categoriesList.Add(category.Replace("###", ","));
                }
            }

            return categoriesList;
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Primitive"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_PRIMITIVES)]
        public IEnumerable<ImplementationGuidePrimitiveModel> GetImplementationGuidePrimitives(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to retrieve this implementation guide");

            var primitives = (from t in this.tdb.Templates
                              join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                              where t.OwningImplementationGuideId == implementationGuideId && tc.IsPrimitive == true
                              select new ImplementationGuidePrimitiveModel()
                              {
                                  TemplateId = t.Id,
                                  TemplateName = t.Name,
                                  OwningImplementationGuideId = t.OwningImplementationGuideId,
                                  ConstraintNumber = tc.Number.Value,
                                  PrimitiveText = tc.PrimitiveText,
                                  Schematron = tc.Schematron
                              });

            return primitives;
        }
        
        private static string GetIGFileContentTypeDisplay(string contentType)
        {
            ImplementationGuideFileTypes igFileType = ImplementationGuideFileTypes.Unknown;
            Enum.TryParse<ImplementationGuideFileTypes>(contentType, out igFileType);

            switch (igFileType)
            {
                case ImplementationGuideFileTypes.Schematron:
                case ImplementationGuideFileTypes.Vocabulary:
                    return igFileType.ToString();
                case ImplementationGuideFileTypes.ImplementationGuide:
                    return "Implementation Guide Document";
                case ImplementationGuideFileTypes.SchematronHelper:
                    return "Schematron Helper";
                case ImplementationGuideFileTypes.DeliverableSample:
                    return "Sample (deliverable)";
                case ImplementationGuideFileTypes.GoodSample:
                    return "Test Sample (good)";
                case ImplementationGuideFileTypes.BadSample:
                    return "Test Sample (bad)";
                case ImplementationGuideFileTypes.FHIRResourceInstance:
                    return "FHIR Resource Instance (XML or JSON)";
                default:
                    return contentType;
            }
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/File"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW)]
        public IEnumerable<ImplementationGuideFileModel> GetImplementationGuideFiles(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to retrieve this implementation guide");

            var retFiles = new List<ImplementationGuideFileModel>();
            var files = this.tdb.ImplementationGuideFiles
                .Where(y => y.ImplementationGuideId == implementationGuideId && y.ContentType != "DataSnapshot")
                .OrderBy(y => y.ContentType)
                .ThenBy(y => y.FileName);

            foreach (var currentFile in files)
            {
                ImplementationGuideFileData latestVersion = currentFile.GetLatestData();

                var newItem = new ImplementationGuideFileModel()
                {
                    FileId = currentFile.Id,
                    VersionId = latestVersion.Id,
                    Date = latestVersion.UpdatedDate,
                    Description = currentFile.Description,
                    Name = currentFile.FileName,
                    Type = GetIGFileContentTypeDisplay(currentFile.ContentType)
                };

                retFiles.Add(newItem);
            }

            return retFiles;
        }

        [HttpDelete, Route("api/ImplementationGuide/{implementationGuideId}"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public void DeleteImplementationGuide(int implementationGuideId, int? replaceImplementationGuideId = null)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to edit this implementation guide");

            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            implementationGuide.Delete(this.tdb, replaceImplementationGuideId);

            this.tdb.SaveChanges();
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Template"), SecurableAction(SecurableNames.TEMPLATE_LIST)]
        public dynamic GetTemplates(int implementationGuideId, [FromUri] int[] parentTemplateIds = null, bool inferred = true, [FromUri] string[] categories = null)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to view this implementation guide");

            if (parentTemplateIds != null && parentTemplateIds.Length == 1 && parentTemplateIds[0] == 0)
                parentTemplateIds = null;

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            List<int> igVersionIds = new List<int>();
            ImplementationGuide current = ig;

            while (current != null)
            {
                igVersionIds.Add(current.Id);

                if (current.PreviousVersionImplementationGuideId != null)
                    current = this.tdb.ImplementationGuides.Single(y => y.Id == current.PreviousVersionImplementationGuideId);
                else
                    current = null;
            }

            var recursiveTemplates = ig.GetRecursiveTemplates(
                this.tdb, 
                parentTemplateIds != null && parentTemplateIds.Length > 0 ? parentTemplateIds.ToList() : null, 
                inferred,
                categories);

            var results = (from t in recursiveTemplates
                           select new
                           {
                               Id = t.Id,
                               Name = t.Name,
                               Oid = t.Oid,
                               ThisIG = igVersionIds.Contains(t.OwningImplementationGuideId)
                           });

            return results;
        }

        [HttpPost, Route("api/ImplementationGuide/{implementationGuideId}/Unpublish"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public void Unpublish(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to edit this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.DefaultIfEmpty(null).SingleOrDefault(i => i.Id == implementationGuideId);
            if (ig == null) throw new ArgumentOutOfRangeException("Could not find an Implementation Guide with the passed in ID");

            string lDraftStatus = Shared.PublishStatuses.Draft.ToString();
            PublishStatus lStatus = this.tdb.PublishStatuses.DefaultIfEmpty(null).SingleOrDefault(s => s.Status == lDraftStatus);

            ig.PublishStatus = lStatus;
            ig.PublishDate = null;

            string lPublishedString = Shared.PublishStatuses.Published.ToString();
            foreach (Template lChildTemplate in ig.ChildTemplates.Where(t => t.Status == null || t.Status.Status == lPublishedString))
            {
                lChildTemplate.Status = lStatus;
            }

            this.tdb.SaveChanges();
        }

        [HttpPost, Route("api/ImplementationGuide/{implementationGuideId}/Ballot"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public void Ballot(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to edit this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.DefaultIfEmpty(null).SingleOrDefault(i => i.Id == implementationGuideId);
            if (ig == null) throw new ArgumentOutOfRangeException("Could not find an Implementation Guide with the passed in ID");
            string lBallotStatus = Shared.PublishStatuses.Ballot.ToString();
            PublishStatus lStatus = this.tdb.PublishStatuses.DefaultIfEmpty(null).SingleOrDefault(s => s.Status == lBallotStatus);

            ig.PublishStatus = lStatus;

            string lDraftStatus = Shared.PublishStatuses.Draft.ToString();
            foreach (Template lChildTemplate in ig.ChildTemplates.Where(t => t.Status == null || t.Status.Status == lDraftStatus))
            {
                lChildTemplate.Status = lStatus;
            }

            this.tdb.SaveChanges();
        }

        [HttpPost, Route("api/ImplementationGuide/{implementationGuideId}/Publish"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public void Publish(int implementationGuideId, DateTime publishDate)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to edit this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            string lPublishedStatus = Shared.PublishStatuses.Published.ToString();
            PublishStatus status = this.tdb.PublishStatuses.SingleOrDefault(y => y.Status == lPublishedStatus);

            ig.PublishDate = publishDate;
            ig.PublishStatusId = status.Id;

            string lDraftStatus = Shared.PublishStatuses.Draft.ToString();
            foreach (Template lChildTemplate in ig.ChildTemplates.Where(t => t.Status == null || t.Status.Status == lDraftStatus))
            {
                lChildTemplate.Status = status;
            }

            string lBallotStatus = Shared.PublishStatuses.Ballot.ToString();
            foreach (Template lChildTemplate in ig.ChildTemplates.Where(t => t.Status == null || t.Status.Status == lBallotStatus))
            {
                lChildTemplate.Status = status;
            }

            this.tdb.SaveChanges();
        }

        [HttpPost, Route("api/ImplementationGuide/{implementationGuideId}/Draft"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public void Draft(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to edit this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.DefaultIfEmpty(null).SingleOrDefault(i => i.Id == implementationGuideId);
            if (ig == null) throw new ArgumentOutOfRangeException("Could not find an Implementation Guide with the passed in ID");
            string lDraftStatus = Shared.PublishStatuses.Draft.ToString();
            PublishStatus lStatus = this.tdb.PublishStatuses.DefaultIfEmpty(null).SingleOrDefault(s => s.Status == lDraftStatus);

            ig.PublishStatus = lStatus;

            string lStatusString = Shared.PublishStatuses.Ballot.ToString();
            foreach (Template lChildTemplate in ig.ChildTemplates.Where(t => t.Status == null || t.Status.Status == Shared.PublishStatuses.Ballot.ToString()))
            {
                lChildTemplate.Status = lStatus;
            }

            this.tdb.SaveChanges();
        }

        [HttpGet, Route("api/ImplementationGuide/Unauthorized"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public dynamic GetUnauthorizedImplementationGuides()
        {
            var currentUser = CheckPoint.Instance.GetUser(this.tdb);
            var accessibleImplementationGuides = this.tdb.ViewImplementationGuidePermissions
                .Where(y => y.UserId == currentUser.Id)
                .Select(y => y.ImplementationGuideId).ToList();
            var allImplementationGuides = this.tdb.ImplementationGuides.ToList();

            return (from ig in allImplementationGuides
                    where !accessibleImplementationGuides.Contains(ig.Id) && ig.AllowAccessRequests && ig.AccessManagerId != null
                    select new
                    {
                        Id = ig.Id,
                        Name = ig.GetDisplayName()
                    }).ToArray();
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Validate"), SecurableAction]
        public ValidationResults Validate(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You are not authorized to view this implementation guide");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            var plugin = ig.ImplementationGuideType.GetPlugin();
            var validator = plugin.GetValidator(this.tdb);
            var results = validator.ValidateImplementationGuide(implementationGuideId);

            return results;
        }

        [HttpGet, Route("api/ImplementationGuide/URL/Validate"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT)]
        public bool ValidateUrl(string url, int? ignoreFileId)
        {
            var files = this.tdb.ImplementationGuideFiles.Where(y =>
                (ignoreFileId == null || y.Id != ignoreFileId) && 
                y.Url != null && 
                y.Url.ToLower() == url.ToLower());
            return files.Count() == 0;
        }

        #region Edit

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Images")]
        public object GetImplementationGuideImageList(int implementationGuideId)
        {
            return tdb.ImplementationGuideFiles
                               .Where(x => x.ImplementationGuideId == implementationGuideId)
                               .Where(x => x.ContentType == "Image")
                               .Select(x => new
                               {
                                   x.Id,
                                   x.FileName,
                                   x.Description,
                                   x.MimeType
                               });
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Image/{fileName}")]
        public HttpResponseMessage GetImplementationGuideImage(int implementationGuideId, string fileName)
        {
            var image = tdb.ImplementationGuideFiles
                    .Where(x => x.ImplementationGuideId == implementationGuideId)
                    .Where(x => x.ContentType == "Image")
                    .FirstOrDefault(x => x.FileName == fileName);

            if (image == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            
            var result = new HttpResponseMessage(HttpStatusCode.OK);

            result.Content = new ByteArrayContent(image.GetLatestData().Data);
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.MimeType);

            return result;
        }

        /// <summary>
        /// Returns all information related to an implementation guide to be edited
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide</param>
        /// <returns>Trifolia.Web.Models.IGManagement.EditModel</returns>
        [HttpGet, Route("api/ImplementationGuide/Edit/{implementationGuideId?}"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public EditModel Edit(int? implementationGuideId = null)
        {
            if (implementationGuideId != null && !CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId.Value))
                throw new AuthorizationException("You do not have permissions to edit this implementation guide!");

            ImplementationGuide implementationGuide = implementationGuideId != null ? this.tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId) : null;
            EditModel model = new EditModel();

            if (implementationGuideId != null)
            {
                model = GetEditModel(implementationGuideId.Value);

                // Only add the implementation guide as a previous if it has been published. Wouldn't make sense to have a new version of a draft IG
                IEnumerable<ImplementationGuide> availableImplementationGuides = null;

                if (CheckPoint.Instance.IsDataAdmin)
                    availableImplementationGuides = (from ig in this.tdb.ImplementationGuides
                                                     join ig2 in this.tdb.ImplementationGuides on ig.Id equals ig2.PreviousVersionImplementationGuideId into ig2j
                                                     from nig in ig2j.DefaultIfEmpty()
                                                     where (nig == null || nig.Id == implementationGuideId)
                                                     select ig).Distinct();
                else
                    availableImplementationGuides = (from vig in this.tdb.ViewImplementationGuidePermissions
                                                     join ig in this.tdb.ImplementationGuides on vig.ImplementationGuideId equals ig.Id
                                                     join ig2 in this.tdb.ImplementationGuides on ig.Id equals ig2.PreviousVersionImplementationGuideId into ig2j
                                                     from nig in ig2j.DefaultIfEmpty()
                                                     where vig.UserId == CheckPoint.Instance.User.Id && (nig == null || nig.Id == implementationGuideId)
                                                     select ig).Distinct();

                foreach (ImplementationGuide ig in availableImplementationGuides.Where(y => y.IsPublished()))
                {
                    model.PreviousIgs.Add(ig.Id, ig.NameWithVersion);
                }
            }
            else
            {
                model.CardinalityAtLeastOne = "at least one [1..*]";
                model.CardinalityExactlyOne = "exactly one [1..1]";
                model.CardinalityZero = "[0..0]";
                model.CardinalityZeroOrMore = "zero or more [0..*]";
                model.CardinalityZeroOrOne = "zero or one [0..1]";
            }

            User me = CheckPoint.Instance.GetUser(this.tdb);
            EditModel.Permission newUserDefaultPermission = new EditModel.Permission()
            {
                Id = me.Id,
                Name = string.Format("{0} {1} ({2})", me.FirstName, me.LastName, me.Email),
                Type = Models.PermissionManagement.PermissionTypes.User
            };

            // Make sure that the current user is part of the default view permissions for the IG
            if (!CheckPoint.Instance.IsDataAdmin && !this.UserHasPermissions(me, model.DefaultViewPermissions))
                model.DefaultViewPermissions.Add(newUserDefaultPermission);

            if (!CheckPoint.Instance.IsDataAdmin && !this.UserHasPermissions(me, model.DefaultEditPermissions))
                model.DefaultEditPermissions.Add(newUserDefaultPermission);

            return model;
        }

        /// <summary>
        /// Validates that the specified name is unique.
        /// </summary>
        /// <param name="igName">The name to validate</param>
        /// <param name="implementationGuideId">If an implementation guide id is specified here, it will ignore the implementation guide as part of validation so that validation doesn't fail against itself.</param>
        /// <returns>bool</returns>
        [HttpGet, Route("api/ImplementationGuide/Validate/Name")]
        public bool ValidateName(string igName, int? implementationGuideId = null)
        {
            if (string.IsNullOrEmpty(igName))
                return false;

            return this.ValidateName(this.tdb, igName, implementationGuideId);
        }

        /// <summary>
        /// Validates that the specified identifier is unique.
        /// </summary>
        /// <param name="identifier">The identifier to validate</param>
        /// <param name="implementationGuideId">If an implementation guide id is specified here, it will ignore the implementation guide as part of validation so that validation doesn't fail against itself.</param>
        /// <returns>bool</returns>
        [HttpGet, Route("api/ImplementationGuide/Validate/Identifier")]
        public bool ValidateIdentifier(string identifier, int? implementationGuideId)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            return this.ValidateIdentifier(this.tdb, identifier, implementationGuideId);
        }

        private List<int> GetIgnoredImplementationGuides(IObjectRepository tdb, int? implementationGuideId = null)
        {
            List<int> ignoreIgs = new List<int>();

            if (implementationGuideId != null)
            {

                // Ignore names for previous versions of the specified implementation guide
                var current = tdb.ImplementationGuides.SingleOrDefault(y => y.Id == implementationGuideId);
                while (current != null)
                {
                    ignoreIgs.Add(current.Id);
                    current = tdb.ImplementationGuides.SingleOrDefault(y => y.Id == current.PreviousVersionImplementationGuideId);
                }

                // Ignore names for next versions of the specified implementation guide
                current = tdb.ImplementationGuides.FirstOrDefault(y => y.PreviousVersionImplementationGuideId == implementationGuideId);
                while (current != null)
                {
                    ignoreIgs.Add(current.Id);
                    current = tdb.ImplementationGuides.FirstOrDefault(y => y.PreviousVersionImplementationGuideId == current.Id);
                }
            }

            return ignoreIgs;
        }

        private bool ValidateName(IObjectRepository tdb, string igName, int? implementationGuideId = null)
        {
            List<int> ignoreIgs = GetIgnoredImplementationGuides(tdb, implementationGuideId);
            var count = tdb.ImplementationGuides.Count(y => !ignoreIgs.Contains(y.Id) && y.Name.ToLower() == igName.ToLower());
            return count == 0;
        }

        private bool ValidateIdentifier(IObjectRepository tdb, string identifier, int? implementationGuideId = null)
        {
            List<int> ignoreIgs = GetIgnoredImplementationGuides(tdb, implementationGuideId);
            var count = tdb.ImplementationGuides.Count(y => !ignoreIgs.Contains(y.Id) && y.Identifier.ToLower() == identifier.ToLower());
            return count == 0;
        }

        /// <summary>
        /// Ensures that in the permissions list provided, the current user shows up as either a direct permissions to the user,
        /// permitted via a group, or permitted via the entire organization.
        /// </summary>
        /// <exception cref="Exception">Throws an exception if the user is not part of the list, which is intended to be handled in the calling method.</exception>
        private bool UserHasPermissions(User user, List<EditModel.Permission> permissions)
        {
            if (permissions.Exists(y => y.Type == PermissionTypes.Everyone))
                return true;

            var foundMeInGroups = (from p in permissions
                                   join ug in this.tdb.UserGroups on p.Id equals ug.GroupId
                                   where p.Type == Models.PermissionManagement.PermissionTypes.Group &&
                                   ug.UserId == user.Id
                                   select ug);

            var foundMeDirectly = (from p in permissions
                                   where p.Id == user.Id && p.Type == Models.PermissionManagement.PermissionTypes.User
                                   select p);

            return foundMeInGroups.Count() > 0 || foundMeDirectly.Count() > 0;
        }

        private EditModel GetEditModel(int implementationGuideId)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            ImplementationGuide previousIg = ig.PreviousVersionImplementationGuideId != null ? this.tdb.ImplementationGuides.Single(y => y.Id == ig.PreviousVersionImplementationGuideId) : null;
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);

            EditModel model = new EditModel()
            {
                Id = ig.Id,
                Identifier = ig.Identifier,
                Name = ig.Name,
                DisplayName = ig.DisplayName,
                WebDisplayName = ig.WebDisplayName,
                WebDescription = ig.WebDescription,
                WebReadmeOverview = ig.WebReadmeOverview,
                TypeId = ig.ImplementationGuideTypeId,
                PreviousVersionId = previousIg != null ? (int?)previousIg.Id : null,
                PreviousVersionName = previousIg != null ? previousIg.NameWithVersion : null,
                CurrentVersion = ig.Version,
                ConsolidatedFormat = igSettings.GetBoolSetting(IGSettingsManager.SettingProperty.UseConsolidatedConstraintFormat),
                CardinalityAtLeastOne = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne),
                CardinalityExactlyOne = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne),
                CardinalityZero = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZero),
                CardinalityZeroOrMore = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore),
                CardinalityZeroOrOne = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne),
                DisableVersionFields = ig.PreviousVersionImplementationGuideId != null,
                OrganizationId = ig.OrganizationId,
                AccessManagerId = ig.AccessManagerId,
                AllowAccessRequests = ig.AllowAccessRequests,
                Html = igSettings.GetSetting(IGSettingsManager.SettingProperty.Volume1Html)
            };

            // Load categories
            string categories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
            if (!string.IsNullOrEmpty(categories))
                model.Categories = new List<string>(categories.Split(','));

            model.Sections = (from igs in ig.Sections
                              select new EditModel.Section()
                              {
                                  Id = igs.Id,
                                  Heading = igs.Heading,
                                  Content = igs.Content,
                                  Order = igs.Order,
                                  Level = igs.Level
                              }).ToList();

            // Load Template Types
            foreach (TemplateType cTemplateType in ig.ImplementationGuideType.TemplateTypes.OrderBy(y => y.OutputOrder))
            {
                ImplementationGuideTemplateType igTemplateType = ig.TemplateTypes.SingleOrDefault(y => y.TemplateTypeId == cTemplateType.Id);

                EditModel.TemplateTypeItem newTemplateTypeItem = new EditModel.TemplateTypeItem()
                {
                    DefaultName = cTemplateType.Name,
                    Name = igTemplateType != null ? igTemplateType.Name : cTemplateType.Name,
                    Description = igTemplateType != null ? igTemplateType.DetailsText : string.Empty
                };

                model.TemplateTypes.Add(newTemplateTypeItem);
            }

            // Load Custom Schematron Patterns
            foreach (ImplementationGuideSchematronPattern cSchematronPattern in ig.SchematronPatterns)
            {
                EditModel.CustomSchematronItem newCustomSchematronItem = new EditModel.CustomSchematronItem()
                {
                    Id = cSchematronPattern.Id,
                    Phase = cSchematronPattern.Phase,
                    PatternId = cSchematronPattern.PatternId,
                    PatternContent = cSchematronPattern.PatternContent
                };

                model.CustomSchematrons.Add(newCustomSchematronItem);
            }

            // Load Permissions
            foreach (ImplementationGuidePermission cPermission in ig.Permissions)
            {
                EditModel.Permission newPermission = new EditModel.Permission()
                {
                    Type = cPermission.MemberType()
                };

                switch (cPermission.MemberType())
                {
                    case Models.PermissionManagement.PermissionTypes.Everyone:
                        newPermission.Id = 0;
                        newPermission.Name = "Everyone";
                        break;
                    case Models.PermissionManagement.PermissionTypes.Group:
                        newPermission.Id = cPermission.GroupId.Value;
                        newPermission.Name = cPermission.Group.Name;
                        break;
                    case Models.PermissionManagement.PermissionTypes.User:
                        newPermission.Id = cPermission.UserId.Value;
                        newPermission.Name = cPermission.User.FirstName + " " + cPermission.User.LastName + " (" + cPermission.User.Email + ")";
                        break;
                }

                if (cPermission.Permission == "View")
                    model.ViewPermissions.Add(newPermission);
                else if (cPermission.Permission == "Edit")
                    model.EditPermissions.Add(newPermission);
            }

            return model;
        }

        /// <summary>
        /// Persists the implementation guide edit model specified. If the model includes a non-0 Id then the existing implementation guide
        /// is updated with the changes specified in the model. In this case, the current user must have access to edit the implementation guide.
        /// </summary>
        /// <param name="aModel">The edit model to create/update</param>
        /// <returns>int</returns>
        /// <remarks>Returns the id of the implementation guide</remarks>
        [HttpPost, Route("api/ImplementationGuide/Save"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public int SaveImplementationGuide(EditModel aModel)
        {
            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                // Use a transaction scope to make sure that any errors that occur are rolled back, even though there shouldn't be any errors
                using (var scope = auditedTdb.BeginTransaction())
                {
                    try
                    {
                        ImplementationGuide ig = null;
                        User me = CheckPoint.Instance.GetUser(auditedTdb);

                        // Validate certain aspects of the IG
                        if (!CheckPoint.Instance.IsDataAdmin && (!this.UserHasPermissions(me, aModel.ViewPermissions) || !this.UserHasPermissions(me, aModel.EditPermissions)))
                            throw new Exception("Saving the implementation guide with the permissions specified would remove your ability to view/edit this implementation guide. Please add yourself to view/edit permissions before saving.");

                        if (aModel.Id == 0)
                        {
                            if (!this.ValidateName(auditedTdb, aModel.Name))
                                throw new Exception("An implementation guide with that name already exists!");

                            ig = new ImplementationGuide();
                            ig.Version = 1;
                            auditedTdb.ImplementationGuides.Add(ig);
                        }
                        else
                        {
                            ig = auditedTdb.ImplementationGuides.Single(y => y.Id == aModel.Id);

                            if (!CheckPoint.Instance.GrantEditImplementationGuide(aModel.Id))
                                throw new AuthorizationException("You do not have permission to edit this implementation guide");

                            if (!this.ValidateName(auditedTdb, aModel.Name, aModel.Id))
                                throw new Exception("An implementation guide with that name already exists!");
                        }

                        ig.Identifier = aModel.Identifier;
                        ig.Name = aModel.Name;
                        ig.DisplayName = aModel.DisplayName;
                        ig.WebDisplayName = aModel.WebDisplayName;
                        ig.WebDescription = aModel.WebDescription;
                        ig.WebReadmeOverview = aModel.WebReadmeOverview;
                        ig.ImplementationGuideType = null;
                        ig.ImplementationGuideTypeId = aModel.TypeId.Value;
                        ig.OrganizationId = aModel.OrganizationId;
                        ig.AccessManagerId = aModel.AccessManagerId;
                        ig.AllowAccessRequests = aModel.AllowAccessRequests;

                        // Set the "Draft" publish status by default
                        if (ig.PublishStatus == null)
                            ig.PublishStatus = PublishStatus.GetDraftStatus(auditedTdb);

                        if (aModel.PreviousVersionId.HasValue)
                        {
                            ig.PreviousVersionImplementationGuideId = aModel.PreviousVersionId;
                            ImplementationGuide lPreviousVersion
                                = auditedTdb.ImplementationGuides.Single(previousIg => previousIg.Id == aModel.PreviousVersionId.Value);

                            int nextVersion = (lPreviousVersion.Version != null ? lPreviousVersion.Version.Value : 1) + 1;

                            if (ig.Version != nextVersion)
                                ig.Version = nextVersion;
                        }

                        // Delete sections that don't exist in the model
                        ig.Sections.Where(y => aModel.Sections.Count(x => x.Id == y.Id) == 0)
                            .ToList()
                            .ForEach(y =>
                            {
                                auditedTdb.ImplementationGuideSections.Remove(y);
                            });

                        // Add and update sections that are in the model
                        foreach (var cSection in aModel.Sections)
                        {
                            ImplementationGuideSection dbSection = null;

                            if (cSection.Id != null)
                            {
                                dbSection = ig.Sections.Single(y => y.Id == cSection.Id);
                            }
                            else
                            {
                                dbSection = new ImplementationGuideSection();
                                dbSection.ImplementationGuide = ig;
                            }

                            dbSection.Heading = cSection.Heading;
                            dbSection.Content = cSection.Content;
                            dbSection.Order = cSection.Order;
                            dbSection.Level = cSection.Level;
                        }

                        this.SaveTemplateTypes(auditedTdb, ig, aModel);
                        this.SaveCustomSchematron(auditedTdb, ig, aModel);
                        this.SavePermissions(auditedTdb, ig, aModel);

                        // Find any constraints associated with the IG that have a category that is removed
                        var removedCategoryConstraints = (from tc in this.tdb.TemplateConstraints
                                                          join t in this.tdb.Templates on tc.TemplateId equals t.Id
                                                          where !string.IsNullOrEmpty(tc.Category) &&
                                                          !aModel.Categories.Contains(tc.Category)
                                                          select tc);

                        foreach (var constraint in removedCategoryConstraints)
                        {
                            constraint.Category = null;
                        }

                        auditedTdb.SaveChanges();

                        // Cardinality
                        IGSettingsManager settings = new IGSettingsManager(auditedTdb, ig.Id);

                        settings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne, aModel.CardinalityAtLeastOne);
                        settings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne, aModel.CardinalityExactlyOne);
                        settings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZero, aModel.CardinalityZero);
                        settings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore, aModel.CardinalityZeroOrMore);
                        settings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne, aModel.CardinalityZeroOrOne);
                        settings.SaveSetting(IGSettingsManager.SettingProperty.Volume1Html, aModel.Html != null ? aModel.Html : string.Empty);

                        settings.SaveBoolSetting(IGSettingsManager.SettingProperty.UseConsolidatedConstraintFormat, aModel.ConsolidatedFormat);

                        // Categories
                        for (var i = 0; i < aModel.Categories.Count; i++)
                        {
                            aModel.Categories[i] = aModel.Categories[i].Replace(",", "");
                        }

                        settings.SaveSetting(IGSettingsManager.SettingProperty.Categories, string.Join(",", aModel.Categories));

                        scope.Commit();

                        return ig.Id;
                    }
                    catch (Exception ex)
                    {
                        scope.Rollback();
                        throw ex;
                    }
                }
            }
        }

        private List<int> GetPreviousImplementationGuides(ImplementationGuide ig)
        {
            List<int> ret = new List<int>();

            if (ig.PreviousVersionImplementationGuideId != null)
            {
                ImplementationGuide next = this.tdb.ImplementationGuides.Single(y => y.Id == ig.PreviousVersionImplementationGuideId);
                ret.Add(next.Id);

                while (next.PreviousVersionImplementationGuideId != null)
                {
                    next = this.tdb.ImplementationGuides.Single(y => y.Id == next.PreviousVersionImplementationGuideId);
                    ret.Add(next.Id);
                }
            }

            return ret;
        }

        private void SaveTemplateTypes(IObjectRepository auditedTdb, ImplementationGuide ig, EditModel model)
        {
            foreach (EditModel.TemplateTypeItem cModelTemplateType in model.TemplateTypes)
            {
                TemplateType cTemplateType = auditedTdb.TemplateTypes.Single(y => y.Name == cModelTemplateType.DefaultName && y.ImplementationGuideTypeId == model.TypeId.Value);
                ImplementationGuideTemplateType foundTemplateType = ig.TemplateTypes.SingleOrDefault(y => y.TemplateType.Name == cModelTemplateType.DefaultName);

                if (foundTemplateType == null)
                {
                    foundTemplateType = new ImplementationGuideTemplateType()
                    {
                        TemplateType = cTemplateType
                    };
                    ig.TemplateTypes.Add(foundTemplateType);
                }

                foundTemplateType.Name = cModelTemplateType.Name;
                foundTemplateType.DetailsText = cModelTemplateType.Description;
            }
        }

        private void SaveCustomSchematron(IObjectRepository auditedTdb, ImplementationGuide ig, EditModel model)
        {
            foreach (int cCustomSchematronPatternId in model.DeletedCustomSchematrons)
            {
                ImplementationGuideSchematronPattern foundSchematronPattern = ig.SchematronPatterns.Single(y => y.Id == cCustomSchematronPatternId);
                auditedTdb.ImplementationGuideSchematronPatterns.Remove(foundSchematronPattern);
            }

            foreach (EditModel.CustomSchematronItem cCustomSchematron in model.CustomSchematrons)
            {
                ImplementationGuideSchematronPattern foundCustomSchematron = null;

                if (cCustomSchematron.Id == 0)
                {
                    foundCustomSchematron = new ImplementationGuideSchematronPattern();
                    ig.SchematronPatterns.Add(foundCustomSchematron);
                }
                else
                {
                    foundCustomSchematron = ig.SchematronPatterns.Single(y => y.Id == cCustomSchematron.Id);
                }

                foundCustomSchematron.Phase = cCustomSchematron.Phase;
                foundCustomSchematron.PatternId = cCustomSchematron.PatternId;
                foundCustomSchematron.PatternContent = cCustomSchematron.PatternContent;
            }
        }

        private void SendPermissionNotificationEmail(string recipientEmail, string firstName, string lastName, string userName, string userEmail, string permission, string igName, string viewUrl, bool approved)
        {
            string toEmailAddress = !string.IsNullOrEmpty(AppSettings.DebugMailTo) ? AppSettings.DebugMailTo : recipientEmail;

            MailMessage mailMessage = new MailMessage(AppSettings.MailFromAddress, toEmailAddress);

            if (approved)
            {
                mailMessage.Subject = string.Format("Trifolia access granted to " + igName);
                mailMessage.Body = string.Format("Hello {0} {1},\n\nYour user account {2} ({3}) has been granted {4} access to the \"{5}\" implementation guide.\n\nYou can {6} the implementation guide here: {7}\n\n-Trifolia",
                    firstName,
                    lastName,
                    userName,
                    userEmail,
                    permission,
                    igName,
                    permission == "Edit" ? "view/edit" : "view",
                    viewUrl);
            }
            else
            {
                mailMessage.Subject = string.Format("Trifolia access denied to " + igName);
                mailMessage.Body = string.Format("Hello {0} {1},\n\nYour request to access {5} with {6} permissions has been denied by the access manager of the implementation guide.\n\n-Trifolia",
                    firstName,  // 0
                    lastName,   // 1
                    userName,   // 2
                    userEmail,  // 3
                    permission, // 4
                    igName,     // 5
                    permission == "Edit" ? "view/edit" : "view",        // 6
                    viewUrl);   // 7
            }

            try
            {
                SmtpClient client = new SmtpClient(AppSettings.MailHost, AppSettings.MailPort);
                client.EnableSsl = AppSettings.MailEnableSSL;
                client.Credentials = new System.Net.NetworkCredential(AppSettings.MailUser, AppSettings.MailPassword);
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Error sending permissions notification.", ex);
            }
        }

        private void SavePermissions(IObjectRepository auditedTdb, ImplementationGuide ig, EditModel model)
        {
            // Remove view permissions that are not needed any longer
            var removePermissions = (from igp in ig.Permissions
                                     where
                                       (igp.Permission == "View" && !model.ViewPermissions.Exists(y => y.Type == igp.MemberType() && y.Id == igp.PrimaryId())) ||
                                       (igp.Permission == "Edit" && !model.EditPermissions.Exists(y => y.Type == igp.MemberType() && y.Id == igp.PrimaryId()))
                                     select igp).ToList();

            foreach (var cRemovePermission in removePermissions)
            {
                auditedTdb.ImplementationGuidePermissions.Remove(cRemovePermission);
            }

            var addedViewPermissions = new List<EditModel.Permission>();
            var addedEditPermissions = new List<EditModel.Permission>();

            foreach (EditModel.Permission cPermission in model.ViewPermissions)
            {
                if (ig.Permissions.Count(y => y.Permission == "View" && y.Type == cPermission.Type.ToString() && y.PrimaryId() == cPermission.Id) > 0)
                    continue;

                ig.Permissions.Add(BuildPermission(ig, "View", cPermission));

                if (cPermission.Type != PermissionTypes.Everyone)
                    addedViewPermissions.Add(cPermission);
            }

            foreach (EditModel.Permission cPermission in model.EditPermissions)
            {
                if (ig.Permissions.Count(y => y.Permission == "Edit" && y.Type == cPermission.Type.ToString() && y.PrimaryId() == cPermission.Id) > 0)
                    continue;

                ig.Permissions.Add(BuildPermission(ig, "Edit", cPermission));

                if (cPermission.Type != PermissionTypes.Everyone)
                {
                    var foundAddedPermission = addedViewPermissions.SingleOrDefault(y => y.Type == cPermission.Type && y.Id == cPermission.Id);

                    // If a permission has already been added for this group or user, change the permission email to reflect "Edit"
                    if (foundAddedPermission != null)
                        addedViewPermissions.Remove(foundAddedPermission);

                    addedEditPermissions.Add(cPermission);
                }
            }

            // Notify (by email) users of their new permissions
            if (model.NotifyNewPermissions)
            {
                // This union flattens all groups into just a list of users and what their permission is (view vs. edit)
                var userEmails = (from a in addedViewPermissions
                                  join b in this.tdb.Groups on a.Id equals b.Id
                                  join c in this.tdb.UserGroups on b.Id equals c.GroupId
                                  join d in this.tdb.Users on c.UserId equals d.Id
                                  where a.Type == PermissionTypes.Group
                                  select new
                                  {
                                      Permission = "View",
                                      User = d
                                  })
                                  .Union(from a in addedViewPermissions
                                         join b in this.tdb.Users on a.Id equals b.Id
                                         where a.Type == PermissionTypes.User
                                         select new
                                         {
                                             Permission = "View",
                                             User = b
                                         })
                                         .Union(from a in addedEditPermissions
                                                join b in this.tdb.Groups on a.Id equals b.Id
                                                join c in this.tdb.UserGroups on b.Id equals c.GroupId
                                                join d in this.tdb.Users on c.UserId equals d.Id
                                                where a.Type == PermissionTypes.Group
                                                select new
                                                {
                                                    Permission = "Edit",
                                                    User = d
                                                })
                                    .Union(from a in addedEditPermissions
                                           join b in this.tdb.Users on a.Id equals b.Id
                                           where a.Type == PermissionTypes.User
                                           select new
                                           {
                                               Permission = "Edit",
                                               User = b
                                           });

                // Send an email to each user (separately) notifying them of their permission
                foreach (var userEmail in userEmails)
                {
                    string recipientEmail = userEmail.User.Email;

                    this.SendPermissionNotificationEmail(
                        recipientEmail,
                        userEmail.User.FirstName,
                        userEmail.User.LastName,
                        userEmail.User.UserName,
                        userEmail.User.Email,
                        userEmail.Permission,
                        ig.GetDisplayName(),
                        ig.GetViewUrl(true),
                        true);
                }
            }
        }

        private ImplementationGuidePermission BuildPermission(ImplementationGuide ig, string viewOrEdit, EditModel.Permission permissionModel)
        {
            ImplementationGuidePermission permission = new ImplementationGuidePermission()
            {
                Permission = viewOrEdit,
                Type = permissionModel.Type.ToString(),
                ImplementationGuide = ig
            };

            if (permissionModel.Type == Models.PermissionManagement.PermissionTypes.Group)
                permission.GroupId = permissionModel.Id;
            else if (permissionModel.Type == Models.PermissionManagement.PermissionTypes.User)
                permission.UserId = permissionModel.Id;

            return permission;
        }

        [HttpGet, Route("api/ImplementationGuide/Edit/Permission/Search"), SecurableAction()]
        public IEnumerable<PermissionsInfoModel.MemberEntry> SearchUsers(string searchText, bool includeGroups)
        {
            if (!string.IsNullOrEmpty(searchText))
                searchText = searchText.ToLower();

            List<PermissionsInfoModel.MemberEntry> matches = new List<PermissionsInfoModel.MemberEntry>();

            if (!string.IsNullOrEmpty(searchText))
            {
                matches.AddRange(
                    from u in this.tdb.Users
                    where
                        (u.FirstName + " " + u.LastName).ToLower().Contains(searchText) ||
                        u.Email.Contains(searchText) ||
                        u.UserName.ToLower().Contains(searchText)
                    select new PermissionsInfoModel.MemberEntry()
                    {
                        Type = PermissionTypes.User,
                        Id = u.Id,
                        Name = u.FirstName + " " + u.LastName
                    });
            }

            // Only add groups if allowGroupSelection and the requested organization is not "my organization"
            if (includeGroups)
            {
                matches.Insert(0, new PermissionsInfoModel.MemberEntry()
                {
                    Id = 0,
                    Name = string.Format("Everyone"),
                    Type = PermissionTypes.Everyone
                });

                if (!string.IsNullOrEmpty(searchText))
                {
                    // TODO: Add other groups from the organization
                    matches.AddRange(
                        from g in this.tdb.Groups
                        where g.Name.ToLower().Contains(searchText)
                        select new PermissionsInfoModel.MemberEntry()
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Type = PermissionTypes.Group
                        });
                }
            }

            return matches.OrderBy(y => y.Type).ThenBy(y => y.Name);
        }

        [HttpGet, Route("api/ImplementationGuide/Edit/TemplateType"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public IEnumerable<EditModel.TemplateTypeItem> GetTypeTemplateTypes(int implementationGuideTypeId)
        {
            List<EditModel.TemplateTypeItem> templateTypes = new List<EditModel.TemplateTypeItem>();
            ImplementationGuideType igType = this.tdb.ImplementationGuideTypes.Single(y => y.Id == implementationGuideTypeId);

            foreach (TemplateType cTemplateType in igType.TemplateTypes)
            {
                EditModel.TemplateTypeItem newTemplateTypeItem = new EditModel.TemplateTypeItem()
                {
                    DefaultName = cTemplateType.Name,
                    Name = cTemplateType.Name,
                    Description = string.Empty
                };

                templateTypes.Add(newTemplateTypeItem);
            }

            return templateTypes;
        }

        #endregion

        #region Bookmarks

        [HttpPost, Route("api/ImplementationGuide/Edit/Bookmark"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS)]
        public void UpdateBookmarks(EditBookmarksModel model)
        {
            using (IObjectRepository tdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                foreach (var currentTemplateModel in model.TemplateItems)
                {
                    Template template = tdb.Templates.Single(y => y.Id == currentTemplateModel.Id);

                    if (template.Name != currentTemplateModel.Name)
                        template.Name = currentTemplateModel.Name;

                    if (template.Bookmark != currentTemplateModel.Bookmark)
                        template.Bookmark = currentTemplateModel.Bookmark;
                }

                tdb.SaveChanges();
            }
        }

        [HttpGet, Route("api/ImplementationGuide/{implementationGuideId}/Edit/Bookmark"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS)]
        public EditBookmarksModel GetBookmarks(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permission to edit this implementation guide!");

            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            EditBookmarksModel model = new EditBookmarksModel()
            {
                ImplementationGuideId = implementationGuideId,
                Name = implementationGuide.NameWithVersion
            };

            var templates = (from t in this.tdb.Templates
                             where t.OwningImplementationGuideId == implementationGuideId
                             select new EditBookmarksModel.TemplateItem()
                             {
                                 Id = t.Id,
                                 Name = t.Name,
                                 Bookmark = t.Bookmark
                             });

            model.TemplateItems = templates.ToList();

            return model;
        }

        [HttpPost, Route("api/ImplementationGuide/{implementationGuideId}/Edit/Bookmark/Regenerate"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS)]
        public void RegenerateBookmarks(int implementationGuideId)
        {
            var templates = this.tdb.Templates.Where(y => y.OwningImplementationGuideId == implementationGuideId);

            foreach (Template cTemplate in templates)
            {
                string abbreviation = string.Empty;

                foreach (BookmarkTemplateTypeElement abbreviationConfig in BookmarkSection.GetSection().TemplateTypes)
                {
                    if (abbreviationConfig.TemplateTypeName.ToLower() == cTemplate.TemplateType.Name.ToLower())
                    {
                        abbreviation = abbreviationConfig.BookmarkAbbreviation;
                        break;
                    }
                }

                cTemplate.Bookmark = Template.GenerateBookmark(cTemplate.Name, abbreviation);
            }

            this.tdb.SaveChanges();
        }

        #endregion

        #region Web-Based View

        [HttpGet, Route("api/ImplementationGuide/ViewData/{implementationGuideId}"), SecurableAction()]
        public ViewDataModel GetViewData(int implementationGuideId, int? fileId, [FromUri] int[] templateIds, bool inferred)
        {
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            ViewDataModel model = null;
            SimpleSchema schema = ig.ImplementationGuideType.GetSimpleSchema();

            if (fileId != null)
            {
                var file = ig.Files.Single(y => y.Id == fileId);
                var fileData = file.GetLatestData();

                if (file.ContentType != "DataSnapshot")
                    throw new Exception("File specified is not a data snapshot!");

                var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                string fileDataContent = System.Text.Encoding.UTF8.GetString(fileData.Data);

                model = jsonSerializer.Deserialize<ViewDataModel>(fileDataContent);
                model.ImplementationGuideFileId = fileId;
            }
            else
            {
                WIKIParser wikiParser = new WIKIParser(this.tdb);
                IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
                var igTypePlugin = IGTypePluginFactory.GetPlugin(ig.ImplementationGuideType);
                var firstTemplateType = ig.ImplementationGuideType.TemplateTypes.OrderBy(y => y.OutputOrder).FirstOrDefault();

                model = new ViewDataModel()
                {
                    ImplementationGuideId = implementationGuideId,
                    ImplementationGuideName = !string.IsNullOrEmpty(ig.DisplayName) ? ig.DisplayName : ig.NameWithVersion,
                    ImplementationGuideFileId = fileId,
                    Status = ig.PublishStatus.Status,
                    PublishDate = ig.PublishDate != null ? ig.PublishDate.Value.ToShortDateString() : null,
                    Volume1Html = igSettings.GetSetting(IGSettingsManager.SettingProperty.Volume1Html)
                };

                model.ImplementationGuideDescription = ig.WebDescription;
                model.ImplementationGuideDisplayName = !string.IsNullOrEmpty(ig.WebDisplayName) ? ig.WebDisplayName : model.ImplementationGuideName;

                // Create the section models
                model.Volume1Sections = (from igs in ig.Sections.OrderBy(y => y.Order)
                                         select new ViewDataModel.Section()
                                         {
                                             Heading = igs.Heading,
                                             Content = igs.Content,
                                             Level = igs.Level
                                         }).ToList();

                // Include any FHIR Resource Instance attachments with the IG
                foreach (var fhirResourceInstanceFile in ig.Files.Where(y => y.ContentType == "FHIRResourceInstance"))
                {
                    var fileData = fhirResourceInstanceFile.GetLatestData();
                    string fileContent = System.Text.Encoding.UTF8.GetString(fileData.Data);

                    ViewDataModel.FHIRResource newFhirResource = new ViewDataModel.FHIRResource()
                    {
                        Name = fhirResourceInstanceFile.FileName,
                        JsonContent = igTypePlugin.GetFHIRResourceInstanceJson(fileContent),
                        XmlContent = igTypePlugin.GetFHIRResourceInstanceXml(fileContent)
                    };

                    if (!string.IsNullOrEmpty(newFhirResource.JsonContent) || !string.IsNullOrEmpty(newFhirResource.XmlContent))
                        model.FHIRResources.Add(newFhirResource);
                }

                // Create the value set models
                var valueSets = ig.GetValueSets(this.tdb);
                
                foreach (var valueSet in valueSets)
                {
                    var newValueSetModel = new ViewDataModel.ValueSet()
                    {
                        Identifier = valueSet.ValueSet.GetIdentifier(igTypePlugin),
                        Name = valueSet.ValueSet.Name,
                        Source = valueSet.ValueSet.Source,
                        Description = valueSet.ValueSet.Description,
                        BindingDate = valueSet.BindingDate != null ? valueSet.BindingDate.Value.ToShortDateString() : null
                    };
                    
                    var members = valueSet.ValueSet.GetActiveMembers(valueSet.BindingDate);
                    newValueSetModel.Members = (from vsm in members
                                                select new ViewDataModel.ValueSetMember()
                                                {
                                                    Code = vsm.Code,
                                                    DisplayName = vsm.DisplayName,
                                                    CodeSystemIdentifier = vsm.CodeSystem.Oid,
                                                    CodeSystemName = vsm.CodeSystem.Name
                                                }).ToList();

                    model.ValueSets.Add(newValueSetModel);

                    // Add code systems used by this value set to the IG
                    var codeSystems = (from vsm in members
                                       select new ViewDataModel.CodeSystem()
                                       {
                                           Identifier = vsm.CodeSystem.Oid,
                                           Name = vsm.CodeSystem.Name
                                       });
                    model.CodeSystems.AddRange(codeSystems);
                }

                // Create the template models
                var templates = ig.GetRecursiveTemplates(this.tdb, templateIds != null ? templateIds.ToList() : null, inferred);
                var constraints = (from t in templates
                                   join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                                   select tc).AsEnumerable();

                foreach (var template in templates)
                {
                    var templateSchema = schema.GetSchemaFromContext(template.PrimaryContextType);
                    var newTemplateModel = new ViewDataModel.Template()
                    {
                        Identifier = template.Oid,
                        Bookmark = template.Bookmark,
                        ContextType = template.PrimaryContextType,
                        Context = template.PrimaryContext,
                        Name = template.Name,
                        ImpliedTemplate = template.ImpliedTemplate != null ? new ViewDataModel.TemplateReference(template.ImpliedTemplate) : null,
                        Description = wikiParser.ParseAsHtml(template.Description),
                        Extensibility = template.IsOpen ? "Open" : "Closed",
                        TemplateTypeId = template.TemplateTypeId
                    };

                    // Load Template Changes
                    var lPreviousVersion = template.PreviousVersion;

                    if (lPreviousVersion != null)
                    {
                        var comparer = VersionComparer.CreateComparer(this.tdb, igTypePlugin, igSettings);
                        var result = comparer.Compare(lPreviousVersion, template);

                        newTemplateModel.Changes = new Trifolia.Web.Models.TemplateManagement.DifferenceModel(result)
                        {
                            Id = template.Id,
                            TemplateName = template.Name,
                            PreviousTemplateName = string.Format("{0} ({1})", lPreviousVersion.Name, lPreviousVersion.Oid),
                            PreviousTemplateId = lPreviousVersion.Id
                        };
                    }

                    // Code systems used in this template to the IG
                    var codeSystems = (from tc in template.ChildConstraints
                                       where tc.CodeSystem != null
                                       select new ViewDataModel.CodeSystem()
                                       {
                                           Identifier = tc.CodeSystem.Oid,
                                           Name = tc.CodeSystem.Name
                                       });
                    model.CodeSystems.AddRange(codeSystems);

                    // Samples
                    newTemplateModel.Samples = (from ts in template.TemplateSamples
                                                select new ViewDataModel.Sample()
                                                {
                                                    Id = ts.Id,
                                                    Name = ts.Name,
                                                    SampleText = ts.XmlSample
                                                }).ToList();

                    // Contained templates
                    var containedTemplates = (from tc in template.ChildConstraints
                                              join t in templates on tc.ContainedTemplateId equals t.Id
                                              select t).Distinct();
                    newTemplateModel.ContainedTemplates = containedTemplates.Select(y => new ViewDataModel.TemplateReference(y)).ToList();

                    // Implying templates
                    var implyingTemplates = (from t in templates
                                             where t.ImpliedTemplateId == template.Id
                                             select t).Distinct().AsEnumerable();
                    newTemplateModel.ImplyingTemplates = implyingTemplates.Select(y => new ViewDataModel.TemplateReference(y)).ToList();

                    // Contained by templates
                    var containedByTemplates = (from tc in template.ContainingConstraints
                                                join t in templates on tc.TemplateId equals t.Id
                                                select t).Distinct().AsEnumerable();
                    newTemplateModel.ContainedByTemplates = containedByTemplates.Select(y => new ViewDataModel.TemplateReference(y)).ToList();

                    model.Templates.Add(newTemplateModel);



                    // Create the constraint models (hierarchically)
                    var parentConstraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null);
                    CreateConstraints(wikiParser, igSettings, igTypePlugin, parentConstraints, newTemplateModel.Constraints, templateSchema);
                }

                // Create models for template types in the IG
                model.TemplateTypes = (from igt in igSettings.TemplateTypes
                                       join tt in this.tdb.TemplateTypes on igt.TemplateTypeId equals tt.Id
                                       where model.Templates.Exists(y => y.TemplateTypeId == tt.Id)
                                       select new ViewDataModel.TemplateType()
                                       {
                                           TemplateTypeId = tt.Id,
                                           Name = igt.Name,
                                           ContextType = tt.RootContextType,
                                           Description = igt.DetailsText
                                       }).ToList();

                model.CodeSystems = model.CodeSystems.Distinct().ToList();
            }

            return model;
        }

        private void CreateConstraints(WIKIParser wikiParser, IGSettingsManager igManager, IIGTypePlugin igTypePlugin, IEnumerable<TemplateConstraint> constraints, List<ViewDataModel.Constraint> parentList, SimpleSchema templateSchema, SimpleSchema.SchemaObject schemaObject = null)
        {
            foreach (var constraint in constraints.OrderBy(y => y.Order))
            {
                var theConstraint = constraint;

                // TODO: Possible bug? Should schemaObject always be re-set? Remove schemaObject == null from if()
                if (templateSchema != null && schemaObject == null)
                    schemaObject = templateSchema.Children.SingleOrDefault(y => y.Name == constraint.Context);

                IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, igManager, igTypePlugin, theConstraint, "#/volume2/", "#/valuesets/#", true, true, true, false);

                var newConstraintModel = new ViewDataModel.Constraint()
                {
                    Number = string.Format("{0}-{1}", theConstraint.Template.OwningImplementationGuideId, theConstraint.Number),
                    Narrative = fc.GetHtml(wikiParser, string.Empty, 1, true),
                    Conformance = theConstraint.Conformance,
                    Cardinality = theConstraint.Cardinality,
                    Context = theConstraint.Context,
                    DataType = theConstraint.DataType,
                    Value = theConstraint.Value,
                    ValueSetIdentifier = theConstraint.ValueSet != null ? theConstraint.ValueSet.GetIdentifier(igTypePlugin) : null,
                    ValueSetDate = theConstraint.ValueSetDate != null ? theConstraint.ValueSetDate.Value.ToShortDateString() : null,
                    ContainedTemplate = theConstraint.ContainedTemplateId != null ? new ViewDataModel.TemplateReference(theConstraint.ContainedTemplate) : null,
                    IsChoice = theConstraint.IsChoice
                };

                var isFhir = constraint.Template.ImplementationGuideType.SchemaURI == ImplementationGuideType.FHIR_NS;

                // Check if we're dealing with a FHIR constraint
                if (isFhir && schemaObject != null)
                    newConstraintModel.DataType = schemaObject.DataType;

                parentList.Add(newConstraintModel);

                var nextSchemaObject = schemaObject != null ?
                    schemaObject.Children.SingleOrDefault(y => y.Name == constraint.Context) :
                    null;

                // Recursively add child constraints
                CreateConstraints(wikiParser, igManager, igTypePlugin, theConstraint.ChildConstraints, newConstraintModel.Constraints, null, nextSchemaObject);
            }
        }

        #endregion

        #region Access Requests

        [HttpGet, Route("api/ImplementationGuide/RequestAuthorization/My")]
        public MyAccessRequestsModel GetAccessRequests()
        {
            User currentUser = CheckPoint.Instance.GetUser(this.tdb);

            if (currentUser == null)
                return new MyAccessRequestsModel();

            var currentUserApprovals = (from igar in this.tdb.ImplementationGuideAccessRequests
                                        join ig in this.tdb.ImplementationGuides on igar.ImplementationGuideId equals ig.Id
                                        where ig.AccessManagerId == currentUser.Id
                                        select igar)
                                        .ToList();
            var currentUserRequests = this.tdb.ImplementationGuideAccessRequests.Where(y => y.RequestUserId == currentUser.Id).ToList();

            var retModel = new MyAccessRequestsModel()
            {
                MyApprovals = currentUserApprovals.Select(y => new MyAccessRequestsModel.Request(y)).ToList(),
                MyRequests = currentUserRequests.Select(y => new MyAccessRequestsModel.Request(y)).ToList()
            };

            return retModel;
        }

        [HttpPost, Route("api/ImplementationGuide/{implementationGuideId}/RequestAuthorization"), SecurableAction(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public void RequestAuthorization(int implementationGuideId, bool edit, string message)
        {
            User currentUser = CheckPoint.Instance.GetUser(this.tdb);
            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            User accessManager = ig.AccessManager;

            if (accessManager == null)
                throw new ArgumentException("Specified IG does not have an access manager defined");

            var foundAccessRequest = this.tdb.ImplementationGuideAccessRequests.FirstOrDefault(y => y.ImplementationGuideId == ig.Id && y.RequestUserId == currentUser.Id);

            if (foundAccessRequest != null)
            {
                // Cancel the access request if it was already requested within the last 6 hours.
                if (foundAccessRequest.RequestDate > DateTime.Now.AddHours(-6))
                    return;

                // Update the access request's information with the latest request
                foundAccessRequest.RequestDate = DateTime.Now;
                foundAccessRequest.Permission = edit ? "Edit" : "View";

                if (!string.IsNullOrEmpty(message))
                    foundAccessRequest.Message = message;
            }
            else
            {
                foundAccessRequest = new ImplementationGuideAccessRequest();
                foundAccessRequest.ImplementationGuide = ig;
                foundAccessRequest.RequestUser = currentUser;
                foundAccessRequest.Message = message;
                foundAccessRequest.Permission = edit ? "Edit" : "View";
                foundAccessRequest.RequestDate = DateTime.Now;
                this.tdb.ImplementationGuideAccessRequests.Add(foundAccessRequest);
            }

            this.tdb.SaveChanges();

            try
            {
                string msgFormat = @"
User {0} {1} ({2}) has requested access to {3} on {4} @ {5}.
The user has requested {6} permissions.

Message from user: {7}

Use this link to grant them permission to the implementation guide: {8}
Use this link to deny them permission to the implementation guide: {9}
";

                string accessRequestLink = this.Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/IGManagement/AuthorizationRequest/" + foundAccessRequest.Id.ToString() + "/";
                string mailTo = !string.IsNullOrEmpty(AppSettings.DebugMailTo) ? AppSettings.DebugMailTo : accessManager.Email;
                MailMessage mailMessage = new MailMessage(AppSettings.MailFromAddress, mailTo);
                mailMessage.Subject = string.Format("Trifolia: Request to access " + ig.NameWithVersion);
                mailMessage.Body = string.Format(msgFormat,
                    currentUser.FirstName,
                    currentUser.LastName,
                    currentUser.Email,
                    ig.GetDisplayName(),
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    edit ? "edit" : "view",
                    !string.IsNullOrEmpty(message) ? message : "None",
                    accessRequestLink + "$approve",
                    accessRequestLink + "$deny");

                if (!string.IsNullOrEmpty(AppSettings.DebugMailTo))
                    mailMessage.Subject = "DEBUG: " + mailMessage.Subject;

                SmtpClient client = new SmtpClient(AppSettings.MailHost, AppSettings.MailPort);
                client.EnableSsl = AppSettings.MailEnableSSL;
                client.Credentials = new System.Net.NetworkCredential(AppSettings.MailUser, AppSettings.MailPassword);
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Log.For(this).Error("Error sending email notification to access manager", ex);
                throw new Exception("Error sending email notification to access manager");
            }
        }

        [HttpPost, Route("api/ImplementationGuide/RequestAuthorization/{accessRequestId}/$complete")]
        public void CompleteAccessRequest(int accessRequestId, bool approved)
        {
            User currentUser = CheckPoint.Instance.GetUser(this.tdb);
            ImplementationGuideAccessRequest igac = this.tdb.ImplementationGuideAccessRequests.Single(y => y.Id == accessRequestId);
            var ig = igac.ImplementationGuide;
            User requestUser = igac.RequestUser;

            if (ig.AccessManager != currentUser)
                throw new UnauthorizedAccessException("You are not the access manager of the implementation guide.");

            if (approved)
            {
                if (igac.Permission == ImplementationGuidePermission.PermissionView || igac.Permission == ImplementationGuidePermission.PermissionEdit)
                {
                    if (!ig.Permissions.Any(y => y.UserId == requestUser.Id && y.Permission == ImplementationGuidePermission.PermissionView))
                    {
                        ImplementationGuidePermission igp = new ImplementationGuidePermission()
                        {
                            ImplementationGuide = ig,
                            User = requestUser,
                            Permission = ImplementationGuidePermission.PermissionView,
                            Type = ImplementationGuidePermission.TypeUser
                        };
                        this.tdb.ImplementationGuidePermissions.Add(igp);
                    }
                }

                if (igac.Permission == ImplementationGuidePermission.PermissionEdit)
                {
                    if (!ig.Permissions.Any(y => y.UserId == requestUser.Id && y.Permission == ImplementationGuidePermission.PermissionEdit))
                    {
                        ImplementationGuidePermission igp = new ImplementationGuidePermission()
                        {
                            ImplementationGuide = ig,
                            User = requestUser,
                            Permission = ImplementationGuidePermission.PermissionEdit,
                            Type = ImplementationGuidePermission.TypeUser
                        };
                        this.tdb.ImplementationGuidePermissions.Add(igp);
                    }
                }
            }

            this.tdb.ImplementationGuideAccessRequests.Remove(igac);
            this.tdb.SaveChanges();

            this.SendPermissionNotificationEmail(
                requestUser.Email, 
                requestUser.FirstName, 
                requestUser.LastName, 
                requestUser.UserName, 
                requestUser.Email, 
                igac.Permission, 
                ig.GetDisplayName(), 
                ig.GetViewUrl(true),
                approved);
        }

        #endregion

        private string GetUrlForImplementationGuide(IGListModes viewMode, int implementationGuideId)
        {
            switch (viewMode)
            {
                case IGListModes.ExportSchematron:
                    return "/Export/Schematron/" + implementationGuideId;
                case IGListModes.ExportVocab:
                    return "/Export/Vocabulary/" + implementationGuideId;
                case IGListModes.ExportMSWord:
                    return "/Export/MSWord/" + implementationGuideId;
                case IGListModes.ExportGreen:
                    return "/Export/Green/" + implementationGuideId;
                case IGListModes.ExportXML:
                    return "/Export/Xml/" + implementationGuideId;
                default:
                    return "/IGManagement/View/" + implementationGuideId;
            }
        }
    }
}
