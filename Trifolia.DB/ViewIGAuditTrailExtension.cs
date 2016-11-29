using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class ViewIGAuditTrail
    {
        public static IQueryable<ViewIGAuditTrail> GetImplementationGuideAuditEntries(int implementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.ViewIGAuditTrails.Where(y => y.ImplementationGuideId == implementationGuideId);
            }
        }
    }
}
