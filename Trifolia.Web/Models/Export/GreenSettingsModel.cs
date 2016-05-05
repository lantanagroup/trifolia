using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.Export
{
    public class GreenSettingsModel
    {
        public int ImplementationGuideId { get; set; }
        public int RootTemplateId { get; set; }
        public bool SeparateDataTypes { get; set; }
    }
}