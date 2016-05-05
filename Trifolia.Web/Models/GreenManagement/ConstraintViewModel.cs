using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.GreenManagement
{
    public class ConstraintViewModel
    {
        #region Private Fields

        private ObservableCollection<ConstraintViewModel> _childConstraints = null;

        #endregion

        #region Ctor

        public ConstraintViewModel()
        {
            this.children = new ObservableCollection<ConstraintViewModel>();
        }

        #endregion

        #region Properties

        public int id { get; set; }
        public int? templateId { get; set; }
        public string text { get; set; }
        public string headingDescription { get; set; }
        public string constraintDescription { get; set; }
        public string constraintLabel { get; set; }
        public string primitiveText { get; set; }
        public string datatype { get; set; }
        public int? datatypeId { get; set; }
        public bool isPrimitive { get; set; }
        public bool isHeading { get; set; }
        public int? parentConstraintId { get; set; }
        public int order { get; set; }
        public int? number { get; set; }
        public string elementName { get; set; }
        public string businessName { get; set; }
        public int? greenConstraintId { get; set; }
        public int? parentGreenConstraintId { get; set; }
        public bool hasGreenConstraint { get; set; }
        public bool isDeleted { get; set; }
        public string xPath { get; set; }
        public string conformance { get; set; }
        public string value { get; set; }

        public ObservableCollection<ConstraintViewModel> children
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
                            foreach (ConstraintViewModel lAddedConstraint in e.NewItems)
                            {
                                lAddedConstraint.parentConstraintId = this.id;
                            }
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Remove)
                        {
                            foreach (ConstraintViewModel lRemovedConstraint in e.OldItems)
                            {
                                if (lRemovedConstraint.parentConstraintId.HasValue && lRemovedConstraint.parentConstraintId.Value == this.id) lRemovedConstraint.parentConstraintId = null;
                            }
                        }
                    };
                }
            }
        }

        #endregion
    }
}