using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class CopyRequestModel
    {
        public int TemplateId { get; set; }
        public bool NewVersion { get; set; }
        public int? NewVersionImplementationGuideId { get; set; }
    }
}