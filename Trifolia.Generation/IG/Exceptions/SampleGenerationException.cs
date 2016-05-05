using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Generation.IG.Exceptions
{
    public class SampleGenerationException : Exception
    {
        public SampleGenerationException(string message)
            : base(message)
        {

        }
    }
}
