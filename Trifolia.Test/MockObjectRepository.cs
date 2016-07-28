using System;
using System.Data.Common;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Reflection;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Authorization;

namespace Trifolia.Test
{
    [Serializable]
    public class MockObjectRepository : IObjectRepository
    {
        public const string DEFAULT_ORGANIZATION = "LCG";
        public const string DEFAULT_FHIR_DSTU1_IG_TYPE_NAME = "FHIR DSTU1";
        public const string DEFAULT_FHIR_DSTU2_IG_TYPE_NAME = "FHIR DSTU2";
        public const string DEFAULT_FHIR_STU3_IG_TYPE_NAME = "FHIR Latest";
        public const string DEFAULT_CDA_IG_TYPE_NAME = "CDA";
        public const string DEFAULT_HQMF_R2_IG_TYPE_NAME = "HQMF R2";

        public MockObjectRepository()
        {
            this.PublishStatuses.AddObject(new PublishStatus()
            {
                Id = 1,
                Status = "Draft"
            });
            this.PublishStatuses.AddObject(new PublishStatus()
            {
                Id = 2,
                Status = "Ballot"
            });
            this.PublishStatuses.AddObject(new PublishStatus()
            {
                Id = 3,
                Status = "Published"
            });
            this.PublishStatuses.AddObject(new PublishStatus()
            {
                Id = 4,
                Status = "Deprecated"
            });
            this.PublishStatuses.AddObject(new PublishStatus()
            {
                Id = 5,
                Status = "Retired"
            });

            // Add the default admin role
            var adminRole = this.FindOrAddRole("admin");
            adminRole.IsAdmin = true;

            // Add all securables to the system
            this.FindOrAddSecurables(SecurableNames.ADMIN, SecurableNames.CODESYSTEM_EDIT, SecurableNames.CODESYSTEM_LIST, SecurableNames.EXPORT_GREEN, SecurableNames.EXPORT_SCHEMATRON, 
                SecurableNames.EXPORT_VOCAB, SecurableNames.EXPORT_WORD, SecurableNames.EXPORT_XML, SecurableNames.GREEN_MODEL, SecurableNames.IMPLEMENTATIONGUIDE_AUDIT_TRAIL, 
                SecurableNames.IMPLEMENTATIONGUIDE_EDIT, SecurableNames.IMPLEMENTATIONGUIDE_EDIT_BOOKMARKS, SecurableNames.IMPLEMENTATIONGUIDE_FILE_MANAGEMENT, SecurableNames.IMPLEMENTATIONGUIDE_FILE_VIEW, 
                SecurableNames.IMPLEMENTATIONGUIDE_LIST, SecurableNames.IMPLEMENTATIONGUIDE_NOTES, SecurableNames.IMPLEMENTATIONGUIDE_PRIMITIVES, SecurableNames.LANDING_PAGE, SecurableNames.ORGANIZATION_DETAILS, 
                SecurableNames.ORGANIZATION_LIST, SecurableNames.PUBLISH_SETTINGS, SecurableNames.REPORT_TEMPLATE_COMPLIANCE, SecurableNames.REPORT_TEMPLATE_REVIEW, SecurableNames.TEMPLATE_COPY, 
                SecurableNames.TEMPLATE_DELETE, SecurableNames.TEMPLATE_EDIT, SecurableNames.TEMPLATE_LIST, SecurableNames.TEMPLATE_MOVE, SecurableNames.TERMINOLOGY_OVERRIDE, SecurableNames.VALUESET_EDIT, 
                SecurableNames.VALUESET_LIST, SecurableNames.WEB_IG);
        }

        public void InitializeCDARepository()
        {
            ImplementationGuideType cdaType = this.FindOrCreateImplementationGuideType(DEFAULT_CDA_IG_TYPE_NAME, "cda.xsd", "cda", "urn:hl7-org:v3");

            this.FindOrCreateTemplateType(cdaType, "Document", "ClinicalDocument", "ClinicalDocument", 1);
            this.FindOrCreateTemplateType(cdaType, "Section", "section", "Section", 2);
            this.FindOrCreateTemplateType(cdaType, "Entry", "entry", "Entry", 3);
        }

        public void InitializeFHIRRepository()
        {
            ImplementationGuideType fhirType = this.FindOrCreateImplementationGuideType(DEFAULT_FHIR_DSTU1_IG_TYPE_NAME, "fhir-all.xsd", "fhir", "http://hl7.org/fhir");

            this.FindOrCreateTemplateType(fhirType, "Composition", "Composition", "Composition", 1);
            this.FindOrCreateTemplateType(fhirType, "Patient", "Patient", "Patient", 2);
            this.FindOrCreateTemplateType(fhirType, "Practitioner", "Practitioner", "Practitioner", 3);
        }

        public void InitializeFHIR2Repository()
        {
            ImplementationGuideType fhirType = this.FindOrCreateImplementationGuideType(DEFAULT_FHIR_DSTU2_IG_TYPE_NAME, "fhir-all.xsd", "fhir", "http://hl7.org/fhir");

            this.FindOrCreateTemplateType(fhirType, "Composition", "Composition", "Composition", 1);
            this.FindOrCreateTemplateType(fhirType, "Patient", "Patient", "Patient", 2);
            this.FindOrCreateTemplateType(fhirType, "Practitioner", "Practitioner", "Practitioner", 3);
            this.FindOrCreateTemplateType(fhirType, "StructureDefinition", "StructureDefinition", "StructureDefinition", 4);
            this.FindOrCreateTemplateType(fhirType, "ImplementationGuide", "ImplementationGuide", "ImplementationGuide", 5);
            this.FindOrCreateTemplateType(fhirType, "ValueSet", "ValueSet", "ValueSet", 6);

            this.FindOrAddImplementationGuide(fhirType, "Unowned FHIR DSTU2 Profiles");
        }

        public void InitializeFHIR3Repository()
        {
            ImplementationGuideType fhirType = this.FindOrCreateImplementationGuideType(DEFAULT_FHIR_STU3_IG_TYPE_NAME, "fhir-all.xsd", "fhir", "http://hl7.org/fhir");

            this.FindOrCreateTemplateType(fhirType, "Composition", "Composition", "Composition", 1);
            this.FindOrCreateTemplateType(fhirType, "Patient", "Patient", "Patient", 2);
            this.FindOrCreateTemplateType(fhirType, "Practitioner", "Practitioner", "Practitioner", 3);
            this.FindOrCreateTemplateType(fhirType, "StructureDefinition", "StructureDefinition", "StructureDefinition", 4);
            this.FindOrCreateTemplateType(fhirType, "ImplementationGuide", "ImplementationGuide", "ImplementationGuide", 5);
            this.FindOrCreateTemplateType(fhirType, "ValueSet", "ValueSet", "ValueSet", 6);
            this.FindOrCreateTemplateType(fhirType, "Questionnaire", "Questionnaire", "Questionnaire", 6);

            this.FindOrAddImplementationGuide(fhirType, "Unowned FHIR STU3 Profiles");
        }

        public void InitializeLCG()
        {
            var org = this.FindOrAddOrganization(DEFAULT_ORGANIZATION);
            this.FindOrAddUser("admin", org);
            this.AssociateUserWithRole("admin", org.Id, "admin");
        }

        public void InitializeLCGAndLogin()
        {
            this.InitializeLCG();
            Helper.AuthLogin(this, "admin", DEFAULT_ORGANIZATION);
        }

