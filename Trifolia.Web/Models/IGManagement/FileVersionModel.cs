using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class FileVersionModel
    {
        public int FileId { get; set; }
        public int VersionId { get; set; }
        public string Date { get; set; }
        public string Note { get; set; }
    }
}