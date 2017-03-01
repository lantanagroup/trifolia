using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Authorization
{
    public static class AuditEntryExtension
    {
        public static void SetContext(this AuditEntry auditEntry, string userName)
        {
            auditEntry.Username = userName;
            auditEntry.IP = CheckPoint.Instance.HostAddress;
            auditEntry.AuditDate = DateTime.Now;
        }

        public static AuditEntry CreateAuditEntry(IObjectRepository tdb, string type, string note, string userName)
        {
            AuditEntry newAuditEntry = new AuditEntry();
            newAuditEntry.SetContext(userName);
            newAuditEntry.Type = type;
            newAuditEntry.Note = note;
            tdb.AuditEntries.Add(newAuditEntry);
            return newAuditEntry;
        }

        public static void SaveAuditEntry(string type, string note, string userName)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                CreateAuditEntry(tdb, type, note, userName);
                tdb.SaveChanges();
            }
        }
    }
}
