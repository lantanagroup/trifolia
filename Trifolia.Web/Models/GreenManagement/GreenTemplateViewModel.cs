using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.GreenManagement
{
    public class GreenTemplateViewModel
    {
        #region Private Fields

        ObservableCollection<ConstraintViewModel> _templateConstraints;

        #endregion

        #region Ctor

        public GreenTemplateViewModel()
        {
            this.childConstraints = new ObservableCollection<ConstraintViewModel>();
        }

        #endregion

        #region Properties

        public int? Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string TemplateName { get; set; }
        public string TemplateOid { get; set; }
        public bool IsNew { get; set; }

        public ObservableCollection<ConstraintViewModel> childConstraints
        {
            get { return _templateConstraints; }
            set
            {
                _templateConstraints = value;

                if (_templateConstraints != null)
                {
                    _templateConstraints.CollectionChanged += (sender, e) =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            foreach (ConstraintViewModel lConstraint in e.NewItems)
                            {
                                lConstraint.templateId = this.Id;
                            }
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Remove)
                        {
                            foreach (ConstraintViewModel lConstraint in e.OldItems)
                            {
                                if (lConstraint.templateId.HasValue && lConstraint.templateId.Value == this.Id) lConstraint.templateId = null;
                            }
                        }
                    };
                }
            }
        }

        #endregion
    }
}