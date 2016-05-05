using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using System.Globalization;

namespace Trifolia.Logging
{
    /// <summary>
    /// Logging component which wraps log4net.
    /// </summary>
    internal sealed class Log4NetLogger : ILogger
    {
        #region Member Variables
        private static readonly Type ThisDeclaringType = typeof(Log4NetLogger);
        private log4net.Core.ILogger _logger;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Log4NetLogger class.
        /// </summary>
        public Log4NetLogger(log4net.Core.ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        public void LogSql(object query)
        {
            if (!(query is System.Data.Objects.ObjectQuery))
                return;

            var sql = ((System.Data.Objects.ObjectQuery)query).ToTraceString();
            Trace(sql);
        }

        public void Error(string message, Exception exception)
        {
            WriteToLog(Level.Error, message, exception);
        }

        public void Warn(string message, Exception exception)
        {
            WriteToLog(Level.Warn, message, exception);
        }

        public void Critical(string message, Exception exception)
        {
            WriteToLog(Level.Critical, message, exception);
        }

        public void Warn(string message)
        {
            WriteToLog(Level.Warn, message, null);
        }

        public void Critical(string message)
        {
            WriteToLog(Level.Critical, message, null);
        }

        public void Debug(string message)
        {
            WriteToLog(Level.Debug, message, null);
        }

        public void Error(string message)
        {
            WriteToLog(Level.Error, message, null);
        }

        public void Info(string message)
        {
            WriteToLog(Level.Info, message, null);
        }

        public void Info(string message, Exception exception)
        {
            WriteToLog(Level.Info, message, exception);
        }

        public void Debug(string message, params object[] messageArguments)
        {
            WriteToLog(Level.Debug, string.Format(message, messageArguments), null);
        }

        public void Trace(string message)
        {
            WriteToLog(Level.Verbose, message, null);
        }

        public void Trace(string message, Exception exception)
        {
            WriteToLog(Level.Verbose, message, exception);
        }

        private void WriteToLog(Level level, string message, Exception exception)
        {
            try
            {
                if (level == Level.Critical)
                    log4net.ThreadContext.Properties["EventID"] = 1;
                else if (level == Level.Error)
                    log4net.ThreadContext.Properties["EventID"] = 2;
                else if (level == Level.Warn)
                    log4net.ThreadContext.Properties["EventID"] = 3;
                else if (level == Level.Trace)
                    log4net.ThreadContext.Properties["EventID"] = 4;
                else if (level == Level.Info)
                    log4net.ThreadContext.Properties["EventID"] = 5;

                _logger.Log(ThisDeclaringType, level, message, exception);
            }
            catch { }
        }


        public void Trace(string message, params object[] messageArguments)
        {
            WriteToLog(Level.Trace, string.Format(CultureInfo.InvariantCulture, message, messageArguments), null);
        }

        public void Critical(string message, params object[] messageArguments)
        {
            WriteToLog(Level.Critical, string.Format(CultureInfo.InvariantCulture, message, messageArguments), null);
        }

        public void Warn(string message, params object[] messageArguments)
        {
            WriteToLog(Level.Warn, string.Format(CultureInfo.InvariantCulture, message, messageArguments), null);
        }

        public void Error(string message, params object[] messageArguments)
        {
            WriteToLog(Level.Error, string.Format(CultureInfo.InvariantCulture, message, messageArguments), null);
        }

        public void Info(string message, params object[] messageArguments)
        {
            WriteToLog(Level.Info, string.Format(CultureInfo.InvariantCulture, message, messageArguments), null);
        }

        public void Trace(string message, Exception exception, params object[] messageArguments)
        {
            WriteToLog(Level.Trace, string.Format(CultureInfo.InvariantCulture, message, messageArguments), exception);
        }

        public void Critical(string message, Exception exception, params object[] messageArguments)
        {
            WriteToLog(Level.Critical, string.Format(CultureInfo.InvariantCulture, message, messageArguments), exception);
        }

        public void Warn(string message, Exception exception, params object[] messageArguments)
        {
            WriteToLog(Level.Warn, string.Format(CultureInfo.InvariantCulture, message, messageArguments), exception);
        }

        public void Error(string message, Exception exception, params object[] messageArguments)
        {
            WriteToLog(Level.Error, string.Format(CultureInfo.InvariantCulture, message, messageArguments), exception);
        }

        public void Info(string message, Exception exception, params object[] messageArguments)
        {
            WriteToLog(Level.Info, string.Format(CultureInfo.InvariantCulture, message, messageArguments), exception);
        }
    }
}
