using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Plugins
{
    public class SampleGenerationException : Exception
    {
        public SampleGenerationException(string message)
            : base(message)
        {

        }
    }
}
