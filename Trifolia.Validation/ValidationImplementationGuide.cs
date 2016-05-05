using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.ValidationService
{
    [Serializable]
    public class ValidationImplementationGuide
    {
        public int ImplementationGuideId { get; set; }
        public string Name { get; set; }
    }
}