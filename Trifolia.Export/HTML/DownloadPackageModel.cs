using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Export.HTML
{
    public class DownloadPackageModel
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
    }
}