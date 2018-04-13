using System.Collections.Generic;
using System.Linq;
using Trifolia.Export.Versioning;

namespace Trifolia.Export.HTML
{
    public class DifferenceModel
    {
        #region Public Properties

        public int Id { get; set; }
        public bool IsAdded { get; set; }
        public string TemplateName { get; set; }
        public string PreviousTemplateName { get; set; }
        public int PreviousTemplateId { get; set; }
        public ComparisonResult Difference { get; set; }
        public List<Constraint> InlineConstraints { get; set; }

        #endregion

        public DifferenceModel(ComparisonResult aResult)
        {
            this.Difference = aResult;
            this.InlineConstraints = GetInlineConstraintChanges(aResult);
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

        public string GetInlineConstraintsHtml()
        {
            if (this.InlineConstraints == null || this.InlineConstraints.Count == 0)
                return string.Empty;

            string html = "<ol>\n";

            for (int i = 1; i <= this.InlineConstraints.Count; i++)
            {
                Constraint cConstraint = this.InlineConstraints[i-1];
                html += cConstraint.GetHtml(i);
            }

            html += "\n</ol>";

            return html;
        }

        public class Constraint
        {
            public Constraint()
            {
                this.Constraints = new List<Constraint>();
            }

            public string Number { get; set; }
            public string Narrative { get; set; }
            public CompareStatuses ChangeType { get; set; }
            public List<Constraint> Constraints { get; set; }

            public string GetHtml(int orderedListNumber)
            {
                string pre = string.Empty;
                string post = string.Empty;

                if (this.ChangeType == CompareStatuses.Added)
                {
                    pre = "<b>";
                    post = "</b>";
                }
                else if (this.ChangeType == CompareStatuses.Modified)
                {
                    pre = "<i>";
                    post = "</i>";
                }
                else if (this.ChangeType == CompareStatuses.Removed)
                {
                    pre = "<strike>";
                    post = "</strike>";
                }

                string html = string.Format("<li value=\"{0}\">{1}<span title=\"{2}\">{3}</span>{4}", orderedListNumber, pre, this.ChangeType.ToString(), this.Narrative, post);

                if (this.Constraints.Count > 0)
                {
                    html += "\n<ol>\n";

                    for (int i = 1; i <= this.Constraints.Count; i++)
                    {
                        Constraint cConstraint = this.Constraints[i - 1];
                        html += cConstraint.GetHtml(i);
                    }

                    html += "</ol>";
                }

                html += "</li>\n";

                return html;
            }
        }
    }
}