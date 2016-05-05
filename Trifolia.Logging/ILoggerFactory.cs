using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Logging
{
    /// <summary>
    /// Represents a factory that knows how to create a component that provides logging services.
    /// </summary>
    public interface ILoggerFactory
    {
        #region Methods

        /// <summary>
        /// Creates the logger for the specified type.
        /// </summary>
        /// <param name="type">System.Type to create the logger for.</param>
        /// <returns>The logger for the specified type.</returns>
        ILogger CreateFor(Type type);
        #endregion
    }
}
