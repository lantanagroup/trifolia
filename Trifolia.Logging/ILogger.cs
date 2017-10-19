using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Trifolia.Logging
{
    /// <summary>
    /// Represents a component which provides logging services.
    /// </summary>
    public interface ILogger
    {
        #region Methods

        void Debug(string message);

        void Debug(string message, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Trace.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        void Trace(string message);

        /// <summary>
        /// Writes a log entry with a level of Trace.
        /// </summary>
        /// <param name="message">Message format string to use when writing to the log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Trace(string message, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Trace.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        void Trace(string message, Exception exception);

        /// <summary>
        /// Writes a log entry with a level of Trace.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Trace(string message, Exception exception, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Critical.
        /// </summary>
        /// <param name="message">Message format string to use when writing to the log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Critical(string message, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Critical.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        void Critical(string message);

        /// <summary>
        /// Writes a log entry with a level of Critical.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        void Critical(string message, Exception exception);

        /// <summary>
        /// Writes a log entry with a level of Critical.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Critical(string message, Exception exception, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Warning.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        void Warn(string message);

        /// <summary>
        /// Writes a log entry with a level of Warning.
        /// </summary>
        /// <param name="message">Message format string to use when writing to the log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Warn(string message, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Warning.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        void Warn(string message, Exception exception);

        /// <summary>
        /// Writes a log entry with a level of Warning.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Warn(string message, Exception exception, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Error.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error")]
        void Error(string message);

        /// <summary>
        /// Writes a log entry with a level of Error.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error")]
        void Error(string message, Exception exception);

        /// <summary>
        /// Writes a log entry with a level of Error.
        /// </summary>
        /// <param name="message">Message format string to use when writing to the log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Error(string message, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Error.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Error(string message, Exception exception, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Informational.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        void Info(string message);

        /// <summary>
        /// Writes a log entry with a level of Informational.
        /// </summary>
        /// <param name="message">Message format string to use when writing to the log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Info(string message, params object[] messageArguments);

        /// <summary>
        /// Writes a log entry with a level of Informational.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        void Info(string message, Exception exception);

        /// <summary>
        /// Writes a log entry with a level of Informational.
        /// </summary>
        /// <param name="message">Message to write to the log.</param>
        /// <param name="exception">System.Exception to log.</param>
        /// <param name="messageArguments">Format string arguments.</param>
        void Info(string message, Exception exception, params object[] messageArguments);
        #endregion
    }
}
