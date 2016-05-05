using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Web.Models.Export
{
    public class VocabularyItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Oid { get; set; }
        public string BindingDate { get; set; }
    }
}