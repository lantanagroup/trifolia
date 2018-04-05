using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using Trifolia.DB;
using Trifolia.Export.MSWord.ConstraintGeneration;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class PublishConstraint
    {
        public PublishConstraint()
        {

        }

        public PublishConstraint(TemplateConstraint constraint, IFormattedConstraint fc)
        {
            this.ChildConstraints = new ObservableCollection<PublishConstraint>();
            this.Samples = new ObservableCollection<ConstraintSample>();
            this.DisplayText = fc.GetPlainText();

            this.ConstraintDescription = constraint.Description;
            this.ConstraintLabel = constraint.Label;
            this.HeadingDescription = constraint.HeadingDescription;
            this.Id = constraint.Id;
            this.IsHeading = constraint.IsHeading;
            this.IsPrimitive = constraint.IsPrimitive;
            this.PrimitiveText = constraint.PrimitiveText;
            this.TemplateId = constraint.TemplateId;
            this.Context = constraint.Context;
            this.Conformance = constraint.Conformance;
            this.Cardinality = constraint.Cardinality;
            this.Value = constraint.Value;
        }

        private ObservableCollection<PublishConstraint> _childConstraints = null;
        private ObservableCollection<ConstraintSample> _samples = new ObservableCollection<ConstraintSample>();

        public string DisplayText { get; set; }
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

        public ObservableCollection<PublishConstraint> ChildConstraints
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
                            foreach (PublishConstraint lAddedConstraint in e.NewItems)
                            {
                                lAddedConstraint.ParentConstraintId = this.Id;
                            }
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Remove)
                        {
                            foreach (PublishConstraint lRemovedConstraint in e.OldItems)
                            {
                                if (lRemovedConstraint.ParentConstraintId.HasValue && lRemovedConstraint.ParentConstraintId.Value == this.Id) lRemovedConstraint.ParentConstraintId = null;
                            }
                        }
                    };
                }
            }
        }

        public ObservableCollection<ConstraintSample> Samples
        {
            get
            {
                return _samples;
            }
            set
            {
                _samples = value;

                if (_samples != null)
                {
                    _samples.CollectionChanged += (sender, e) =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            foreach (ConstraintSample lSample in e.NewItems)
                            {
                                lSample.ConstraintId = this.Id;
                            }
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Remove)
                        {
                            foreach (ConstraintSample lSample in e.OldItems)
                            {
                                if (lSample.ConstraintId.HasValue && lSample.ConstraintId.Value == this.Id) lSample.ConstraintId = null;
                            }
                        }
                    };
                }
            }
        }
    }
}