using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public static class ObjectSetExtensions
    {
        public static DbContext GetContext<TEntity>(this IDbSet<TEntity> dbSet)
            where TEntity : class
        {
            object internalSet = dbSet
                .GetType()
                .GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(dbSet);
            object internalContext = internalSet
                .GetType()
                .BaseType
                .GetField("_internalContext", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(internalSet);
            return (DbContext)internalContext
                .GetType()
                .GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public)
                .GetValue(internalContext, null);
        }

        public static IEnumerable<T> WhereInclAdded<T>(this IDbSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            return iSet.GetContext().ChangeTracker.Entries<T>()
                .Where(y => y.State != EntityState.Deleted)
                .Select(y => y.Entity)
                .Where(predicate.Compile());
        }

        public static T SingleInclAdded<T>(this IDbSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            return iSet.GetContext().ChangeTracker.Entries<T>()
                .Where(y => y.State != EntityState.Deleted)
                .Select(y => y.Entity)
                .Single(predicate.Compile());
        }

        public static T SingleOrDefaultInclAdded<T>(this IDbSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            return iSet.GetContext().ChangeTracker.Entries<T>()
                .Where(y => y.State != EntityState.Deleted)
                .Select(y => y.Entity)
                .SingleOrDefault(predicate.Compile());
        }
    }
}
