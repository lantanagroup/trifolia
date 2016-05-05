using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.Web.Models.TemplateManagement;

namespace Trifolia.Web.Models.IGManagement
{
    public class ViewChangesModel
    {
        #region Ctor

        public ViewChangesModel()
        {
            this.TemplateDifferences = new List<DifferenceModel>();
        }

        #endregion

        #region Properties

        public string IgName { get; set; }

        public List<DifferenceModel> TemplateDifferences { get; set; }

        #endregion
    }
}