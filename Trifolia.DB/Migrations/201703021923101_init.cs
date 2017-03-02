namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.app_securable",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 50),
                        displayName = c.String(maxLength: 255),
                        description = c.String(storeType: "ntext"),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.appsecurable_role",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        appSecurableId = c.Int(nullable: false),
                        roleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.role", t => t.roleId, cascadeDelete: true)
                .ForeignKey("dbo.app_securable", t => t.appSecurableId)
                .Index(t => t.appSecurableId)
                .Index(t => t.roleId);
            
            CreateTable(
                "dbo.role",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(maxLength: 50),
                        isDefault = c.Boolean(nullable: false),
                        isAdmin = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.role_restriction",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        roleId = c.Int(nullable: false),
                        organizationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.organization", t => t.organizationId, cascadeDelete: true)
                .ForeignKey("dbo.role", t => t.roleId, cascadeDelete: true)
                .Index(t => t.roleId)
                .Index(t => t.organizationId);
            
            CreateTable(
                "dbo.organization",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 255),
                        contactName = c.String(maxLength: 128),
                        contactEmail = c.String(maxLength: 255),
                        contactPhone = c.String(maxLength: 50),
                        authProvider = c.String(maxLength: 1024),
                        isInternal = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.implementationguide",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideTypeId = c.Int(nullable: false),
                        organizationId = c.Int(),
                        name = c.String(nullable: false, maxLength: 255),
                        publishDate = c.DateTime(),
                        publishStatusId = c.Int(),
                        previousVersionImplementationGuideId = c.Int(),
                        version = c.Int(),
                        displayName = c.String(maxLength: 255, unicode: false),
                        accessManagerId = c.Int(),
                        allowAccessRequests = c.Boolean(nullable: false),
                        webDisplayName = c.String(maxLength: 255),
                        webDescription = c.String(),
                        webReadmeOverview = c.String(),
                        identifier = c.String(maxLength: 255, unicode: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.user", t => t.accessManagerId)
                .ForeignKey("dbo.implementationguidetype", t => t.implementationGuideTypeId)
                .ForeignKey("dbo.publish_status", t => t.publishStatusId)
                .ForeignKey("dbo.implementationguide", t => t.previousVersionImplementationGuideId)
                .ForeignKey("dbo.organization", t => t.organizationId)
                .Index(t => t.implementationGuideTypeId)
                .Index(t => t.organizationId)
                .Index(t => t.publishStatusId)
                .Index(t => t.previousVersionImplementationGuideId)
                .Index(t => t.accessManagerId);
            
            CreateTable(
                "dbo.user",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userName = c.String(nullable: false, maxLength: 50),
                        firstName = c.String(nullable: false, maxLength: 125),
                        lastName = c.String(nullable: false, maxLength: 125),
                        email = c.String(nullable: false, maxLength: 255),
                        phone = c.String(nullable: false, maxLength: 50),
                        okayToContact = c.Boolean(),
                        externalOrganizationName = c.String(maxLength: 50),
                        externalOrganizationType = c.String(maxLength: 50),
                        apiKey = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.user_group",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userId = c.Int(nullable: false),
                        groupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.group", t => t.groupId, cascadeDelete: true)
                .ForeignKey("dbo.user", t => t.userId)
                .Index(t => t.userId)
                .Index(t => t.groupId);
            
            CreateTable(
                "dbo.group",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 100),
                        description = c.String(storeType: "ntext"),
                        disclaimer = c.String(storeType: "ntext"),
                        isOpen = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.implementationguide_permission",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideId = c.Int(nullable: false),
                        permission = c.String(nullable: false, maxLength: 50),
                        type = c.String(nullable: false, maxLength: 50),
                        groupId = c.Int(),
                        userId = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.group", t => t.groupId)
                .ForeignKey("dbo.user", t => t.userId)
                .ForeignKey("dbo.implementationguide", t => t.implementationGuideId, cascadeDelete: true)
                .Index(t => t.implementationGuideId)
                .Index(t => t.groupId)
                .Index(t => t.userId);
            
            CreateTable(
                "dbo.group_manager",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        groupId = c.Int(nullable: false),
                        userId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.group", t => t.groupId, cascadeDelete: true)
                .ForeignKey("dbo.user", t => t.userId)
                .Index(t => t.groupId)
                .Index(t => t.userId);
            
            CreateTable(
                "dbo.user_role",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        roleId = c.Int(nullable: false),
                        userId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.role", t => t.roleId, cascadeDelete: true)
                .ForeignKey("dbo.user", t => t.userId, cascadeDelete: true)
                .Index(t => t.roleId)
                .Index(t => t.userId);
            
            CreateTable(
                "dbo.template",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideTypeId = c.Int(nullable: false),
                        templateTypeId = c.Int(nullable: false),
                        impliedTemplateId = c.Int(),
                        owningImplementationGuideId = c.Int(nullable: false),
                        oid = c.String(nullable: false, maxLength: 255),
                        isOpen = c.Boolean(nullable: false),
                        name = c.String(nullable: false, maxLength: 255),
                        bookmark = c.String(nullable: false, maxLength: 255),
                        description = c.String(),
                        primaryContext = c.String(maxLength: 255),
                        notes = c.String(),
                        lastupdated = c.DateTime(nullable: false, storeType: "date"),
                        authorId = c.Int(nullable: false),
                        previousVersionTemplateId = c.Int(),
                        version = c.Int(nullable: false),
                        primaryContextType = c.String(maxLength: 255),
                        statusId = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguidetype", t => t.implementationGuideTypeId)
                .ForeignKey("dbo.templatetype", t => t.templateTypeId)
                .ForeignKey("dbo.template", t => t.impliedTemplateId)
                .ForeignKey("dbo.template", t => t.previousVersionTemplateId)
                .ForeignKey("dbo.publish_status", t => t.statusId)
                .ForeignKey("dbo.user", t => t.authorId)
                .ForeignKey("dbo.implementationguide", t => t.owningImplementationGuideId)
                .Index(t => t.implementationGuideTypeId)
                .Index(t => t.templateTypeId)
                .Index(t => t.impliedTemplateId)
                .Index(t => t.owningImplementationGuideId)
                .Index(t => t.authorId)
                .Index(t => t.previousVersionTemplateId)
                .Index(t => t.statusId);
            
            CreateTable(
                "dbo.template_constraint",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        number = c.Int(),
                        templateId = c.Int(nullable: false),
                        parentConstraintId = c.Int(),
                        codeSystemId = c.Int(),
                        valueSetId = c.Int(),
                        containedTemplateId = c.Int(),
                        order = c.Int(nullable: false),
                        isBranch = c.Boolean(nullable: false),
                        isPrimitive = c.Boolean(nullable: false),
                        conformance = c.String(maxLength: 128),
                        cardinality = c.String(maxLength: 50),
                        context = c.String(maxLength: 255),
                        dataType = c.String(maxLength: 255),
                        valueConformance = c.String(maxLength: 50),
                        isStatic = c.Boolean(),
                        value = c.String(maxLength: 255),
                        displayName = c.String(maxLength: 255),
                        valueSetDate = c.DateTime(),
                        schematron = c.String(),
                        description = c.String(),
                        notes = c.String(),
                        primitiveText = c.String(),
                        isInheritable = c.Boolean(nullable: false),
                        label = c.String(storeType: "ntext"),
                        isBranchIdentifier = c.Boolean(nullable: false),
                        isSchRooted = c.Boolean(nullable: false),
                        isHeading = c.Boolean(nullable: false),
                        headingDescription = c.String(storeType: "ntext"),
                        category = c.String(maxLength: 255),
                        displayNumber = c.String(maxLength: 128),
                        isModifier = c.Boolean(nullable: false),
                        mustSupport = c.Boolean(nullable: false),
                        isChoice = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.template_constraint", t => t.parentConstraintId)
                .ForeignKey("dbo.codesystem", t => t.codeSystemId)
                .ForeignKey("dbo.valueset", t => t.valueSetId)
                .ForeignKey("dbo.template", t => t.templateId, cascadeDelete: true)
                .ForeignKey("dbo.template", t => t.containedTemplateId)
                .Index(t => t.templateId)
                .Index(t => t.parentConstraintId)
                .Index(t => t.codeSystemId)
                .Index(t => t.valueSetId)
                .Index(t => t.containedTemplateId);
            
            CreateTable(
                "dbo.codesystem",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 255),
                        oid = c.String(nullable: false, maxLength: 255),
                        description = c.String(),
                        lastUpdate = c.DateTime(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.valueset_member",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        valueSetId = c.Int(nullable: false),
                        codeSystemId = c.Int(),
                        code = c.String(maxLength: 255),
                        displayName = c.String(maxLength: 1024),
                        status = c.String(maxLength: 255),
                        statusDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.codesystem", t => t.codeSystemId)
                .ForeignKey("dbo.valueset", t => t.valueSetId, cascadeDelete: true)
                .Index(t => t.valueSetId)
                .Index(t => t.codeSystemId);
            
            CreateTable(
                "dbo.valueset",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        oid = c.String(nullable: false, maxLength: 255),
                        name = c.String(nullable: false, maxLength: 255),
                        code = c.String(maxLength: 255),
                        description = c.String(),
                        intensional = c.Boolean(),
                        intensionalDefinition = c.String(),
                        lastUpdate = c.DateTime(),
                        source = c.String(maxLength: 1024),
                        isIncomplete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.green_constraint",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        greenTemplateId = c.Int(nullable: false),
                        templateConstraintId = c.Int(nullable: false),
                        parentGreenConstraintId = c.Int(),
                        order = c.Int(),
                        name = c.String(nullable: false, maxLength: 255),
                        description = c.String(),
                        isEditable = c.Boolean(nullable: false),
                        rootXpath = c.String(maxLength: 250),
                        igtype_datatypeId = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.green_constraint", t => t.parentGreenConstraintId)
                .ForeignKey("dbo.green_template", t => t.greenTemplateId)
                .ForeignKey("dbo.implementationguidetype_datatype", t => t.igtype_datatypeId)
                .ForeignKey("dbo.template_constraint", t => t.templateConstraintId, cascadeDelete: true)
                .Index(t => t.greenTemplateId)
                .Index(t => t.templateConstraintId)
                .Index(t => t.parentGreenConstraintId)
                .Index(t => t.igtype_datatypeId);
            
            CreateTable(
                "dbo.green_template",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        parentGreenTemplateId = c.Int(),
                        templateId = c.Int(nullable: false),
                        order = c.Int(),
                        name = c.String(nullable: false, maxLength: 255),
                        description = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.green_template", t => t.parentGreenTemplateId)
                .ForeignKey("dbo.template", t => t.templateId, cascadeDelete: true)
                .Index(t => t.parentGreenTemplateId)
                .Index(t => t.templateId);
            
            CreateTable(
                "dbo.implementationguidetype_datatype",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideTypeId = c.Int(nullable: false),
                        dataTypeName = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguidetype", t => t.implementationGuideTypeId)
                .Index(t => t.implementationGuideTypeId);
            
            CreateTable(
                "dbo.implementationguidetype",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 255),
                        schemaLocation = c.String(nullable: false, maxLength: 255),
                        schemaPrefix = c.String(nullable: false, maxLength: 255),
                        schemaURI = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.templatetype",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideTypeId = c.Int(nullable: false),
                        name = c.String(nullable: false, maxLength: 255),
                        outputOrder = c.Int(nullable: false),
                        rootContext = c.String(maxLength: 255),
                        rootContextType = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguidetype", t => t.implementationGuideTypeId, cascadeDelete: true)
                .Index(t => t.implementationGuideTypeId);
            
            CreateTable(
                "dbo.implementationguide_templatetype",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideId = c.Int(nullable: false),
                        templateTypeId = c.Int(nullable: false),
                        name = c.String(nullable: false, maxLength: 255),
                        detailsText = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.templatetype", t => t.templateTypeId, cascadeDelete: true)
                .ForeignKey("dbo.implementationguide", t => t.implementationGuideId, cascadeDelete: true)
                .Index(t => t.implementationGuideId)
                .Index(t => t.templateTypeId);
            
            CreateTable(
                "dbo.template_constraint_sample",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        templateConstraintId = c.Int(nullable: false),
                        name = c.String(nullable: false, maxLength: 255),
                        sampleText = c.String(nullable: false, storeType: "ntext"),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.template_constraint", t => t.templateConstraintId, cascadeDelete: true)
                .Index(t => t.templateConstraintId);
            
            CreateTable(
                "dbo.template_extension",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        templateId = c.Int(nullable: false),
                        identifier = c.String(nullable: false, maxLength: 255),
                        type = c.String(nullable: false, maxLength: 55),
                        value = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.template", t => t.templateId, cascadeDelete: true)
                .Index(t => t.templateId);
            
            CreateTable(
                "dbo.publish_status",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        status = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.template_sample",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        templateId = c.Int(nullable: false),
                        lastUpdated = c.DateTime(nullable: false, storeType: "date"),
                        xmlSample = c.String(nullable: false, unicode: false, storeType: "text"),
                        name = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.template", t => t.templateId, cascadeDelete: true)
                .Index(t => t.templateId);
            
            CreateTable(
                "dbo.implementationguide_file",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideId = c.Int(nullable: false),
                        fileName = c.String(nullable: false, maxLength: 255),
                        mimeType = c.String(nullable: false, maxLength: 255),
                        contentType = c.String(nullable: false, maxLength: 255),
                        expectedErrorCount = c.Int(),
                        description = c.String(nullable: false, storeType: "ntext"),
                        url = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguide", t => t.implementationGuideId, cascadeDelete: true)
                .Index(t => t.implementationGuideId);
            
            CreateTable(
                "dbo.implementationguide_filedata",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideFileId = c.Int(nullable: false),
                        data = c.Binary(nullable: false, storeType: "image"),
                        updatedDate = c.DateTime(nullable: false),
                        updatedBy = c.String(nullable: false, maxLength: 255),
                        note = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguide_file", t => t.implementationGuideFileId, cascadeDelete: true)
                .Index(t => t.implementationGuideFileId);
            
            CreateTable(
                "dbo.implementationguide_schpattern",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideId = c.Int(nullable: false),
                        phase = c.String(nullable: false, maxLength: 128),
                        patternId = c.String(nullable: false, maxLength: 255),
                        patternContent = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguide", t => t.implementationGuideId, cascadeDelete: true)
                .Index(t => t.implementationGuideId);
            
            CreateTable(
                "dbo.implementationguide_section",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideId = c.Int(nullable: false),
                        heading = c.String(nullable: false, maxLength: 255),
                        content = c.String(storeType: "ntext"),
                        order = c.Int(nullable: false),
                        level = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguide", t => t.implementationGuideId, cascadeDelete: true)
                .Index(t => t.implementationGuideId);
            
            CreateTable(
                "dbo.implementationguide_setting",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideId = c.Int(nullable: false),
                        propertyName = c.String(nullable: false, maxLength: 255),
                        propertyValue = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.implementationguide", t => t.implementationGuideId, cascadeDelete: true)
                .Index(t => t.implementationGuideId);
            
            CreateTable(
                "dbo.audit",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        username = c.String(nullable: false, maxLength: 255),
                        auditDate = c.DateTime(nullable: false),
                        ip = c.String(nullable: false, maxLength: 50),
                        type = c.String(nullable: false, maxLength: 128),
                        implementationGuideId = c.Int(),
                        templateId = c.Int(),
                        templateConstraintId = c.Int(),
                        note = c.String(),
                    })
                .PrimaryKey(t => t.id);

            this.SqlResource("Trifolia.DB.Migrations.201703021923101_init_views_up.sql");
            this.SqlResource("Trifolia.DB.Migrations.201703021923101_init_programmability_up.sql");
        }
        
        public override void Down()
        {
            this.SqlResource("Trifolia.DB.Migrations.201703021923101_init_views_down.sql");
            this.SqlResource("Trifolia.DB.Migrations.201703021923101_init_programmability_down.sql");

            DropForeignKey("dbo.appsecurable_role", "appSecurableId", "dbo.app_securable");
            DropForeignKey("dbo.role_restriction", "roleId", "dbo.role");
            DropForeignKey("dbo.role_restriction", "organizationId", "dbo.organization");
            DropForeignKey("dbo.implementationguide_templatetype", "implementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide_setting", "implementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide_section", "implementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide_schpattern", "implementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide_permission", "implementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide", "organizationId", "dbo.organization");
            DropForeignKey("dbo.implementationguide", "previousVersionImplementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide_file", "implementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide_filedata", "implementationGuideFileId", "dbo.implementationguide_file");
            DropForeignKey("dbo.template", "owningImplementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.template", "authorId", "dbo.user");
            DropForeignKey("dbo.template_sample", "templateId", "dbo.template");
            DropForeignKey("dbo.template", "statusId", "dbo.publish_status");
            DropForeignKey("dbo.implementationguide", "publishStatusId", "dbo.publish_status");
            DropForeignKey("dbo.template", "previousVersionTemplateId", "dbo.template");
            DropForeignKey("dbo.template", "impliedTemplateId", "dbo.template");
            DropForeignKey("dbo.template_extension", "templateId", "dbo.template");
            DropForeignKey("dbo.template_constraint", "containedTemplateId", "dbo.template");
            DropForeignKey("dbo.template_constraint", "templateId", "dbo.template");
            DropForeignKey("dbo.template_constraint_sample", "templateConstraintId", "dbo.template_constraint");
            DropForeignKey("dbo.green_constraint", "templateConstraintId", "dbo.template_constraint");
            DropForeignKey("dbo.templatetype", "implementationGuideTypeId", "dbo.implementationguidetype");
            DropForeignKey("dbo.template", "templateTypeId", "dbo.templatetype");
            DropForeignKey("dbo.implementationguide_templatetype", "templateTypeId", "dbo.templatetype");
            DropForeignKey("dbo.template", "implementationGuideTypeId", "dbo.implementationguidetype");
            DropForeignKey("dbo.implementationguide", "implementationGuideTypeId", "dbo.implementationguidetype");
            DropForeignKey("dbo.implementationguidetype_datatype", "implementationGuideTypeId", "dbo.implementationguidetype");
            DropForeignKey("dbo.green_constraint", "igtype_datatypeId", "dbo.implementationguidetype_datatype");
            DropForeignKey("dbo.green_template", "templateId", "dbo.template");
            DropForeignKey("dbo.green_template", "parentGreenTemplateId", "dbo.green_template");
            DropForeignKey("dbo.green_constraint", "greenTemplateId", "dbo.green_template");
            DropForeignKey("dbo.green_constraint", "parentGreenConstraintId", "dbo.green_constraint");
            DropForeignKey("dbo.valueset_member", "valueSetId", "dbo.valueset");
            DropForeignKey("dbo.template_constraint", "valueSetId", "dbo.valueset");
            DropForeignKey("dbo.valueset_member", "codeSystemId", "dbo.codesystem");
            DropForeignKey("dbo.template_constraint", "codeSystemId", "dbo.codesystem");
            DropForeignKey("dbo.template_constraint", "parentConstraintId", "dbo.template_constraint");
            DropForeignKey("dbo.user_role", "userId", "dbo.user");
            DropForeignKey("dbo.user_role", "roleId", "dbo.role");
            DropForeignKey("dbo.group_manager", "userId", "dbo.user");
            DropForeignKey("dbo.user_group", "userId", "dbo.user");
            DropForeignKey("dbo.user_group", "groupId", "dbo.group");
            DropForeignKey("dbo.group_manager", "groupId", "dbo.group");
            DropForeignKey("dbo.implementationguide_permission", "userId", "dbo.user");
            DropForeignKey("dbo.implementationguide_permission", "groupId", "dbo.group");
            DropForeignKey("dbo.implementationguide", "accessManagerId", "dbo.user");
            DropForeignKey("dbo.appsecurable_role", "roleId", "dbo.role");
            DropIndex("dbo.implementationguide_setting", new[] { "implementationGuideId" });
            DropIndex("dbo.implementationguide_section", new[] { "implementationGuideId" });
            DropIndex("dbo.implementationguide_schpattern", new[] { "implementationGuideId" });
            DropIndex("dbo.implementationguide_filedata", new[] { "implementationGuideFileId" });
            DropIndex("dbo.implementationguide_file", new[] { "implementationGuideId" });
            DropIndex("dbo.template_sample", new[] { "templateId" });
            DropIndex("dbo.template_extension", new[] { "templateId" });
            DropIndex("dbo.template_constraint_sample", new[] { "templateConstraintId" });
            DropIndex("dbo.implementationguide_templatetype", new[] { "templateTypeId" });
            DropIndex("dbo.implementationguide_templatetype", new[] { "implementationGuideId" });
            DropIndex("dbo.templatetype", new[] { "implementationGuideTypeId" });
            DropIndex("dbo.implementationguidetype_datatype", new[] { "implementationGuideTypeId" });
            DropIndex("dbo.green_template", new[] { "templateId" });
            DropIndex("dbo.green_template", new[] { "parentGreenTemplateId" });
            DropIndex("dbo.green_constraint", new[] { "igtype_datatypeId" });
            DropIndex("dbo.green_constraint", new[] { "parentGreenConstraintId" });
            DropIndex("dbo.green_constraint", new[] { "templateConstraintId" });
            DropIndex("dbo.green_constraint", new[] { "greenTemplateId" });
            DropIndex("dbo.valueset_member", new[] { "codeSystemId" });
            DropIndex("dbo.valueset_member", new[] { "valueSetId" });
            DropIndex("dbo.template_constraint", new[] { "containedTemplateId" });
            DropIndex("dbo.template_constraint", new[] { "valueSetId" });
            DropIndex("dbo.template_constraint", new[] { "codeSystemId" });
            DropIndex("dbo.template_constraint", new[] { "parentConstraintId" });
            DropIndex("dbo.template_constraint", new[] { "templateId" });
            DropIndex("dbo.template", new[] { "statusId" });
            DropIndex("dbo.template", new[] { "previousVersionTemplateId" });
            DropIndex("dbo.template", new[] { "authorId" });
            DropIndex("dbo.template", new[] { "owningImplementationGuideId" });
            DropIndex("dbo.template", new[] { "impliedTemplateId" });
            DropIndex("dbo.template", new[] { "templateTypeId" });
            DropIndex("dbo.template", new[] { "implementationGuideTypeId" });
            DropIndex("dbo.user_role", new[] { "userId" });
            DropIndex("dbo.user_role", new[] { "roleId" });
            DropIndex("dbo.group_manager", new[] { "userId" });
            DropIndex("dbo.group_manager", new[] { "groupId" });
            DropIndex("dbo.implementationguide_permission", new[] { "userId" });
            DropIndex("dbo.implementationguide_permission", new[] { "groupId" });
            DropIndex("dbo.implementationguide_permission", new[] { "implementationGuideId" });
            DropIndex("dbo.user_group", new[] { "groupId" });
            DropIndex("dbo.user_group", new[] { "userId" });
            DropIndex("dbo.implementationguide", new[] { "accessManagerId" });
            DropIndex("dbo.implementationguide", new[] { "previousVersionImplementationGuideId" });
            DropIndex("dbo.implementationguide", new[] { "publishStatusId" });
            DropIndex("dbo.implementationguide", new[] { "organizationId" });
            DropIndex("dbo.implementationguide", new[] { "implementationGuideTypeId" });
            DropIndex("dbo.role_restriction", new[] { "organizationId" });
            DropIndex("dbo.role_restriction", new[] { "roleId" });
            DropIndex("dbo.appsecurable_role", new[] { "roleId" });
            DropIndex("dbo.appsecurable_role", new[] { "appSecurableId" });
            DropTable("dbo.audit");
            DropTable("dbo.implementationguide_setting");
            DropTable("dbo.implementationguide_section");
            DropTable("dbo.implementationguide_schpattern");
            DropTable("dbo.implementationguide_filedata");
            DropTable("dbo.implementationguide_file");
            DropTable("dbo.template_sample");
            DropTable("dbo.publish_status");
            DropTable("dbo.template_extension");
            DropTable("dbo.template_constraint_sample");
            DropTable("dbo.implementationguide_templatetype");
            DropTable("dbo.templatetype");
            DropTable("dbo.implementationguidetype");
            DropTable("dbo.implementationguidetype_datatype");
            DropTable("dbo.green_template");
            DropTable("dbo.green_constraint");
            DropTable("dbo.valueset");
            DropTable("dbo.valueset_member");
            DropTable("dbo.codesystem");
            DropTable("dbo.template_constraint");
            DropTable("dbo.template");
            DropTable("dbo.user_role");
            DropTable("dbo.group_manager");
            DropTable("dbo.implementationguide_permission");
            DropTable("dbo.group");
            DropTable("dbo.user_group");
            DropTable("dbo.user");
            DropTable("dbo.implementationguide");
            DropTable("dbo.organization");
            DropTable("dbo.role_restriction");
            DropTable("dbo.role");
            DropTable("dbo.appsecurable_role");
            DropTable("dbo.app_securable");
        }
    }
}
