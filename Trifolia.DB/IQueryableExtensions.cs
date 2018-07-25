using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Trifolia.DB
{
    public static class IQueryableExtensions
    {
        public static IQueryable<Template> IncludeDetails(
            this IQueryable<Template> templates, 
            bool includeImplied = false, 
            bool includeConstraints = false, 
            bool includeSamples = false,
            int? constraintDepth = 6)
        {
            var next = templates;

            if (includeImplied)
                next = next.Include("ImpliedTemplate");

            if (includeSamples)
                next = next.Include("TemplateSamples");

            if (includeConstraints)
            {
                string includeBase = "ChildConstraints";

                for (int i = 0; i < constraintDepth - 1; i++)
                {
                    next = next
                        .Include(includeBase)
                        .Include(includeBase + ".ValueSet")
                        .Include(includeBase + ".CodeSystem")
                        .Include(includeBase + ".References");

                    includeBase = includeBase + ".ChildConstraints";
                }
            }

            return next;
        }
    }
}
