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
        private List<TempAuditEntry> auditRecords = new List<TempAuditEntry>();
        private string auditUserName = null;
        private string auditIP = null;

        #region IObjectRepository Implementation

        IObjectSet<ViewIGAuditTrail> IObjectRepository.ViewIGAuditTrails
        {
            get
            {
                return this.ViewIGAuditTrails;
            }
        }

        IObjectSet<AuditEntry> IObjectRepository.AuditEntries
        {
            get
            {
                return this.AuditEntries;
            }
        }

        IObjectSet<CodeSystem> IObjectRepository.CodeSystems
        {
            get
            {
                return this.CodeSystems;
            }
        }

        IObjectSet<GreenConstraint> IObjectRepository.GreenConstraints
        {
            get
            {
                return this.GreenConstraints;
            }
        }

        IObjectSet<GreenTemplate> IObjectRepository.GreenTemplates
        {
            get
            {
                return this.GreenTemplates;
            }
        }

        IObjectSet<ImplementationGuide> IObjectRepository.ImplementationGuides
        {
            get
            {
                return this.ImplementationGuides;
            }
        }

        IObjectSet<ImplementationGuideSetting> IObjectRepository.ImplementationGuideSettings
        {
            get
            {
                return this.ImplementationGuideSettings;
            }
        }

        IObjectSet<Template> IObjectRepository.Templates
        {
            get
            {
                return this.Templates;
            }
        }

        IObjectSet<TemplateConstraint> IObjectRepository.TemplateConstraints
        {
            get
            {
                return this.TemplateConstraints;
            }
        }

        IObjectSet<TemplateType> IObjectRepository.TemplateTypes
        {
            get
            {
                return this.TemplateTypes;
            }
        }

        IObjectSet<ValueSet> IObjectRepository.ValueSets
        {
            get
            {
                return this.ValueSets;
            }
        }

        IObjectSet<ValueSetMember> IObjectRepository.ValueSetMembers
        {
            get
            {
                return this.ValueSetMembers;
            }
        }

        IObjectSet<ImplementationGuideTemplateType> IObjectRepository.ImplementationGuideTemplateTypes
        {
            get
            {
                return this.ImplementationGuideTemplateTypes;
            }
        }

        IObjectSet<ImplementationGuideTypeDataType> IObjectRepository.ImplementationGuideTypeDataTypes
        {
            get
            {
                return this.ImplementationGuideTypeDataTypes;
            }
        }

        IObjectSet<ImplementationGuideType> IObjectRepository.ImplementationGuideTypes
        {
            get
            {
                return this.ImplementationGuideTypes;
            }
        }

        IObjectSet<ViewImplementationGuideFile> IObjectRepository.ViewImplementationGuideFiles
        {
            get { return this.ViewImplementationGuideFiles; }
        }

        IObjectSet<ImplementationGuideFile> IObjectRepository.ImplementationGuideFiles
        {
            get { return this.ImplementationGuideFiles; }
        }

        IObjectSet<ImplementationGuideFileData> IObjectRepository.ImplementationGuideFileDatas
        {
            get { return this.ImplementationGuideFileDatas; }
        }

        IObjectSet<Organization> IObjectRepository.Organizations
        {
            get { return this.Organizations; }
        }

        IObjectSet<ViewTemplate> IObjectRepository.ViewTemplates
        {
            get { return this.ViewTemplates; }
        }

        IObjectSet<ImplementationGuideSchematronPattern> IObjectRepository.ImplementationGuideSchematronPatterns
        {
            get { return this.ImplementationGuideSchematronPatterns; }
        }

        IObjectSet<PublishStatus> IObjectRepository.PublishStatuses
        {
            get { return this.PublishStatuses; }
        }

        IObjectSet<Role> IObjectRepository.Roles
        {
            get { return this.Roles; }
        }

        IObjectSet<AppSecurable> IObjectRepository.AppSecurables
        {
            get { return this.AppSecurables; }
        }

        IObjectSet<RoleAppSecurable> IObjectRepository.RoleAppSecurables
        {
            get { return this.RoleAppSecurables; }
        }

        IObjectSet<UserRole> IObjectRepository.UserRoles
        {
            get { return this.UserRoles; }
        }

        IObjectSet<User> IObjectRepository.Users
        {
            get { return this.Users; }
        }

        IObjectSet<ViewTemplateList> IObjectRepository.ViewTemplateLists
        {
            get { return this.ViewTemplateLists; }
        }

        IObjectSet<RoleRestriction> IObjectRepository.RoleRestrictions
        {
            get { return this.RoleRestrictions; }
        }

        IObjectSet<Group> IObjectRepository.Groups
        {
            get { return this.Groups; }
        }

        IObjectSet<UserGroup> IObjectRepository.UserGroups
        {
            get { return this.UserGroups; }
        }

        IObjectSet<ImplementationGuidePermission> IObjectRepository.ImplementationGuidePermissions
        {
            get { return this.ImplementationGuidePermissions; }
        }

        IObjectSet<ViewImplementationGuidePermission> IObjectRepository.ViewImplementationGuidePermissions
        {
            get { return this.ViewImplementationGuidePermissions; }
        }

        IObjectSet<ViewTemplatePermission> IObjectRepository.ViewTemplatePermissions
        {
            get { return this.ViewTemplatePermissions; }
        }

        IObjectSet<ViewUserSecurable> IObjectRepository.ViewUserSecurables
        {
            get { return this.ViewUserSecurables; }
        }

        IObjectSet<OrganizationDefaultPermission> IObjectRepository.OrganizationDefaultPermissions
        {
            get { return this.OrganizationDefaultPermissions; }
        }

        IObjectSet<ViewImplementationGuideTemplate> IObjectRepository.ViewImplementationGuideTemplates
        {
            get { return this.ViewImplementationGuideTemplates; }
        }

        IObjectSet<TemplateConstraintSample> IObjectRepository.TemplateConstraintSamples
        {
            get { return this.TemplateConstraintSamples; }
        }

        IObjectSet<TemplateSample> IObjectRepository.TemplateSamples
        {
            get { return this.TemplateSamples; }
        }

        IObjectSet<ImplementationGuideSection> IObjectRepository.ImplementationGuideSections
        {
            get { return this.ImplementationGuideSections; }
        }

        IObjectSet<TemplateExtension> IObjectRepository.TemplateExtensions
        {
            get { return this.TemplateExtensions; }
        }

        #endregion

        #region State Functionality

        public void ChangeObjectState(Object entity, EntityState entityState)
        {
            ObjectStateManager.ChangeObjectState(entity, entityState);
        }

        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId)
        {
            return this.GetImplementationGuideTemplates(implementationGuideId, inferred, parentTemplateId);
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

        #region Auditing

        private List<string> auditableTypes = new List<string>(new string[] { 
            "ImplementationGuide",
            "Template",
            "TemplateConstraint",
            "CodeSystem",
            "ValueSet" });

        public void AuditChanges(string auditUserName, string auditIP)
        {
            if (!string.IsNullOrEmpty(auditUserName) && !string.IsNullOrEmpty(auditIP))
            {
                this.auditUserName = auditUserName;
                this.auditIP = auditIP;
                this.SavingChanges += TrifoliaDatabase_SavingChanges;
            }
        }

        void TrifoliaDatabase_SavingChanges(object sender, EventArgs e)
        {
            ObjectContext context = sender as ObjectContext;

            if (context == null)
                return;

            foreach (ObjectStateEntry entry in
                context.ObjectStateManager.GetObjectStateEntries(
                EntityState.Added | EntityState.Modified | EntityState.Deleted))
            {
                var entityType = entry.Entity.GetType();

                if (!auditableTypes.Contains(entityType.Name))
                    continue;

                this.auditRecords.Add(new TempAuditEntry(this, entry));
            }
        }

        public override int SaveChanges(SaveOptions options)
        {
            int num = base.SaveChanges(options);

            if (this.AddAudits())
                base.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);

            return num;
        }

        private bool AddAudits()
        {
            bool savedAudits = false;

            foreach (TempAuditEntry tempAuditEntry in this.auditRecords)
            {
                AuditEntry newAuditEntry = tempAuditEntry.CreateAuditEntry(this.auditUserName, this.auditIP);

                if (newAuditEntry != null)
                {
                    this.AuditEntries.AddObject(newAuditEntry);
                    savedAudits = true;
                }
            }

            this.auditRecords.Clear();

            return savedAudits;
        }

        private class TempAuditEntry
        {
            private List<string> fieldChanges = new List<string>();
            private EntityState changeState = EntityState.Unchanged;
            private object entity;
            private TrifoliaDatabase context;
            private int? implementationGuideId;
            private int? templateId;
            private int? constraintId;

            public TempAuditEntry(TrifoliaDatabase context, ObjectStateEntry entry)
            {
                this.context = context;
                this.entity = entry.Entity;
                this.changeState = entry.State;
                this.fieldChanges = this.GetFieldChanges(context, entry);

                if (entry.State == EntityState.Deleted)
                    this.SetIds();
            }

            public AuditEntry CreateAuditEntry(string auditUserName, string auditIP)
            {
                if (this.fieldChanges.Count == 0)
                    return null;

                AuditEntry newAuditEntry = new AuditEntry();
                ImplementationGuide ig = this.entity as ImplementationGuide;
                Template template = this.entity as Template;
                TemplateConstraint constraint = this.entity as TemplateConstraint;

                if (this.changeState != EntityState.Deleted)
                    this.SetIds();

                string note = string.Format("{0}: {1} ({2})",
                    this.changeState,
                    this.GetUniqueId(this.entity),
                    String.Join(", ", this.fieldChanges));

                newAuditEntry.ImplementationGuideId = this.implementationGuideId;
                newAuditEntry.TemplateId = this.templateId;
                newAuditEntry.TemplateConstraintId = this.constraintId;
                newAuditEntry.Note = note;
                newAuditEntry.IP = auditIP;
                newAuditEntry.Username = auditUserName;
                newAuditEntry.AuditDate = DateTime.Now;
                newAuditEntry.Type = this.entity.GetType().Name;

                return newAuditEntry;
            }

            private void SetIds()
            {
                var alteredIg = this.entity as ImplementationGuide;
                var alteredTemplate = this.entity as Template;
                var alteredConstraint = this.entity as TemplateConstraint;

                if (alteredIg != null)
                {
                    if (this.implementationGuideId == null)
                        this.implementationGuideId = alteredIg.Id;
                }
                else if (alteredTemplate != null)
                {
                    if (this.templateId == null)
                        this.templateId = alteredTemplate.Id;

                    if (this.implementationGuideId == null)
                        this.implementationGuideId = alteredTemplate.OwningImplementationGuideId;
                }
                else if (alteredConstraint != null)
                {
                    if (this.constraintId == null)
                        this.constraintId = alteredConstraint.Id;

                    if (this.templateId == null)
                        this.templateId = alteredConstraint.TemplateId;

                    var alteredConstraintTemplate = this.context.Templates.SingleOrDefault(y => y.Id == alteredConstraint.TemplateId);

                    if (this.implementationGuideId == null && alteredConstraintTemplate != null)
                        this.implementationGuideId = alteredConstraintTemplate.OwningImplementationGuideId;
                }
            }

            private List<string> GetFieldChanges(ObjectContext context, ObjectStateEntry entry)
            {
                object entity = entry.Entity;
                List<string> fieldChanges = new List<string>();
                ObjectStateEntry state = context.ObjectStateManager.GetObjectStateEntry(entity);

                foreach (string cFieldName in GetEntityFields(entry))
                {
                    if (state.State == EntityState.Added || state.State == EntityState.Deleted)
                    {
                        string cFieldValue = state.State == EntityState.Added ?
                            state.CurrentValues[cFieldName].ToString() :
                            state.OriginalValues[cFieldName].ToString();
                        string changedField = string.Format("{0} = \"{1}\"", cFieldName, cFieldValue);

                        fieldChanges.Add(changedField);
                    }
                    else
                    {
                        string cFieldValueNew = state.CurrentValues[cFieldName].ToString();
                        string cFieldValueOld = state.OriginalValues[cFieldName].ToString();

                        if (cFieldValueOld != cFieldValueNew)
                        {
                            string changedField = string.Format("{0}=\"{1}\" => \"{2}\"", cFieldName, cFieldValueOld, cFieldValueNew);

                            fieldChanges.Add(changedField);
                        }
                    }
                }

                return fieldChanges;
            }

            private List<string> GetEntityFields(ObjectStateEntry entry)
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    return entry.CurrentValues.DataRecordInfo.FieldMetadata.Select(y => y.FieldType.Name).ToList();
                }
                else if (entry.State == EntityState.Deleted)
                {
                    List<string> fields = new List<string>();

                    for (int i = 0; i < entry.OriginalValues.FieldCount; i++)
                    {
                        string fieldName = entry.OriginalValues.GetName(i);
                        fields.Add(fieldName);
                    }

                    return fields;
                }

                return new List<string>();
            }

            private string GetUniqueId(object entity)
            {
                switch (entity.GetType().Name)
                {
                    case "ImplementationGuide":
                        var ig = (ImplementationGuide)entity;
                        return ig.NameWithVersion;
                    case "Template":
                        var template = (Template)entity;
                        return template.Oid;
                    case "TemplateConstraint":
                        var constraint = (TemplateConstraint)entity;

                        // Number is generated (sometimes) automatically by the DB. Entity Framework does not know to re-populate this field
                        // for new constraint records. When Number is not complete, go back to the DB to get it.
                        if (constraint.Number == null)
                        {
                            int? number = this.context.TemplateConstraints.Where(y => y.Id == constraint.Id).Select(y => y.Number).FirstOrDefault();

                            if (number == null)
                                return "N/A";
                            else
                                return number.ToString();
                        }

                        return constraint.Number.ToString();
                    case "CodeSystem":
                        var codeSystem = (CodeSystem)entity;
                        return codeSystem.Oid;
                    case "ValueSet":
                        var valueSet = (ValueSet)entity;
                        return valueSet.Oid;
                }

                return "N/A";
            }
        }

        #endregion

        #region Get IG Templates with Categories


        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId, string[] categories)
        {
            var entityConnection = (System.Data.Entity.Core.EntityClient.EntityConnection)this.Connection;
            IDbConnection dbConnection = entityConnection.StoreConnection;
            IDbCommand command = dbConnection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "GetImplementationGuideTemplates";

            var implementationGuideIdParameter = command.CreateParameter();
            implementationGuideIdParameter.ParameterName = "implementationGuideId";
            implementationGuideIdParameter.Value = implementationGuideId;
            command.Parameters.Add(implementationGuideIdParameter);

            var inferredParameter = command.CreateParameter();
            inferredParameter.ParameterName = "inferred";
            inferredParameter.Value = inferred;
            command.Parameters.Add(inferredParameter);

            var parentTemplateIdParameter = command.CreateParameter();
            parentTemplateIdParameter.ParameterName = "parentTemplateId";
            parentTemplateIdParameter.Value = parentTemplateId;
            command.Parameters.Add(parentTemplateIdParameter);

            DataTable table = new DataTable();
            table.Columns.Add("category");

            if (categories != null)
            {
                foreach (var category in categories)
                {
                    table.Rows.Add(category);
                }
            }

            var categoriesParameter = command.CreateParameter();
            categoriesParameter.ParameterName = "categories";
            categoriesParameter.Value = table;
            command.Parameters.Add(categoriesParameter);

            if (command.Connection.State != ConnectionState.Open)
                command.Connection.Open();

            using (var reader = command.ExecuteReader())
            {
                List<int?> returnValues = new List<int?>();

                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        returnValues.Add(reader.GetInt32(0));
                }

                return returnValues.AsEnumerable();
            }
        }

        #endregion
    }
}
