using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Authorization
{
    /// <summary>
    /// The exception that is thrown when a user is not authorized to access a securable
    /// </summary>
    public class AuthorizationException : Exception
    {
        #region Ctor

        public AuthorizationException(string aUserName, string[] aSecurableNames) 
            : base(string.Format("{0} is not authorized to access to {1}", aUserName, String.Join(", ", aSecurableNames)))
        {
        }

        public AuthorizationException(string message)
            : base(message)
        {
        }

        #endregion
    }
}