using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Shared
{
    public class TemplateTypeComboModel
    {
        public TemplateTypeComboModel()
        {
            this.TemplateTypes = new List<LookupTemplateType>();
        }

        public int? SelectedTemplateTypeId { get; set; }
        public List<LookupTemplateType> TemplateTypes { get; set; }
    }
}