        #region IObjectRepository

        public void AuditChanges(string auditUserName, string auditOrganization, string auditIP)
        {

        }

        MockDbSet<AuditEntry> auditEntries = new MockDbSet<AuditEntry>();
        MockDbSet<Template> templates = new MockDbSet<Template>();
        MockDbSet<CodeSystem> codeSystems = new MockDbSet<CodeSystem>();
        MockDbSet<GreenConstraint> greenConstraints = new MockDbSet<GreenConstraint>();
        MockDbSet<GreenTemplate> greenTemplates = new MockDbSet<GreenTemplate>();
        MockDbSet<ImplementationGuide> implementationGuides = new MockDbSet<ImplementationGuide>();
        MockDbSet<ImplementationGuideSetting> implementationGuideSettings = new MockDbSet<ImplementationGuideSetting>();
        MockDbSet<TemplateConstraint> constraints = new MockDbSet<TemplateConstraint>();
        MockDbSet<TemplateType> templateTypes = new MockDbSet<TemplateType>();
        MockDbSet<ValueSet> valuesets = new MockDbSet<ValueSet>();
        MockDbSet<ValueSetMember> valuesetMembers = new MockDbSet<ValueSetMember>();
        MockDbSet<ImplementationGuideTemplateType> implementationGuideTemplateTypes = new MockDbSet<ImplementationGuideTemplateType>();
        MockDbSet<ImplementationGuideTypeDataType> dataTypes = new MockDbSet<ImplementationGuideTypeDataType>();
        MockDbSet<ImplementationGuideType> implementationGuideTypes = new MockDbSet<ImplementationGuideType>();
        MockDbSet<ImplementationGuideFile> implementationGuideFiles = new MockDbSet<ImplementationGuideFile>();
        MockDbSet<ImplementationGuideFileData> implementationGuideFileDatas = new MockDbSet<ImplementationGuideFileData>();
        MockDbSet<Organization> organizations = new MockDbSet<Organization>();
        MockDbSet<ImplementationGuideSchematronPattern> implementationGuideSchematronPatterns = new MockDbSet<ImplementationGuideSchematronPattern>();
        MockDbSet<PublishStatus> publishStatuses = new MockDbSet<PublishStatus>();
        MockDbSet<Role> roles = new MockDbSet<Role>();
        MockDbSet<AppSecurable> appSecurables = new MockDbSet<AppSecurable>();
        MockDbSet<RoleAppSecurable> roleAppSecurables = new MockDbSet<RoleAppSecurable>();
        MockDbSet<UserRole> userRoles = new MockDbSet<UserRole>();
        MockDbSet<User> users = new MockDbSet<User>();
        MockDbSet<RoleRestriction> roleRestrictions = new MockDbSet<RoleRestriction>();
        MockDbSet<Group> groups = new MockDbSet<Group>();
        MockDbSet<UserGroup> userGroups = new MockDbSet<UserGroup>();
        MockDbSet<ImplementationGuidePermission> implementationGuidePermissions = new MockDbSet<ImplementationGuidePermission>();
        MockDbSet<OrganizationDefaultPermission> organizationDefaultPermissions = new MockDbSet<OrganizationDefaultPermission>();
        MockDbSet<TemplateConstraintSample> templateConstraintSamples = new MockDbSet<TemplateConstraintSample>();
        MockDbSet<TemplateSample> templateSamples = new MockDbSet<TemplateSample>();
        MockDbSet<ImplementationGuideSection> implementationGuideSections = new MockDbSet<ImplementationGuideSection>();
        MockDbSet<TemplateExtension> templateExtensions = new MockDbSet<TemplateExtension>();

        public IObjectSet<AuditEntry> AuditEntries
        {
            get { return auditEntries; }
        }

        public IObjectSet<CodeSystem> CodeSystems
        {
            get { return codeSystems; }
        }

        public IObjectSet<GreenConstraint> GreenConstraints
        {
            get { return greenConstraints; }
        }

        public IObjectSet<GreenTemplate> GreenTemplates
        {
            get { return greenTemplates; }
        }

        public IObjectSet<ImplementationGuide> ImplementationGuides
        {
            get { return implementationGuides; }
        }

        public IObjectSet<ImplementationGuideSetting> ImplementationGuideSettings
        {
            get { return implementationGuideSettings; }
        }

        public IObjectSet<Template> Templates
        {
            get { return this.templates; }
        }

        public IObjectSet<TemplateConstraint> TemplateConstraints
        {
            get { return constraints; }
        }

        public IObjectSet<TemplateType> TemplateTypes
        {
            get { return templateTypes; }
        }

        public IObjectSet<ValueSet> ValueSets
        {
            get { return valuesets; }
        }

        public IObjectSet<ValueSetMember> ValueSetMembers
        {
            get { return valuesetMembers; }
        }

        public IObjectSet<ImplementationGuideTemplateType> ImplementationGuideTemplateTypes
        {
            get { return implementationGuideTemplateTypes; }
        }

        public IObjectSet<ImplementationGuideTypeDataType> ImplementationGuideTypeDataTypes
        {
            get { return dataTypes; }
        }

        public IObjectSet<ImplementationGuideType> ImplementationGuideTypes
        {
            get { return implementationGuideTypes; }
        }

        public IObjectSet<ImplementationGuideFile> ImplementationGuideFiles
        {
            get { return implementationGuideFiles; }
        }

        public IObjectSet<ImplementationGuideFileData> ImplementationGuideFileDatas
        {
            get { return implementationGuideFileDatas; }
        }

        public IObjectSet<Organization> Organizations
        {
            get { return organizations; }
        }

        public IObjectSet<ImplementationGuideSchematronPattern> ImplementationGuideSchematronPatterns
        {
            get { return implementationGuideSchematronPatterns; }
        }

        public IObjectSet<PublishStatus> PublishStatuses
        {
            get { return publishStatuses; }
        }

        public IObjectSet<Role> Roles
        {
            get { return roles; }
        }

        public IObjectSet<AppSecurable> AppSecurables
        {
            get { return appSecurables; }
        }

        public IObjectSet<RoleAppSecurable> RoleAppSecurables
        {
            get { return roleAppSecurables; }
        }

        public IObjectSet<UserRole> UserRoles
        {
            get { return userRoles; }
        }

        public IObjectSet<User> Users
        {
            get { return users; }
        }

        public IObjectSet<RoleRestriction> RoleRestrictions
        {
            get { return roleRestrictions; }
        }

        public IObjectSet<Group> Groups
        {
            get { return groups; }
        }

        public IObjectSet<UserGroup> UserGroups
        {
            get { return userGroups; }
        }

        public IObjectSet<ImplementationGuidePermission> ImplementationGuidePermissions
        {
            get { return implementationGuidePermissions; }
        }

        public IObjectSet<OrganizationDefaultPermission> OrganizationDefaultPermissions
        {
            get { return organizationDefaultPermissions; }
        }

        public IObjectSet<TemplateConstraintSample> TemplateConstraintSamples
        {
            get { return templateConstraintSamples; }
        }

        public IObjectSet<TemplateSample> TemplateSamples
        {
            get { return templateSamples; }
        }

        public IObjectSet<ViewIGAuditTrail> ViewIGAuditTrails
        {
            get
            {
                return null;
            }
        }

