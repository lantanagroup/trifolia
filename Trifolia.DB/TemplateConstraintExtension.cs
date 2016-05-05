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
            clone._Cardinality = this._Cardinality;
            clone._CodeSystemId = this._CodeSystemId;
            clone._Conformance = this._Conformance;
            clone._ContainedTemplateId = this._ContainedTemplateId;
            clone._Context = this._Context;
            clone._DataType = this._DataType;
            clone._Description = this._Description;
            clone._DisplayName = this._DisplayName;
            clone._Id = this._Id;
            clone._IsBranch = this._IsBranch;
            clone._IsBranchIdentifier = this._IsBranchIdentifier;
            clone._IsInheritable = this._IsInheritable;
            clone._IsPrimitive = this._IsPrimitive;
            clone._IsSchRooted = this._IsSchRooted;
            clone._IsStatic = this._IsStatic;
            clone._Label = this._Label;
            clone._Notes = this._Notes;
            clone._Order = this._Order;
            clone._ParentConstraintId = this._ParentConstraintId;
            clone._PrimitiveText = this._PrimitiveText;
            clone._Schematron = this._Schematron;
            clone._TemplateId = this._TemplateId;
            clone._Value = this._Value;
            clone._ValueConformance = this._ValueConformance;
            clone._ValueSetDate = this._ValueSetDate;
            clone._ValueSetId = this._ValueSetId;
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
