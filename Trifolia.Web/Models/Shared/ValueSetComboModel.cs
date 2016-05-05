using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Shared
{
    public class ValueSetComboModel
    {
        private int? selectedValueSetId;

        public int? SelectedValueSetId
        {
            get { return selectedValueSetId; }
            set { selectedValueSetId = value; }
        }
        private List<LookupValueSet> valueSets;

        public List<LookupValueSet> ValueSets
        {
            get { return valueSets; }
            set { valueSets = value; }
        }
    }
}