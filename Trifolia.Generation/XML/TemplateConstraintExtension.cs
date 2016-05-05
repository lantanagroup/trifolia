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
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Shared;

namespace Trifolia.Generation.XML
{
    public static class TemplateConstraintExtension
    {
        #region Export XML

        public static ExportConstraint Export(this TemplateConstraint constraint, IObjectRepository tdb, IGSettingsManager igSettings, bool isVerbose = false, List<string> categories = null)
        {
            ExportConformanceTypes exportConformance = ExportConformanceTypes.MAY;
            bool exportConformanceSpecified = Enum.TryParse<ExportConformanceTypes>(constraint.Conformance, out exportConformance);

            ExportConstraint exportConstraint = new ExportConstraint()
            {
                number = constraint.Number != null ? constraint.Number.Value : 0,
                context = constraint.Context,
                conformance = exportConformance,
                conformanceSpecified = exportConformanceSpecified,
                cardinality = !string.IsNullOrEmpty(constraint.Cardinality) ? constraint.Cardinality : null,
                dataType = !string.IsNullOrEmpty(constraint.DataType) ? constraint.DataType : null,
                containedTemplateOid = constraint.ContainedTemplate != null ? constraint.ContainedTemplate.Oid : null,
                containedTemplateType = constraint.ContainedTemplate != null ? constraint.ContainedTemplate.PrimaryContextType : null,
                isBranch = constraint.IsBranch,
                isBranchIdentifier = constraint.IsBranchIdentifier,
                isSchRooted = constraint.IsSchRooted,
                isPrimitive = constraint.IsPrimitive,
                isStatic = constraint.IsStatic == true,
                isInheritable = constraint.IsInheritable,
                SchematronTest = !string.IsNullOrEmpty(constraint.Schematron) ? constraint.Schematron : null,
                isVerbose = isVerbose
            };

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
                    oid = constraint.ValueSet.Oid
                };
                exportConstraint.Item = exportValueSet;
            }

            if (constraint.CodeSystem != null)
            {
                exportConstraint.CodeSystem = new ExportCodeSystem()
                {
                    oid = constraint.CodeSystem.Oid
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
                IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(tdb, igSettings, constraint);

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

                exportConstraint.Constraint.Add(cChildConstraint.Export(tdb, igSettings, isVerbose, categories));
            }

            return exportConstraint;
        }

        #endregion
    }
}
