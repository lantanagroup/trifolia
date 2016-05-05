using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Shared
{
    public class TemplateComboModel
    {
        private List<LookupTemplate> templates;

        public List<LookupTemplate> Templates
        {
            get { return templates; }
            set { templates = value; }
        }
        private int? selectedTemplateId;

        public int? SelectedTemplateId
        {
            get { return selectedTemplateId; }
            set { selectedTemplateId = value; }
        }
    }
}