        public IObjectSet<ImplementationGuideSection> ImplementationGuideSections
        {
            get
            {
                return implementationGuideSections;
            }
        }

        public IObjectSet<TemplateExtension> TemplateExtensions
        {
            get
            {
                return templateExtensions;
            }
        }

        public IObjectSet<ViewTemplatePermission> ViewTemplatePermissions
        {
            get
            {
                var results = (from ig in this.ImplementationGuides
                               join igp in this.implementationGuidePermissions on ig.Id equals igp.ImplementationGuideId
                               join t in this.Templates on ig.Id equals t.OwningImplementationGuideId
                               join u in this.Users on igp.UserId equals u.Id
                               select new ViewTemplatePermission()
                               {
                                   Permission = igp.Permission,
                                   TemplateId = t.Id,
                                   UserId = u.Id
                               })
                               .Union(from ig in this.ImplementationGuides
                                      join igp in this.ImplementationGuidePermissions on ig.Id equals igp.ImplementationGuideId
                                      join t in this.Templates on ig.Id equals t.OwningImplementationGuideId
                                      join ug in this.UserGroups on igp.GroupId equals ug.GroupId
                                      select new ViewTemplatePermission()
                                      {
                                          Permission = igp.Permission,
                                          TemplateId = t.Id,
                                          UserId = ug.UserId
                                      })
                               .Union(from ig in this.ImplementationGuides
                                      join igp in this.ImplementationGuidePermissions on ig.Id equals igp.ImplementationGuideId
                                      join t in this.Templates on ig.Id equals t.OwningImplementationGuideId
                                      join u in this.Users on igp.OrganizationId equals u.OrganizationId
                                      select new ViewTemplatePermission()
                                      {
                                          Permission = igp.Permission,
                                          TemplateId = t.Id,
                                          UserId = u.Id
                                      });

                return new MockDbSet<ViewTemplatePermission>(results);
            }
        }

        public IObjectSet<ViewImplementationGuidePermission> ViewImplementationGuidePermissions
        {
            get
            {
                var results = (from igp in this.implementationGuidePermissions 
                               join u in this.Users on igp.UserId equals u.Id
                               select new ViewImplementationGuidePermission()
                               {
                                   Permission = igp.Permission,
                                   ImplementationGuideId = igp.ImplementationGuideId,
                                   UserId = u.Id
                               })
                               .Union(from igp in this.ImplementationGuidePermissions
                                      join ug in this.UserGroups on igp.GroupId equals ug.GroupId
                                      select new ViewImplementationGuidePermission()
                                      {
                                          Permission = igp.Permission,
                                          ImplementationGuideId = igp.ImplementationGuideId,
                                          UserId = ug.UserId
                                      })
                               .Union(from igp in this.implementationGuidePermissions
                                      join u in this.Users on igp.OrganizationId equals u.OrganizationId
                                      select new ViewImplementationGuidePermission()
                                      {
                                          Permission = igp.Permission,
                                          ImplementationGuideId = igp.ImplementationGuideId,
                                          UserId = u.Id
                                      });

                return new MockDbSet<ViewImplementationGuidePermission>(results);
            }
        }

        public IObjectSet<ViewImplementationGuideTemplate> ViewImplementationGuideTemplates
        {
            get
            {
                var results = (from ig in this.ImplementationGuides
                               join t in this.Templates on ig.Id equals t.OwningImplementationGuideId
                               select new ViewImplementationGuideTemplate()
                               {
                                   ImplementationGuideId = ig.Id,
                                   Template = t,
                                   TemplateId = t.Id
                               })
                               .Union(
                               from ig in this.ImplementationGuides
                               join t in this.Templates on ig.PreviousVersionImplementationGuideId equals t.OwningImplementationGuideId
                               where this.Templates.Count(y => y.PreviousVersionTemplateId == t.Id) == 0
                               select new ViewImplementationGuideTemplate()
                               {
                                   ImplementationGuideId = ig.Id,
                                   Template = t,
                                   TemplateId = t.Id
                               });

                return new MockDbSet<ViewImplementationGuideTemplate>(results);                                           
            }
        }

        public IObjectSet<ViewUserSecurable> ViewUserSecurables
        {
            get
            {
                var results = (from ur in this.UserRoles
                               join asr in this.RoleAppSecurables on ur.RoleId equals asr.RoleId
                               join aps in this.AppSecurables on asr.AppSecurableId equals aps.Id
                               select new ViewUserSecurable()
                               {
                                   UserId = ur.UserId,
                                   SecurableName = aps.Name
                               }).Distinct();

                return new MockDbSet<ViewUserSecurable>(results);
            }
        }

        public IObjectSet<ViewImplementationGuideFile> ViewImplementationGuideFiles
        {
            get
            {
                var maxFileDataDates = (from ifgd in this.ImplementationGuideFileDatas 
                                        group ifgd by ifgd.ImplementationGuideFileId into g
                                        select new
                                        {
                                            ImplementationGuideFileId = g.Key,
                                            LastUpdateDate = g.Max(y => y.UpdatedDate)
                                        });

                var results = (from igf in this.ImplementationGuideFiles
                               join mfdd in maxFileDataDates on igf.Id equals mfdd.ImplementationGuideFileId
                               join igfd in this.ImplementationGuideFileDatas on mfdd.ImplementationGuideFileId equals igfd.ImplementationGuideFileId
                               where igfd.UpdatedDate == mfdd.LastUpdateDate
                               select new ViewImplementationGuideFile()
                               {
                                   ContentType = igf.ContentType,
                                   Data = igfd.Data,
                                   ExpectedErrorCount = igf.ExpectedErrorCount,
                                   FileName = igf.FileName,
                                   Id = igfd.Id,
                                   ImplementationGuideId = igf.ImplementationGuideId,
                                   MimeType = igf.MimeType,
                                   Note = igfd.Note,
                                   UpdatedBy = igfd.UpdatedBy,
                                   UpdatedDate = igfd.UpdatedDate
                               });

                return new MockDbSet<ViewImplementationGuideFile>(results);
            }
        }

        public IObjectSet<ViewTemplate> ViewTemplates
        {
            get
            {
                var results = (from t in this.Templates
                               select new ViewTemplate()
                               {
                                   ConstraintCount = t.ChildConstraints.Count,
                                   ContainedTemplateCount = 0,  // TODO: Fix
                                   Id = t.Id,
                                   ImplementationGuideTypeName = t.ImplementationGuideType.Name,
                                   ImpliedTemplateCount = t.ImplyingTemplates.Count,
                                   ImpliedTemplateOid = t.ImpliedTemplate != null ? t.ImpliedTemplate.Oid : null,
                                   ImpliedTemplateTitle = t.ImpliedTemplate != null ? t.ImpliedTemplate.Name : null,
                                   IsOpen = t.IsOpen,
                                   Name = t.Name,
                                   Oid = t.Oid,
                                   OrganizationName = t.Organization != null ? t.Organization.Name : null,
                                   OwningImplementationGuideId = t.OwningImplementationGuideId,
                                   OwningImplementationGuideTitle = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.Name : null,
                                   PrimaryContext = t.PrimaryContext,
                                   PublishDate = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.PublishDate : null,
                                   TemplateTypeDisplay = t.TemplateType != null ? t.TemplateType.Name : null,
                                   TemplateTypeId = t.TemplateTypeId,
                                   TemplateTypeName = t.TemplateType != null ? t.TemplateType.Name : null
                               }).Distinct();

                return new MockDbSet<ViewTemplate>(results);
            }
        }

