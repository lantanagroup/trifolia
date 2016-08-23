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

        IObjectSet<AuditEntry> AuditEntries { get; }
        IObjectSet<CodeSystem> CodeSystems { get; }
        IObjectSet<GreenConstraint> GreenConstraints { get; }
        IObjectSet<GreenTemplate> GreenTemplates { get; }
        IObjectSet<ImplementationGuide> ImplementationGuides { get; }
        IObjectSet<ImplementationGuideSetting> ImplementationGuideSettings { get; }
        IObjectSet<Template> Templates { get; }
        IObjectSet<TemplateConstraint> TemplateConstraints { get; }
        IObjectSet<TemplateType> TemplateTypes { get; }
        IObjectSet<ValueSet> ValueSets { get; }
        IObjectSet<ValueSetMember> ValueSetMembers { get; }
        IObjectSet<ImplementationGuideTemplateType> ImplementationGuideTemplateTypes { get; }
        IObjectSet<ImplementationGuideTypeDataType> ImplementationGuideTypeDataTypes { get; }
        IObjectSet<ImplementationGuideType> ImplementationGuideTypes { get; }
        IObjectSet<ImplementationGuideFile> ImplementationGuideFiles { get; }
        IObjectSet<ViewImplementationGuideFile> ViewImplementationGuideFiles { get; }
        IObjectSet<ImplementationGuideFileData> ImplementationGuideFileDatas { get; }
        IObjectSet<Organization> Organizations { get; }
        IObjectSet<ViewTemplate> ViewTemplates { get; }
        IObjectSet<ImplementationGuideSchematronPattern> ImplementationGuideSchematronPatterns { get; }
        IObjectSet<PublishStatus> PublishStatuses { get; }
        IObjectSet<Role> Roles { get; }
        IObjectSet<AppSecurable> AppSecurables { get; }
        IObjectSet<RoleAppSecurable> RoleAppSecurables { get; }
        IObjectSet<UserRole> UserRoles { get; }
        IObjectSet<User> Users { get; }
        IObjectSet<ViewTemplateList> ViewTemplateLists { get; }
        IObjectSet<RoleRestriction> RoleRestrictions { get; }
        IObjectSet<Group> Groups { get; }
        IObjectSet<UserGroup> UserGroups { get; }
        IObjectSet<ImplementationGuidePermission> ImplementationGuidePermissions { get; }
        IObjectSet<ViewTemplatePermission> ViewTemplatePermissions { get; }
        IObjectSet<ViewImplementationGuidePermission> ViewImplementationGuidePermissions { get; }
        IObjectSet<ViewUserSecurable> ViewUserSecurables { get; }
        IObjectSet<ViewImplementationGuideTemplate> ViewImplementationGuideTemplates { get; }
        IObjectSet<TemplateConstraintSample> TemplateConstraintSamples { get; }
        IObjectSet<TemplateSample> TemplateSamples { get; }
        IObjectSet<ViewIGAuditTrail> ViewIGAuditTrails { get; }
        IObjectSet<ImplementationGuideSection> ImplementationGuideSections { get; }
        IObjectSet<TemplateExtension> TemplateExtensions { get; }
        IObjectSet<GroupManager> GroupManagers { get; }
        ObjectResult<SearchValueSetResult> SearchValueSet(Nullable<global::System.Int32> userId, global::System.String searchText, Nullable<global::System.Int32> count, Nullable<global::System.Int32> page, global::System.String orderProperty, Nullable<global::System.Boolean> orderDesc);
        IEnumerable<Nullable<global::System.Int32>> GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId);
        IEnumerable<Nullable<global::System.Int32>> GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId, string[] categories);
        IEnumerable<Nullable<global::System.Int32>> SearchTemplates(Nullable<global::System.Int32> userId, Nullable<global::System.Int32> filterImplementationGuideId, global::System.String filterName, global::System.String filterIdentifier, Nullable<global::System.Int32> filterTemplateTypeId, Nullable<global::System.Int32> filterOrganizationId, global::System.String filterContextType, global::System.String queryText);

        void ChangeObjectState(Object entity, EntityState entityState);
        IDbTransaction BeginTransaction();
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);
        int SaveChanges();
        void Refresh(RefreshMode refreshMode, Object entity);
        DbConnection Connection { get; }
    }
}
