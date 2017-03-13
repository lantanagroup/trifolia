using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Trifolia.Logging
{
    /// <summary>
    /// Represents static gateway for logging services.
    /// </summary>
    public static class Log
    {
        #region Methods

        /// <summary>
        /// Returns the logger for the object provided.
        /// </summary>
        /// <param name="itemThatRequiresLoggingServices">Object which requires logging services.</param>
        /// <returns>The logger for the object provided.</returns>
        public static ILogger For(object itemThatRequiresLoggingServices)
        {
            if (itemThatRequiresLoggingServices == null)
                throw new ArgumentNullException("itemThatRequiresLoggingServices");

            return For(itemThatRequiresLoggingServices.GetType());
        }

        readonly static object locker = new object();
        static ILoggerFactory _factory;
        public static ILoggerFactory Factory
        {
            get 
            { 
                lock (locker) 
                {
                    return _factory = _factory ?? new Log4NetLoggerFactory(); 
                } 
            }
            set
            {
                lock (locker)
                {
                    _factory = value;
                }
            }
        }
        /// <summary>
        /// Returns the logger for the type provided.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The logger for the type provided.</returns>
        public static ILogger For(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            //ILoggerFactory factory = new Log4NetLoggerFactory();

            var factory = Factory.CreateFor(type);

            if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null && !string.IsNullOrEmpty(Thread.CurrentPrincipal.Identity.Name))
                log4net.GlobalContext.Properties["log4net:UserName"] = Thread.CurrentPrincipal.Identity.Name;

            return factory;
        }
        #endregion
    }
}
