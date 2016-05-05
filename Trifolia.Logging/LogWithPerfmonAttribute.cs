using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Logging
{
    /// <summary>
    /// Represents ...
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class LogWithPerfmonAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the CommonLayer.Logging.LogWithPerfmonAttribute class.
        /// </summary>
        public LogWithPerfmonAttribute(string categoryName, string counterName, string baseCounterName)
        {
            CategoryName = categoryName;
            CounterName = counterName;
            BaseCounterName = baseCounterName;
        }

        public string CategoryName
        {
            get;
            private set;
        }

        public string CounterName
        {
            get;
            private set;
        }

        public string BaseCounterName
        {
            get;
            private set;
        }
    }
}
