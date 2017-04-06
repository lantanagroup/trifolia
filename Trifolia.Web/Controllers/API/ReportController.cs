using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Web.Models.Report;
using Trifolia.Shared;

namespace Trifolia.Web.Controllers.API
{
    public class ReportController : ApiController
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public ReportController()
            : this(DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
        {
        }

        public ReportController(IObjectRepository tdb)
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

        [HttpGet, Route("api/Report/User")]
        public UserReportDetail Organizations()
        {
            UserReportDetail userReportDetail = new UserReportDetail();

            var editingUserIds = (from u in this.tdb.Users
                                  join ur in this.tdb.UserRoles on u.Id equals ur.UserId
                                  join ras in this.tdb.RoleAppSecurables on ur.RoleId equals ras.RoleId
                                  join ap in this.tdb.AppSecurables on ras.AppSecurableId equals ap.Id
                                  where ap.Name == SecurableNames.TEMPLATE_EDIT
                                  select u.Id).Distinct();

            userReportDetail.TotalAuthoredTemplates = (from u in this.tdb.Users
                                                join t in this.tdb.Templates on u.Id equals t.AuthorId
                                                select t.Id).Count();
            userReportDetail.TotalEditingUsers = editingUserIds.Count();
            userReportDetail.TotalNonEditingUsers = this.tdb.Users.Where(y => !editingUserIds.Contains(y.Id)).Count();
            userReportDetail.TotalUsers = this.tdb.Users.Count();

            foreach (User currentUser in this.tdb.Users
                .Where(y => y.ExternalOrganizationName != null && y.ExternalOrganizationName != "")
                .OrderBy(y => y.ExternalOrganizationName))
            {
                ExternalOrganizationDetail extOrgDetail = new ExternalOrganizationDetail()
                {
                    Name = currentUser.ExternalOrganizationName,
                    Type = currentUser.ExternalOrganizationType,
                    CanContact = currentUser.OkayToContact == true,
                    ContactEmail = currentUser.Email,
                    ContactName = string.Format("{0} {1}", currentUser.FirstName, currentUser.LastName),
                    ContactPhone = currentUser.Phone
                };

                extOrgDetail.CanUserEdit = (from ur in this.tdb.UserRoles
                                            join ras in this.tdb.RoleAppSecurables on ur.RoleId equals ras.RoleId
                                            join ap in this.tdb.AppSecurables on ras.AppSecurableId equals ap.Id
                                            where ur.UserId == currentUser.Id && ap.Name == SecurableNames.TEMPLATE_EDIT
                                            select ap.Id).Count() > 0;

                userReportDetail.ExternalOrganizations.Add(extOrgDetail);
            }

            userReportDetail.Users = (from u in this.tdb.Users
                               select new UserReportDetail.UserInfo()
                               {
                                   Email = u.Email,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   OkayToContact = u.OkayToContact == true,
                                   ExternalOrganizationName = u.ExternalOrganizationName,
                                   ExternalOrganizationType = u.ExternalOrganizationType,
                                   Phone = u.Phone
                               })
                               .OrderBy(y => y.LastName)
                               .ToList();

            return userReportDetail;
        }

        [HttpGet, Route("api/Report/ImplementationGuide/{implementationGuideId}/Validate")]
        public List<TemplateValidation> ValidateTemplates(int implementationGuideId)
        {
            var implementationGuide = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
            var results = new List<TemplateValidation>();
            var igSchema = implementationGuide.ImplementationGuideType.GetSimpleSchema();

            foreach (var template in implementationGuide.ChildTemplates)
            {
                var result = new TemplateValidation()
                {
                    Id = template.Id,
                    Name = template.Name,
                    Oid = template.Oid
                };

                var validationResults = template.ValidateTemplate(null, igSchema);
                result.Items.AddRange(validationResults);

                results.Add(result);
            }

            return results;
        }

        [HttpPost, Route("api/Report/Template/Review")]
        public TemplateReviewResultsModel GetTemplateReviews(TemplateReviewFilter filter)
        {
            IEnumerable<Template> templates = this.tdb.Templates;
            int userId = CheckPoint.Instance.User.Id;

            int page = filter != null ? filter.PageCount : 1;
            int count = filter != null ? filter.Count : 1;

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                templates = (from t in this.tdb.Templates
                             join vtp in this.tdb.ViewTemplatePermissions on t.Id equals vtp.TemplateId
                             where vtp.UserId == userId
                             select t).Distinct();
            }

            TemplateReviewResultsModel results = new TemplateReviewResultsModel();

            var reviewModels = (from t in templates
                                join tc in this.tdb.TemplateConstraints on t.Id equals tc.TemplateId
                                select new TemplateReviewModel()
                                {
                                    TemplateId = t.Id,
                                    TemplateOid = t.Oid,
                                    TemplateName = t.Name,
                                    ImpliedTemplateId = t.ImpliedTemplateId,
                                    ImpliedTemplateName = t.ImpliedTemplate != null ? t.ImpliedTemplate.Name : null,
                                    ImpliedTemplateOid = t.ImpliedTemplate != null ? t.ImpliedTemplate.Oid : null,
                                    ImplementationGuideId = t.OwningImplementationGuideId,
                                    ImplementationGuideName = t.OwningImplementationGuide.Name,
                                    AppliesTo = t.PrimaryContext + " (" + t.PrimaryContextType + ")",
                                    ConstraintNumber = t.OwningImplementationGuideId.ToString() + "-" + tc.Number.Value.ToString(),
                                    IsPrimitive = tc.IsPrimitive ? "Yes" : "No",
                                    HasSchematron = (!tc.IsPrimitive && (tc.Schematron == null || tc.Schematron.Length == 0)) || (tc.Schematron != null && tc.Schematron.Length != 0) ? "Yes" : "No",
                                    ValueSetName = tc.ValueSet != null ? tc.ValueSet.Name + " (" + tc.ValueSet.GetIdentifier() + ")" : null,
                                    CodeSystemName = tc.CodeSystem != null ? tc.CodeSystem.Name + " (" + tc.CodeSystem.Oid + ")" : null
                                });

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.AppliesTo))
                    reviewModels = reviewModels.Where(y => y.AppliesTo != null && y.AppliesTo.ToLower().Contains(filter.AppliesTo.ToLower()));

                if (!string.IsNullOrEmpty(filter.CodeSystemName))
                    reviewModels = reviewModels.Where(y => y.CodeSystemName != null && y.CodeSystemName.ToLower().Contains(filter.CodeSystemName.ToLower()));

                if (!string.IsNullOrEmpty(filter.ConstraintNumber))
                    reviewModels = reviewModels.Where(y => y.ConstraintNumber != null && y.ConstraintNumber.Contains(filter.ConstraintNumber));

                if (!string.IsNullOrEmpty(filter.HasSchematron))
                    reviewModels = reviewModels.Where(y => y.HasSchematron != null && y.HasSchematron.Contains(filter.HasSchematron));

                if (!string.IsNullOrEmpty(filter.ImplementationGuideName))
                    reviewModels = reviewModels.Where(y => y.ImplementationGuideName != null && y.ImplementationGuideName.ToLower().Contains(filter.ImplementationGuideName.ToLower()));

                if (!string.IsNullOrEmpty(filter.ImpliedTemplateName))
                    reviewModels = reviewModels.Where(y => y.ImpliedTemplateName != null && y.ImpliedTemplateName.ToLower().Contains(filter.ImpliedTemplateName.ToLower()));

                if (!string.IsNullOrEmpty(filter.ImpliedTemplateOid))
                    reviewModels = reviewModels.Where(y => y.ImpliedTemplateOid != null && y.ImpliedTemplateOid.ToLower().Contains(filter.ImpliedTemplateOid.ToLower()));

                if (!string.IsNullOrEmpty(filter.IsPrimitive))
                    reviewModels = reviewModels.Where(y => y.IsPrimitive != null && y.IsPrimitive.ToLower().Contains(filter.IsPrimitive.ToLower()));

                if (!string.IsNullOrEmpty(filter.TemplateName))
                    reviewModels = reviewModels.Where(y => y.TemplateName != null && y.TemplateName.ToLower().Contains(filter.TemplateName.ToLower()));

                if (!string.IsNullOrEmpty(filter.TemplateOid))
                    reviewModels = reviewModels.Where(y => y.TemplateOid != null && y.TemplateOid.ToLower().Contains(filter.TemplateOid.ToLower()));

                if (!string.IsNullOrEmpty(filter.ValueSetName))
                    reviewModels = reviewModels.Where(y => y.ValueSetName != null && y.ValueSetName.ToLower().Contains(filter.ValueSetName.ToLower()));
            }

            results.Total = reviewModels.Count();

            reviewModels = reviewModels
                .OrderBy(y => y.TemplateName)
                .Skip(count * (page - 1))
                .Take(count);

            results.Items = reviewModels;

            return results;
        }
    }
}
