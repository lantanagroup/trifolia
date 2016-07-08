using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExportTemplate = Trifolia.Shared.ImportExport.Model.TrifoliaTemplate;
using ExportConstraint = Trifolia.Shared.ImportExport.Model.ConstraintType;
using ExportPreviousVersion = Trifolia.Shared.ImportExport.Model.TrifoliaTemplatePreviousVersion;
using ExportSample = Trifolia.Shared.ImportExport.Model.TrifoliaTemplateSample;
using Trifolia.DB;
using Trifolia.Shared.ImportExport.Model;
using Trifolia.Shared;

namespace Trifolia.Generation.XML
{
    public static class TemplateExtension
    {
        #region Export XML

        public static ExportTemplate Export(this Template template, IObjectRepository tdb, IGSettingsManager igSettings, SimpleSchema schema = null, List<string> categories = null)
        {
            ExportTemplate exportTemplate = new ExportTemplate()
            {
                identifier = template.Oid,
                implementationGuideType = template.ImplementationGuideType.Name,
                templateType = template.TemplateType.Name,
                title = template.Name,
                bookmark = template.Bookmark,
                context = template.PrimaryContext,
                contextType = template.PrimaryContextType,
                isOpen = template.IsOpen,
                isOpenSpecified = true,
                impliedTemplateOid = template.ImpliedTemplate != null ? template.ImpliedTemplate.Oid : null,
                Description = !string.IsNullOrEmpty(template.Description) ? template.Description : null,
                Notes = !string.IsNullOrEmpty(template.Notes) ? template.Notes : null,
                organizationName = template.Organization != null ? template.Organization.Name : null,
                publishStatus = template.Status != null ? template.Status.Status : null,
                PreviousVersion = null,
                ImplementationGuide = new TrifoliaTemplateImplementationGuide()
                {
                    name = template.OwningImplementationGuide.Name,
                    version = template.OwningImplementationGuide.Version.HasValue ? template.OwningImplementationGuide.Version.Value : 1
                },
            };

            if (template.PreviousVersion != null)
            {
                exportTemplate.PreviousVersion = new ExportPreviousVersion()
                {
                    name = template.PreviousVersion.Name,
                    identifier = template.PreviousVersion.Oid
                };
            }

            exportTemplate.Extension = (from e in template.Extensions
                                        select new TrifoliaTemplateExtension()
                                        {
                                            identifier = e.Identifier,
                                            type = e.Type,
                                            value = e.Value
                                        }).ToList();

            exportTemplate.Sample = (from s in template.TemplateSamples
                                     select new ExportSample()
                                     {
                                         name = s.Name,
                                         Value = s.XmlSample
                                     }).ToList();

            // Get all root-level child constraints and build a new export-version of the constraint
            var childConstraints = template.ChildConstraints.Where(y => y.ParentConstraintId == null).OrderBy(y => y.Order);

            // If a schema has been specified, include schema nodes verbosely
            if (schema != null)
            {
                var templateSchema = schema.GetSchemaFromContext(template.PrimaryContextType);

                if (templateSchema == null)
                    throw new Exception("Could not find context for template within schema");

                foreach (var schemaNode in templateSchema.Children)
                {
                    var foundConstraints = childConstraints.Where(y => y.Context == schemaNode.Name);

                    if (foundConstraints.Count() == 0)
                    {
                        var foundConstraint = schemaNode.CreateComputable(template.IsOpen);
                        exportTemplate.Constraint.Add(foundConstraint.Export(tdb, igSettings, isVerbose: true, categories: categories));
                    }
                    else
                    {
                        foreach (var foundConstraint in foundConstraints) 
                        {
                            if (!foundConstraint.CategoryIsMatch(categories))
                                continue;

                            exportTemplate.Constraint.Add(foundConstraint.Export(tdb, igSettings, categories: categories));
                        }
                    }
                }
            }
            else
            {
                foreach (var cChildConstraint in childConstraints)
                {
                    if (!cChildConstraint.CategoryIsMatch(categories))
                        continue;

                    exportTemplate.Constraint.Add(cChildConstraint.Export(tdb, igSettings, categories: categories));
                }
            }

            return exportTemplate;
        }

        #endregion
    }
}
