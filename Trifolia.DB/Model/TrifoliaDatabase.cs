namespace Trifolia.DB
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Data.Common;
    using System.Data;

    public partial class TrifoliaDatabase : DbContext, IObjectRepository
    {
        public TrifoliaDatabase()
            : base("name=TrifoliaDatabase")
        {
        }

        public TrifoliaDatabase(string connectionString)
            : base(connectionString)
        {
        }

        public virtual DbSet<AppSecurable> AppSecurables { get; set; }
        public virtual DbSet<RoleAppSecurable> RoleAppSecurables { get; set; }
        public virtual DbSet<AuditEntry> AuditEntries { get; set; }
        public virtual DbSet<CodeSystem> CodeSystems { get; set; }
        public virtual DbSet<CodeSystemIdentifier> CodeSystemIdentifiers { get; set; }
        public virtual DbSet<GreenConstraint> GreenConstraints { get; set; }
        public virtual DbSet<GreenTemplate> GreenTemplates { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupManager> GroupManagers { get; set; }
        public virtual DbSet<ImplementationGuide> ImplementationGuides { get; set; }
        public virtual DbSet<ImplementationGuideFile> ImplementationGuideFiles { get; set; }
        public virtual DbSet<ImplementationGuideFileData> ImplementationGuideFileDatas { get; set; }
        public virtual DbSet<ImplementationGuidePermission> ImplementationGuidePermissions { get; set; }
        public virtual DbSet<ImplementationGuideSchematronPattern> ImplementationGuideSchematronPatterns { get; set; }
        public virtual DbSet<ImplementationGuideSection> ImplementationGuideSections { get; set; }
        public virtual DbSet<ImplementationGuideSetting> ImplementationGuideSettings { get; set; }
        public virtual DbSet<ImplementationGuideTemplateType> ImplementationGuideTemplateTypes { get; set; }
        public virtual DbSet<ImplementationGuideType> ImplementationGuideTypes { get; set; }
        public virtual DbSet<ImplementationGuideTypeDataType> ImplementationGuideTypeDataTypes { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<PublishStatus> PublishStatuses { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RoleRestriction> RoleRestrictions { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<TemplateConstraint> TemplateConstraints { get; set; }
        public virtual DbSet<TemplateConstraintSample> TemplateConstraintSamples { get; set; }
        public virtual DbSet<TemplateExtension> TemplateExtensions { get; set; }
        public virtual DbSet<TemplateSample> TemplateSamples { get; set; }
        public virtual DbSet<TemplateType> TemplateTypes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<ValueSet> ValueSets { get; set; }
        public virtual DbSet<ValueSetMember> ValueSetMembers { get; set; }
        public virtual DbSet<ValueSetIdentifier> ValueSetIdentifiers { get; set; }
        public virtual DbSet<ViewImplementationGuideCodeSystem> ViewImplementationGuideCodeSystems { get; set; }
        public virtual DbSet<ViewConstraintCount> ViewConstraintCounts { get; set; }
        public virtual DbSet<ViewIGAuditTrail> ViewIGAuditTrails { get; set; }
        public virtual DbSet<ViewImplementationGuideFile> ViewImplementationGuideFiles { get; set; }
        public virtual DbSet<ViewImplementationGuidePermission> ViewImplementationGuidePermissions { get; set; }
        public virtual DbSet<ViewImplementationGuideTemplate> ViewImplementationGuideTemplates { get; set; }
        public virtual DbSet<ViewLatestImplementationGuideFileData> ViewLatestImplementationGuideFileDatas { get; set; }
        public virtual DbSet<ViewTemplate> ViewTemplates { get; set; }
        public virtual DbSet<ViewTemplateList> ViewTemplateLists { get; set; }
        public virtual DbSet<ViewTemplatePermission> ViewTemplatePermissions { get; set; }
        public virtual DbSet<ViewTemplateUsage> ViewTemplateUsages { get; set; }
        public virtual DbSet<ViewUserSecurable> ViewUserSecurables { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppSecurable>()
                .HasMany(e => e.AppSecurableRoles)
                .WithRequired(e => e.AppSecurable)
                .HasForeignKey(e => e.AppSecurableId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GreenConstraint>()
                .HasMany(e => e.ChildGreenConstraints)
                .WithOptional(e => e.ParentGreenConstraint)
                .HasForeignKey(e => e.ParentGreenConstraintId);

            modelBuilder.Entity<GreenTemplate>()
                .HasMany(e => e.ChildGreenConstraints)
                .WithRequired(e => e.GreenTemplate)
                .HasForeignKey(e => e.GreenTemplateId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GreenTemplate>()
                .HasMany(e => e.ChildGreenTemplates)
                .WithOptional(e => e.ParentGreenTemplate)
                .HasForeignKey(e => e.ParentGreenTemplateId);

            modelBuilder.Entity<Group>()
                .HasMany(e => e.Managers)
                .WithRequired(e => e.Group)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuide>()
                .Property(e => e.DisplayName)
                .IsUnicode(false);

            modelBuilder.Entity<ImplementationGuide>()
                .Property(e => e.Identifier)
                .IsUnicode(false);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.NextVersions)
                .WithOptional(e => e.PreviousVersion)
                .HasForeignKey(e => e.PreviousVersionImplementationGuideId);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.Files)
                .WithRequired(e => e.ImplementationGuide)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.Permissions)
                .WithRequired(e => e.ImplementationGuide)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.SchematronPatterns)
                .WithRequired(e => e.ImplementationGuide)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.Sections)
                .WithRequired(e => e.ImplementationGuide)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.Settings)
                .WithRequired(e => e.ImplementationGuide)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.TemplateTypes)
                .WithRequired(e => e.ImplementationGuide)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuide>()
                .HasMany(e => e.ChildTemplates)
                .WithRequired(e => e.OwningImplementationGuide)
                .HasForeignKey(e => e.OwningImplementationGuideId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ImplementationGuideFile>()
                .HasMany(e => e.Versions)
                .WithRequired(e => e.ImplementationGuideFile)
                .HasForeignKey(e => e.ImplementationGuideFileId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuideType>()
                .HasMany(e => e.ImplementationGuides)
                .WithRequired(e => e.ImplementationGuideType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ImplementationGuideType>()
                .HasMany(e => e.DataTypes)
                .WithRequired(e => e.ImplementationGuideType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ImplementationGuideType>()
                .HasMany(e => e.Templates)
                .WithRequired(e => e.ImplementationGuideType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ImplementationGuideType>()
                .HasMany(e => e.TemplateTypes)
                .WithRequired(e => e.ImplementationGuideType)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ImplementationGuideTypeDataType>()
                .HasMany(e => e.GreenConstraints)
                .WithOptional(e => e.ImplementationGuideTypeDataType)
                .HasForeignKey(e => e.ImplementationGuideTypeDataTypeId);

            modelBuilder.Entity<Organization>()
                .HasMany(e => e.RoleRestrictions)
                .WithRequired(e => e.Organization)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PublishStatus>()
                .HasMany(e => e.ImplementationGuides)
                .WithOptional(e => e.PublishStatus)
                .HasForeignKey(e => e.PublishStatusId);

            modelBuilder.Entity<PublishStatus>()
                .HasMany(e => e.Templates)
                .WithOptional(e => e.Status)
                .HasForeignKey(e => e.StatusId);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.AppSecurables)
                .WithRequired(e => e.Role)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.Restrictions)
                .WithRequired(e => e.Role)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Template>()
                .HasMany(e => e.ChildConstraints)
                .WithRequired(e => e.Template)
                .HasForeignKey(e => e.TemplateId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Template>()
                .HasMany(e => e.ContainingConstraints)
                .WithOptional(e => e.ContainedTemplate)
                .HasForeignKey(e => e.ContainedTemplateId);

            modelBuilder.Entity<Template>()
                .HasMany(e => e.Extensions)
                .WithRequired(e => e.Template)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Template>()
                .HasMany(e => e.ImplyingTemplates)
                .WithOptional(e => e.ImpliedTemplate)
                .HasForeignKey(e => e.ImpliedTemplateId);

            modelBuilder.Entity<Template>()
                .HasMany(e => e.NextVersions)
                .WithOptional(e => e.PreviousVersion)
                .HasForeignKey(e => e.PreviousVersionTemplateId);

            modelBuilder.Entity<Template>()
                .HasMany(e => e.TemplateSamples)
                .WithRequired(e => e.Template)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TemplateConstraint>()
                .HasMany(e => e.GreenConstraints)
                .WithRequired(e => e.TemplateConstraint)
                .HasForeignKey(e => e.TemplateConstraintId);

            modelBuilder.Entity<TemplateConstraint>()
                .HasMany(e => e.Samples)
                .WithRequired(e => e.Constraint)
                .HasForeignKey(e => e.TemplateConstraintId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TemplateConstraint>()
                .HasMany(e => e.ChildConstraints)
                .WithOptional(e => e.ParentConstraint)
                .HasForeignKey(e => e.ParentConstraintId);

            modelBuilder.Entity<TemplateSample>()
                .Property(e => e.XmlSample)
                .IsUnicode(false);

            modelBuilder.Entity<TemplateType>()
                .HasMany(e => e.ImplementationGuideTemplateTypes)
                .WithRequired(e => e.TemplateType)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TemplateType>()
                .HasMany(e => e.Templates)
                .WithRequired(e => e.TemplateType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.ManagingGroups)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.AccessManagerImplemntationGuides)
                .WithOptional(e => e.AccessManager)
                .HasForeignKey(e => e.AccessManagerId);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Templates)
                .WithRequired(e => e.Author)
                .HasForeignKey(e => e.AuthorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Groups)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ValueSet>()
                .HasMany(e => e.Members)
                .WithRequired(e => e.ValueSet)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ValueSet>()
                .HasMany(e => e.Identifiers)
                .WithRequired(e => e.ValueSet)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CodeSystem>()
                .HasMany(e => e.Identifiers)
                .WithRequired(e => e.CodeSystem)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ViewIGAuditTrail>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<ViewIGAuditTrail>()
                .Property(e => e.Ip)
                .IsUnicode(false);

            modelBuilder.Entity<ViewIGAuditTrail>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<ViewIGAuditTrail>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<ViewIGAuditTrail>()
                .Property(e => e.TemplateName)
                .IsUnicode(false);

            modelBuilder.Entity<ViewImplementationGuideFile>()
                .Property(e => e.FileName)
                .IsUnicode(false);

            modelBuilder.Entity<ViewImplementationGuideFile>()
                .Property(e => e.MimeType)
                .IsUnicode(false);

            modelBuilder.Entity<ViewImplementationGuideFile>()
                .Property(e => e.ContentType)
                .IsUnicode(false);

            modelBuilder.Entity<ViewImplementationGuideFile>()
                .Property(e => e.UpdatedBy)
                .IsUnicode(false);

            modelBuilder.Entity<ViewImplementationGuideFile>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<ViewLatestImplementationGuideFileData>()
                .Property(e => e.UpdatedBy)
                .IsUnicode(false);

            modelBuilder.Entity<ViewLatestImplementationGuideFileData>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<ViewTemplateList>()
                .Property(e => e.Open)
                .IsUnicode(false);

            modelBuilder.Entity<ViewTemplateUsage>()
                .Property(e => e.TemplateDisplay)
                .IsUnicode(false);

            modelBuilder.Entity<ViewTemplateUsage>()
                .Property(e => e.ImplementationGuideDisplay)
                .IsUnicode(false);

            modelBuilder.Entity<ViewUserSecurable>()
                .Property(e => e.SecurableName)
                .IsUnicode(false);
        }

        #region Function Imports

        private static SqlParameter CreateParameter(string name, int? value)
        {
            if (value.HasValue)
                return CreateParameter(name, (object)value.Value);
            else
                return CreateParameter(name, (object)DBNull.Value);
        }

        private static SqlParameter CreateParameter(string name, object value)
        {
            var param = new SqlParameter();
            param.ParameterName = name;

            if (value == null)
                param.Value = DBNull.Value;
            else
                param.Value = value;

            return param;
        }

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userId">No Metadata Documentation available.</param>
        /// <param name="searchText">No Metadata Documentation available.</param>
        /// <param name="count">No Metadata Documentation available.</param>
        /// <param name="page">No Metadata Documentation available.</param>
        /// <param name="orderProperty">No Metadata Documentation available.</param>
        /// <param name="orderDesc">No Metadata Documentation available.</param>
        public IEnumerable<SearchValueSetResult> SearchValueSet(
            int? userId, 
            string searchText, 
            int? count, 
            int? page, 
            string orderProperty, 
            bool? orderDesc)
        {
            var queryResults = this.Database.SqlQuery<SearchValueSetResult>("SearchValueSet @userId, @searchText, @count, @page, @orderProperty, @orderDesc",
                CreateParameter("userId", userId),
                CreateParameter("searchText", searchText),
                CreateParameter("count", count),
                CreateParameter("page", page),
                CreateParameter("orderProperty", orderProperty),
                CreateParameter("orderDesc", orderDesc));
            return queryResults;
        }

        public IEnumerable<int?> SearchTemplates(
            int? userId, 
            int? filterImplementationGuideId, 
            string filterName, 
            string filterIdentifier, 
            int? filterTemplateTypeId, 
            int? filterOrganizationId, 
            string filterContextType, 
            string queryText)
        {
            var userIdParameter = CreateParameter("userId", userId);
            var filterImplementationGuideIdParameter = CreateParameter("filterImplementationGuideId", filterImplementationGuideId);
            var filterNameParameter = CreateParameter("filterName", filterName);
            var filterIdentifierParameter = CreateParameter("filterIdentifier", filterIdentifier);
            var filterTemplateTypeIdParameter = CreateParameter("filterTemplateTypeId", filterTemplateTypeId);
            var filterOrganizationIdParameter = CreateParameter("filterOrganizationId", filterOrganizationId);
            var filterContextTypeParameter = CreateParameter("filterContextType", filterContextType);
            var queryTextParameter = CreateParameter("queryText", queryText);

            var results = this.Database.SqlQuery<int?>("SearchTemplates @userId, @filterImplementationGuideId, @filterName, @filterIdentifier, @filterTemplateTypeId, @filterOrganizationId, @filterContextType, @queryText",
                userIdParameter,
                filterImplementationGuideIdParameter,
                filterNameParameter,
                filterIdentifierParameter,
                filterTemplateTypeIdParameter,
                filterOrganizationIdParameter,
                filterContextTypeParameter,
                queryTextParameter);
            return results;
        }

        public IEnumerable<int?> GetImplementationGuideTemplates(
            int? implementationGuideId, 
            bool? inferred, 
            int? parentTemplateId, 
            string[] categories)
        {
            var implementationGuideIdParameter = CreateParameter("implementationGuideId", implementationGuideId);
            var inferredParameter = CreateParameter("inferred", inferred);
            var parentTemplateIdParameter = CreateParameter("parentTemplateId", parentTemplateId);

            var categoriesParameter = new SqlParameter("categories", SqlDbType.Structured);
            categoriesParameter.TypeName = "dbo.CategoryList";

            if (categories != null)
            {
                DataTable table = new DataTable();
                table.Columns.Add("category");
                foreach (var category in categories)
                {
                    table.Rows.Add(category);
                }

                categoriesParameter.Value = table;
            }

            var results = this.Database.SqlQuery<int?>("GetImplementationGuideTemplates @implementationGuideId, @inferred, @parentTemplateId, @categories",
                implementationGuideIdParameter,
                inferredParameter,
                parentTemplateIdParameter,
                categoriesParameter);
            return results;
        }

        #endregion

        #region IObjectRepository Implementation

        public DbConnection Connection
        {
            get { return base.Database.Connection; }
        }

        #endregion
    }
}