        public IObjectSet<ViewTemplateList> ViewTemplateLists
        {
            get
            {
                var results = (from t in this.Templates
                               select new ViewTemplateList()
                               {
                                   Id = t.Id,
                                   Oid = t.Oid,
                                   Name = t.Name,
                                   Open = t.IsOpen ? "Yes" : "No",
                                   Organization = t.Organization != null ? t.Organization.Name : null,
                                   ImplementationGuide = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.Name : null,
                                   PublishDate = t.OwningImplementationGuide != null ? t.OwningImplementationGuide.PublishDate : null,
                                   TemplateType = t.TemplateType.Name + " (" + t.TemplateType.ImplementationGuideType.Name + ")",
                                   ImpliedTemplateName = t.ImpliedTemplate != null ? t.ImpliedTemplate.Name : null,
                                   ImpliedTemplateOid = t.ImpliedTemplate != null ? t.ImpliedTemplate.Oid : null
                                   // TODO: Fill in other fields
                               }).Distinct();

                return new MockDbSet<ViewTemplateList>(results);
            }
        }

        #endregion

        #region Generic Test Data Generation

        public Organization FindOrAddOrganization(string name, string contactName = null, string contactEmail = null, string contactPhone = null)
        {
            Organization foundOrganization = this.organizations.SingleOrDefault(y => y.Name == name);

            if (foundOrganization == null)
            {
                foundOrganization = new Organization()
                {
                    Id = this.Organizations.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    ContactName = contactName,
                    ContactEmail = contactEmail,
                    ContactPhone = contactPhone
                };
                this.Organizations.AddObject(foundOrganization);
            }

            return foundOrganization;
        }

        public Template GenerateTemplate(
            string oid, 
            string typeName, 
            string title, 
            ImplementationGuide owningImplementationGuide, 
            string primaryContext = null, 
            string primaryContextType = null, 
            string description = null, 
            string notes = null, 
            Organization organization = null, 
            Template impliedTemplate = null,
            Template previousVersion = null,
            string status = null)
        {
            TemplateType templateType = this.FindTemplateType(owningImplementationGuide.ImplementationGuideType.Name, typeName);
            PublishStatus publishStatus = null;

            if (!string.IsNullOrEmpty(status))
                publishStatus = this.publishStatuses.Single(y => y.Status == status);

            var template = GenerateTemplate(oid, templateType, title, owningImplementationGuide, primaryContext, primaryContextType, description, notes, organization, impliedTemplate, publishStatus);

            if (previousVersion != null)
                template.SetPreviousVersion(previousVersion);

            return template;
        }
        
        /// <summary>
        /// Creates a new instance of a template with the specified parameters.
        /// </summary>
        /// <remarks>Generates a bookmark automatically based on the title and type of the template.</remarks>
        /// <param name="oid">Required. The oid of the template</param>
        /// <param name="type">Required. The type of template</param>
        /// <param name="title">Required. The title of the template</param>
        /// <param name="description">The description of the template</param>
        /// <param name="notes">The notes of the template</param>
        /// <param name="owningImplementationGuide">The implementation guide that owns the template.</param>
        /// <returns>A new instance of Template that has been appropriately added to the mock object repository.</returns>
        public Template GenerateTemplate(string oid, TemplateType type, string title, ImplementationGuide owningImplementationGuide, string primaryContext = null, string primaryContextType = null, string description = null, string notes = null, Organization organization = null, Template impliedTemplate = null, PublishStatus status = null)
        {
            if (string.IsNullOrEmpty(oid))
                throw new ArgumentNullException("oid");

            if (type == null)
                throw new ArgumentNullException("type");

            if (string.IsNullOrEmpty("title"))
                throw new ArgumentNullException("title");

            if (status == null)
                status = PublishStatus.GetDraftStatus(this);

            Template template = new Template()
            {
                Id = this.Templates.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                OrganizationId = organization != null ? new Nullable<int>(organization.Id) : null,
                Organization = organization,
                OwningImplementationGuideId = owningImplementationGuide.Id,
                OwningImplementationGuide = owningImplementationGuide,
                ImplementationGuideTypeId = type.ImplementationGuideTypeId,
                ImplementationGuideType = type.ImplementationGuideType,
                ImpliedTemplateId = impliedTemplate != null ? (int?)impliedTemplate.Id : null,
                ImpliedTemplate = impliedTemplate,
                PrimaryContext = primaryContext,
                PrimaryContextType = primaryContextType,
                TemplateType = type,
                TemplateTypeId = type.Id,
                Name = title,
                Oid = oid,
                Description = description,
                Notes = notes,
                Status = status,
                StatusId = status.Id
            };

            template.Bookmark = title
                .Replace(" ", "_")
                .Replace("\t", "_");

            switch (type.Id)
            {
                case 1:
                    template.Bookmark = "D_" + template.Bookmark;
                    break;
                case 2:
                    template.Bookmark = "S_" + template.Bookmark;
                    break;
                case 3:
                    template.Bookmark = "E_" + template.Bookmark;
                    break;
                case 4:
                    template.Bookmark = "SE_" + template.Bookmark;
                    break;
                case 5:
                    template.Bookmark = "O_" + template.Bookmark;
                    break;
            }

            this.Templates.AddObject(template);

            if (owningImplementationGuide != null)
                owningImplementationGuide.ChildTemplates.Add(template);

            return template;
        }

        /// <summary>
        /// Creates a new instance of a GreenConstraint, associates it to the TemplateConstraint and adds it to the object context.
        /// </summary>
        /// <param name="gt">Required. The green template of the green constraint.</param>
        /// <param name="tc">Required. The TemplateConstraint that the green constraint should be based on.</param>
        /// <param name="parent">The parent green constraint, if any.</param>
        /// <param name="order">The order of green constraint within the green template.</param>
        /// <param name="name">Required. The business name for the green constraint.</param>
        /// <param name="isEditable">Whether or not the green constraint is an XML-editable constraint.</param>
        /// <returns>A new instance of GreenConstraint that has been appropriately added to the mock object repository.</returns>
        public GreenConstraint GenerateGreenConstraint(GreenTemplate gt, TemplateConstraint tc, GreenConstraint parent, int order, string name, bool isEditable)
        {
            if (gt == null)
                throw new ArgumentNullException("gt");

            if (tc == null)
                throw new ArgumentNullException("tc");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            GreenConstraint gc = new GreenConstraint()
            {
                Id = this.GreenConstraints.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                GreenTemplate = gt,
                GreenTemplateId = gt.Id,
                TemplateConstraint = tc,
                TemplateConstraintId = tc.Id,
                ParentGreenConstraint = parent != null ? parent : null,
                ParentGreenConstraintId = parent != null ? new Nullable<int>(parent.Id) : null,
                Order = order,
                Name = name,
                IsEditable = isEditable
            };

            this.GreenConstraints.AddObject(gc);
            tc.GreenConstraints.Add(gc);
            gt.ChildGreenConstraints.Add(gc);

            return gc;
        }

