using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Shared
{
    public class IGTypeComboModel
    {
        public int? SelectedImplementationGuideTypeId;
        public IEnumerable<LookupImplementationGuideType> ImplementationGuideTypes = new List<LookupImplementationGuideType>();
    }
}