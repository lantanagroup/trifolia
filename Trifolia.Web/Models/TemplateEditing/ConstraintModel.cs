using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.DB;

namespace Trifolia.Web.Models.TemplateEditing
{
    public class ConstraintModel : IConstraint
    {
        public ConstraintModel()
        {
            this.Children = new List<ConstraintModel>();
        }

        // Core constraint fields
        public int Id { get; set; }
        public bool IsNew { get; set; }
        public int? Number { get; set; }
        public bool IsPrimitive { get; set; }
        public string Context { get; set; }
        public string Conformance { get; set; }
        public string Cardinality { get; set; }
        public string DataType { get; set; }
        public bool IsBranch { get; set; }
        public bool IsBranchIdentifier { get; set; }
        public string PrimitiveText { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Label { get; set; }
        public string ValueConformance { get; set; }
        public string Value { get; set; }
        public string ValueDisplayName { get; set; }
        public string Binding { get; set; }
        public int? ValueSetId { get; set; }
        public DateTime? ValueSetDate { get; set; }
        public int? ValueCodeSystemId { get; set; }
        public int? ContainedTemplateId { get; set; }
        public string NarrativeProseHtml { get; set; }
        public string Schematron { get; set; }
        public string Category { get; set; }
        public string DisplayNumber { get; set; }
        public bool IsModifier { get; set; }
        public bool MustSupport { get; set; }

        // Tech editor fields
        public bool IsHeading { get; set; }
        public string HeadingDescription { get; set; }

        // Engineer fields
        public bool IsSchRooted { get; set; }
        public bool IsInheritable { get; set; }

        public List<ConstraintModel> Children { get; set; }

        IEnumerable<IConstraint> IConstraint.Children
        {
            get
            {
                if (this.Children == null)
                    return null;

                return this.Children.Cast<IConstraint>();
            }
        }

        public ITemplate Template
        {
            get
            {
                return null;
            }
        }

        public IConstraint Parent
        {
            get
            {
                return null;
            }
            set { }
        }

        public bool? IsStatic
        {
            get
            {
                switch (this.Binding)
                {
                    case "STATIC":
                        return true;
                    case "DYNAMIC":
                        return false;
                    default:
                        return null;
                }
            }
            set
            {
                if (value == true)
                    this.Binding = "STATIC";
                else if (value == false)
                    this.Binding = "DYNAMIC";
                else
                    this.Binding = "";
            }
        }

        public int Order
        {
            get
            {
                return 0;
            }
            set { }
        }

        public bool IsValueSetStatic
        {
            get
            {
                if (this.IsStatic == true)
                    return true;
                return false;
            }
            set { }
        }
    }
}