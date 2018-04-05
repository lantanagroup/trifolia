using System.Collections.Generic;

namespace Trifolia.Export.Versioning
{
    public enum CompareStatuses
    {
        Added,
        Modified,
        Removed,
        Unchanged
    }

    public interface ICompareFields
    {
        List<ComparisonFieldResult> ChangedFields { get; set; }
    }

    public class ComparisonResult : ICompareFields
    {
        #region Ctor

        public ComparisonResult()
        {
            this.ChangedFields = new List<ComparisonFieldResult>();
            this.ChangedConstraints = new List<ComparisonConstraintResult>();
        }

        #endregion

        #region Properties

        public List<ComparisonFieldResult> ChangedFields { get; set; }
        public List<ComparisonConstraintResult> ChangedConstraints { get; set; }

        #endregion
    }

    public class ComparisonConstraintResult : ICompareFields
    {
        public ComparisonConstraintResult()
        {
            this.ChangedFields = new List<ComparisonFieldResult>();
        }

        public CompareStatuses Type { get; set; }
        public string Number { get; set; }
        public string ParentNumber { get; set; }
        public int Order { get; set; }
        public string OldNarrative { get; set; }
        public string NewNarrative { get; set; }
        public List<ComparisonFieldResult> ChangedFields { get; set; }
    }

    public class ComparisonFieldResult
    {
        public string Name { get; set; }
        public string Old { get; set; }
        public string New { get; set; }
    }
}
