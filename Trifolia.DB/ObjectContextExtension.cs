using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;

namespace Trifolia.DB
{
    public partial class TrifoliaDatabase : IObjectRepository
    {
        private string auditUserName = null;
        private string auditIP = null;
        

        #region State Functionality

        public void ChangeObjectState(Object entity, EntityState entityState)
        {
            this.Entry(entity).State = entityState;
        }

        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.SearchTemplates(Nullable<global::System.Int32> userId, Nullable<global::System.Int32> filterImplementationGuideId, global::System.String filterName, global::System.String filterIdentifier, Nullable<global::System.Int32> filterTemplateTypeId, Nullable<global::System.Int32> filterOrganizationId, global::System.String filterContextType, global::System.String queryText)
        {
            return this.SearchTemplates(userId, filterImplementationGuideId, filterName, filterIdentifier, filterTemplateTypeId, filterOrganizationId, filterContextType, queryText);
        }

        #endregion

        #region Transactions

        public System.Data.IDbTransaction BeginTransaction()
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }
            return Connection.BeginTransaction();
        }

        public System.Data.IDbTransaction BeginTransaction
        (System.Data.IsolationLevel isolationLevel)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }
            return Connection.BeginTransaction(isolationLevel);
        }

        #endregion

        public override int SaveChanges()
        {
            int num = base.SaveChanges();

            if (this.AddAudits())
                base.SaveChanges();

            return num;
        }

        #region Auditing

        public void AuditChanges(string auditUserName, string auditIP)
        {
            if (!string.IsNullOrEmpty(auditUserName) && !string.IsNullOrEmpty(auditIP))
            {
                this.auditUserName = auditUserName;
                this.auditIP = auditIP;
            }
        }

        private bool AddAudits()
        {
            bool savedAudits = false;
            var changedEntities = this.ChangeTracker.Entries().Where(y =>
                y.State == EntityState.Added ||
                y.State == EntityState.Modified ||
                y.State == EntityState.Deleted)
                .ToList();

            foreach (var changedEntity in changedEntities)
            {
                if (!AuditEntry.IsAuditable(changedEntity.Entity))
                    continue;

                this.AuditEntries.Add(new AuditEntry(changedEntity.Entity, changedEntity.CurrentValues, changedEntity.OriginalValues));

                savedAudits = true;
            }

            return savedAudits;
        }

        #endregion
    }
}
