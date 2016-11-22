using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public enum ImplementationGuideFileTypes
    {
        Unknown,
        ImplementationGuide,
        Schematron,
        SchematronHelper,
        Vocabulary,
        DeliverableSample,
        GoodSample,
        BadSample,
        FHIRResourceInstance
    }
}
