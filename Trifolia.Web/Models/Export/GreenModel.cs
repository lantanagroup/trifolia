using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Export
{
    public class GreenModel
    {
        public GreenModel()
        {
            this.Templates = new List<LookupTemplate>();
        }

        public string Name { get; set; }
        public int ImplementationGuideId { get; set; }
        public List<LookupTemplate> Templates { get; set; }
    }
}