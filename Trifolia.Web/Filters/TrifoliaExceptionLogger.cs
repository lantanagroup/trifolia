using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;

namespace Trifolia.Web.Filters
{
    public class TrifoliaExceptionLogger : IExceptionLogger
    {
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            object forObject = context.ExceptionContext.ControllerContext;

            if (forObject == null)
                forObject = this;

            Trifolia.Logging.Log.For(forObject)
                .Error(context.ExceptionContext.Exception.Message, context.ExceptionContext.Exception);

            return Task.FromResult(0);
        }
    }
}