        /// <summary>
        /// Generates a data-type for the specified name that can be used by the green libraries.
        /// </summary>
        /// <param name="igType">The implementation guide type for the data type</param>
        /// <param name="name">The name of the data-type</param>
        /// <returns>A new instance of DataType that has been appropriately added to the mock object repository.</returns>
        public ImplementationGuideTypeDataType GenerateDataType(ImplementationGuideType igType, string name)
        {
            ImplementationGuideTypeDataType dt = this.ImplementationGuideTypeDataTypes.SingleOrDefault(y => y.ImplementationGuideTypeId == igType.Id && y.DataTypeName == name);

            if (dt == null)
            {
                dt = new ImplementationGuideTypeDataType()
                {
                    Id = this.ImplementationGuideTypeDataTypes.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    ImplementationGuideType = igType,
                    ImplementationGuideTypeId = igType.Id,
                    DataTypeName = name
                };

                this.ImplementationGuideTypeDataTypes.AddObject(dt);
                igType.DataTypes.Add(dt);
            }

            return dt;
        }

        /// <summary>
        /// Attempts to find the implementation guide type based on the name specified.
        /// If none can be found, a new implementation guide type is created and added to the mock object repository.
        /// </summary>
        /// <param name="name">Required. The name of the implementation guide type. This value is used to perform the search.</param>
        /// <param name="schemaLocation">Required. The location of the schema for the implementation guide type.</param>
        /// <param name="prefix">The prefix of the schema for the implementation guide type.</param>
        /// <param name="uri">The namespace uri of the schema for the implementation guide type.</param>
        /// <returns>Either the found implementation guide type, or a new implementation guide tpye that has been appropriately added to the mock object repository.</returns>
        public ImplementationGuideType FindOrCreateImplementationGuideType(string name, string schemaLocation, string prefix, string uri)
        {
            ImplementationGuideType igType = this.ImplementationGuideTypes.SingleOrDefault(y => y.Name == name);

            if (igType == null)
            {
                igType = new ImplementationGuideType()
                {
                    Id = this.ImplementationGuideTypes.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    SchemaLocation = schemaLocation,
                    SchemaPrefix = prefix,
                    SchemaURI = uri
                };

                this.implementationGuideTypes.AddObject(igType);
            }

            return igType;
        }

        /// <summary>
        /// Attempts to find a template type based on the implementation guide type and name specified.
        /// If none can be found, a new instance is created and added to the mock object repository and ig type.
        /// </summary>
        /// <param name="igType">Required. The implementation guide type. This value is used to perform the search.</param>
        /// <param name="name">The name of the template type. This value is used to perform the search.</param>
        /// <param name="context">The default contetx of the template type.</param>
        /// <returns>Either the found template type, or a new one which has been added to the mock object repository and the ig type.</returns>
        public TemplateType FindOrCreateTemplateType(ImplementationGuideType igType, string name, string context, string contextType, int outputOrder)
        {
            if (igType == null)
                throw new ArgumentNullException("igType");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            TemplateType type = this.TemplateTypes.SingleOrDefault(y => y.ImplementationGuideTypeId == igType.Id && y.Name == name);

            if (type == null)
            {
                type = new TemplateType()
                {
                    Id = this.TemplateTypes.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    ImplementationGuideType = igType,
                    ImplementationGuideTypeId = igType.Id,
                    RootContext = context,
                    RootContextType = contextType,
                    OutputOrder = outputOrder
                };

                igType.TemplateTypes.Add(type);
                this.TemplateTypes.AddObject(type);
            }

            return type;
        }

        public ImplementationGuide FindOrAddImplementationGuide(string igTypeName, string title, Organization organization = null, DateTime? publishDate = null, ImplementationGuide previousVersion = null)
        {
            ImplementationGuideType igType = this.FindImplementationGuideType(igTypeName);

            var ig = FindOrAddImplementationGuide(igType, title, organization, publishDate);

            if (previousVersion != null)
                ig.SetPreviousVersion(previousVersion);

            return ig;
        }

        /// <summary>
        /// Craetes a new implementation guide for the specified implementation guide type.
        /// The new implementation guide is added to the mock object repository and the implementation guide type.
        /// </summary>
        /// <param name="igType">Required. The implementation guide type for the implementation guide (ex: CDA vs eMeasure)</param>
        /// <param name="title">Required. The title of the implementation guide.</param>
        /// <param name="organization">The organization for the implementation guide.</param>
        /// <param name="publishDate">The publishing date for the implementation guide.</param>
        /// <returns>A new instance of an implementation guide that has been added to the mock object repository.</returns>
        public ImplementationGuide FindOrAddImplementationGuide(ImplementationGuideType igType, string title, Organization organization = null, DateTime? publishDate = null)
        {
            if (igType == null)
                throw new ArgumentNullException("igType");

            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException("title");

            ImplementationGuide ig = this.implementationGuides.SingleOrDefault(y => y.ImplementationGuideType == igType && y.Name.ToLower() == title.ToLower());

            if (organization == null)
                organization = this.FindOrAddOrganization(DEFAULT_ORGANIZATION);

            if (ig == null)
            {
                ig = new ImplementationGuide()
                {
                    Id = this.ImplementationGuides.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = title,
                    ImplementationGuideType = igType,
                    ImplementationGuideTypeId = igType.Id,
                    Organization = organization,
                    OrganizationId = organization.Id,
                    PublishDate = publishDate,
                    Version = 1
                };

                igType.ImplementationGuides.Add(ig);
                this.ImplementationGuides.AddObject(ig);
            }

            return ig;
        }

        public TemplateConstraint GeneratePrimitive(
            Template template, 
            TemplateConstraint parent, 
            string conformanceText, 
            string narrative, 
            string schematronTest = null, 
            bool? isBranch = null, 
            bool isPrimitiveSchRooted=false, 
            bool isInheritable=false, 
            int? number = null)
        {
            TemplateConstraint constraint = new TemplateConstraint()
            {
                Id = this.TemplateConstraints.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                IsPrimitive = true,
                Template = template,
                TemplateId = template.Id,
                Conformance = conformanceText,
                PrimitiveText = narrative,
                Schematron = schematronTest,
                IsBranch = isBranch != null ? isBranch.Value : false,
                IsSchRooted = isPrimitiveSchRooted,
                IsInheritable = isInheritable
            };

            if (number == null)
                constraint.Number = constraint.Id;
            else
                constraint.Number = number;

            if (parent != null)
            {
                constraint.ParentConstraint = parent;
                constraint.ParentConstraintId = parent.Id;
            }
            
            template.ChildConstraints.Add(constraint);
            this.TemplateConstraints.AddObject(constraint);

            return constraint;
        }

