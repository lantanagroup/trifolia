using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Shared;

namespace Trifolia.Web.Models.Shared
{
    public class ImplementationGuideComboModel
    {
        private List<LookupImplementationGuide> implementationGuides;

        public List<LookupImplementationGuide> ImplementationGuides
        {
            get { return implementationGuides; }
            set { implementationGuides = value; }
        }
        private int? selectedImplementationGuideId;

        public int? SelectedImplementationGuideId
        {
            get { return selectedImplementationGuideId; }
            set { selectedImplementationGuideId = value; }
        }
    }
}