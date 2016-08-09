using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public static class ObjectSetExtensions
    {
        public static IEnumerable<T> WhereInclAdded<T>(this IObjectSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            ObjectSet<T> set = iSet as ObjectSet<T>;

            if (set != null)
            {
                var dbResult = iSet.Where(predicate);
                var offlineResult = set.Context.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(entry => entry.Entity).OfType<T>().Where(predicate.Compile());

                return offlineResult.Union(dbResult);
            }

            return iSet.Where(predicate.Compile());
        }

        public static T SingleInclAdded<T>(this IObjectSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            ObjectSet<T> set = iSet as ObjectSet<T>;

            if (set != null)
            {
                var offlineResult = set.Context.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(entry => entry.Entity).OfType<T>();
                var allResults = offlineResult.Union(iSet);

                return allResults.Single(predicate.Compile());
            }

            return iSet.Single(predicate.Compile());
        }

        public static T SingleOrDefaultInclAdded<T>(this IObjectSet<T> iSet, Expression<Func<T, bool>> predicate) where T : class
        {
            ObjectSet<T> set = iSet as ObjectSet<T>;

            if (set != null)
            {
                var offlineResult = set.Context.ObjectStateManager.GetObjectStateEntries(EntityState.Added).Select(entry => entry.Entity).OfType<T>();
                var allResults = offlineResult.Union(iSet);

                return allResults.SingleOrDefault(predicate.Compile());
            }

            return iSet.SingleOrDefault(predicate.Compile());
        }
    }
}
