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
            var internalSetProperty = dbSet
                .GetType()
                .GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance);

            if (internalSetProperty == null)
                return null;

            object internalSet = internalSetProperty.GetValue(dbSet);
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
            var context = iSet.GetContext();

            if (context != null)
                return iSet.GetContext().ChangeTracker.Entries<T>()
                    .Where(y => y.State != EntityState.Deleted)
                    .Select(y => y.Entity)
                    .Where(predicate.Compile());

            return iSet.Where(predicate.Compile());
        }

        public static T SingleInclAdded<T>(this IDbSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            var context = iSet.GetContext();

            if (context != null)
                return context.ChangeTracker.Entries<T>()
                    .Where(y => y.State != EntityState.Deleted)
                    .Select(y => y.Entity)
                    .Single(predicate.Compile());

            return iSet.Single(predicate.Compile());
        }

        public static T SingleOrDefaultInclAdded<T>(this IDbSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            var context = iSet.GetContext();

            if (context != null)
                return context.ChangeTracker.Entries<T>()
                    .Where(y => y.State != EntityState.Deleted)
                    .Select(y => y.Entity)
                    .SingleOrDefault(predicate.Compile());

            return iSet.SingleOrDefault(predicate.Compile());
        }
    }
}
