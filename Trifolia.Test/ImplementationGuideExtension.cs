using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public static class ImplementationGuideExtension
    {
        public static void SetPreviousVersion(this ImplementationGuide ig, ImplementationGuide previousVersion)
        {
            ig.PreviousVersion = previousVersion;
            ig.PreviousVersionImplementationGuideId = previousVersion.Id;
        }
    }
}