        /// <summary>
        /// Creates a new constraint for the specified template.
        /// The constraint is added to the mock object repository and the template.
        /// </summary>
        /// <param name="template">Required. The template for the constraint.</param>
        /// <param name="parentConstraint">The parent constraint.</param>
        /// <param name="containedTemplate">The contained template.</param>
        /// <param name="context">The context of the constraint.</param>
        /// <param name="conformance">The conformance of the constraint (SHALL, SHOULD, MAY, etc)</param>
        /// <param name="cardinality">The cardinality of the constraint ("1..1", "0..1", etc)</param>
        /// <param name="dataType">The data type of the constraint ("CD")</param>
        /// <param name="valueConformance">The value conformance of the constraint.</param>
        /// <param name="value">The value of the constraint (ex: "23423-X")</param>
        /// <param name="displayName">The display name for the value of the constraint.</param>
        /// <param name="valueSet">The valueset associated with the constraint.</param>
        /// <param name="codeSystem">The code system associated with the constraint.</param>
        /// <param name="description">The description of the constraint.</param>
        /// <returns>A new instance of TemplateConstraint that has been added to the specified Template's child constraints and to the mock object repository.</returns>
        public TemplateConstraint GenerateConstraint(
            Template template, 
            TemplateConstraint parentConstraint, 
            Template containedTemplate, 
            string context, 
            string conformance, 
            string cardinality, 
            string dataType = null, 
            string valueConformance = null, 
            string value = null, 
            string displayName = null, 
            ValueSet valueSet = null, 
            CodeSystem codeSystem = null,
            string description = null,
            bool? isBranch = null,
            bool? isBranchIdentifier = null,
            bool isPrimitiveSchRooted = false,
            int? number = null,
            string category = null)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            TemplateConstraint constraint = new TemplateConstraint()
            {
                Id = this.TemplateConstraints.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                Template = template,
                Context = context,
                Conformance = conformance,
                ValueConformance = valueConformance,
                TemplateId = template.Id,
                IsSchRooted = isPrimitiveSchRooted,
                Category = category
            };

            if (number == null)
                constraint.Number = constraint.Id;
            else
                constraint.Number = number;

            if (parentConstraint != null)
            {
                constraint.ParentConstraint = parentConstraint;
                constraint.ParentConstraintId = parentConstraint.Id;
            }

            if (containedTemplate != null)
            {
                constraint.ContainedTemplate = containedTemplate;
                constraint.ContainedTemplateId = containedTemplate.Id;
            }

            if (!string.IsNullOrEmpty(cardinality))
                constraint.Cardinality = cardinality;

            if (!string.IsNullOrEmpty(dataType))
                constraint.DataType = dataType;

            if (!string.IsNullOrEmpty(value))
                constraint.Value = value;

            if (!string.IsNullOrEmpty(displayName))
                constraint.DisplayName = displayName;

            if (valueSet != null)
            {
                constraint.ValueSet = valueSet;
                constraint.ValueSetId = valueSet.Id;
            }

            if (codeSystem != null)
            {
                constraint.CodeSystem = codeSystem;
                constraint.CodeSystemId = codeSystem.Id;
            }

            if (!string.IsNullOrEmpty(description))
                constraint.Description = description;

            if (isBranch != null)
                constraint.IsBranch = isBranch.Value;

            if (isBranchIdentifier != null)
                constraint.IsBranchIdentifier = isBranchIdentifier.Value;

            template.ChildConstraints.Add(constraint);
            this.TemplateConstraints.AddObject(constraint);

            return constraint;
        }

        /// <summary>
        /// Attempts to find a Code System based on the oid specified.
        /// If none is found, a new instance is created and added to the mock object repository.
        /// </summary>
        /// <param name="name">Required. The name of the code system (Ex: "SNOMED CT")</param>
        /// <param name="oid">Required. The oid of the code system. This value is used to perform the search.</param>
        /// <param name="description">The description of the code system.</param>
        /// <param name="lastUpdate">The last update date of the code system.</param>
        /// <returns></returns>
        public CodeSystem FindOrCreateCodeSystem(string name, string oid, string description = null, DateTime? lastUpdate = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(oid))
                throw new ArgumentNullException("oid");

            CodeSystem codeSystem = this.CodeSystems.SingleOrDefault(y => y.Oid == oid);

            if (codeSystem == null)
            {
                codeSystem = new CodeSystem()
                {
                    Id = this.CodeSystems.Count() > 0 ? this.CodeSystems.Max(y => y.Id) + 1 : 1,
                    Name = name,
                    Oid = oid,
                    Description = description,
                    LastUpdate = lastUpdate != null ? lastUpdate.Value : DateTime.Now
                };

                this.CodeSystems.AddObject(codeSystem);
            }

