using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Filters;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;

namespace Trifolia.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SecurableAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        #region Ctor

        public SecurableAttribute(params string[] aSecurableNames)
        {
            this.SecurableNames = aSecurableNames;
        }

        #endregion

        #region Properties

        public string[] SecurableNames { get; private set; }

        #endregion
    }
}