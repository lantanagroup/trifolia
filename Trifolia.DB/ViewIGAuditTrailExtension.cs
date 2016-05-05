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
            using (TemplateDatabaseDataSource tdb = new TemplateDatabaseDataSource())
            {
                return tdb.ViewIGAuditTrails.Where(y => y.ImplementationGuideId == implementationGuideId);
            }
        }
    }
}
