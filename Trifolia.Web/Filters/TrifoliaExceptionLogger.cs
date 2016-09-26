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
            Trifolia.Logging.Log.For(context.ExceptionContext.ActionContext.ControllerContext.Controller)
                .Error(context.ExceptionContext.Exception.Message, context.ExceptionContext.Exception);

            return Task.FromResult(0);
        }
    }
}