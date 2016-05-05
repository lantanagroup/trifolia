using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Export
{
    public class MSWordModel
    {
        public MSWordModel()
        {
            this.Categories = new List<string>();
        }

        public string Name { get; set; }
        public int ImplementationGuideId { get; set; }
        public bool CanEdit { get; set; }
        public List<string> Categories { get; set; }
    }
}