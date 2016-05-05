using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class ImplementationGuideFileModel
    {
        public int FileId { get; set; }
        public int VersionId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}