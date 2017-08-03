using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;
using ExportConstraint = Trifolia.Shared.ImportExport.Model.ConstraintType;
using ExportSingleValueCode = Trifolia.Shared.ImportExport.Model.ConstraintTypeSingleValueCode;
using ExportValueSet = Trifolia.Shared.ImportExport.Model.ConstraintTypeValueSet;
using ExportCodeSystem = Trifolia.Shared.ImportExport.Model.ConstraintTypeCodeSystem;
using ExportConformanceTypes = Trifolia.Shared.ImportExport.Model.ConstraintTypeConformance;
using ExportCategory = Trifolia.Shared.ImportExport.Model.ConstraintTypeCategory;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Export.Native
{
    public static class TemplateConstraintExtension
    {
        #region Export XML

        private static ExportConformanceTypes GetExportConformance(string conformance)
        {
            switch (conformance)
            {
                case "SHALL":
                    return ExportConformanceTypes.SHALL;
                case "SHALL NOT":
                    return ExportConformanceTypes.SHALLNOT;
                case "SHOULD":
                    return ExportConformanceTypes.SHOULD;
                case "SHOULD NOT":
                    return ExportConformanceTypes.SHOULDNOT;
                case "MAY":
                    return ExportConformanceTypes.MAY;
                case "MAY NOT":
                    return ExportConformanceTypes.MAYNOT;
                default:
                    return ExportConformanceTypes.NONE;
            }
        }

        public static ExportConstraint Export(this TemplateConstraint constraint, IObjectRepository tdb, IGSettingsManager igSettings, IIGTypePlugin igTypePlugin, bool isVerbose = false, List<string> categories = null)
        {
            ExportConstraint exportConstraint = new ExportConstraint()
            {
                number = constraint.Number != null ? constraint.Number.Value : 0,
                numberSpecified = constraint.Number != null,
                displayNumber = constraint.DisplayNumber,
                context = constraint.Context,
                conformance = GetExportConformance(constraint.Conformance),
                cardinality = !string.IsNullOrEmpty(constraint.Cardinality) ? constraint.Cardinality : null,
                dataType = !string.IsNullOrEmpty(constraint.DataType) ? constraint.DataType : null,
                isBranch = constraint.IsBranch,
                isBranchIdentifier = constraint.IsBranchIdentifier,
                isSchRooted = constraint.IsSchRooted,
                isPrimitive = constraint.IsPrimitive,
                isStatic = constraint.IsStatic == true,
                isStaticSpecified = constraint.IsStatic != null,
                isInheritable = constraint.IsInheritable,
                SchematronTest = !string.IsNullOrEmpty(constraint.Schematron) ? constraint.Schematron : null,
                isVerbose = isVerbose,
                mustSupport = constraint.MustSupport,
                isModifier = constraint.IsModifier,
                isHeading = constraint.IsHeading,
                HeadingDescription = constraint.HeadingDescription,
                Notes = constraint.Notes,
                Label = constraint.Label
            };

            var containedTemplates = (from tcr in constraint.References
                                      join t in tdb.Templates on tcr.ReferenceIdentifier equals t.Oid
                                      where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                      select t);

            foreach (var containedTemplate in containedTemplates)
            {
                exportConstraint.ContainedTemplate.Add(new Shared.ImportExport.Model.ConstraintTypeContainedTemplate()
                {
                    identifier = containedTemplate.Oid,
                    type = containedTemplate.PrimaryContextType
                });
            }

            if (!string.IsNullOrEmpty(constraint.Category))
            {
                exportConstraint.Category = (from c in constraint.Category.Split(',')
                                             select new ExportCategory()
                                             {
                                                 name = c
                                             }).ToList();
            }

            if (!string.IsNullOrEmpty(constraint.Value))
            {
                ExportSingleValueCode exportSVC = new ExportSingleValueCode()
                {
                    code = constraint.Value,
                    displayName = constraint.DisplayName
                };
                exportConstraint.Item = exportSVC;
            }
            else if (constraint.ValueSet != null)
            {
                ExportValueSet exportValueSet = new ExportValueSet()
                {
                    name = constraint.ValueSet.Name,
                    isStatic = constraint.IsStatic == true,
                    isStaticSpecified = constraint.IsStatic != null,
                    identifier = constraint.ValueSet.GetIdentifier(igTypePlugin),
                    date = constraint.ValueSetDate.HasValue ? constraint.ValueSetDate.Value : DateTime.MinValue,
                    dateSpecified = constraint.ValueSetDate.HasValue
                };
                exportConstraint.Item = exportValueSet;
            }

            if (constraint.CodeSystem != null)
            {
                exportConstraint.CodeSystem = new ExportCodeSystem()
                {
                    identifier = constraint.CodeSystem.Oid,
                    name = constraint.CodeSystem.Name
                };
            }
            else
            {
                exportConstraint.CodeSystem = null;
            }

            if (!string.IsNullOrEmpty(constraint.Description))
                exportConstraint.Description = constraint.Description;

            if (!string.IsNullOrEmpty(constraint.Label))
                exportConstraint.Label = constraint.Label;

            if (!constraint.IsPrimitive)
            {
                IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(tdb, igSettings, igTypePlugin, constraint);

                // Only include the generated narrative, as Description and Label are already exported in separate fields.
                exportConstraint.NarrativeText = fc.GetPlainText(false, false, false);
            }
            else
            {
                exportConstraint.NarrativeText = constraint.PrimitiveText;
            }

            // Get all child constraints and build a new export-version of the constraint
            var childConstraints = constraint.ChildConstraints.Where(y => y.ParentConstraintId == constraint.Id).OrderBy(y => y.Order);

            foreach (var cChildConstraint in childConstraints)
            {
                if (!cChildConstraint.CategoryIsMatch(categories))
                    continue;

                exportConstraint.Constraint.Add(cChildConstraint.Export(tdb, igSettings, igTypePlugin, isVerbose, categories));
            }

            return exportConstraint;
        }

        #endregion
    }
}
