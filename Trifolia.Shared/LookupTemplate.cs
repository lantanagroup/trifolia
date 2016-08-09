using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.Authentication;
using Trifolia.Logging;
using Trifolia.DB;
using Trifolia.Authorization;

namespace Trifolia.Shared
{
    public class LookupTemplate : IFilterOrganization
    {
        #region Properties

        public int Id { get; set; }
        public string Title { get; set; }
        public string Oid { get; set; }
        public string Open { get; set; }
        public string IgType { get; set; }
        public string TemplateType { get; set; }
        public string TemplateTypeDisplay { get; set; }
        public string ImplementationGuide { get; set; }
        public string Organization { get; set; }
        public DateTime? PublishDate { get; set; }
        public string ImpliedTemplateOid { get; set; }
        public string ImpliedTemplateTitle { get; set; }
        public int ConstraintCount { get; set; }
        public int ContainedByCount { get; set; }
        public int ImpliedByCount { get; set; }

        private bool? isOpen;

        public bool? IsOpen
        {
            get { return this.isOpen; }
            set
            {
                this.isOpen = value;

                if (value == null)
                    this.Open = "N/A";
                else if (value == true)
                    this.Open = "Yes";
                else if (value == false)
                    this.Open = "No";
            }
        }

        #endregion

        #region IFilterOrganization Implementation

        public string OrganizationName
        {
            get { return this.Organization; }
        }

        #endregion

        public static List<LookupTemplate> GetImplementationGuideTemplates(bool? includeInferred, int? implementationGuideId, bool includeDeprecated = true)
        {
            if (includeInferred == null || implementationGuideId == null)
                return new List<LookupTemplate>();

            using (IObjectRepository tdb = DBContext.Create())
            {
                ImplementationGuide ig = tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);
                List<Template> igTemplates = ig.GetRecursiveTemplates(tdb, inferred: includeInferred == true);

                if (!includeDeprecated)
                {
                    string lDeprecatedStatus = PublishStatuses.Deprecated.ToString();
                    igTemplates.RemoveAll(t => t.Status != null && t.Status.Status == lDeprecatedStatus);
                }

                return FilterAndConvertTemplates(tdb, igTemplates);
            }
        }
        
