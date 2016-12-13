using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.DB.Exceptions
{
    public class TrifoliaModelException : Exception
    {
        public TrifoliaModelException(string message)
            : base(message)
        {
        }

        public TrifoliaModelException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}