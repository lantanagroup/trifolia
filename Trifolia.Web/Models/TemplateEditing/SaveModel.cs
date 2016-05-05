using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateEditing
{
    public class SaveModel
    {
        public TemplateMetaDataModel Template { get; set; }
        public List<ConstraintModel> RemovedConstraints { get; set; }
        public List<ConstraintModel> Constraints { get; set; }
    }
}