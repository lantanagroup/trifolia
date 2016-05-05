using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class FilesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<FileModel> Files { get; set; }
    }
}