        /// <summary>
        /// Gets all templates in the database
        /// </summary>
        public static List<LookupTemplate> GetTemplates()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetTemplates(tdb);
            }
        }

        public static List<LookupTemplate> GetAvailablePreviousTemplates(int implementationGuideId)
        {
            List<LookupTemplate> lAvailableTemplates = new List<LookupTemplate>();

            using (IObjectRepository tdb = DBContext.Create())
            {
                ImplementationGuide lSelectedGuide = tdb.ImplementationGuides.SingleOrDefault(ig => ig.Id == implementationGuideId);

                if (lSelectedGuide == null)
                    return new List<LookupTemplate>();

                if (!lSelectedGuide.PreviousVersionImplementationGuideId.HasValue) return lAvailableTemplates;
                ImplementationGuide lPreviousVersion = tdb.ImplementationGuides.Single(ig => ig.Id == lSelectedGuide.PreviousVersionImplementationGuideId);

                List<Template> lFirstGenerationTemplates = new List<Template>();

                foreach (Template lChildTemplate in lPreviousVersion.ChildTemplates)
                {
                    IEnumerable<Template> lNewerVersions = from t in tdb.Templates
                                                           where t.OwningImplementationGuideId == lPreviousVersion.Id
                                                           && t.PreviousVersionTemplateId == lChildTemplate.Id
                                                           select t;
                    if (lNewerVersions.Count() == 0) lFirstGenerationTemplates.Add(lChildTemplate);
                }

                return FilterAndConvertTemplates(tdb, lFirstGenerationTemplates);
            }
        }

        /// <summary>
        /// Gets all templates in the database
        /// </summary>
        public static List<LookupTemplate> GetTemplates(IObjectRepository tdb)
        {
            var templateQuery = tdb.ViewTemplates;
            Log.For(typeof(Template)).LogSql(templateQuery);

            return FilterAndConvertTemplates(tdb, templateQuery);
        }

        /// <summary>
        /// Gets templates that can be used with the specified list of data types. Only templates that can be used with the specified
        /// data-types (by derivation or directly) will be returned.
        /// </summary>
        /// <param name="dataTypes">List of data-types that should be used to filter the return list of templates.</param>
        /// <param name="excludedTemplateId">Excludes the specified template id from the return list</param>
        public static List<LookupTemplate> GetTemplateDataTypeTemplatesExcluding(List<string> dataTypes, int? excludedTemplateId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                if (!CheckPoint.Instance.IsAuthenticated)
                    return new List<LookupTemplate>();

                var excludedTemplates = (from t in tdb.ViewTemplates
                                         where excludedTemplateId == null || t.Id != excludedTemplateId
                                         select t);
                var templates = excludedTemplates;

                if (dataTypes != null && dataTypes.Count > 0 && !dataTypes.Contains("ANY"))
                    templates = templates.Where(y => y.PrimaryContext != null && dataTypes.Contains(y.PrimaryContext));

                return FilterAndConvertTemplates(tdb, templates);
            }
        }

        /// <summary>
        /// Gets a list of templates that match the specified template type.
        /// </summary>
        /// <param name="templateTypeId">The template type that should be used to filter the return list of templates.</param>
        /// <param name="excludedTemplateId">The template that should be excluded from the return list.</param>
        public static List<LookupTemplate> GetTemplateTypeTemplatesExcluding(int? templateTypeId, int? excludedTemplateId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                if (!CheckPoint.Instance.IsAuthenticated)
                    return new List<LookupTemplate>();

                var templates = (from t in tdb.ViewTemplates
                                 where (templateTypeId == null || t.TemplateTypeId == templateTypeId)
                                     && (excludedTemplateId == null || t.Id != excludedTemplateId)
                                 select t);

                return FilterAndConvertTemplates(tdb, templates);
            }
        }

        /// <summary>
        /// Gets a list of all templates in the database, excluding the specified template.
        /// </summary>
        /// <param name="excludeTemplate">The template instance that should not be returned as part of the list</param>
        public static List<LookupTemplate> GetTemplatesExcluding(Template excludeTemplate)
        {
            return GetTemplatesExcluding(excludeTemplate.Id);
        }

        /// <summary>
        /// Gets a list of all templates in the database, excluding the specified template.
        /// </summary>
        /// <param name="excludeTemplate">The template instance that should not be returned as part of the list</param>
        public static List<LookupTemplate> GetTemplatesExcluding(int excludeTemplateId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetTemplatesExcluding(tdb, excludeTemplateId);
            }
        }

        /// <summary>
        /// Gets a list of all templates in the database, excluding the specified template.
        /// </summary>
        /// <param name="excludeTemplate">The template instance that should not be returned as part of the list</param>
        public static List<LookupTemplate> GetTemplatesExcluding(IObjectRepository tdb, int excludeTemplateId)
        {
            var templates = tdb.ViewTemplates.Where(y => y.Id != excludeTemplateId);
            return FilterAndConvertTemplates(tdb, templates);
        }

        /// <summary>
        /// Gets all templates that are owned by the specified implementation guide
        /// </summary>
        /// <param name="implementationGuideId">The id of the implementation guide that templates should be returned for.</param>
        public static List<LookupTemplate> GetImplementationGuideTemplates(int? implementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                if (implementationGuideId == null)
                    return new List<LookupTemplate>();

                var templates = (from t in tdb.ViewTemplates
                                 where implementationGuideId == null || t.OwningImplementationGuideId == implementationGuideId.Value
                                 orderby t.Name
                                 select t);

                return FilterAndConvertTemplates(tdb, templates);
            }
        }

        /// <summary>
        /// Filters out the templates that are not accessible to external users based on the user's organization.
        /// Converts the each Template instance into a LookupTemplate
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        private static List<LookupTemplate> FilterAndConvertTemplates(IObjectRepository tdb, List<Template> templates)
        {
            User currentUser = CheckPoint.Instance.User;
            List<Template> filteredTemplates = templates;

            if (!CheckPoint.Instance.IsDataAdmin)
            {
                filteredTemplates = (from t in templates
                                     join p in tdb.ViewTemplatePermissions on t.Id equals p.TemplateId
                                     where p.UserId == currentUser.Id
                                     select t).ToList();
            }

            // Convert the templates to LookupTemplate instances
            return (from t in filteredTemplates
                    select ConvertTemplate(t)).ToList();
        }

        /// <summary>
        /// Filters out the templates that are not accessible to external users based on the user's organization.
        /// Converts the each Template instance into a LookupTemplate
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        internal static List<LookupTemplate> FilterAndConvertTemplates(IObjectRepository tdb, IEnumerable<ViewTemplate> templates)
        {
            User currentUser = CheckPoint.Instance.User;
            IEnumerable<ViewTemplate> filteredTemplates = templates;

            if (!CheckPoint.Instance.IsDataAdmin)
                filteredTemplates = (from p in tdb.ViewTemplatePermissions
                                     join t in templates on p.TemplateId equals t.Id
                                     where p.UserId == currentUser.Id
                                     select t);

            // Convert the templates to LookupTemplate instances
            return (from t in filteredTemplates
                    select ConvertTemplate(t)).ToList();
        }

        public static LookupTemplate ConvertTemplate(Template template)
        {
            LookupTemplate lookupTemplate = new LookupTemplate()
            {
                Id = template.Id,
                Oid = template.Oid,
                Title = template.Name,
                IsOpen = template.IsOpen,
                ImplementationGuide = template.OwningImplementationGuide != null ? template.OwningImplementationGuide.Name : string.Empty,
                IgType = template.ImplementationGuideType.Name,
                TemplateType = template.TemplateType.Name,
                TemplateTypeDisplay = template.TemplateType.Name + " (" + template.ImplementationGuideType.Name + ")",
                Organization = template.Organization != null ? template.Organization.Name : string.Empty,
                PublishDate = template.OwningImplementationGuide != null ? template.OwningImplementationGuide.PublishDate : null,
                ImpliedTemplateOid = template.ImpliedTemplate != null ? template.ImpliedTemplate.Oid : null,
                ImpliedTemplateTitle = template.ImpliedTemplate != null ? template.ImpliedTemplate.Name : null,
                ConstraintCount = template.ChildConstraints.Count,
                ContainedByCount = template.ContainingConstraints.Count,
                ImpliedByCount = template.ImplyingTemplates.Count
            };

            return lookupTemplate;
        }

        internal static LookupTemplate ConvertTemplate(ViewTemplate template)
        {
            LookupTemplate lookupTemplate = new LookupTemplate()
            {
                Id = template.Id,
                Oid = template.Oid,
                Title = template.Name,
                IsOpen = template.IsOpen,
                ImplementationGuide = template.OwningImplementationGuideTitle,
                IgType = template.ImplementationGuideTypeName,
                TemplateType = template.TemplateTypeName,
                TemplateTypeDisplay = template.TemplateTypeDisplay,
                Organization = template.OrganizationName,
                PublishDate = template.PublishDate,
                ImpliedTemplateOid = template.ImpliedTemplateOid,
                ImpliedTemplateTitle = template.ImpliedTemplateTitle,
                ConstraintCount = template.ConstraintCount != null ? (int)template.ConstraintCount.Value : 0,
                ContainedByCount = template.ContainedTemplateCount != null ? (int)template.ContainedTemplateCount.Value : 0,
                ImpliedByCount = template.ImpliedTemplateCount != null ? (int)template.ImpliedTemplateCount.Value : 0
            };

            return lookupTemplate;
        }
    }
}
