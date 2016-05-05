using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public enum Conformance
    {
        SHALL = 1,
        SHALL_NOT = 2,
        SHOULD = 3,
        SHOULD_NOT = 4,
        MAY = 5,
        MAY_NOT = 6,
        UNKNOWN = 999
    }
}
