using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Trifolia.Authorization;
using Trifolia.DB;
using Trifolia.Export.Versioning;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;
using Trifolia.Web.Models.IGManagement;
using Trifolia.Web.Models.TemplateManagement;

namespace Trifolia.Web.Controllers
{
    public class IGManagementController : Controller
    {
        public enum ViewModes
        {
            Default,
            Files,
            Test,
            ExportMSWord,
            ExportXML,
            ExportVocab,
            ExportSchematron,
            ExportGreen
        }

        private IObjectRepository tdb;

        #region Construct/Dispose

        public IGManagementController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public IGManagementController()
        {
            this.tdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Edit Actions

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public ActionResult Edit(int? implementationGuideId)
        {
            return View("Edit", implementationGuideId);
        }

        #endregion

        #region View Changes Actions

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public ActionResult ViewIgChanges(int id)
        {
            if (!CheckPoint.Instance.GrantViewImplementationGuide(id))
                throw new AuthorizationException("You do not have access to view this implementation guide's changes.");

            ImplementationGuide lGuide = this.tdb.ImplementationGuides.Single(ig => ig.Id == id);
            ViewChangesModel lModel = new ViewChangesModel();
            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, lGuide.Id);
            IIGTypePlugin igTypePlugin = lGuide.ImplementationGuideType.GetPlugin();
            VersionComparer lComparer = VersionComparer.CreateComparer(tdb, igTypePlugin, igSettings);

            lModel.IgName = lGuide.Name;

            // Modified templates
            foreach (Template lNewTemplate in lGuide.ChildTemplates.Where(t => t.PreviousVersionTemplateId.HasValue))
            {
                Template lPreviousVersion = this.tdb.Templates.Single(t => t.Id == lNewTemplate.PreviousVersionTemplateId.Value);
                ComparisonResult lResult = lComparer.Compare(lPreviousVersion, lNewTemplate);

                lModel.TemplateDifferences.Add(
                    new DifferenceModel(lResult)
                    {
                        Id = lNewTemplate.Id,
                        IsAdded = false,
                        TemplateName = lNewTemplate.Name,
                        PreviousTemplateName = string.Format("{0} ({1})", lPreviousVersion.Name, lPreviousVersion.Oid),
                        PreviousTemplateId = lPreviousVersion.Id,
                        Difference = lResult,
                        InlineConstraints = GetInlineConstraintChanges(lResult)
                    });
            }

            // Added templates
            foreach (Template lNewTemplate in lGuide.ChildTemplates.Where(t => !t.PreviousVersionTemplateId.HasValue))
            {
                ComparisonResult lResult = lComparer.Compare(new Template(), lNewTemplate);

                lModel.TemplateDifferences.Add(
                    new DifferenceModel(lResult)
                    {
                        Id = lNewTemplate.Id,
                        IsAdded = true,
                        TemplateName = lNewTemplate.Name
                    });
            }

            return View(lModel);
        }

        private List<DifferenceModel.Constraint> GetInlineConstraintChanges(ComparisonResult compareResult)
        {
            List<DifferenceModel.Constraint> inlineConstraints = new List<DifferenceModel.Constraint>();

            // Get top-level constraints from the template
            foreach (var cConstraint in compareResult.ChangedConstraints.Where(y => y.ParentNumber == null).OrderBy(y => y.Order))
            {
                inlineConstraints.Add(GetInlineConstraintChanges(compareResult, cConstraint));
            }

            return inlineConstraints;
        }

        private DifferenceModel.Constraint GetInlineConstraintChanges(ComparisonResult compareResult, ComparisonConstraintResult constraint)
        {
            DifferenceModel.Constraint inlineConstraint = new DifferenceModel.Constraint()
            {
                Number = constraint.Number,
                ChangeType = constraint.Type
            };

            if (constraint.Type == CompareStatuses.Added || constraint.Type == CompareStatuses.Modified || constraint.Type == CompareStatuses.Unchanged)
                inlineConstraint.Narrative = constraint.NewNarrative;
            else if (constraint.Type == CompareStatuses.Removed)
                inlineConstraint.Narrative = constraint.OldNarrative;

            foreach (var cConstraint in compareResult.ChangedConstraints.Where(y => y.ParentNumber == constraint.Number).OrderBy(y => y.Order))
            {
                inlineConstraint.Constraints.Add(
                    GetInlineConstraintChanges(compareResult, cConstraint));
            }

            return inlineConstraint;
        }

        #endregion

