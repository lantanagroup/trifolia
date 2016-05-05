using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TerminologyManagement
{
    public class ImportCheckRequest
    {
        public byte[] Data { get; set; }
        public bool FirstRowIsHeader { get; set; }
    }
}