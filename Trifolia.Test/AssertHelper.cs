using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Trifolia.Test
{
    public class AssertHelper
    {
        /// <summary>
        /// Asserts that the object is an instance of the type T
        /// </summary>
        /// <typeparam name="T">The type to check and return</typeparam>
        /// <param name="obj">The object to check</param>
        /// <param name="message">The message to throw an assertion error on if it is not of the type</param>
        /// <returns>The object as type T</returns>
        public static T IsType<T>(object obj, string message = null)
        {
            Assert.IsInstanceOfType(obj, typeof(T), message);
            return (T)obj;
        }
    }
}