        #region View Actions

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public ActionResult View(int implementationGuideId)
        {
            return View("View", implementationGuideId);
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public ActionResult DataTypes(int id)
        {
            var typeTitle = tdb.ImplementationGuideTypes.Where(i => i.Id == id).Select(i => i.Name).Single();

            var v = new ViewDataDictionary();
            v.Add("id", id);
            v.Add("title", typeTitle);

            return View("DataTypes", v);
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_AUDIT_TRAIL)]
        public ActionResult ViewAuditTrail(int implementationGuideId)
        {
            return PartialView("ViewAuditTrail", implementationGuideId);
        }

        #endregion

        #region Edit Bookmarks

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS)]
        public ActionResult EditBookmarks(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to edit this implementation guide!");

            return View("EditBookmarks", implementationGuideId);
        }

        #endregion

        #region New Version

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public ActionResult NewVersion(int implementationGuideId)
        {
            if (!CheckPoint.Instance.GrantEditImplementationGuide(implementationGuideId))
                throw new AuthorizationException("You do not have permissions to edit this implementation guide!");

            if (this.tdb.ImplementationGuides.Count(y => y.PreviousVersionImplementationGuideId == implementationGuideId) > 0)
                throw new Exception("This implementation guide already has a new version.");

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            if (!ig.IsPublished())
                throw new Exception("Cannot version an implementation guide that has not been published");

            ImplementationGuide newIg = new ImplementationGuide()
            {
                Name = ig.Name,
                ImplementationGuideType = ig.ImplementationGuideType,
                Organization = ig.Organization,
                PreviousVersionImplementationGuideId = ig.Id,
                PublishStatus = PublishStatus.GetDraftStatus(this.tdb),
                Version = ig.Version + 1,
                WebReadmeOverview = ig.WebReadmeOverview,
                WebDescription = ig.WebDescription,
                Identifier = ig.Identifier
            };

            // Copy permissions
            foreach (var cPermission in ig.Permissions)
            {
                ImplementationGuidePermission newPermission = new ImplementationGuidePermission()
                {
                    GroupId = cPermission.GroupId,
                    UserId = cPermission.UserId,
                    Type = cPermission.Type,
                    Permission = cPermission.Permission
                };
                newIg.Permissions.Add(newPermission);
            }

            // Copy settings
            foreach (var cSetting in ig.Settings)
            {
                ImplementationGuideSetting newSetting = new ImplementationGuideSetting()
                {
                    PropertyName = cSetting.PropertyName,
                    PropertyValue = cSetting.PropertyValue
                };
                newIg.Settings.Add(newSetting);                
            }

            // Copy IG Template Types
            foreach (var cTemplateType in ig.TemplateTypes)
            {
                ImplementationGuideTemplateType newTemplateType = new ImplementationGuideTemplateType()
                {
                    TemplateTypeId = cTemplateType.TemplateTypeId,
                    Name = cTemplateType.Name,
                    DetailsText = cTemplateType.DetailsText
                };
                newIg.TemplateTypes.Add(newTemplateType);
            }

            // Copy volume 1
            foreach (var cSection in ig.Sections)
            {
                var newSection = new ImplementationGuideSection()
                {
                    Content = cSection.Content,
                    Heading = cSection.Heading,
                    Level = cSection.Level,
                    Order = cSection.Order
                };
                newIg.Sections.Add(newSection);
            }

            // Copy custom schematron
            foreach (var cSchematron in ig.SchematronPatterns)
            {
                var newSchematron = new ImplementationGuideSchematronPattern()
                {
                    PatternId = cSchematron.PatternId,
                    PatternContent = cSchematron.PatternContent,
                    Phase = cSchematron.Phase
                };
                newIg.SchematronPatterns.Add(newSchematron);
            }

            this.tdb.ImplementationGuides.Add(newIg);
            this.tdb.SaveChanges();

            IGSettingsManager igSettings = new IGSettingsManager(this.tdb, implementationGuideId);
            IGSettingsManager newIgSettings = new IGSettingsManager(this.tdb, newIg.Id);

            var cardinalityAtLeastOne = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne);
            var cardinalityOneToOne = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne);
            var cardinalityZero = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZero);
            var cardinalityZeroOrMore = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore);
            var cardinalityZeroToOne = igSettings.GetSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne);
            var categories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);
            var useConsolidationConstraintFormat = igSettings.GetBoolSetting(IGSettingsManager.SettingProperty.UseConsolidatedConstraintFormat);
            var volume1Html = igSettings.GetSetting(IGSettingsManager.SettingProperty.Volume1Html);

            if (!string.IsNullOrEmpty(cardinalityAtLeastOne))
                newIgSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne, cardinalityAtLeastOne);

            if (!string.IsNullOrEmpty(cardinalityOneToOne))
                newIgSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZero, cardinalityZero);

            if (!string.IsNullOrEmpty(cardinalityZeroOrMore))
                newIgSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore, cardinalityZeroOrMore);

            if (!string.IsNullOrEmpty(cardinalityZeroToOne))
                newIgSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne, cardinalityZeroToOne);

            if (!string.IsNullOrEmpty(categories))
                newIgSettings.SaveSetting(IGSettingsManager.SettingProperty.Categories, categories);

            if (!string.IsNullOrEmpty(volume1Html))
                newIgSettings.SaveSetting(IGSettingsManager.SettingProperty.Volume1Html, volume1Html);

            newIgSettings.SaveBoolSetting(IGSettingsManager.SettingProperty.UseConsolidatedConstraintFormat, useConsolidationConstraintFormat);

            return Json(new { ImplementationGuideId = newIg.Id });
        }

        #endregion

        #region Access Requests

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public ActionResult ApproveAuthorizationRequest(int accessRequestId)
        {
            return this.CompleteAuthorizationRequest(accessRequestId, true);
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public ActionResult DenyAuthorizationRequest(int accessRequestId)
        {
            return this.CompleteAuthorizationRequest(accessRequestId, false);
        }

        private ActionResult CompleteAuthorizationRequest(int accessRequestId, bool approved)
        {
            ImplementationGuideAccessRequest igac = this.tdb.ImplementationGuideAccessRequests.SingleOrDefault(y => y.Id == accessRequestId);

            if (igac == null)
                return RedirectToAction("LoggedInIndex", "Home", new { message = "Authorization request no longer exists. It may have already been approved/denied." });

            User requestUser = igac.RequestUser;
            ImplementationGuide ig = igac.ImplementationGuide;

            API.ImplementationGuideController igController = new API.ImplementationGuideController(this.tdb);
            igController.CompleteAccessRequest(accessRequestId, approved);

            string msgFormat = "{0} {1} {2} access to {3}";
            string msg = string.Format(msgFormat,
                approved ? "Granted" : "Denied",
                requestUser.FirstName,
                requestUser.LastName,
                ig.GetDisplayName());

            return RedirectToAction("LoggedInIndex", "Home", new { message = msg });
        }

        #endregion

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_EDIT)]
        public ActionResult Delete(int implementationGuideId)
        {
            return View("Delete", implementationGuideId);
        }

        [Securable(SecurableNames.IMPLEMENTATIONGUIDE_LIST)]
        public ActionResult List(IGListModes listMode = IGListModes.Default)
        {
            return View("List", listMode);
        }
    }

    #region Extensions to ImplementationGuidePermission

    public static class PermissionExtension
    {
        /// <summary>
        /// Converts the "Type" property of ImplementationGuidePermission to the PermissionTypes enum used by the PermissionManagement model
        /// </summary>
        public static Trifolia.Web.Models.PermissionManagement.PermissionTypes MemberType(this ImplementationGuidePermission igp)
        {
            return ConvertType(igp.Type);
        }

        /// <summary>
        /// Returns a single Id based on the type of the permission.
        /// </summary>
        /// <returns>Returns OrganizationId when type is "EntireOrganization", GroupId when type is "Group" and "UserId" when type is "User"</returns>
        public static int PrimaryId(this ImplementationGuidePermission igp)
        {
            int? id = ConvertId(igp.MemberType(), igp.GroupId, igp.UserId);
            return id != null ? id.Value : -1;
        }

        private static int? ConvertId(Trifolia.Web.Models.PermissionManagement.PermissionTypes type, int? groupId, int? userId)
        {
            switch (type)
            {
                case Models.PermissionManagement.PermissionTypes.Everyone:
                    return 0;
                case Models.PermissionManagement.PermissionTypes.Group:
                    return groupId;
                case Models.PermissionManagement.PermissionTypes.User:
                    return userId;
            }

            return null;
        }

        private static Trifolia.Web.Models.PermissionManagement.PermissionTypes ConvertType(string type)
        {
            switch (type)
            {
                case "Everyone":
                    return Trifolia.Web.Models.PermissionManagement.PermissionTypes.Everyone;
                case "Group":
                    return Trifolia.Web.Models.PermissionManagement.PermissionTypes.Group;
                case "User":
                    return Trifolia.Web.Models.PermissionManagement.PermissionTypes.User;
            }

            return Trifolia.Web.Models.PermissionManagement.PermissionTypes.User;
        }
    }

    #endregion
}
