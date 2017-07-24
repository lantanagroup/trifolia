using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Web.Models
{
    public class ImportValueSetModel
    {
        public ValueSetImportSources Source { get; set; }
        public string Id { get; set; }
    }
}