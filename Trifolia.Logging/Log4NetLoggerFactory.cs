using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Config;
using log4net.Core;

namespace Trifolia.Logging
{
    /// <summary>
    /// Represents ...
    /// </summary>
    //!!--[ServiceProvider(typeof(ILoggerFactory), Lifestyle = ContainerLifestyle.Singleton)]
    public sealed class Log4NetLoggerFactory : ILoggerFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Log4NetLogFactory class.
        /// </summary>
        public Log4NetLoggerFactory()
        {
            ConfigureLogging();
        }

        #endregion

        #region Methods
        private static void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        /// <summary>
        /// Creates the logger for the specified type.
        /// </summary>
        /// <param name="type">System.Type to create the logger for.</param>
        /// <returns>The logger for the specified type.</returns>
        public ILogger CreateFor(Type type)
        {
            log4net.Core.ILogger logger = LoggerManager.GetLogger(type.Assembly, type);

            return new Log4NetLogger(logger);
        }
        #endregion
    }
}
