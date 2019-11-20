using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.ComponentModel;

using Trifolia.Logging;

namespace Trifolia.DB
{
    public partial class Template : IFilterOrganization, ITemplate
    {
        #region Field Constants

        public const string IdField = "Id";
        public const string TitleField = "Name";
        public const string OidField = "Oid";
        public const string BookmarkField = "Bookmark";
        public const string TemplateTypeIdField = "TemplateTypeId";
        public const string ContextField = "Context";
        public const string ImpliedTemplateIdField = "ImpliedTemplateId";
        public const string IsOpenField = "IsOpen";
        public const string DescriptionField = "Description";
        public const string NotesField = "Notes";
        public const string OwningImplementationGuideIdField = "OwningImplementationGuideId";
        public const string OrganizationIdField = "OrganizationId";
        public const string AuthorField = "AuthorId";

        #endregion

        #region Extra Properties

        /// <summary>
        /// Get only. If the template has an owning implementation guide, returns the publish date of the implementation guide.
        /// </summary>
        /// <remarks>
        /// Used by ASP version of template editor to determine if user should be allowed to edit.
        /// </remarks>
        public bool IsPublished
        {
            get
            {
                return this.OwningImplementationGuide.IsPublished();
            }
        }

        #endregion

        #region IFilterOrganization Implementation

        /// <summary>
        /// Get only. Required by IFilterOrganization to allow templates to be filtered based on the authenticated user's organization.
        /// </summary>
        public string OrganizationName
        {
            get
            {
                if (this.OwningImplementationGuide != null && this.OwningImplementationGuide.Organization != null)
                    return this.OwningImplementationGuide.Organization.Name;

                return string.Empty;
            }
        }

        #endregion

        #region ITemplate Implementation

        public List<IConstraint> Constraints
        {
            get
            {
                return this.ChildConstraints.ToList<IConstraint>();
            }
        }
        
        string ITemplate.Status
        {
            get
            {
                return this.Status != null ? this.Status.Status : "Draft";
            }
        }

        #endregion

        #region Static Retrieval Methods

