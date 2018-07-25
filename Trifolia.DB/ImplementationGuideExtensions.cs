using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data.Entity;
using Trifolia.Logging;
using System.Data.Entity.Core.Objects;

namespace Trifolia.DB
{
    public partial class ImplementationGuide
    {
        #region Public Properties

        public string NameWithVersion
        {
            get
            {
                if (this.Version > 1)
                    return string.Format("{0} V{1}", this.Name, this.Version);

                return this.Name;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all value sets in the implementation guide.
        /// Does this, by get all templates in the implementation guide, searching for constraints of those templates that have value sets.
        /// If isStatic is null (default), returns all value sets, regardless of the constraint's static binding to the value set.
        /// If isStatic is true, returns all value sets that are statically bound to the templates in the implementation guide.
        /// If isStatic is false, returns all value sets that are dynamically bound
        /// </summary>
        /// <param name="tdb"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public List<ImplementationGuideValueSet> GetValueSets(IObjectRepository tdb, bool? isStatic = null, bool readOnly = true)
        {
            var templateIds = tdb.GetImplementationGuideTemplates(this.Id, true, null, null);
            List<ImplementationGuideValueSet> retValueSets = new List<ImplementationGuideValueSet>();
            
            var valueSetConstraints = (from tid in templateIds
                                       join tc in (readOnly ? tdb.TemplateConstraints.AsNoTracking() : tdb.TemplateConstraints) on tid equals tc.TemplateId
                                       join vs in (readOnly ? tdb.ValueSets.AsNoTracking() : tdb.ValueSets) on tc.ValueSetId equals vs.Id
                                       where 
                                         (isStatic == null) || 
                                         (isStatic == true && (tc.IsStatic == null || tc.IsStatic == true)) || 
                                         (isStatic == false && (tc.IsStatic == null || tc.IsStatic == false))
                                       select new
                                       {
                                           Constraint = tc,
                                           ValueSet = vs,
                                           BindingDate = tc.ValueSetDate
                                       })
                                        .Distinct();

            var groupedValueSetConstraints = valueSetConstraints.GroupBy(
                y => y.ValueSet,
                (key, g) => new { ValueSet = key, Constraints = g.OrderBy(y => y.BindingDate) });

            foreach (var cGroupedValueSet in groupedValueSetConstraints)
            {
                if (cGroupedValueSet.Constraints.Count() > 1)
                {
                    string msg = string.Format("Vocabulary for IG \"{0}\" includes {1} valueset bindings with different binding dates. Using the latest binding date.",
                        this.Name,
                        cGroupedValueSet.Constraints.Count());
                    Log.For(typeof(ValueSet)).Trace(msg);
                }

                var maxBindingDate = cGroupedValueSet.Constraints.Max(y => y.BindingDate);

                var cConstraint = cGroupedValueSet.Constraints.FirstOrDefault(y => y.BindingDate == maxBindingDate);

                if (cConstraint == null)
                    cConstraint = cGroupedValueSet.Constraints.Last();

                // Use the binding date of the constraint, or if the constraint does not specify one, use the date that the implementation guide was published
                DateTime? bindingDate = cConstraint.BindingDate != null ? cConstraint.BindingDate : this.PublishDate;

                // If the constraint doesn't have a binding date for the valueset AND the implementation guide hasn't been published yet, use the current date
                if (bindingDate == null)
                    bindingDate = DateTime.Now;

                retValueSets.Add(
                    new ImplementationGuideValueSet()
                    {
                        ValueSet = cGroupedValueSet.ValueSet,
                        BindingDate = bindingDate
                    });
            }

            return retValueSets
                .OrderBy(y => y.ValueSet.Name)
                .ToList();
        }

        public IQueryable<Template> GetQueryableRecursiveTemplates(IObjectRepository tdb, List<int> parentTemplateIds = null, bool inferred = true, string[] categories = null)
        {
            List<int?> templateIds;

            if (parentTemplateIds != null && parentTemplateIds.Count > 0)
            {
                templateIds = new List<int?>();

                foreach (int parentTemplateId in parentTemplateIds)
                {
                    var cParentTemplateIds =
                        tdb.GetImplementationGuideTemplates(this.Id, inferred, parentTemplateId, categories)
                        .Select(y => y.Value);
                    templateIds.AddRange(cParentTemplateIds.Cast<int?>());
                }
            }
            else
            {
                templateIds = tdb.GetImplementationGuideTemplates(this.Id, inferred, null, categories).ToList();
            }

            var templatesQuery =
                tdb.Templates.Where(y => templateIds.Contains(y.Id))
                .Where(y => y.Oid != "http://hl7.org/fhir/StructureDefinition/" + y.Bookmark)       // Don't return base profiles from FHIR
                .Distinct()
                .OrderBy(y => y.TemplateType.Name)
                .ThenBy(y => y.Name)
                .AsQueryable<Template>();

            return templatesQuery;
        }

        /// <summary>
        /// Gets all templates associated to an implementation guide.
        /// Starts out with all templates that are directly associated to an implementation guide via ownership.
        /// Recursively looks at each directly associated template and gets all implied and contained templates, if the request for inferred templates is true.
        /// </summary>
        /// <param name="tdb"></param>
        /// <returns></returns>
        public List<Template> GetRecursiveTemplates(IObjectRepository tdb, List<int> parentTemplateIds = null, bool inferred = true, string[] categories = null, bool includeImplied = false, bool includeConstraints = false, bool includePrevious = false, bool includeSamples = false)
        {
            var templatesQuery = 
                this.GetQueryableRecursiveTemplates(tdb, parentTemplateIds, inferred, categories)
                .IncludeDetails(
                    includeImplied: includeImplied,
                    includeConstraints: includeConstraints,
                    includeSamples: includeSamples);

            return templatesQuery.ToList();
        }

        public string GetDisplayName()
        {
            return this.GetDisplayName(false);
        }

        public string GetDisplayName(bool fileNameSafe)
        {
            string name = this.NameWithVersion;

            if (!string.IsNullOrEmpty(this.DisplayName))
                name = this.DisplayName;

            if (fileNameSafe)
            {
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    name = name.Replace(c, '_');
                }
            }

            return name;
        }

        /// <summary>
        /// Gets whether this Implementation Guide is published
        /// </summary>
        /// <returns></returns>
        public bool IsPublished()
        {
            if (this.PublishStatus == null)
                return false;

            return this.PublishStatus.IsPublished;
        }

        public static List<ImplementationGuide> SearchImplementationGuides()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                List<ImplementationGuide> retList = tdb.ImplementationGuides
                    .OrderBy(y => y.Name)
                    .ToList();
                return retList;
            }
        }

