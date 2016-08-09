using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Authorization
{
    public static class AuditEntryExtension
    {
        public static void SetContext(this AuditEntry auditEntry, string userName = null, string organizationName = null)
        {
            auditEntry.Username = string.Format("{0} ({1})",
                userName != null ? userName : CheckPoint.Instance.UserName,
                organizationName != null ? organizationName : CheckPoint.Instance.OrganizationName);
            auditEntry.IP = CheckPoint.Instance.HostAddress;
            auditEntry.AuditDate = DateTime.Now;
        }

        public static AuditEntry CreateAuditEntry(IObjectRepository tdb, string type, string note, string userName = null, string organizationName = null)
        {
            AuditEntry newAuditEntry = new AuditEntry();
            newAuditEntry.SetContext(userName, organizationName);
            newAuditEntry.Type = type;
            newAuditEntry.Note = note;
            tdb.AuditEntries.AddObject(newAuditEntry);
            return newAuditEntry;
        }

        public static void SaveAuditEntry(string type, string note, string userName = null, string organizationName = null)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                CreateAuditEntry(tdb, type, note, userName, organizationName);
                tdb.SaveChanges();
            }
        }
    }
}
