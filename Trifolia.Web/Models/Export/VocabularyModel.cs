using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Web.Models.Export
{
    public class VocabularyModel
    {
        public VocabularyModel()
        {
            this.ValueSets = new List<ImplementationGuideValueSet>();
        }

        public string Name { get; set; }
        public int ImplementationGuideId { get; set; }
        public List<ImplementationGuideValueSet> ValueSets { get; set; }
        public string CancelUrl { get; set; }
    }
}