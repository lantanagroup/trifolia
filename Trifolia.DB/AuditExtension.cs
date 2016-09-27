using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class AuditEntry
    {
        public static List<AuditEntry> GetAllEntries()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return tdb.AuditEntries.ToList();
            }
        }

        public static List<AuditEntry> GetImplementationGuideEntries(int implementationGuideId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                List<AuditEntry> igEntries = tdb.AuditEntries.Where(y => y.ImplementationGuideId == implementationGuideId).ToList();

                // Get the template audit entries for the implementation guide
                List<AuditEntry> relatedEntries = (from t in tdb.Templates 
                                                   join a in tdb.AuditEntries on t.Id equals a.TemplateId
                                                   where t.OwningImplementationGuideId == implementationGuideId
                                                   select a).ToList();

                return igEntries.Union(relatedEntries).ToList();
            }
        }
    }
}
