using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

using System.Data.Entity.Core.Objects;
using System.Data;

namespace Trifolia.DB
{
    public interface IObjectRepository :IDisposable
    {
        void AuditChanges(string auditUserName, string auditIP);
        
        DbSet<AuditEntry> AuditEntries { get; }
        DbSet<CodeSystem> CodeSystems { get; }
        DbSet<GreenConstraint> GreenConstraints { get; }
        DbSet<GreenTemplate> GreenTemplates { get; }
        DbSet<ImplementationGuide> ImplementationGuides { get; }
        DbSet<ImplementationGuideSetting> ImplementationGuideSettings { get; }
        DbSet<Template> Templates { get; }
        DbSet<TemplateConstraint> TemplateConstraints { get; }
        DbSet<TemplateType> TemplateTypes { get; }
        DbSet<ValueSet> ValueSets { get; }
        DbSet<ValueSetMember> ValueSetMembers { get; }
        DbSet<ValueSetIdentifier> ValueSetIdentifiers { get; }
        DbSet<ImplementationGuideTemplateType> ImplementationGuideTemplateTypes { get; }
        DbSet<ImplementationGuideTypeDataType> ImplementationGuideTypeDataTypes { get; }
        DbSet<ImplementationGuideType> ImplementationGuideTypes { get; }
        DbSet<ImplementationGuideFile> ImplementationGuideFiles { get; }
        DbSet<ViewImplementationGuideFile> ViewImplementationGuideFiles { get; }
        DbSet<ImplementationGuideFileData> ImplementationGuideFileDatas { get; }
        DbSet<Organization> Organizations { get; }
        DbSet<ViewTemplate> ViewTemplates { get; }
        DbSet<ImplementationGuideSchematronPattern> ImplementationGuideSchematronPatterns { get; }
        DbSet<PublishStatus> PublishStatuses { get; }
        DbSet<Role> Roles { get; }
        DbSet<AppSecurable> AppSecurables { get; }
        DbSet<RoleAppSecurable> RoleAppSecurables { get; }
        DbSet<UserRole> UserRoles { get; }
        DbSet<User> Users { get; }
        DbSet<ViewTemplateList> ViewTemplateLists { get; }
        DbSet<RoleRestriction> RoleRestrictions { get; }
        DbSet<Group> Groups { get; }
        DbSet<UserGroup> UserGroups { get; }
        DbSet<ImplementationGuidePermission> ImplementationGuidePermissions { get; }
        DbSet<ViewTemplatePermission> ViewTemplatePermissions { get; }
        DbSet<ViewImplementationGuidePermission> ViewImplementationGuidePermissions { get; }
        DbSet<ViewUserSecurable> ViewUserSecurables { get; }
        DbSet<ViewImplementationGuideTemplate> ViewImplementationGuideTemplates { get; }
        DbSet<TemplateConstraintSample> TemplateConstraintSamples { get; }
        DbSet<TemplateSample> TemplateSamples { get; }
        DbSet<ViewIGAuditTrail> ViewIGAuditTrails { get; }
        DbSet<ImplementationGuideSection> ImplementationGuideSections { get; }
        DbSet<TemplateExtension> TemplateExtensions { get; }
        DbSet<GroupManager> GroupManagers { get; }
        IEnumerable<SearchValueSetResult> SearchValueSet(Nullable<global::System.Int32> userId, global::System.String searchText, Nullable<global::System.Int32> count, Nullable<global::System.Int32> page, global::System.String orderProperty, Nullable<global::System.Boolean> orderDesc);
        IEnumerable<Nullable<global::System.Int32>> GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId, string[] categories);
        IEnumerable<Nullable<global::System.Int32>> SearchTemplates(Nullable<global::System.Int32> userId, Nullable<global::System.Int32> filterImplementationGuideId, global::System.String filterName, global::System.String filterIdentifier, Nullable<global::System.Int32> filterTemplateTypeId, Nullable<global::System.Int32> filterOrganizationId, global::System.String filterContextType, global::System.String queryText);

        void ChangeObjectState(Object entity, EntityState entityState);
        DbContextTransaction BeginTransaction();
        DbContextTransaction BeginTransaction(IsolationLevel isolationLevel);
        int SaveChanges();
        DbConnection Connection { get; }
    }
}