        public static List<Template> GetTemplates()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetTemplates(tdb);
            }
        }

        /// <summary>
        /// Gets all templates.
        /// </summary>
        public static List<Template> GetTemplates(IObjectRepository tdb)
        {
            var templateQuery = tdb.Templates;
            
            return templateQuery.ToList();
        }

        public static List<Template> GetTemplates(int excludeId, bool shouldFilter = false)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetTemplates(tdb, excludeId, shouldFilter);
            }
        }

        /// <summary>
        /// Gets all templates excluding the template specified by excludeId.
        /// </summary>
        public static List<Template> GetTemplates(IObjectRepository tdb, int excludeId, bool shouldFilter = false)
        {
            List<Template> templates = tdb.Templates
                                        .Where(y => y.Id != excludeId)
                                        .OrderBy(y => y.Name)
                                        .ToList();

            return templates;
        }

        public static List<Template> GetTemplates(string implementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetTemplates(tdb, implementationGuideId);
            }
        }

        /// <summary>
        /// Gets all templates associated with a specific implementation guide. If no implementation guide is
        /// specified, then all templates are returned.
        /// </summary>
        public static List<Template> GetTemplates(IObjectRepository tdb, string implementationGuideId)
        {
            if (string.IsNullOrEmpty(implementationGuideId))
                return tdb.Templates
                    .OrderBy(y => y.Name)
                    .ToList();

            int realImplementationGuideId = int.Parse(implementationGuideId);

            List<Template> templates = (from t in tdb.Templates
                                        where t.OwningImplementationGuideId == realImplementationGuideId
                                        select t)
                                        .OrderBy(y => y.Name)
                                        .ToList();

            return templates;
        }

        public static List<Template> GetIgTypeTemplates(int implementationGuideTypeId, bool shouldFilter = false)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return GetIgTypeTemplates(tdb, implementationGuideTypeId);
            }
        }

        /// <summary>
        /// Gets all templates associated with a specific implementation guide type.
        /// </summary>
        public static List<Template> GetIgTypeTemplates(IObjectRepository tdb, int implementationGuideTypeId)
        {
            List<Template> templates = tdb.Templates
                                        .Where(y => y.ImplementationGuideTypeId == implementationGuideTypeId)
                                        .OrderBy(y => y.Name)
                                        .ToList();

            return templates;
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Clones this <see cref="Template"/> instance, optionally doing a deep copy of all children
        /// </summary>
        /// <param name="tdb">The object repository instance</param>
        /// <param name="userName">The name of the user who is copying the template</param>
        /// <param name="aDeepCopy">Whether to copy all related children</param>
        /// <returns></returns>
        public Template CloneTemplate(IObjectRepository tdb, int? authorId)
        {
            // Create a new name for the template that indicates its a copy
            string name = this.Name + " (Copy";
            int titleCount = tdb.Templates.Count(y => y.Name.StartsWith(name));

            if (titleCount > 0)
                name = name + " " + (titleCount + 1) + ")";
            else
                name += ")";

            string oid = this.Oid + ".1";

            int lastOidCount = 1;
            int oidCount = 2;
            while (tdb.Templates.Count(y => y.Oid == oid) > 0)
            {
                oid = oid.Substring(0, oid.Length - lastOidCount.ToString().Length);
                oid += oidCount.ToString();

                lastOidCount++;
                oidCount++;
            }

            string bookmark = this.Bookmark + " (Copy";

            if (titleCount > 0)
                bookmark = bookmark + " " + (titleCount + 1) + ")";
            else
                bookmark += ")";

            Template newTemplate = new Template()
            {
                Name = name,
                OwningImplementationGuideId = this.OwningImplementationGuideId,
                Oid = oid,
                Bookmark = bookmark,
                ImplementationGuideTypeId = this.ImplementationGuideType.Id,
                PrimaryContext = this.PrimaryContext,
                PrimaryContextType = this.PrimaryContextType,
                IsOpen = this.IsOpen,
                TemplateTypeId = this.TemplateType.Id,
                Description = this.Description,
                Notes = this.Notes,
                StatusId = this.StatusId,
                ImpliedTemplateId = this.ImpliedTemplateId
            };

            // If an author was provided, use it, otherwise retain the original author of the template
            if (authorId != null)
                newTemplate.AuthorId = authorId.Value;
            else
                newTemplate.AuthorId = this.AuthorId;

            // Copy the constraints
            List<TemplateConstraint> constraints = this.ChildConstraints.Where(y => y.ParentConstraintId == null).ToList();
            constraints.ForEach(y => 
            {
                newTemplate.ChildConstraints.Add(
                    CloneTemplateConstraint(newTemplate, null, y));
            });

            // Copy samples
            foreach (var sourceSample in this.TemplateSamples)
            {
                TemplateSample newTemplateSample = new TemplateSample()
                {
                    Name = sourceSample.Name,
                    XmlSample = sourceSample.XmlSample
                };
                newTemplate.TemplateSamples.Add(newTemplateSample);
            }

            return newTemplate;
        }

        private TemplateConstraint CloneTemplateConstraint(Template newTemplate, TemplateConstraint parentConstraint, TemplateConstraint constraint)
        {
            TemplateConstraint newConstraint = new TemplateConstraint()
            {
                Template = newTemplate,
                Conformance = constraint.Conformance,
                Cardinality = constraint.Cardinality,
                Value = constraint.Value,
                PrimitiveText = constraint.PrimitiveText,
                DataType = constraint.DataType,
                Context = constraint.Context,
                CodeSystemId = constraint.CodeSystemId,
                DisplayName = constraint.DisplayName,
                IsBranch = constraint.IsBranch,
                Order = constraint.Order,
                Schematron = constraint.Schematron,
                ValueConformance = constraint.ValueConformance,
                ValueSetId = constraint.ValueSetId,
                IsStatic = constraint.IsStatic,
                IsBranchIdentifier = constraint.IsBranchIdentifier,
                IsSchRooted = constraint.IsSchRooted,
                IsPrimitive = constraint.IsPrimitive,
                IsInheritable = constraint.IsInheritable,
                ValueSetDate = constraint.ValueSetDate,
                Label = constraint.Label,
                Notes = constraint.Notes,
                Description = constraint.Description,
                Number = constraint.Number.Value,
                Category = constraint.Category,
                MustSupport = constraint.MustSupport,
                IsModifier = constraint.IsModifier,
                IsFixed = constraint.IsFixed,
                IsChoice = constraint.IsChoice,
                IsHeading = constraint.IsHeading,
                HeadingDescription = constraint.HeadingDescription
            };

            if (parentConstraint != null)
            {
                newConstraint.ParentConstraintId = null;
                newConstraint.ParentConstraint = parentConstraint;
            }

            constraint.References.ToList().ForEach(y =>
            {
                newConstraint.References.Add(new TemplateConstraintReference()
                {
                    Constraint = newConstraint,
                    ReferenceIdentifier = y.ReferenceIdentifier,
                    ReferenceType = y.ReferenceType
                });
            });

            // Clone each child constraint
            constraint.ChildConstraints.ToList().ForEach(y =>
            {
                newTemplate.ChildConstraints.Add(
                    CloneTemplateConstraint(newTemplate, newConstraint, y));
            });

            return newConstraint;
        }

        public static string GenerateBookmark(string title, string abbreviation)
        {
            string bookmark = Helper.GetCleanName(title);

            if (!string.IsNullOrEmpty(abbreviation))
                bookmark = abbreviation + "_" + bookmark;

            while (bookmark.Contains("__"))
                bookmark = bookmark.Replace("__", "_");

            return bookmark.Length <= 40 ? bookmark : bookmark.Substring(0, 39);
        }

        public IEnumerable<TemplateConstraint> GetReferencingConstraints(IObjectRepository tdb)
        {
            string thisIdentifier = this.Oid.ToLower().Trim();
            return (from tc in tdb.TemplateConstraints
                    join tcr in tdb.TemplateConstraintReferences on tc.Id equals tcr.TemplateConstraintId
                    where tcr.ReferenceIdentifier.ToLower().Trim() == thisIdentifier && tcr.ReferenceType == ConstraintReferenceTypes.Template
                    select tc)
                    .Distinct();
        }

        public void Delete(IObjectRepository tdb, int? replacementTemplateId)
        {
            string thisIdentifier = this.Oid.ToLower().Trim();
            var constraintReferences = tdb.TemplateConstraintReferences
                .Where(y => y.ReferenceIdentifier == thisIdentifier && y.ReferenceType == ConstraintReferenceTypes.Template)
                .ToList();
            Template replacementTemplate = replacementTemplateId != null ? tdb.Templates.SingleOrDefault(y => y.Id == replacementTemplateId) : null;

            // Update the constraints to reference the selected template instead of this one
            foreach (var constraintReference in constraintReferences)
            {
                if (replacementTemplate != null)
                {
                    bool alreadyHasNewReference = constraintReference.Constraint.References.Any(y =>
                        y.ReferenceIdentifier.ToLower().Trim() == replacementTemplate.Oid.ToLower().Trim()
                        && y.ReferenceType == ConstraintReferenceTypes.Template);

                    if (alreadyHasNewReference)
                        tdb.TemplateConstraintReferences.Remove(constraintReference);
                    else
                        constraintReference.ReferenceIdentifier = replacementTemplate.Oid;
                }
                else
                {
                    tdb.TemplateConstraintReferences.Remove(constraintReference);
                }
            }

            // Update the tempaltes to reference the selected template instead of this one
            List<Template> templates = this.ImplyingTemplates.ToList();

            if (replacementTemplateId == null)
                templates.ForEach(ct => ct.Delete(tdb, null));
            else
                templates.ForEach(y => y.ImpliedTemplateId = replacementTemplateId);

            // Remove the constraints (and their green constraints) associated with the template
            this.ChildConstraints.ToList().ForEach(y =>
            {
                tdb.TemplateConstraints.Remove(y);
                y.GreenConstraints.ToList().ForEach(x => tdb.GreenConstraints.Remove(x));

                // TODO: Not sure why, by y.Samples is not populating correctly from EF right now.
                var samples = tdb.TemplateConstraintSamples.Where(x => x.TemplateConstraintId == y.Id);
                samples.ToList().ForEach(x => tdb.TemplateConstraintSamples.Remove(x));
            });

            // Remove any green templates associated with this template
            this.GreenTemplates.ToList().ForEach(y => tdb.GreenTemplates.Remove(y));

            // Delete samples associated with the template
            this.TemplateSamples.ToList().ForEach(y => tdb.TemplateSamples.Remove(y));

            tdb.Templates.Remove(this);
        }

        public List<Template> GetRelatedTemplates(IObjectRepository tdb, bool includeImplied = true, bool includeContained = true)
        {
            List<Template> templates = new List<Template>();

            GetRelatedTemplates(tdb, templates, this, includeImplied, includeContained);

            return templates;
        }

        private static void GetRelatedTemplates(IObjectRepository tdb, List<Template> list, Template currentTemplate, bool includeImplied, bool includeContained)
        {
            if (list.Contains(currentTemplate))
                return;

            if (currentTemplate.ImpliedTemplate != null && includeImplied)
            {
                list.Add(currentTemplate.ImpliedTemplate);
                GetRelatedTemplates(tdb, list, currentTemplate.ImpliedTemplate, includeImplied, includeContained);
            }

            if (includeContained)
            {
                var containedTemplates = (from tcr in tdb.TemplateConstraintReferences
                                          join tc in tdb.TemplateConstraints on tcr.TemplateConstraintId equals tc.Id
                                          join t in tdb.Templates on tc.TemplateId equals t.Id
                                          where tcr.ReferenceType == ConstraintReferenceTypes.Template && tcr.ReferenceIdentifier == currentTemplate.Oid
                                          select t);

                foreach (Template currentContainedTemplate in containedTemplates)
                {
                    list.Add(currentContainedTemplate);
                    GetRelatedTemplates(tdb, list, currentContainedTemplate, includeImplied, includeContained);
                }
            }

            if (!list.Contains(currentTemplate))
                list.Add(currentTemplate);
        }

        #endregion

        public string TypeDisplay
        {
            get
            {
                string context = string.IsNullOrEmpty(this.PrimaryContext) ? this.TemplateType.RootContext : this.PrimaryContext;

                return string.Format("{0} ({1})", this.TemplateType.Name, context);
            }
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(this.Oid) ? this.Oid : "NO OID";
        }
    }
}
