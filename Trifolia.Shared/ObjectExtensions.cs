using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Shared
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Allows wrapping of an object whose usage will traverse multiple lines into a single "with" clause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="work"></param>
        public static void Use<T>(this T item, Action<T> work)
        {
            work(item);
        }
    }
}