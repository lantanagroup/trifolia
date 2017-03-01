using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class TemplateConstraint : IConstraint, ICloneable
    {
        #region IConstraint

        ITemplate IConstraint.Template
        {
            get
            {
                return this.Template;
            }
        }

        public IConstraint Parent
        {
            get
            {
                return this.ParentConstraint;
            }
        }

        public IEnumerable<IConstraint> Children
        {
            get
            {
                return this.ChildConstraints;
            }
        }

        bool IConstraint.IsBranch
        {
            get
            {
                return this.IsBranch == true;
            }
        }

        int? IConstraint.ValueCodeSystemId
        {
            get
            {
                return this.CodeSystemId;
            }
        }

        bool IConstraint.IsValueSetStatic
        {
            get
            {
                return this.IsStatic.HasValue ? this.IsStatic.Value : true;
            }
        }

        bool IConstraint.IsBranchIdentifier
        {
            get 
            {
                return this.IsBranchIdentifier ;
            }
        }

        bool IConstraint.IsChoice
        {
            get
            {
                return this.IsChoice;
            }
        }

        #endregion

        #region Properties

        public string ValueDisplayName
        {
            get
            {
                return this.DisplayName;
            }
        }

        public int? ValueCodeSystemId
        {
            get
            {
                return this.CodeSystemId;
            }
        }

        public Cardinality CardinalityType
        {
            get
            {
                if (string.IsNullOrEmpty(Cardinality))
                    return null;

                return CardinalityParser.Parse(Cardinality);
            }
        }

        public Conformance BusinessConformanceType
        {
            get
            {
                return ConformanceParser.Parse(this.Conformance);
            }
        }

        public Conformance ValueConformanceType
        {
            get
            {
                return ConformanceParser.Parse(this.ValueConformance);
            }
        }

        #endregion

        #region Clone

        public object Clone()
        {
            TemplateConstraint clone = new TemplateConstraint();
            clone.Cardinality = this.Cardinality;
            clone.CodeSystemId = this.CodeSystemId;
            clone.Conformance = this.Conformance;
            clone.ContainedTemplateId = this.ContainedTemplateId;
            clone.Context = this.Context;
            clone.DataType = this.DataType;
            clone.Description = this.Description;
            clone.DisplayName = this.DisplayName;
            clone.Id = this.Id;
            clone.IsBranch = this.IsBranch;
            clone.IsBranchIdentifier = this.IsBranchIdentifier;
            clone.IsInheritable = this.IsInheritable;
            clone.IsPrimitive = this.IsPrimitive;
            clone.IsSchRooted = this.IsSchRooted;
            clone.IsStatic = this.IsStatic;
            clone.Label = this.Label;
            clone.Notes = this.Notes;
            clone.Order = this.Order;
            clone.ParentConstraintId = this.ParentConstraintId;
            clone.PrimitiveText = this.PrimitiveText;
            clone.Schematron = this.Schematron;
            clone.TemplateId = this.TemplateId;
            clone.Value = this.Value;
            clone.ValueConformance = this.ValueConformance;
            clone.ValueSetDate = this.ValueSetDate;
            clone.ValueSetId = this.ValueSetId;
            clone.Template = this.Template;
            clone.ParentConstraint = this.ParentConstraint;
            clone.CodeSystem = this.CodeSystem;
            clone.ContainedTemplate = this.ContainedTemplate;
            clone.ValueSet = this.ValueSet;
            clone.Number = this.Number;
            return clone;
        }

        #endregion

        public string GetXpath()
        {
            if (this.IsPrimitive)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            TemplateConstraint current = this;

            while (current != null && !string.IsNullOrEmpty(current.Context))
            {
                if (sb.Length > 0)
                    sb.Insert(0, "/");

                sb.Insert(0, current.Context);
                current = current.ParentConstraint;
            }

            return sb.ToString();
        }

        public List<string> GetCategories()
        {
            IConstraint current = this;

            while (current != null)
            {
                if (!string.IsNullOrEmpty(current.Category))
                {
                    string[] categories = current.Category.Split(',');
                    return new List<string>(categories);
                }

                current = current.Parent;
            }

            return null;
        }

        public bool CategoryIsMatch(IEnumerable<string> selectedCategories)
        {
            if (selectedCategories == null || selectedCategories.Count() == 0)
                return true;

            var constraintCategories = this.GetCategories();

            if (constraintCategories == null || constraintCategories.Count == 0)
                return true;

            foreach (string category in constraintCategories)
            {
                if (selectedCategories.Contains(category))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("CONF#: {0}, Context: {1}", this.Number != null ? this.Number.ToString() : "N/A", this.Context);
        }
    }
}
