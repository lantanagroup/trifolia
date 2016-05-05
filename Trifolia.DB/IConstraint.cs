using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public interface IConstraint
    {
        ITemplate Template { get; }
        IConstraint Parent { get; }
        IEnumerable<IConstraint> Children { get; }
        int? Number { get; }
        string Context { get; }
        string Conformance { get; }
        string Cardinality { get; }
        string DataType { get; }
        string ValueConformance { get; }
        string Value { get; }
        string ValueDisplayName { get; }
        bool IsValueSetStatic { get; }
        int? ValueCodeSystemId { get; }
        bool IsBranch { get; }
        bool IsBranchIdentifier { get; }
        bool IsInheritable { get; }
        bool IsSchRooted { get; }
        bool IsPrimitive { get; }
        bool? IsStatic { get; }
        string PrimitiveText { get; }
        int Order { get; }
        string Category { get; }
        string DisplayNumber { get; set; }

        int? ContainedTemplateId { get; }

        int? ValueSetId { get; }
        DateTime? ValueSetDate { get; }

        string Description { get; }
        string Label { get; }
        bool IsHeading { get; }
        string HeadingDescription { get; }
    }

    public static class IConstraintExtensions
    {
        public static string GetFormattedNumber(this IConstraint constraint, DateTime? publishDate = null)
        {
            if (publishDate == null || publishDate.Value >= new DateTime(2015, 4, 21))      // On or after 4/21/2015
            {
                if (!string.IsNullOrEmpty(constraint.DisplayNumber))
                    return constraint.DisplayNumber;

                return string.Format("{0}-{1}",
                    constraint.Template != null ? constraint.Template.OwningImplementationGuideId.ToString() : "X",
                    constraint.Number.HasValue ? constraint.Number.ToString() : "X");
            }
            else if (publishDate.Value >= new DateTime(2014, 4, 15))        // On or after 4/15/2014
            {
                return string.Format("{0}-{1}",
                    constraint.Template != null ? constraint.Template.OwningImplementationGuideId.ToString() : "X",
                    constraint.Number.HasValue ? constraint.Number.ToString() : "X");
            }
            else
            {
                return constraint.Number.HasValue ? constraint.Number.ToString() : "X";
            }
        }
    }
}
