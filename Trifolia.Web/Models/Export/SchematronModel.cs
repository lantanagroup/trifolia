using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Web.Models.Export
{
    public class SchematronModel
    {
        public SchematronModel()
        {
            this.Categories = new List<string>();
        }

        public string Name { get; set; }
        public int ImplementationGuideId { get; set; }
        public string CancelUrl { get; set; }
        public List<string> Categories { get; set; }
    }
}