            return codeSystem;
        }

        /// <summary>
        /// Attempts to find a Value Set based on the oid specified.
        /// If none is found, a new instance is created and added to the mock object repository.
        /// </summary>
        /// <param name="name">Required. The name of the value set.</param>
        /// <param name="oid">Required. The oid of the value set. This value is used to perform the search.</param>
        /// <param name="code">The code of the value set.</param>
        /// <param name="description">The description of the valueset.</param>
        /// <param name="intensional">Indicates whether the value set is intensional or extensional.</param>
        /// <param name="intensionalDefinition">Describes the algorithm used for intensional value sets.</param>
        /// <param name="lastUpdate">The last update date of the value set.</param>
        /// <returns>Either the found ValueSet or a new instance of one, which has been added to the mock object repository.</returns>
        public ValueSet FindOrCreateValueSet(string name, string oid, string code = null, string description = null, bool? intensional = null, string intensionalDefinition = null, DateTime? lastUpdate = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(oid))
                throw new ArgumentNullException("oid");

            ValueSet valueSet = this.ValueSets.SingleOrDefault(y => y.Oid == oid);

            if (valueSet == null)
            {
                valueSet = new ValueSet()
                {
                    Id = this.ValueSets.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    Name = name,
                    Oid = oid,
                    Description = intensionalDefinition,
                    Intensional = intensional,
                    IntensionalDefinition = intensionalDefinition,
                    Code = code,
                    LastUpdate = lastUpdate != null ? lastUpdate.Value : DateTime.Now
                };

                this.ValueSets.AddObject(valueSet);
            }

            return valueSet;
        }

        /// <summary>
        /// Attempts to find a ValueSetMember based on the valueset and code specified.
        /// If none is found, a new instance is created and added to the mock object repository.
        /// </summary>
        /// <param name="valueSet">Required. The valueset owning the member. This value is used to perform the search.</param>
        /// <param name="codeSystem">Required. The code system for the member.</param>
        /// <param name="code">Required. The code for the member. This value is used to perform the search.</param>
        /// <param name="displayName">The display name of the member.</param>
        /// <param name="valueSetStatus">The status of the member (ex: "active" or "inactive").</param>
        /// <param name="dateOfValueSetStatus">The date that the status of the member was set.</param>
        /// <param name="lastUpdate">The last update date of the value set member.</param>
        /// <returns>Either the found ValueSetMember, or a new instance of one which has been added to the valueset and the mock object repository.</returns>
        public ValueSetMember FindOrCreateValueSetMember(ValueSet valueSet, CodeSystem codeSystem, string code, string displayName, string valueSetStatus = null, string dateOfValueSetStatus = null, DateTime? lastUpdate = null)
        {
            if (valueSet == null)
                throw new ArgumentNullException("valueSet");

            if (codeSystem == null)
                throw new ArgumentNullException("codeSystem");

            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException("code");
            
            DateTime? statusDate = !string.IsNullOrEmpty(dateOfValueSetStatus) ? new DateTime?(DateTime.Parse(dateOfValueSetStatus)) : null;
            ValueSetMember member = this.ValueSetMembers.SingleOrDefault(y => y.ValueSetId == valueSet.Id && y.Code == code && y.Status == valueSetStatus && y.StatusDate == statusDate);

            if (member == null)
            {
                member = new ValueSetMember()
                {
                    Id = this.ValueSetMembers.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    ValueSet = valueSet,
                    ValueSetId = valueSet.Id,
                    CodeSystem = codeSystem,
                    CodeSystemId = codeSystem.Id,
                    Code = code,
                    DisplayName = displayName,
                    Status = valueSetStatus,
                    StatusDate = statusDate
                };

                this.ValueSetMembers.AddObject(member);
            }

            return member;
        }

        public ImplementationGuideFile GenerateImplementationGuideFile(ImplementationGuide ig, string fileName, string contentType, string mimeType, int? expectedErrorCount=null, byte[] content=null)
        {
            ImplementationGuideFile igf = new ImplementationGuideFile()
            {
                Id = this.ImplementationGuideFiles.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                FileName = fileName,
                ContentType = contentType,
                MimeType = mimeType,
                ExpectedErrorCount = expectedErrorCount,
                ImplementationGuide = ig,
                ImplementationGuideId = ig.Id
            };

            if (content != null)
            {
                this.GenerateImplementationGuideFileData(igf, content);
            }

            this.ImplementationGuideFiles.AddObject(igf);
            ig.Files.Add(igf);

            return igf;
        }

        public ImplementationGuideFileData GenerateImplementationGuideFileData(ImplementationGuideFile igFile, byte[] content, string note=null, DateTime? updatedDate = null)
        {
            ImplementationGuideFileData igFileData = new ImplementationGuideFileData()
            {
                Id = this.ImplementationGuideFileDatas.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                ImplementationGuideFile = igFile,
                ImplementationGuideFileId = igFile.Id,
                Data = content,
                Note = note,
                UpdatedDate = updatedDate != null ? updatedDate.Value : DateTime.Now,
                UpdatedBy = "unit test"
            };

            this.ImplementationGuideFileDatas.AddObject(igFileData);
            igFile.Versions.Add(igFileData);

            return igFileData;
        }

        public User FindOrAddUser(string username, Organization organization)
        {
            User foundUser = this.Users.SingleOrDefault(y => y.UserName.ToLower() == username.ToLower());

            if (foundUser != null)
                return foundUser;

            User newUser = new User()
            {
                Id = this.Users.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                UserName = username,
                OrganizationId = organization.Id,
                Organization = organization
            };

            this.Users.AddObject(newUser);

            return newUser;
        }

        public Role FindOrAddRole(string roleName)
        {
            Role role = this.Roles.SingleOrDefault(y => y.Name.ToLower() == roleName.ToLower());

            if (role != null)
                return role;

            role = new Role()
            {
                Id = this.Roles.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                Name = roleName
            };

            this.Roles.AddObject(role);

            return role;
        }

        public void AddRole(string roleName, params string[] securables)
        {
            Role role = FindOrAddRole(roleName);

            AssociateSecurableToRole(roleName, securables);
        }

        public void AssociateSecurableToRole(string roleName, params string[] securables)
        {
            Role role = FindOrAddRole(roleName);

            foreach (string cSecurable in securables)
            {
                AppSecurable securable = FindOrAddSecurable(cSecurable);

                RoleAppSecurable newRoleAppSecurable = new RoleAppSecurable()
                {
                    Id = this.RoleAppSecurables.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                    AppSecurableId = securable.Id,
                    AppSecurable = securable,
                    RoleId = role.Id,
                    Role = role
                };

                this.RoleAppSecurables.AddObject(newRoleAppSecurable);
                role.AppSecurables.Add(newRoleAppSecurable);
            }
        }

        public void RemoveSecurableFromRole(string roleName, params string[] securables)
        {
            Role role = FindOrAddRole(roleName);

            foreach (string cSecurable in securables)
            {
                AppSecurable securable = FindOrAddSecurable(cSecurable);

                RoleAppSecurable foundRoleSecurable = this.RoleAppSecurables.Single(y => y.Role == role && y.AppSecurable == securable);

                if (foundRoleSecurable != null)
                {
                    role.AppSecurables.Remove(foundRoleSecurable);
                    this.RoleAppSecurables.DeleteObject(foundRoleSecurable);
                }
            }
        }

        public void AssociateUserWithRole(string userName, string organizationName, string roleName)
        {
            Organization foundOrg = this.Organizations.Single(y => y.Name.ToLower() == organizationName.ToLower());

            AssociateUserWithRole(userName, foundOrg.Id, roleName);
        }

        public void AssociateUserWithRole(string userName, int organizationId, string roleName)
        {
            User foundUser = this.Users.Single(y => y.UserName.ToLower() == userName.ToLower() && y.OrganizationId == organizationId);
            Role foundRole = this.Roles.Single(y => y.Name.ToLower() == roleName.ToLower());

            if (foundUser.Roles.Count(y => y.RoleId == foundRole.Id) > 0)
                return;

            UserRole newUserRole = new UserRole()
            {
                Id = this.UserRoles.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                UserId = foundUser.Id,
                User = foundUser,
                RoleId = foundRole.Id,
                Role = foundRole
            };

            foundUser.Roles.Add(newUserRole);
            this.UserRoles.AddObject(newUserRole);
        }

        public AppSecurable FindOrAddSecurable(string securableName, string description = null)
        {
            AppSecurable securable = this.AppSecurables.SingleOrDefault(y => y.Name.ToLower() == securableName.ToLower());

            if (securable != null)
                return securable;

            securable = new AppSecurable()
            {
                Id = this.AppSecurables.DefaultIfEmpty().Max(y => y != null ? y.Id : 0) + 1,
                Name = securableName,
                Description = description
            };

            this.AppSecurables.AddObject(securable);

            return securable;
        }

        public void FindOrAddSecurables(params string[] securableNames)
        {
            foreach (string securableName in securableNames)
            {
                this.FindOrAddSecurable(securableName);
            }
        }

        public void GrantImplementationGuidePermission(User user, ImplementationGuide ig, string permission)
        {
            var newPerm = new ImplementationGuidePermission()
            {
                ImplementationGuide = ig,
                ImplementationGuideId = ig.Id,
                User = user,
                UserId = user.Id,
                Permission = permission,
                Type = "User"
            };

            this.ImplementationGuidePermissions.AddObject(newPerm);
            ig.Permissions.Add(newPerm);
        }

        public ImplementationGuideTypeDataType FindOrAddDataType(string igTypeName, string dataTypeName)
        {
            var igType = this.FindImplementationGuideType(igTypeName);
            var dataType = igType.DataTypes.SingleOrDefault(y => y.DataTypeName == dataTypeName);

            if (dataType == null)
            {
                dataType = new ImplementationGuideTypeDataType()
                {
                    DataTypeName = dataTypeName,
                    ImplementationGuideType = igType
                };

                this.ImplementationGuideTypeDataTypes.AddObject(dataType);
            }

            return dataType;
        }

        #endregion

        #region Simple Find Methods

        public ImplementationGuideType FindImplementationGuideType(string igType)
        {
            return this.ImplementationGuideTypes.SingleOrDefault(y => y.Name == igType);
        }

        public TemplateType FindTemplateType(string igType, string templateType)
        {
            return this.TemplateTypes.SingleOrDefault(y => y.ImplementationGuideType.Name == igType && y.Name == templateType);
        }

        #endregion

        #region Unimplemented Methods

        public DbConnection Connection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Refresh(RefreshMode refreshMode, Object entity)
        {

        }

        public void ChangeObjectState(object entity, System.Data.Entity.EntityState entityState)
        {
            
        }

        public System.Data.IDbTransaction BeginTransaction()
        {
            return null;
        }

        public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return null;
        }


        public int SaveChanges()
        {
            return 0;
        }

        public void Dispose()
        {

        }

        #endregion

        #region Stored Procedures

        private IEnumerable<Template> GetTemplateReferences(List<Template> checkedTemplates, Template template, bool includeInferred)
        {
            List<Template> templates = new List<Template>();

            if (checkedTemplates.Contains(template))
                return templates;

            // Add the template to the list of checked templates right away, because we know we are going to check it
            // and in case one of the contained or implied template references itself, directly.
            checkedTemplates.Add(template);

            foreach (var constraint in template.ChildConstraints.Where(y => y.ContainedTemplate != null))
            {
                var templateAlreadyIncluded = templates.Contains(constraint.ContainedTemplate);
                var externalTemplate = constraint.ContainedTemplate.OwningImplementationGuideId != template.OwningImplementationGuideId;

                if ((!externalTemplate || includeInferred) && !templateAlreadyIncluded)
                {
                    templates.Add(constraint.ContainedTemplate);

                    templates.AddRange(
                        GetTemplateReferences(checkedTemplates, constraint.ContainedTemplate, includeInferred));
                }
            }

            if (template.ImpliedTemplate != null)
            {
                var externalTemplate = template.ImpliedTemplate.OwningImplementationGuideId != template.OwningImplementationGuideId;
                var templateAlreadyIncluded = templates.Contains(template.ImpliedTemplate);

                if ((!externalTemplate || includeInferred) && !templateAlreadyIncluded)
                {
                    templates.Add(template.ImpliedTemplate);

                    templates.AddRange(
                        GetTemplateReferences(checkedTemplates, template.ImpliedTemplate, includeInferred));
                }
            }

            return templates.Distinct();
        }

        public ObjectResult<SearchValueSetResult> SearchValueSet(Nullable<int> userId, global::System.String searchText, Nullable<global::System.Int32> count, Nullable<global::System.Int32> page, global::System.String orderProperty, Nullable<global::System.Boolean> orderDesc)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId)
        {
            var implementationGuide = this.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            // A list of templates that will be used by GetTemplateReferences() to determine
            // if a template has already been checked, so that an endless loop does not occur
            List<Template> checkedTemplates = new List<Template>();

            List<Template> templates = new List<Template>();
            var currentVersionId = (Nullable<int>)implementationGuide.Id;
            var retiredStatus = PublishStatus.GetRetiredStatus(this);

            while (currentVersionId.HasValue)
            {
                ImplementationGuide currentVersion = this.ImplementationGuides.Single(ig => ig.Id == currentVersionId);

                // Ignore previous version IG templates that have been retired
                var childTemplates = currentVersion.ChildTemplates.Where(y => y.OwningImplementationGuideId == implementationGuide.Id || y.Status != retiredStatus);

                // Loop through all templates in the current version
                foreach (Template lTemplate in childTemplates)
                {
                    List<Template> nextVersionTemplates = new List<Template>();
                    var nextCurrent = lTemplate;

                    // Build a list of all this template's next ids
                    while (nextCurrent != null)
                    {
                        nextVersionTemplates.Add(nextCurrent);

                        // Make sure we stop at this version of the IG, and don't look at future versions of the IG
                        if (nextCurrent.OwningImplementationGuideId != implementationGuide.Id)
                            nextCurrent = nextCurrent.NextVersion.FirstOrDefault();
                        else
                            nextCurrent = null;
                    }

                    // Skip the tempalte if it has been retired in a future version
                    if (nextVersionTemplates.Count(y => y.Status == retiredStatus && y.OwningImplementationGuideId != implementationGuide.Id) > 0)
                        continue;

                    // Look if the previous version has a newer version already in the list
                    IEnumerable<Template> lNewerVersions = from t in templates
                                                           join nt in nextVersionTemplates on t.PreviousVersion equals nt
                                                           select t;

                    // If there are no newer versions of the template (which means it is not included yet), add it to the list
                    if (lNewerVersions.Count() == 0)
                    {
                        templates.Add(lTemplate);

                        templates.AddRange(
                            GetTemplateReferences(checkedTemplates, lTemplate, inferred == null ? true : inferred.Value));
                    }
                }

                // Move to the previous version of the IG
                currentVersionId = currentVersion.PreviousVersionImplementationGuideId;
            }

            return templates.Select(y => (int?)y.Id);
        }

        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.GetImplementationGuideTemplates(Nullable<global::System.Int32> implementationGuideId, Nullable<global::System.Boolean> inferred, Nullable<global::System.Int32> parentTemplateId, string[] categories)
        {
            return ((IObjectRepository)this).GetImplementationGuideTemplates(implementationGuideId, inferred, parentTemplateId);
        }

        IEnumerable<Nullable<global::System.Int32>> IObjectRepository.SearchTemplates(Nullable<global::System.Int32> userId, Nullable<global::System.Int32> filterImplementationGuideId, global::System.String filterName, global::System.String filterIdentifier, Nullable<global::System.Int32> filterTemplateTypeId, Nullable<global::System.Int32> filterOrganizationId, global::System.String filterContextType, global::System.String queryText)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    #region MockDbSet

    public class MockDbSet<T> : IObjectSet<T> where T : EntityObject
    {
        private List<T> theList;
        private EnumerableQuery<T> theEnumerableQuery;

        public MockDbSet()
        {
            theList = new List<T>();
            theEnumerableQuery = new EnumerableQuery<T>(theList);
        }

        public MockDbSet(IEnumerable<T> items)
        {
            theList = new List<T>(items);
            theEnumerableQuery = new EnumerableQuery<T>(theList);
        }

        public void AddObject(T entity)
        {
            InitializeKey(this, entity);
            this.theList.Add(entity);
        }

        public void Attach(T entity)
        {
            
        }

        public void Detach(T entity)
        {

        }

        public void DeleteObject(T entity)
        {
            this.theList.Remove(entity);
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return default(TDerivedEntity);
        }

        public T Create()
        {
            return default(T);
        }

        public T Find(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ObservableCollection<T> Local
        {
            get { throw new NotImplementedException(); }
        }

        public T Remove(T entity)
        {
            this.theList.Remove(entity);
            return entity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return theList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return theList.GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider
        {
            get { return theEnumerableQuery; }
        }


        public System.Linq.Expressions.Expression Expression
        {
            get { return ((IQueryable)this.theEnumerableQuery).Expression; }
        }

        private void InitializeKey(IEnumerable<EntityObject> list, EntityObject entity)
        {
            PropertyInfo keyProperty = null;

            foreach (PropertyInfo itemProperty in entity.GetType().GetProperties())
            {
                EdmScalarPropertyAttribute attr = itemProperty
                    .GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false)
                    .FirstOrDefault() as EdmScalarPropertyAttribute;

                if (attr != null && attr.EntityKeyProperty && itemProperty.PropertyType == typeof(int))
                {
                    keyProperty = itemProperty;
                    break;
                }
            }

            int keyValue = (int)keyProperty.GetValue(entity);

            // Need to generate a key value
            if (keyValue <= 0)
            {
                int maxKeyValue = 0;

                foreach (EntityObject item in list)
                {
                    int itemKeyValue = (int)keyProperty.GetValue(item);

                    if (itemKeyValue > maxKeyValue)
                        maxKeyValue = itemKeyValue;
                }

                keyProperty.SetValue(entity, maxKeyValue + 1);
            }
        }
    }

    #endregion
}