        #endregion

        public void Delete(IObjectRepository tdb, int? replacementImplementationGuideId)
        {
            // Remove custom template types associated with the IG
            this.TemplateTypes.ToList().ForEach(y => tdb.ImplementationGuideTemplateTypes.Remove(y));

            // Remove custom settings (such as cardinality settings) associated with the IG
            this.Settings.ToList().ForEach(y => tdb.ImplementationGuideSettings.Remove(y));

            // Update the child templates of the IG to indicate the new replacing implementation guide
            if (replacementImplementationGuideId != null)
                this.ChildTemplates.ToList().ForEach(y => y.OwningImplementationGuideId = replacementImplementationGuideId.Value);
            else
                this.ChildTemplates.ToList().ForEach(t => t.Delete(tdb, null));
            
            this.Files.ToList().ForEach(y => {
                y.Versions.ToList().ForEach(x => tdb.ImplementationGuideFileDatas.Remove(x));
                tdb.ImplementationGuideFiles.Remove(y);
            });

            this.Permissions.ToList().ForEach(y => tdb.ImplementationGuidePermissions.Remove(y));

            this.SchematronPatterns.ToList().ForEach(y => tdb.ImplementationGuideSchematronPatterns.Remove(y));

            tdb.ImplementationGuides.Remove(this);
        }
    }

    [Serializable]
    public class ImplementationGuideValueSet
    {
        public ValueSet ValueSet { get; set; }
        public DateTime? BindingDate { get; set; }
    }
}
