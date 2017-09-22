using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Generation.Versioning
{
    public class VersionComparer
    {
        private ComparisonResult result;
        private IObjectRepository tdb;
        private IIGTypePlugin igTypePlugin;
        private IGSettingsManager igSettings;

        #region Constructor

        /// <summary>
        /// Creates a new instance of VersionComparer
        /// </summary>
        /// <param name="tdb">The repository for data</param>
        private VersionComparer()
        {
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Creates a new instance of VersionComparer
        /// </summary>
        public static VersionComparer CreateComparer(IObjectRepository tdb, IIGTypePlugin igTypePlugin, IGSettingsManager igSettings)
        {
            VersionComparer comparer = new VersionComparer();
            comparer.tdb = tdb;
            comparer.igTypePlugin = igTypePlugin;
            comparer.igSettings = igSettings;
            return comparer;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares the previous template to the new template in two stages; first the fields of the templates and then the constraints of the templates.
        /// The comparison between constraints are based on the IConstraint.Number field for matching the constraints together.
        /// </summary>
        /// <remarks>
        /// Template fields: Name, Oid, Description, Is Open, Implied Template
        /// Constraint fields: Description, Label, Conformance, Cardinality, DataType, Value, Display Name, ValueSet Date, Value Set, Code System, Contained Template
        /// </remarks>
        /// <returns>A ComparisonResult instance which contains the differences between the old and new templates fields 
        /// and a list of ComparisonConstraintResult instances that represents each modified constraint.</returns>
        public ComparisonResult Compare(ITemplate previousTemplate, ITemplate newTemplate)
        {
            this.result = new ComparisonResult();

            this.CompareTemplate(previousTemplate, newTemplate);

            if (newTemplate.Status != PublishStatus.RETIRED_STATUS)
            {
                // Added constraints
                foreach (var cConstraint in newTemplate.Constraints.Where(b => !previousTemplate.Constraints.Exists(a => a.Number == b.Number)))
                {
                    IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.igSettings, this.igTypePlugin, cConstraint, null);

                    ComparisonConstraintResult cResult = new ComparisonConstraintResult()
                    {
                        ParentNumber = cConstraint.Parent != null ?
                            string.Format("{0}-{1}", cConstraint.Template.OwningImplementationGuideId, cConstraint.Parent.Number) :
                            null,
                        Number = string.Format("{0}-{1}", cConstraint.Template.OwningImplementationGuideId, cConstraint.Number.Value),
                        Order = cConstraint.Order,
                        Type = CompareStatuses.Added,
                        NewNarrative = fc.GetPlainText()
                    };

                    this.result.ChangedConstraints.Add(cResult);
                }

                // Deleted constraints
                foreach (var cConstraint in previousTemplate.Constraints.Where(a => !newTemplate.Constraints.Exists(b => b.Number == a.Number)))
                {
                    IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.igSettings, this.igTypePlugin, cConstraint, null);

                    ComparisonConstraintResult cResult = new ComparisonConstraintResult()
                    {
                        ParentNumber = cConstraint.Parent != null ?
                            string.Format("{0}-{1}", cConstraint.Parent.Template.OwningImplementationGuideId, cConstraint.Parent.Number) :
                            null,
                        Number = string.Format("{0}-{1}", cConstraint.Template.OwningImplementationGuideId, cConstraint.Number),
                        Order = cConstraint.Order,
                        Type = CompareStatuses.Removed,
                        OldNarrative = fc.GetPlainText()
                    };

                    this.result.ChangedConstraints.Add(cResult);
                }

                // Modified constraints
                foreach (var oldConstraint in previousTemplate.Constraints.Where(a => newTemplate.Constraints.Exists(b => b.Number == a.Number)))
                {
                    var newConstraint = newTemplate.Constraints.Single(b => b.Number == oldConstraint.Number);

                    ComparisonConstraintResult compareResult = this.CompareConstraint(this.igSettings, oldConstraint, newConstraint);

                    if (compareResult != null)
                        result.ChangedConstraints.Add(compareResult);
                }
            }

            return this.result;
        }

        #endregion

        #region Private Methods

        private void CompareTemplate(ITemplate previousTemplate, ITemplate newTemplate)
        {
            CheckField(this.result, "Status", previousTemplate.Status, newTemplate.Status);
            CheckField(this.result, "Name", previousTemplate.Name, newTemplate.Name);
            CheckField(this.result, "Oid", previousTemplate.Oid, newTemplate.Oid);
            CheckField(this.result, "Description", previousTemplate.Description, newTemplate.Description);
            CheckField(this.result, "Open/Closed", GetIsOpen(previousTemplate), GetIsOpen(newTemplate));
            CheckField(this.result, "Implied Template", GetImpliedName(previousTemplate), GetImpliedName(newTemplate));
        }

        private ComparisonConstraintResult CompareConstraint(IGSettingsManager igSettings, IConstraint oldConstraint, IConstraint newConstraint)
        {
            var oldFc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.igSettings, this.igTypePlugin, oldConstraint, null);
            var newFc = FormattedConstraintFactory.NewFormattedConstraint(this.tdb, this.igSettings, this.igTypePlugin, newConstraint, null);

            var newNarrative = newFc.GetPlainText();
            var oldNarrative = oldFc.GetPlainText();

            var cResult = new ComparisonConstraintResult
            {
                ParentNumber = newConstraint.Parent != null 
                    ? string.Format("{0}-{1}", newConstraint.Parent.Template.OwningImplementationGuideId, newConstraint.Parent.Number) 
                    : null,
                Number = string.Format("{0}-{1}", newConstraint.Template.OwningImplementationGuideId, newConstraint.Number.Value),
                Order = newConstraint.Order,
                NewNarrative = newNarrative,
                OldNarrative = oldNarrative
            };

            // If all that changed was the CONF number, then treat as unchanged.
            if (newNarrative.IndexOf("(CONF") >= 0 && oldNarrative.IndexOf("(CONF") >= 0)
            {
                var newNarrativeCompare = newNarrative.Substring(0, newNarrative.IndexOf("(CONF"));
                var oldNarrativeCompare = oldNarrative.Substring(0, oldNarrative.IndexOf("(CONF"));

                cResult.Type = newNarrativeCompare != oldNarrativeCompare
                                   ? CompareStatuses.Modified
                                   : CompareStatuses.Unchanged;
            }
            else
            {
                cResult.Type = newNarrative != oldNarrative 
                                    ? CompareStatuses.Modified 
                                    : CompareStatuses.Unchanged;
            }

            if (!oldConstraint.IsPrimitive && cResult.Type != CompareStatuses.Unchanged)
            {
                CheckField(cResult, "Description", oldConstraint.Description, newConstraint.Description);
                CheckField(cResult, "Label", oldConstraint.Label, newConstraint.Label);
                CheckField(cResult, "Conformance", oldConstraint.Conformance, newConstraint.Conformance);
                CheckField(cResult, "Cardinality", oldConstraint.Cardinality, newConstraint.Cardinality);
                CheckField(cResult, "DataType", oldConstraint.DataType, newConstraint.DataType);
                CheckField(cResult, "Value", oldConstraint.Value, newConstraint.Value);
                CheckField(cResult, "Display Name", oldConstraint.ValueDisplayName, newConstraint.ValueDisplayName);
                CheckField(cResult, "ValueSet Date", oldConstraint.ValueSetDate, newConstraint.ValueSetDate);
                CheckField(cResult, "ValueSet", GetValueSetDisplay(oldConstraint), GetValueSetDisplay(newConstraint));
                CheckField(cResult, "Code System", GetCodeSystemDisplay(oldConstraint), GetCodeSystemDisplay(newConstraint));
                CheckField(cResult, "Contained Template", GetContainedTemplateDisplay(oldConstraint), GetContainedTemplateDisplay(newConstraint));
            }

            return cResult;
        }

        #endregion

        #region Helper Functions for Field Comparison

        private string GetContainedTemplateDisplay(IConstraint constraint)
        {
            var containedTemplates = (from r in this.tdb.TemplateConstraintReferences
                                      join t in this.tdb.Templates on r.ReferenceIdentifier equals t.Oid
                                      where r.ReferenceType == ConstraintReferenceTypes.Template
                                      select new { t.Name, t.Oid }).ToList();
            var containedTemplateStrings = containedTemplates.Select(t => string.Format("{0} ({1})", t.Name, t.Oid));
            return string.Join(", ", containedTemplateStrings);
        }

        private string GetCodeSystemDisplay(IConstraint constraint)
        {
            if (constraint.ValueCodeSystemId != null)
            {
                CodeSystem codeSystem = this.tdb.CodeSystems.Single(y => y.Id == constraint.ValueCodeSystemId);
                return string.Format("{0} ({1})", codeSystem.Name, codeSystem.Oid);
            }

            return string.Empty;
        }

        private string GetValueSetDisplay(IConstraint constraint)
        {
            if (constraint.ValueSetId != null)
            {
                ValueSet valueSet = this.tdb.ValueSets.Single(y => y.Id == constraint.ValueSetId);
                return string.Format("{0} ({1})", valueSet.Name, valueSet.GetIdentifier(this.igTypePlugin));
            }

            return string.Empty;
        }

        private string GetImpliedName(ITemplate template)
        {
            if (template == null || template.ImpliedTemplateId == null)
                return string.Empty;

            Template impliedTemplate = this.tdb.Templates.Single(y => y.Id == template.ImpliedTemplateId);

            return string.Format("{0} ({1})", impliedTemplate.Name, impliedTemplate.Oid);
        }

        private string GetIsOpen(ITemplate template)
        {
            if (template == null)
                return string.Empty;

            return template.IsOpen ? "Open" : "Closed";
        }

        private void CheckField(ICompareFields result, string fieldName, object fieldA, object fieldB)
        {
            if (fieldA is IComparable && fieldB is IComparable)
            {
                IComparable comparableFieldA = fieldA as IComparable;
                IComparable comparableFieldB = fieldB as IComparable;

                if (comparableFieldA.CompareTo(fieldB) == 0)
                    return;
            }
            else if (fieldA != null && fieldB != null && fieldA.ToString() == fieldB.ToString())
            {
                return;
            }
            else if (fieldA == fieldB)
            {
                return;
            }

            result.ChangedFields.Add(new ComparisonFieldResult()
            {
                Name = fieldName,
                Old = fieldA == null ? string.Empty : fieldA.ToString(),
                New = fieldB == null ? string.Empty : fieldB.ToString()
            });
        }

        #endregion
    }
}
