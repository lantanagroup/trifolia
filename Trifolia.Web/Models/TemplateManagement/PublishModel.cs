using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class PublishModel
    {
        #region Private Fields

        ObservableCollection<XmlSample> _xmlSamples;
        ObservableCollection<PublishConstraint> _constraints;

        #endregion

        #region Ctor

        public PublishModel()
        {
            _xmlSamples = new ObservableCollection<XmlSample>();
            _constraints = new ObservableCollection<PublishConstraint>();
        }

        #endregion

        #region Properties

        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateOid { get; set; }

        public ObservableCollection<XmlSample> XmlSamples
        {
            get
            {
                return _xmlSamples;
            }
            set
            {
                _xmlSamples = value;

                if (_xmlSamples != null)
                {
                    _xmlSamples.CollectionChanged += (sender, e) =>
                        {
                            if (e.Action == NotifyCollectionChangedAction.Add)
                            {
                                foreach (XmlSample lSample in e.NewItems)
                                {
                                    lSample.TemplateId = this.TemplateId;
                                }
                            }
                            else if (e.Action == NotifyCollectionChangedAction.Remove)
                            {
                                foreach (XmlSample lSample in e.OldItems)
                                {
                                    if (lSample.TemplateId.HasValue && lSample.TemplateId.Value == this.TemplateId) lSample.TemplateId = null;
                                }
                            }
                        };
                }
            }
        }

        public ObservableCollection<PublishConstraint> Constraints
        {
            get
            {
                return _constraints;
            }
            set
            {
                _constraints = value;

                if (_constraints != null)
                {
                    _constraints.CollectionChanged += (sender, e) =>
                        {
                            if (e.Action == NotifyCollectionChangedAction.Add)
                            {
                                foreach (Constraint lConstraint in e.NewItems)
                                {
                                    lConstraint.TemplateId = this.TemplateId;
                                }
                            }
                            else if (e.Action == NotifyCollectionChangedAction.Remove)
                            {
                                foreach (Constraint lConstraint in e.OldItems)
                                {
                                    if (lConstraint.TemplateId.HasValue && lConstraint.TemplateId.Value == this.TemplateId) lConstraint.TemplateId = null;
                                }
                            }
                        };
                }
            }
        }

        #endregion
    }
}