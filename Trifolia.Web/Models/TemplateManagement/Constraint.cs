using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using Trifolia.DB;
using Trifolia.Export.MSWord.ConstraintGeneration;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class Constraint
    {
        #region Private Fields

        private ObservableCollection<Constraint> _childConstraints = null;

        #endregion

        #region Ctor

        public Constraint()
        {
            this.ChildConstraints = new ObservableCollection<Constraint>();
        }

        public Constraint(TemplateConstraint dbConstraint)
            : this()
        {
            this.ConstraintDescription = dbConstraint.Description;
            this.ConstraintLabel = dbConstraint.Label;
            this.HeadingDescription = dbConstraint.HeadingDescription;
            this.Id = dbConstraint.Id;
            this.IsHeading = dbConstraint.IsHeading;
            this.IsPrimitive = dbConstraint.IsPrimitive;
            this.PrimitiveText = dbConstraint.PrimitiveText;
            this.TemplateId = dbConstraint.TemplateId;
            this.Context = dbConstraint.Context;
            this.Conformance = dbConstraint.Conformance;
            this.Cardinality = dbConstraint.Cardinality;
            this.Value = dbConstraint.Value;
        }

        #endregion

        #region Properties

        public int Id { get; set; }
        public string Context { get; set; }
        public int? TemplateId { get; set; }
        public string Conformance { get; set; }
        public string Cardinality { get; set; }
        public string HeadingDescription { get; set; }
        public string ConstraintDescription { get; set; }
        public string ConstraintLabel { get; set; }
        public string PrimitiveText { get; set; }
        public bool IsPrimitive { get; set; }
        public bool IsHeading { get; set; }
        public int? ParentConstraintId { get; set; }
        public string Value { get; set; }

        public ObservableCollection<Constraint> ChildConstraints
        {
            get
            {
                return _childConstraints;
            }
            set
            {
                _childConstraints = value;

                if (_childConstraints != null)
                {
                    _childConstraints.CollectionChanged += (sender, e) =>
                        {
                            if (e.Action == NotifyCollectionChangedAction.Add)
                            {
                                foreach (Constraint lAddedConstraint in e.NewItems)
                                {
                                    lAddedConstraint.ParentConstraintId = this.Id;
                                }
                            }
                            else if (e.Action == NotifyCollectionChangedAction.Remove)
                            {
                                foreach (Constraint lRemovedConstraint in e.OldItems)
                                {
                                    if (lRemovedConstraint.ParentConstraintId.HasValue && lRemovedConstraint.ParentConstraintId.Value == this.Id) lRemovedConstraint.ParentConstraintId = null;
                                }
                            }
                        };
                }
            }
        }

        #endregion
    }
}