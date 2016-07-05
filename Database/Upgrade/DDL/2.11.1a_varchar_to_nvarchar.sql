SET XACT_ABORT ON

BEGIN TRAN

/****** appsecurable Table ******/

CREATE TABLE dbo.Tmp_app_securable
	(
	id int NOT NULL IDENTITY (1, 1),
	name nvarchar(50) NOT NULL,
	displayName nvarchar(255) NULL,
	description ntext NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

SET IDENTITY_INSERT dbo.Tmp_app_securable ON

IF EXISTS(SELECT * FROM dbo.app_securable)
	 EXEC('INSERT INTO dbo.Tmp_app_securable (id, name, displayName, description)
		SELECT id, CONVERT(nvarchar(50), name), CONVERT(nvarchar(255), displayName), description FROM dbo.app_securable WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_app_securable OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_appsecurable_role_appsecurable')
BEGIN
	ALTER TABLE dbo.appsecurable_role
		DROP CONSTRAINT FK_appsecurable_role_appsecurable
END

DROP TABLE dbo.app_securable

EXECUTE sp_rename N'dbo.Tmp_app_securable', N'app_securable', 'OBJECT' 

ALTER TABLE dbo.app_securable ADD CONSTRAINT
	PK_app_securable PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.appsecurable_role ADD CONSTRAINT
	FK_appsecurable_role_appsecurable FOREIGN KEY
	(
	appSecurableId
	) REFERENCES dbo.app_securable
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/********* appsecurable Table **************/

/********* audit Table ***************/

CREATE TABLE dbo.Tmp_audit
	(
	id int NOT NULL IDENTITY (1, 1),
	username nvarchar(255) NOT NULL,
	auditDate datetime NOT NULL,
	ip nvarchar(50) NOT NULL,
	type nvarchar(128) NOT NULL,
	implementationGuideId int NULL,
	templateId int NULL,
	templateConstraintId int NULL,
	note nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_audit SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_audit ON

IF EXISTS(SELECT * FROM dbo.audit)
	 EXEC('INSERT INTO dbo.Tmp_audit (id, username, auditDate, ip, type, implementationGuideId, templateId, templateConstraintId, note)
		SELECT id, CONVERT(nvarchar(255), username), auditDate, CONVERT(nvarchar(50), ip), CONVERT(nvarchar(128), type), implementationGuideId, templateId, templateConstraintId, CONVERT(nvarchar(MAX), note) FROM dbo.audit WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_audit OFF

DROP TABLE dbo.audit

EXECUTE sp_rename N'dbo.Tmp_audit', N'audit', 'OBJECT' 

ALTER TABLE dbo.audit ADD CONSTRAINT
	PK_audit PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

/******** audit Table **************/

/******** codesystem Table *********************************/

CREATE TABLE dbo.Tmp_codesystem
	(
	id int NOT NULL IDENTITY (1, 1),
	name nvarchar(255) NOT NULL,
	oid nvarchar(255) NOT NULL,
	description nvarchar(MAX) NULL,
	lastUpdate datetime NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_codesystem SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_codesystem ON

IF EXISTS(SELECT * FROM dbo.codesystem)
	 EXEC('INSERT INTO dbo.Tmp_codesystem (id, name, oid, description, lastUpdate)
		SELECT id, CONVERT(nvarchar(255), name), CONVERT(nvarchar(255), oid), CONVERT(nvarchar(MAX), description), lastUpdate FROM dbo.codesystem WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_codesystem OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_valueset_member_codesystem')
BEGIN
	ALTER TABLE dbo.valueset_member
		DROP CONSTRAINT FK_valueset_member_codesystem
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_codesystem')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_codesystem
END

DROP TABLE dbo.codesystem

EXECUTE sp_rename N'dbo.Tmp_codesystem', N'codesystem', 'OBJECT' 

ALTER TABLE dbo.codesystem ADD CONSTRAINT
	PK_codesystem PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_codesystem FOREIGN KEY
	(
	codeSystemId
	) REFERENCES dbo.codesystem
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	

ALTER TABLE dbo.template_constraint SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.valueset_member WITH NOCHECK ADD CONSTRAINT
	FK_valueset_member_codesystem FOREIGN KEY
	(
	codeSystemId
	) REFERENCES dbo.codesystem
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	

ALTER TABLE dbo.valueset_member SET (LOCK_ESCALATION = TABLE)

/******* codesystem Table **************************************/

/******* codevalidation Table *********************************/

IF EXISTS(SELECT
    * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='DF__codevalida__SAUI__3E52440B')
BEGIN
	ALTER TABLE dbo.codevalidation
		DROP CONSTRAINT DF__codevalida__SAUI__3E52440B
END

IF EXISTS(SELECT
    * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='DF__codevalida__SCUI__3F466844')
BEGIN
	ALTER TABLE dbo.codevalidation
		DROP CONSTRAINT DF__codevalida__SCUI__3F466844
END

IF EXISTS(SELECT
    * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='DF__codevalida__SDUI__403A8C7D')
BEGIN
	ALTER TABLE dbo.codevalidation
		DROP CONSTRAINT DF__codevalida__SDUI__403A8C7D
END

IF EXISTS(SELECT
    * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='DF__codevalidat__CVF__412EB0B6')
BEGIN
	ALTER TABLE dbo.codevalidation
		DROP CONSTRAINT DF__codevalidat__CVF__412EB0B6
END

CREATE TABLE dbo.Tmp_codevalidation
	(
	CUI char(8) NOT NULL,
	LAT char(3) NOT NULL,
	TS char(1) NOT NULL,
	LUI nvarchar(50) NOT NULL,
	STT nvarchar(50) NOT NULL,
	SUI nvarchar(50) NOT NULL,
	ISPREF char(1) NOT NULL,
	AUI nvarchar(50) NOT NULL,
	SAUI nvarchar(50) NULL DEFAULT NULL,
	SCUI nvarchar(50) NULL DEFAULT NULL,
	SDUI nvarchar(50) NULL DEFAULT NULL,
	SAB nvarchar(50) NOT NULL,
	TTY nvarchar(50) NOT NULL,
	CODE nvarchar(50) NOT NULL,
	STR ntext NOT NULL,
	SRL int NOT NULL,
	SUPPRESS char(1) NOT NULL,
	CVF int NULL DEFAULT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_codevalidation SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * FROM dbo.codevalidation)
	 EXEC('INSERT INTO dbo.Tmp_codevalidation (CUI, LAT, TS, LUI, STT, SUI, ISPREF, AUI, SAUI, SCUI, SDUI, SAB, TTY, CODE, STR, SRL, SUPPRESS, CVF)
		SELECT CUI, LAT, TS, CONVERT(nvarchar(50), LUI), CONVERT(nvarchar(50), STT), CONVERT(nvarchar(50), SUI), ISPREF, CONVERT(nvarchar(50), AUI), CONVERT(nvarchar(50), SAUI), CONVERT(nvarchar(50), SCUI), CONVERT(nvarchar(50), SDUI), CONVERT(nvarchar(50), SAB), CONVERT(nvarchar(50), TTY), CONVERT(nvarchar(50), CODE), STR, SRL, SUPPRESS, CVF FROM dbo.codevalidation WITH (HOLDLOCK TABLOCKX)')

DROP TABLE dbo.codevalidation

EXECUTE sp_rename N'dbo.Tmp_codevalidation', N'codevalidation', 'OBJECT' 

CREATE NONCLUSTERED INDEX IX_codevalidation ON dbo.codevalidation
	(
	AUI
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

/******* codevalidation Table *********************************/

/******* codevalidation_map Table *********************************/

CREATE TABLE dbo.Tmp_codevalidation_map
	(
	OID nvarchar(255) NOT NULL DEFAULT '',
	SAB nvarchar(50) NOT NULL DEFAULT ''
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_codevalidation_map SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * FROM dbo.codevalidation_map)
	 EXEC('INSERT INTO dbo.Tmp_codevalidation_map (OID, SAB)
		SELECT CONVERT(nvarchar(255), OID), CONVERT(nvarchar(50), SAB) FROM dbo.codevalidation_map WITH (HOLDLOCK TABLOCKX)')

DROP TABLE dbo.codevalidation_map

EXECUTE sp_rename N'dbo.Tmp_codevalidation_map', N'codevalidation_map', 'OBJECT' 

ALTER TABLE dbo.codevalidation_map ADD CONSTRAINT
	IX_codevalidation_map UNIQUE NONCLUSTERED 
	(
	OID,
	SAB
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

/******* codevalidation_map Table *********************************/

/******* DatabaseVersion ******************************************/

CREATE TABLE dbo.Tmp_DatabaseVersion
	(
	DatabaseVersionNumber nvarchar(50) NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_DatabaseVersion SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * FROM dbo.DatabaseVersion)
	 EXEC('INSERT INTO dbo.Tmp_DatabaseVersion (DatabaseVersionNumber)
		SELECT CONVERT(nvarchar(50), DatabaseVersionNumber) FROM dbo.DatabaseVersion WITH (HOLDLOCK TABLOCKX)')

DROP TABLE dbo.DatabaseVersion

EXECUTE sp_rename N'dbo.Tmp_DatabaseVersion', N'DatabaseVersion', 'OBJECT' 

/******* DatabaseVersion ******************************************/

/******* green_constraint *****************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_constraint_igtype_datatype')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT FK_green_constraint_igtype_datatype
END

ALTER TABLE dbo.implementationguidetype_datatype SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_constraint_green_template')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT FK_green_constraint_green_template
END

ALTER TABLE dbo.green_template SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_constraint_template_constraint')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT FK_green_constraint_template_constraint
END

ALTER TABLE dbo.template_constraint SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_green_constraint_isEditable')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT DF_green_constraint_isEditable
END

CREATE TABLE dbo.Tmp_green_constraint
	(
	id int NOT NULL IDENTITY (1, 1),
	greenTemplateId int NOT NULL,
	templateConstraintId int NOT NULL,
	parentGreenConstraintId int NULL,
	[order] int NULL,
	name nvarchar(255) NOT NULL,
	description nvarchar(MAX) NULL,
	isEditable bit NOT NULL DEFAULT 0,
	rootXPath nvarchar(250) NULL,
	igtype_datatypeId int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_green_constraint SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_green_constraint ON

IF EXISTS(SELECT * FROM dbo.green_constraint)
	 EXEC('INSERT INTO dbo.Tmp_green_constraint (id, greenTemplateId, templateConstraintId, parentGreenConstraintId, [order], name, description, isEditable, rootXPath, igtype_datatypeId)
		SELECT id, greenTemplateId, templateConstraintId, parentGreenConstraintId, [order], CONVERT(nvarchar(255), name), CONVERT(nvarchar(MAX), description), isEditable, CONVERT(nvarchar(250), rootXPath), igtype_datatypeId FROM dbo.green_constraint WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_green_constraint OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_constraint_green_constraint1')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT FK_green_constraint_green_constraint1
END

DROP TABLE dbo.green_constraint

EXECUTE sp_rename N'dbo.Tmp_green_constraint', N'green_constraint', 'OBJECT' 

ALTER TABLE dbo.green_constraint ADD CONSTRAINT
	PK_green_constraint PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


CREATE NONCLUSTERED INDEX IDX_green_constraint_greentemplateId ON dbo.green_constraint
	(
	greenTemplateId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_green_constraint_parentGreenConstraintId ON dbo.green_constraint
	(
	parentGreenConstraintId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_green_constraint_templateConstraintId ON dbo.green_constraint
	(
	templateConstraintId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.green_constraint WITH NOCHECK ADD CONSTRAINT
	FK_green_constraint_template_constraint FOREIGN KEY
	(
	templateConstraintId
	) REFERENCES dbo.template_constraint
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
ALTER TABLE dbo.green_constraint WITH NOCHECK ADD CONSTRAINT
	FK_green_constraint_green_constraint1 FOREIGN KEY
	(
	parentGreenConstraintId
	) REFERENCES dbo.green_constraint
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.green_constraint WITH NOCHECK ADD CONSTRAINT
	FK_green_constraint_green_template FOREIGN KEY
	(
	greenTemplateId
	) REFERENCES dbo.green_template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.green_constraint ADD CONSTRAINT
	FK_green_constraint_igtype_datatype FOREIGN KEY
	(
	igtype_datatypeId
	) REFERENCES dbo.implementationguidetype_datatype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
/******* green_constraint *****************************************/

/******* green_template *******************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_template_template')
BEGIN
	ALTER TABLE dbo.green_template
		DROP CONSTRAINT FK_green_template_template
END

ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_green_template
	(
	id int NOT NULL IDENTITY (1, 1),
	parentGreenTemplateId int NULL,
	templateId int NOT NULL,
	[order] int NULL,
	name nvarchar(255) NOT NULL,
	description nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_green_template SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_green_template ON

IF EXISTS(SELECT * FROM dbo.green_template)
	 EXEC('INSERT INTO dbo.Tmp_green_template (id, parentGreenTemplateId, templateId, [order], name, description)
		SELECT id, parentGreenTemplateId, templateId, [order], CONVERT(nvarchar(255), name), CONVERT(nvarchar(MAX), description) FROM dbo.green_template WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_green_template OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_template_green_template')
BEGIN
	ALTER TABLE dbo.green_template
		DROP CONSTRAINT FK_green_template_green_template
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_constraint_green_template')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT FK_green_constraint_green_template
END

DROP TABLE dbo.green_template

EXECUTE sp_rename N'dbo.Tmp_green_template', N'green_template', 'OBJECT' 

ALTER TABLE dbo.green_template ADD CONSTRAINT
	PK_green_template PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


CREATE NONCLUSTERED INDEX IDX_green_template_parentGreenTemplateId ON dbo.green_template
	(
	parentGreenTemplateId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_green_template_templateId ON dbo.green_template
	(
	templateId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.green_template WITH NOCHECK ADD CONSTRAINT
	FK_green_template_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
ALTER TABLE dbo.green_template WITH NOCHECK ADD CONSTRAINT
	FK_green_template_green_template FOREIGN KEY
	(
	parentGreenTemplateId
	) REFERENCES dbo.green_template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.green_constraint WITH NOCHECK ADD CONSTRAINT
	FK_green_constraint_green_template FOREIGN KEY
	(
	greenTemplateId
	) REFERENCES dbo.green_template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	

ALTER TABLE dbo.green_constraint SET (LOCK_ESCALATION = TABLE)


/******* green_template *******************************************/

/******* group ****************************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_group_organization')
BEGIN
	ALTER TABLE dbo.[group]
		DROP CONSTRAINT FK_group_organization
END

CREATE TABLE dbo.Tmp_group
	(
	id int NOT NULL IDENTITY (1, 1),
	name nvarchar(100) NOT NULL,
	organizationId int NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_group SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_group ON

IF EXISTS(SELECT * FROM dbo.[group])
	 EXEC('INSERT INTO dbo.Tmp_group (id, name, organizationId)
		SELECT id, CONVERT(nvarchar(100), name), organizationId FROM dbo.[group] WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_group OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_group')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_group
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_user_group_group')
BEGIN
	ALTER TABLE dbo.user_group
		DROP CONSTRAINT FK_user_group_group
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_group')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_group
END

DROP TABLE dbo.[group]

EXECUTE sp_rename N'dbo.Tmp_group', N'group', 'OBJECT' 

ALTER TABLE dbo.[group] ADD CONSTRAINT
	PK_group PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.[group] ADD CONSTRAINT
	FK_group_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_group FOREIGN KEY
	(
	groupId
	) REFERENCES dbo.[group]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

ALTER TABLE dbo.organization_defaultpermission SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.user_group ADD CONSTRAINT
	FK_user_group_group FOREIGN KEY
	(
	groupId
	) REFERENCES dbo.[group]
	(
	id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
ALTER TABLE dbo.user_group SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_group FOREIGN KEY
	(
	groupId
	) REFERENCES dbo.[group]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_permission SET (LOCK_ESCALATION = TABLE)

/******* group ****************************************************/

/******* implementationguide **************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_organization')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT FK_implementationguide_organization
END

ALTER TABLE dbo.organization SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_implementationguidetype')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT FK_implementationguide_implementationguidetype
END

ALTER TABLE dbo.implementationguidetype SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_publish_status')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT FK_implementationguide_publish_status
END

ALTER TABLE dbo.publish_status SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='previousVersionIGDefault')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT previousVersionIGDefault
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='defaultVersion')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT defaultVersion
END

CREATE TABLE dbo.Tmp_implementationguide
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideTypeId int NOT NULL,
	organizationId int NOT NULL,
	name nvarchar(255) NOT NULL,
	publishDate datetime NULL,
	publishStatusId int NULL,
	previousVersionImplementationGuideId int NULL DEFAULT NULL,
	[version] int NULL DEFAULT 1
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguide SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguide ON

IF EXISTS(SELECT * FROM dbo.implementationguide)
	 EXEC('INSERT INTO dbo.Tmp_implementationguide (id, implementationGuideTypeId, organizationId, name, publishDate, publishStatusId, previousVersionImplementationGuideId, version)
		SELECT id, implementationGuideTypeId, organizationId, CONVERT(nvarchar(255), name), publishDate, publishStatusId, previousVersionImplementationGuideId, version FROM dbo.implementationguide WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguide OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_implementationguide
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_implementationguide')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_implementationguide
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_schpattern_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_schpattern
		DROP CONSTRAINT FK_implementationguide_schpattern_implementationguide
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_setting_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_setting
		DROP CONSTRAINT FK_implementationguide_setting_implementationguide
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_templatetype_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_templatetype
		DROP CONSTRAINT FK_implementationguide_templatetype_implementationguide
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_file_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_file
		DROP CONSTRAINT FK_implementationguide_file_implementationguide
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_ig_igPreviousVersion')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT FK_ig_igPreviousVersion
END

DROP TABLE dbo.implementationguide

EXECUTE sp_rename N'dbo.Tmp_implementationguide', N'implementationguide', 'OBJECT' 

ALTER TABLE dbo.implementationguide ADD CONSTRAINT
	PK_implementationguide PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguide_implementationGuideTypeId ON dbo.implementationguide
	(
	implementationGuideTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
CREATE NONCLUSTERED INDEX IDX_implementationguide_organizationid ON dbo.implementationguide
	(
	organizationId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
ALTER TABLE dbo.implementationguide ADD CONSTRAINT
	FK_implementationguide_publish_status FOREIGN KEY
	(
	publishStatusId
	) REFERENCES dbo.publish_status
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide ADD CONSTRAINT
	FK_ig_igPreviousVersion FOREIGN KEY
	(
	previousVersionImplementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_file ADD CONSTRAINT
	FK_implementationguide_file_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_file SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide_templatetype WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_templatetype_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_templatetype SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide_setting WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_setting_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_setting SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide_schpattern ADD CONSTRAINT
	FK_implementationguide_schpattern_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_schpattern SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_implementationguide FOREIGN KEY
	(
	owningImplementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_permission SET (LOCK_ESCALATION = TABLE)


/******* implementationguide **************************************/

/******* implementationguide_file *********************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_file_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_file
		DROP CONSTRAINT FK_implementationguide_file_implementationguide
END

ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_implementationguide_file_description')
BEGIN
	ALTER TABLE dbo.implementationguide_file
		DROP CONSTRAINT DF_implementationguide_file_description
END

CREATE TABLE dbo.Tmp_implementationguide_file
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideId int NOT NULL,
	fileName nvarchar(255) NOT NULL,
	mimeType nvarchar(255) NOT NULL,
	contentType nvarchar(255) NOT NULL,
	expectedErrorCount int NULL,
	[description] ntext NOT NULL DEFAULT ''
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguide_file SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguide_file ON

IF EXISTS(SELECT * FROM dbo.implementationguide_file)
	 EXEC('INSERT INTO dbo.Tmp_implementationguide_file (id, implementationGuideId, fileName, mimeType, contentType, expectedErrorCount, description)
		SELECT id, implementationGuideId, CONVERT(nvarchar(255), fileName), CONVERT(nvarchar(255), mimeType), CONVERT(nvarchar(255), contentType), expectedErrorCount, CONVERT(ntext, description) FROM dbo.implementationguide_file WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguide_file OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_filedata_implementationguide_file')
BEGIN
	ALTER TABLE dbo.implementationguide_filedata
		DROP CONSTRAINT FK_implementationguide_filedata_implementationguide_file
END

DROP TABLE dbo.implementationguide_file

EXECUTE sp_rename N'dbo.Tmp_implementationguide_file', N'implementationguide_file', 'OBJECT' 

ALTER TABLE dbo.implementationguide_file ADD CONSTRAINT
	PK_implementationguide_file PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguide_file_implementationGuideId ON dbo.implementationguide_file
	(
	implementationGuideId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide_file ADD CONSTRAINT
	FK_implementationguide_file_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

ALTER TABLE dbo.implementationguide_filedata ADD CONSTRAINT
	FK_implementationguide_filedata_implementationguide_file FOREIGN KEY
	(
	implementationGuideFileId
	) REFERENCES dbo.implementationguide_file
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_filedata SET (LOCK_ESCALATION = TABLE)

/******* implementationguide_file *********************************/

/******* implementationguide_filedata *****************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_filedata_implementationguide_file')
BEGIN
	ALTER TABLE dbo.implementationguide_filedata
		DROP CONSTRAINT FK_implementationguide_filedata_implementationguide_file
END

ALTER TABLE dbo.implementationguide_file SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_implementationguide_filedata
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideFileId int NOT NULL,
	data image NOT NULL,
	updatedDate datetime NOT NULL,
	updatedBy nvarchar(255) NOT NULL,
	note nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguide_filedata SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguide_filedata ON

IF EXISTS(SELECT * FROM dbo.implementationguide_filedata)
	 EXEC('INSERT INTO dbo.Tmp_implementationguide_filedata (id, implementationGuideFileId, data, updatedDate, updatedBy, note)
		SELECT id, implementationGuideFileId, data, updatedDate, CONVERT(nvarchar(255), updatedBy), CONVERT(nvarchar(MAX), note) FROM dbo.implementationguide_filedata WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguide_filedata OFF

DROP TABLE dbo.implementationguide_filedata

EXECUTE sp_rename N'dbo.Tmp_implementationguide_filedata', N'implementationguide_filedata', 'OBJECT' 

ALTER TABLE dbo.implementationguide_filedata ADD CONSTRAINT
	PK_implementationguide_filedata PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguide_filedata_implementationguideFileId ON dbo.implementationguide_filedata
	(
	implementationGuideFileId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide_filedata ADD CONSTRAINT
	FK_implementationguide_filedata_implementationguide_file FOREIGN KEY
	(
	implementationGuideFileId
	) REFERENCES dbo.implementationguide_file
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/******* implementationguide_filedata *****************************/

/******* implementationguide_permission ***************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_implementationguide
END

ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_user')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_user
END

ALTER TABLE dbo.[user] SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_organization')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_organization
END

ALTER TABLE dbo.organization SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_group')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_group
END

ALTER TABLE dbo.[group] SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_implementationguide_permission
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideId int NOT NULL,
	permission nvarchar(50) NOT NULL,
	type nvarchar(50) NOT NULL,
	organizationId int NULL,
	groupId int NULL,
	userId int NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguide_permission SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguide_permission ON

IF EXISTS(SELECT * FROM dbo.implementationguide_permission)
	 EXEC('INSERT INTO dbo.Tmp_implementationguide_permission (id, implementationGuideId, permission, type, organizationId, groupId, userId)
		SELECT id, implementationGuideId, CONVERT(nvarchar(50), permission), CONVERT(nvarchar(50), type), organizationId, groupId, userId FROM dbo.implementationguide_permission WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguide_permission OFF

DROP TABLE dbo.implementationguide_permission

EXECUTE sp_rename N'dbo.Tmp_implementationguide_permission', N'implementationguide_permission', 'OBJECT' 

ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	PK_implementationguide_permission PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_group FOREIGN KEY
	(
	groupId
	) REFERENCES dbo.[group]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_user FOREIGN KEY
	(
	userId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 


/******* implementationguide_permission ***************************/

/******* implementationguide_schpattern ***************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_schpattern_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_schpattern
		DROP CONSTRAINT FK_implementationguide_schpattern_implementationguide
END

ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_implementationguide_schpattern
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideId int NOT NULL,
	phase nvarchar(128) NOT NULL,
	patternId nvarchar(255) NOT NULL,
	patternContent nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguide_schpattern SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguide_schpattern ON

IF EXISTS(SELECT * FROM dbo.implementationguide_schpattern)
	 EXEC('INSERT INTO dbo.Tmp_implementationguide_schpattern (id, implementationGuideId, phase, patternId, patternContent)
		SELECT id, implementationGuideId, CONVERT(nvarchar(128), phase), CONVERT(nvarchar(255), patternId), CONVERT(nvarchar(MAX), patternContent) FROM dbo.implementationguide_schpattern WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguide_schpattern OFF

DROP TABLE dbo.implementationguide_schpattern

EXECUTE sp_rename N'dbo.Tmp_implementationguide_schpattern', N'implementationguide_schpattern', 'OBJECT' 

ALTER TABLE dbo.implementationguide_schpattern ADD CONSTRAINT
	PK_implementationguide_schpattern PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguide_schpattern_implementationGuideId ON dbo.implementationguide_schpattern
	(
	implementationGuideId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide_schpattern ADD CONSTRAINT
	FK_implementationguide_schpattern_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/******* implementationguide_schpattern ***************************/

/******* implementationguide_setting ******************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_setting_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_setting
		DROP CONSTRAINT FK_implementationguide_setting_implementationguide
END

ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_implementationguide_setting
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideId int NOT NULL,
	propertyName nvarchar(255) NOT NULL,
	propertyValue nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguide_setting SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguide_setting ON

IF EXISTS(SELECT * FROM dbo.implementationguide_setting)
	 EXEC('INSERT INTO dbo.Tmp_implementationguide_setting (id, implementationGuideId, propertyName, propertyValue)
		SELECT id, implementationGuideId, CONVERT(nvarchar(255), propertyName), CONVERT(nvarchar(MAX), propertyValue) FROM dbo.implementationguide_setting WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguide_setting OFF

DROP TABLE dbo.implementationguide_setting

EXECUTE sp_rename N'dbo.Tmp_implementationguide_setting', N'implementationguide_setting', 'OBJECT' 

ALTER TABLE dbo.implementationguide_setting ADD CONSTRAINT
	PK_implementationguide_setting PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguide_setting_implementationguideId ON dbo.implementationguide_setting
	(
	implementationGuideId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide_setting WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_setting_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/******* implementationguide_setting ******************************/

/******* implementationguide_templatetype *************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_templatetype_implementationguide')
BEGIN
	ALTER TABLE dbo.implementationguide_templatetype
		DROP CONSTRAINT FK_implementationguide_templatetype_implementationguide
END

ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_templatetype_templatetype')
BEGIN
	ALTER TABLE dbo.implementationguide_templatetype
		DROP CONSTRAINT FK_implementationguide_templatetype_templatetype
END

ALTER TABLE dbo.templatetype SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_implementationguide_templatetype
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideId int NOT NULL,
	templateTypeId int NOT NULL,
	name nvarchar(255) NOT NULL,
	detailsText nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguide_templatetype SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguide_templatetype ON

IF EXISTS(SELECT * FROM dbo.implementationguide_templatetype)
	 EXEC('INSERT INTO dbo.Tmp_implementationguide_templatetype (id, implementationGuideId, templateTypeId, name, detailsText)
		SELECT id, implementationGuideId, templateTypeId, CONVERT(nvarchar(255), name), CONVERT(nvarchar(MAX), detailsText) FROM dbo.implementationguide_templatetype WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguide_templatetype OFF

DROP TABLE dbo.implementationguide_templatetype

EXECUTE sp_rename N'dbo.Tmp_implementationguide_templatetype', N'implementationguide_templatetype', 'OBJECT' 

ALTER TABLE dbo.implementationguide_templatetype ADD CONSTRAINT
	PK_implementationguide_templatetype PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguide_templatetype_implementationguideid ON dbo.implementationguide_templatetype
	(
	implementationGuideId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguide_templatetype_templateTypeId ON dbo.implementationguide_templatetype
	(
	templateTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide_templatetype WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_templatetype_templatetype FOREIGN KEY
	(
	templateTypeId
	) REFERENCES dbo.templatetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_templatetype WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_templatetype_implementationguide FOREIGN KEY
	(
	implementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/******* implementationguide_templatetype *************************/

/******* implementationguidetype **********************************/

CREATE TABLE dbo.Tmp_implementationguidetype
	(
	id int NOT NULL IDENTITY (1, 1),
	name nvarchar(255) NOT NULL,
	schemaLocation nvarchar(255) NOT NULL,
	schemaPrefix nvarchar(255) NOT NULL,
	schemaURI nvarchar(255) NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguidetype SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguidetype ON

IF EXISTS(SELECT * FROM dbo.implementationguidetype)
	 EXEC('INSERT INTO dbo.Tmp_implementationguidetype (id, name, schemaLocation, schemaPrefix, schemaURI)
		SELECT id, CONVERT(nvarchar(255), name), CONVERT(nvarchar(255), schemaLocation), CONVERT(nvarchar(255), schemaPrefix), CONVERT(nvarchar(255), schemaURI) FROM dbo.implementationguidetype WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguidetype OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_templatetype_implementationguidetype')
BEGIN
	ALTER TABLE dbo.templatetype
		DROP CONSTRAINT FK_templatetype_implementationguidetype
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguidetype_datatype_implementationguidetype')
BEGIN
	ALTER TABLE dbo.implementationguidetype_datatype
		DROP CONSTRAINT FK_implementationguidetype_datatype_implementationguidetype
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_implementationguidetype')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_implementationguidetype
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_implementationguidetype')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT FK_implementationguide_implementationguidetype
END

DROP TABLE dbo.implementationguidetype

EXECUTE sp_rename N'dbo.Tmp_implementationguidetype', N'implementationguidetype', 'OBJECT' 

ALTER TABLE dbo.implementationguidetype ADD CONSTRAINT
	PK_implementationguidetype PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguidetype_datatype ADD CONSTRAINT
	FK_implementationguidetype_datatype_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguidetype_datatype SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.templatetype WITH NOCHECK ADD CONSTRAINT
	FK_templatetype_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.templatetype SET (LOCK_ESCALATION = TABLE)

/******* implementationguidetype **********************************/

/******* implementationguidetype_datatype *************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguidetype_datatype_implementationguidetype')
BEGIN
	ALTER TABLE dbo.implementationguidetype_datatype
		DROP CONSTRAINT FK_implementationguidetype_datatype_implementationguidetype
END

ALTER TABLE dbo.implementationguidetype SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_implementationguidetype_datatype
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideTypeId int NOT NULL,
	dataTypeName nvarchar(255) NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_implementationguidetype_datatype SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_implementationguidetype_datatype ON

IF EXISTS(SELECT * FROM dbo.implementationguidetype_datatype)
	 EXEC('INSERT INTO dbo.Tmp_implementationguidetype_datatype (id, implementationGuideTypeId, dataTypeName)
		SELECT id, implementationGuideTypeId, CONVERT(nvarchar(255), dataTypeName) FROM dbo.implementationguidetype_datatype WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_implementationguidetype_datatype OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_constraint_igtype_datatype')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT FK_green_constraint_igtype_datatype
END

DROP TABLE dbo.implementationguidetype_datatype

EXECUTE sp_rename N'dbo.Tmp_implementationguidetype_datatype', N'implementationguidetype_datatype', 'OBJECT' 

ALTER TABLE dbo.implementationguidetype_datatype ADD CONSTRAINT
	PK_implementationguidetype_datatype PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_implementationguidetype_datatype_implementationguidetypeid ON dbo.implementationguidetype_datatype
	(
	implementationGuideTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguidetype_datatype ADD CONSTRAINT
	FK_implementationguidetype_datatype_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.green_constraint ADD CONSTRAINT
	FK_green_constraint_igtype_datatype FOREIGN KEY
	(
	igtype_datatypeId
	) REFERENCES dbo.implementationguidetype_datatype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.green_constraint SET (LOCK_ESCALATION = TABLE)

/******* implementationguidetype_datatype *************************/

/******* organization *********************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_organization_isInternal')
BEGIN
	ALTER TABLE dbo.organization
		DROP CONSTRAINT DF_organization_isInternal
END

CREATE TABLE dbo.Tmp_organization
	(
	id int NOT NULL IDENTITY (1, 1),
	name nvarchar(255) NOT NULL,
	contactName nvarchar(128) NULL,
	contactEmail nvarchar(255) NULL,
	contactPhone nvarchar(50) NULL,
	authProvider nvarchar(1024) NULL,
	isInternal bit NOT NULL DEFAULT 0
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_organization SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_organization ON

IF EXISTS(SELECT * FROM dbo.organization)
	 EXEC('INSERT INTO dbo.Tmp_organization (id, name, contactName, contactEmail, contactPhone, authProvider, isInternal)
		SELECT id, CONVERT(nvarchar(255), name), CONVERT(nvarchar(128), contactName), CONVERT(nvarchar(255), contactEmail), CONVERT(nvarchar(50), contactPhone), CONVERT(nvarchar(1024), authProvider), isInternal FROM dbo.organization WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_organization OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_user_organization')
BEGIN
	ALTER TABLE dbo.[user]
		DROP CONSTRAINT FK_user_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_role_restriction_organization')
BEGIN
	ALTER TABLE dbo.role_restriction
		DROP CONSTRAINT FK_role_restriction_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_group_organization')
BEGIN
	ALTER TABLE dbo.[group]
		DROP CONSTRAINT FK_group_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_organization')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_organization')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_organization')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT FK_implementationguide_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_organization')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_organization1')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_organization1
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_user_organization')
BEGIN
	ALTER TABLE dbo.organization
			DROP CONSTRAINT FK_user_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_organization')
BEGIN
	ALTER TABLE dbo.organization
			DROP CONSTRAINT FK_implementationguide_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_organization')
BEGIN
	ALTER TABLE dbo.organization
			DROP CONSTRAINT FK_implementationguide_permission_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_group_organization')
BEGIN
	ALTER TABLE dbo.organization
			DROP CONSTRAINT FK_group_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_role_restriction_organization')
BEGIN
	ALTER TABLE dbo.organization
			DROP CONSTRAINT FK_role_restriction_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_organization')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
			DROP CONSTRAINT FK_organization_defaultpermission_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_organization1')
BEGIN
	ALTER TABLE dbo.organization
			DROP CONSTRAINT FK_organization_defaultpermission_organization1
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_organization')
BEGIN
	ALTER TABLE dbo.organization
			DROP CONSTRAINT FK_template_organization
END

DROP TABLE dbo.organization

EXECUTE sp_rename N'dbo.Tmp_organization', N'organization', 'OBJECT' 

ALTER TABLE dbo.organization ADD CONSTRAINT
	PK_organization PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_organization1 FOREIGN KEY
	(
	entireOrganizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.organization_defaultpermission SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_permission SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.[group] ADD CONSTRAINT
	FK_group_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

ALTER TABLE dbo.[group] SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.role_restriction ADD CONSTRAINT
	FK_role_restriction_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.role_restriction SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.[user] ADD CONSTRAINT
	FK_user_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.[user] SET (LOCK_ESCALATION = TABLE)

/******* organization *********************************************/

/******* organization_defaultpermission ***************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_user')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_user
END

ALTER TABLE dbo.[user] SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_organization')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_organization
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_organization1')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_organization1
END

ALTER TABLE dbo.organization SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_group')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_group
END

ALTER TABLE dbo.[group] SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_organization_defaultpermission
	(
	id int NOT NULL IDENTITY (1, 1),
	organizationId int NOT NULL,
	permission nvarchar(50) NOT NULL,
	type nvarchar(50) NOT NULL,
	entireOrganizationId int NULL,
	groupId int NULL,
	userId int NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_organization_defaultpermission SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_organization_defaultpermission ON

IF EXISTS(SELECT * FROM dbo.organization_defaultpermission)
	 EXEC('INSERT INTO dbo.Tmp_organization_defaultpermission (id, organizationId, permission, type, entireOrganizationId, groupId, userId)
		SELECT id, organizationId, CONVERT(nvarchar(50), permission), CONVERT(nvarchar(50), type), entireOrganizationId, groupId, userId FROM dbo.organization_defaultpermission WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_organization_defaultpermission OFF

DROP TABLE dbo.organization_defaultpermission

EXECUTE sp_rename N'dbo.Tmp_organization_defaultpermission', N'organization_defaultpermission', 'OBJECT' 

ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	PK_organization_defaultpermission PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_group FOREIGN KEY
	(
	groupId
	) REFERENCES dbo.[group]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_organization1 FOREIGN KEY
	(
	entireOrganizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_user FOREIGN KEY
	(
	userId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/******* organization_defaultpermission ***************************/

/******* publish_status *******************************************/

CREATE TABLE dbo.Tmp_publish_status
	(
	id int NOT NULL IDENTITY (1, 1),
	status nvarchar(50) NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_publish_status SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_publish_status ON

IF EXISTS(SELECT * FROM dbo.publish_status)
	 EXEC('INSERT INTO dbo.Tmp_publish_status (id, status)
		SELECT id, CONVERT(nvarchar(50), status) FROM dbo.publish_status WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_publish_status OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_status')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_status
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_publish_status')
BEGIN
	ALTER TABLE dbo.implementationguide
		DROP CONSTRAINT FK_implementationguide_publish_status
END

DROP TABLE dbo.publish_status

EXECUTE sp_rename N'dbo.Tmp_publish_status', N'publish_status', 'OBJECT' 

ALTER TABLE dbo.publish_status ADD CONSTRAINT
	PK_publish_status PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.implementationguide ADD CONSTRAINT
	FK_implementationguide_publish_status FOREIGN KEY
	(
	publishStatusId
	) REFERENCES dbo.publish_status
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.template ADD CONSTRAINT
	FK_template_status FOREIGN KEY
	(
	statusId
	) REFERENCES dbo.publish_status
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

/******* publish_status *******************************************/

/******* role *****************************************************/

IF EXISTS(SELECT
    * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='DF__roles__IsDefault__19DFD96B')
BEGIN
	ALTER TABLE dbo.role
		DROP CONSTRAINT DF__roles__IsDefault__19DFD96B
END

CREATE TABLE dbo.Tmp_role
	(
	id int NOT NULL IDENTITY (1, 1),
	name nvarchar(50) NULL,
	isDefault bit NOT NULL DEFAULT 0
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_role SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_role ON

IF EXISTS(SELECT * FROM dbo.role)
	 EXEC('INSERT INTO dbo.Tmp_role (id, name, isDefault)
		SELECT id, CONVERT(nvarchar(50), name), isDefault FROM dbo.role WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_role OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_appsecurable_role_roles')
BEGIN
	ALTER TABLE dbo.appsecurable_role
		DROP CONSTRAINT FK_appsecurable_role_roles
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_role_restriction_roles')
BEGIN
	ALTER TABLE dbo.role_restriction
		DROP CONSTRAINT FK_role_restriction_roles
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_user_role_roles')
BEGIN
	ALTER TABLE dbo.user_role
		DROP CONSTRAINT FK_user_role_roles
END

DROP TABLE dbo.role

EXECUTE sp_rename N'dbo.Tmp_role', N'role', 'OBJECT' 

ALTER TABLE dbo.role ADD CONSTRAINT
	PK_roles PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.user_role ADD CONSTRAINT
	FK_user_role_roles FOREIGN KEY
	(
	roleId
	) REFERENCES dbo.role
	(
	id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
ALTER TABLE dbo.user_role SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.role_restriction ADD CONSTRAINT
	FK_role_restriction_roles FOREIGN KEY
	(
	roleId
	) REFERENCES dbo.role
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.role_restriction SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.appsecurable_role ADD CONSTRAINT
	FK_appsecurable_role_roles FOREIGN KEY
	(
	roleId
	) REFERENCES dbo.role
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.appsecurable_role SET (LOCK_ESCALATION = TABLE)

/******* role *****************************************************/

/******* template *************************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_implementationguide')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_implementationguide
END

ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_templatetype')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_templatetype
END

ALTER TABLE dbo.templatetype SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_organization')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_organization
END

ALTER TABLE dbo.organization SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_implementationguidetype')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_implementationguidetype
END

ALTER TABLE dbo.implementationguidetype SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_user')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_user
END

ALTER TABLE dbo.[user] SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_status')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_status
END

ALTER TABLE dbo.publish_status SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_isOpen')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT DF_template_isOpen
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_lastupdated')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT DF_template_lastupdated
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='previousVersionTemplateDefault')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT previousVersionTemplateDefault
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_version')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT DF_template_version
END

CREATE TABLE dbo.Tmp_template
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideTypeId int NOT NULL,
	templateTypeId int NOT NULL,
	organizationId int NULL,
	impliedTemplateId int NULL,
	owningImplementationGuideId int NOT NULL,
	oid nvarchar(255) NOT NULL,
	isOpen bit NOT NULL DEFAULT (1),
	name nvarchar(255) NOT NULL,
	bookmark nvarchar(255) NOT NULL,
	description nvarchar(MAX) NULL,
	primaryContext nvarchar(255) NULL,
	notes nvarchar(MAX) NULL,
	lastupdated date NOT NULL DEFAULT getdate(),
	authorId int NOT NULL,
	previousVersionTemplateId int NULL DEFAULT (NULL),
	[version] int NOT NULL DEFAULT (1),
	primaryContextType nvarchar(255) NULL,
	statusId int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_template SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_template ON

IF EXISTS(SELECT * FROM dbo.template)
	 EXEC('INSERT INTO dbo.Tmp_template (id, implementationGuideTypeId, templateTypeId, organizationId, impliedTemplateId, owningImplementationGuideId, oid, isOpen, name, bookmark, description, primaryContext, notes, lastupdated, authorId, previousVersionTemplateId, version, primaryContextType, statusId)
		SELECT id, implementationGuideTypeId, templateTypeId, organizationId, impliedTemplateId, owningImplementationGuideId, CONVERT(nvarchar(255), oid), isOpen, CONVERT(nvarchar(255), name), CONVERT(nvarchar(255), bookmark), CONVERT(nvarchar(MAX), description), CONVERT(nvarchar(255), primaryContext), CONVERT(nvarchar(MAX), notes), lastupdated, authorId, previousVersionTemplateId, version, CONVERT(nvarchar(255), primaryContextType), statusId FROM dbo.template WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_template OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_template')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_template
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_templatesample_template')
BEGIN
	ALTER TABLE dbo.template_sample
		DROP CONSTRAINT FK_templatesample_template
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_template_template')
BEGIN
	ALTER TABLE dbo.green_template
		DROP CONSTRAINT FK_green_template_template
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_templatePreviousVersion')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_templatePreviousVersion
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_template')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_template
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_template1')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_template1
END

DROP TABLE dbo.template

EXECUTE sp_rename N'dbo.Tmp_template', N'template', 'OBJECT' 

ALTER TABLE dbo.template ADD CONSTRAINT
	PK_template PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_implementationGuideTypeId ON dbo.template
	(
	implementationGuideTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_impliedTemplateId ON dbo.template
	(
	impliedTemplateId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_name ON dbo.template
	(
	name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_oid ON dbo.template
	(
	oid
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_organizationId ON dbo.template
	(
	organizationId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_templateTypeId ON dbo.template
	(
	templateTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.template ADD CONSTRAINT
	FK_template_status FOREIGN KEY
	(
	statusId
	) REFERENCES dbo.publish_status
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template ADD CONSTRAINT
	FK_template_user FOREIGN KEY
	(
	authorId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_template FOREIGN KEY
	(
	impliedTemplateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_templatetype FOREIGN KEY
	(
	templateTypeId
	) REFERENCES dbo.templatetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_implementationguide FOREIGN KEY
	(
	owningImplementationGuideId
	) REFERENCES dbo.implementationguide
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template ADD CONSTRAINT
	FK_template_templatePreviousVersion FOREIGN KEY
	(
	previousVersionTemplateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_template1 FOREIGN KEY
	(
	containedTemplateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.green_template WITH NOCHECK ADD CONSTRAINT
	FK_green_template_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
ALTER TABLE dbo.green_template SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.template_sample ADD CONSTRAINT
	FK_templatesample_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_sample SET (LOCK_ESCALATION = TABLE)

/******* template *************************************************/

/******* template_constraint **************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_template')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_template
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_template1')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_template1
END

ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_valueset')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_valueset
END

ALTER TABLE dbo.valueset SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_codesystem')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_codesystem
END

ALTER TABLE dbo.codesystem SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_constraint_isBranch')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT DF_template_constraint_isBranch
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_constraint_isPrimitive')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT DF_template_constraint_isPrimitive
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_constraint_isInheritable')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT DF_template_constraint_isInheritable
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF__template___isBra__282DF8C2')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT DF__template___isBra__282DF8C2
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF__template___isSch__29221CFB')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT DF__template___isSch__29221CFB
END

IF EXISTS (SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF__template___isHea__1A9EF37A')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT DF__template___isHea__1A9EF37A
END

CREATE TABLE dbo.Tmp_template_constraint
	(
	id int NOT NULL IDENTITY (1, 1),
	number int NULL,
	templateId int NOT NULL,
	parentConstraintId int NULL,
	codeSystemId int NULL,
	valueSetId int NULL,
	containedTemplateId int NULL,
	[order] int NOT NULL,
	isBranch bit NOT NULL DEFAULT (0),
	isPrimitive bit NOT NULL DEFAULT (0),
	conformance nvarchar(128) NULL,
	cardinality nvarchar(50) NULL,
	context nvarchar(255) NULL,
	dataType nvarchar(255) NULL,
	valueConformance nvarchar(50) NULL,
	isStatic bit NULL,
	value nvarchar(255) NULL,
	displayName nvarchar(255) NULL,
	valueSetDate datetime NULL,
	schematron nvarchar(MAX) NULL,
	description nvarchar(MAX) NULL,
	notes nvarchar(MAX) NULL,
	primitiveText nvarchar(MAX) NULL,
	isInheritable bit NOT NULL DEFAULT (1),
	label ntext NULL,
	isBranchIdentifier bit NOT NULL DEFAULT (0),
	isSchRooted bit NOT NULL DEFAULT (0),
	isHeading bit NOT NULL DEFAULT (0),
	headingDescription ntext NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_template_constraint SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_template_constraint ON

IF EXISTS(SELECT * FROM dbo.template_constraint)
	 EXEC('INSERT INTO dbo.Tmp_template_constraint (id, number, templateId, parentConstraintId, codeSystemId, valueSetId, containedTemplateId, [order], isBranch, isPrimitive, conformance, cardinality, context, dataType, valueConformance, isStatic, value, displayName, valueSetDate, schematron, description, notes, primitiveText, isInheritable, label, isBranchIdentifier, isSchRooted, isHeading, headingDescription)
		SELECT id, number, templateId, parentConstraintId, codeSystemId, valueSetId, containedTemplateId, [order], isBranch, isPrimitive, CONVERT(nvarchar(128), conformance), CONVERT(nvarchar(50), cardinality), CONVERT(nvarchar(255), context), CONVERT(nvarchar(255), dataType), CONVERT(nvarchar(50), valueConformance), isStatic, CONVERT(nvarchar(255), value), CONVERT(nvarchar(255), displayName), valueSetDate, CONVERT(nvarchar(MAX), schematron), CONVERT(nvarchar(MAX), description), CONVERT(nvarchar(MAX), notes), CONVERT(nvarchar(MAX), primitiveText), isInheritable, label, isBranchIdentifier, isSchRooted, isHeading, headingDescription FROM dbo.template_constraint WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_template_constraint OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_sample_template_constraint')
BEGIN
	ALTER TABLE dbo.template_constraint_sample
		DROP CONSTRAINT FK_template_constraint_sample_template_constraint
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_green_constraint_template_constraint')
BEGIN
	ALTER TABLE dbo.green_constraint
		DROP CONSTRAINT FK_green_constraint_template_constraint
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_template_constraint1')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_template_constraint1
END

DROP TABLE dbo.template_constraint

EXECUTE sp_rename N'dbo.Tmp_template_constraint', N'template_constraint', 'OBJECT' 

ALTER TABLE dbo.template_constraint ADD CONSTRAINT
	PK_template_constraint PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_constraint_codeSystemId ON dbo.template_constraint
	(
	codeSystemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_constraint_containedTemplateId ON dbo.template_constraint
	(
	containedTemplateId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_constraint_parentConstraintId ON dbo.template_constraint
	(
	parentConstraintId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_constraint_templateId ON dbo.template_constraint
	(
	templateId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_template_constraint_valueSetId ON dbo.template_constraint
	(
	valueSetId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_codesystem FOREIGN KEY
	(
	codeSystemId
	) REFERENCES dbo.codesystem
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_template_constraint1 FOREIGN KEY
	(
	parentConstraintId
	) REFERENCES dbo.template_constraint
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_valueset FOREIGN KEY
	(
	valueSetId
	) REFERENCES dbo.valueset
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_template1 FOREIGN KEY
	(
	containedTemplateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.green_constraint WITH NOCHECK ADD CONSTRAINT
	FK_green_constraint_template_constraint FOREIGN KEY
	(
	templateConstraintId
	) REFERENCES dbo.template_constraint
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
ALTER TABLE dbo.green_constraint SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.template_constraint_sample ADD CONSTRAINT
	FK_template_constraint_sample_template_constraint FOREIGN KEY
	(
	templateConstraintId
	) REFERENCES dbo.template_constraint
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template_constraint_sample SET (LOCK_ESCALATION = TABLE)

/******* template_constraint **************************************/

/******* template_constraint_sample *******************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_sample_template_constraint')
BEGIN
	ALTER TABLE dbo.template_constraint_sample
		DROP CONSTRAINT FK_template_constraint_sample_template_constraint
END

ALTER TABLE dbo.template_constraint SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_template_constraint_sample
	(
	id int NOT NULL IDENTITY (1, 1),
	templateConstraintId int NOT NULL,
	name nvarchar(255) NOT NULL,
	sampleText ntext NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_template_constraint_sample SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_template_constraint_sample ON

IF EXISTS(SELECT * FROM dbo.template_constraint_sample)
	 EXEC('INSERT INTO dbo.Tmp_template_constraint_sample (id, templateConstraintId, name, sampleText)
		SELECT id, templateConstraintId, CONVERT(nvarchar(255), name), CONVERT(ntext, sampleText) FROM dbo.template_constraint_sample WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_template_constraint_sample OFF

DROP TABLE dbo.template_constraint_sample

EXECUTE sp_rename N'dbo.Tmp_template_constraint_sample', N'template_constraint_sample', 'OBJECT' 

ALTER TABLE dbo.template_constraint_sample ADD CONSTRAINT
	PK_template_constraint_sample PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.template_constraint_sample ADD CONSTRAINT
	FK_template_constraint_sample_template_constraint FOREIGN KEY
	(
	templateConstraintId
	) REFERENCES dbo.template_constraint
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/******* template_constraint_sample *******************************/

/******* template_sample ******************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_templatesample_template')
BEGIN
	ALTER TABLE dbo.template_sample
		DROP CONSTRAINT FK_templatesample_template
END

ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_sample_lastupdated')
BEGIN
	ALTER TABLE dbo.template_sample
		DROP CONSTRAINT DF_template_sample_lastupdated
END

CREATE TABLE dbo.Tmp_template_sample
	(
	id int NOT NULL IDENTITY (1, 1),
	templateId int NOT NULL,
	lastUpdated date NOT NULL DEFAULT (getdate()),
	xmlSample ntext NOT NULL,
	name nvarchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_template_sample SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_template_sample ON

IF EXISTS(SELECT * FROM dbo.template_sample)
	 EXEC('INSERT INTO dbo.Tmp_template_sample (id, templateId, lastUpdated, xmlSample, name)
		SELECT id, templateId, lastUpdated, xmlSample, CONVERT(nvarchar(50), name) FROM dbo.template_sample WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_template_sample OFF

DROP TABLE dbo.template_sample

EXECUTE sp_rename N'dbo.Tmp_template_sample', N'template_sample', 'OBJECT' 

ALTER TABLE dbo.template_sample ADD CONSTRAINT
	PK_Table_1 PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.template_sample ADD CONSTRAINT
	FK_templatesample_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 


/******* template_sample ******************************************/

/******* template_type ********************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_templatetype_implementationguidetype')
BEGIN
	ALTER TABLE dbo.templatetype
		DROP CONSTRAINT FK_templatetype_implementationguidetype
END

ALTER TABLE dbo.implementationguidetype SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_templatetype_outputOrder')
BEGIN
	ALTER TABLE dbo.templatetype
		DROP CONSTRAINT DF_templatetype_outputOrder
END

CREATE TABLE dbo.Tmp_templatetype
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideTypeId int NOT NULL,
	name nvarchar(255) NOT NULL,
	outputOrder int NOT NULL DEFAULT (1),
	rootContext nvarchar(255) NULL,
	rootContextType nvarchar(255) NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_templatetype SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_templatetype ON

IF EXISTS(SELECT * FROM dbo.templatetype)
	 EXEC('INSERT INTO dbo.Tmp_templatetype (id, implementationGuideTypeId, name, outputOrder, rootContext, rootContextType)
		SELECT id, implementationGuideTypeId, CONVERT(nvarchar(255), name), outputOrder, CONVERT(nvarchar(255), rootContext), CONVERT(nvarchar(255), rootContextType) FROM dbo.templatetype WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_templatetype OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_templatetype_templatetype')
BEGIN
	ALTER TABLE dbo.implementationguide_templatetype
		DROP CONSTRAINT FK_implementationguide_templatetype_templatetype
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_templatetype')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_templatetype
END

DROP TABLE dbo.templatetype

EXECUTE sp_rename N'dbo.Tmp_templatetype', N'templatetype', 'OBJECT' 

ALTER TABLE dbo.templatetype ADD CONSTRAINT
	PK_templatetype PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_templatetype_implementationGuideTypeId ON dbo.templatetype
	(
	implementationGuideTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.templatetype WITH NOCHECK ADD CONSTRAINT
	FK_templatetype_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_templatesample_template')
BEGIN
	ALTER TABLE dbo.template_sample
		DROP CONSTRAINT FK_templatesample_template
END

ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_templatetype FOREIGN KEY
	(
	templateTypeId
	) REFERENCES dbo.templatetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_sample_lastupdated')
BEGIN
	ALTER TABLE dbo.template_sample
		DROP CONSTRAINT DF_template_sample_lastupdated
END

CREATE TABLE dbo.Tmp_template_sample
	(
	id int NOT NULL IDENTITY (1, 1),
	templateId int NOT NULL,
	lastUpdated date NOT NULL,
	xmlSample text NOT NULL,
	name nvarchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_template_sample SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.Tmp_template_sample ADD CONSTRAINT
	DF_template_sample_lastupdated DEFAULT (getdate()) FOR lastUpdated

SET IDENTITY_INSERT dbo.Tmp_template_sample ON

IF EXISTS(SELECT * FROM dbo.template_sample)
	 EXEC('INSERT INTO dbo.Tmp_template_sample (id, templateId, lastUpdated, xmlSample, name)
		SELECT id, templateId, lastUpdated, xmlSample, CONVERT(nvarchar(50), name) FROM dbo.template_sample WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_template_sample OFF

DROP TABLE dbo.template_sample

EXECUTE sp_rename N'dbo.Tmp_template_sample', N'template_sample', 'OBJECT' 

ALTER TABLE dbo.template_sample ADD CONSTRAINT
	PK_Table_1 PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.template_sample ADD CONSTRAINT
	FK_templatesample_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_templatetype WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_templatetype_templatetype FOREIGN KEY
	(
	templateTypeId
	) REFERENCES dbo.templatetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_templatetype SET (LOCK_ESCALATION = TABLE)

/******* template_type ********************************************/

/******* user *****************************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_templatetype_implementationguidetype')
BEGIN
	ALTER TABLE dbo.templatetype
		DROP CONSTRAINT FK_templatetype_implementationguidetype
END

ALTER TABLE dbo.implementationguidetype SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_templatetype_outputOrder')
BEGIN
	ALTER TABLE dbo.templatetype
		DROP CONSTRAINT DF_templatetype_outputOrder
END

CREATE TABLE dbo.Tmp_templatetype
	(
	id int NOT NULL IDENTITY (1, 1),
	implementationGuideTypeId int NOT NULL,
	name nvarchar(255) NOT NULL,
	outputOrder int NOT NULL,
	rootContext nvarchar(255) NULL,
	rootContextType nvarchar(255) NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_templatetype SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.Tmp_templatetype ADD CONSTRAINT
	DF_templatetype_outputOrder DEFAULT ((1)) FOR outputOrder

SET IDENTITY_INSERT dbo.Tmp_templatetype ON

IF EXISTS(SELECT * FROM dbo.templatetype)
	 EXEC('INSERT INTO dbo.Tmp_templatetype (id, implementationGuideTypeId, name, outputOrder, rootContext, rootContextType)
		SELECT id, implementationGuideTypeId, CONVERT(nvarchar(255), name), outputOrder, CONVERT(nvarchar(255), rootContext), CONVERT(nvarchar(255), rootContextType) FROM dbo.templatetype WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_templatetype OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_templatetype_templatetype')
BEGIN
	ALTER TABLE dbo.implementationguide_templatetype
		DROP CONSTRAINT FK_implementationguide_templatetype_templatetype
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_templatetype')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_templatetype
END

DROP TABLE dbo.templatetype

EXECUTE sp_rename N'dbo.Tmp_templatetype', N'templatetype', 'OBJECT' 

ALTER TABLE dbo.templatetype ADD CONSTRAINT
	PK_templatetype PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_templatetype_implementationGuideTypeId ON dbo.templatetype
	(
	implementationGuideTypeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.templatetype WITH NOCHECK ADD CONSTRAINT
	FK_templatetype_implementationguidetype FOREIGN KEY
	(
	implementationGuideTypeId
	) REFERENCES dbo.implementationguidetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_templatetype WITH NOCHECK ADD CONSTRAINT
	FK_implementationguide_templatetype_templatetype FOREIGN KEY
	(
	templateTypeId
	) REFERENCES dbo.templatetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_templatetype SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_user_organization')
BEGIN
	ALTER TABLE dbo.[user]
		DROP CONSTRAINT FK_user_organization
END

ALTER TABLE dbo.organization SET (LOCK_ESCALATION = TABLE)

CREATE TABLE dbo.Tmp_user
	(
	id int NOT NULL IDENTITY (1, 1),
	userName nvarchar(50) NOT NULL,
	organizationId int NOT NULL,
	firstName nvarchar(125) NOT NULL,
	lastName nvarchar(125) NOT NULL,
	email nvarchar(255) NOT NULL,
	phone nvarchar(50) NOT NULL,
	okayToContact bit NULL,
	externalOrganizationName nvarchar(50) NULL,
	externalOrganizationType nvarchar(50) NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_user SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_user ON

IF EXISTS(SELECT * FROM dbo.[user])
	 EXEC('INSERT INTO dbo.Tmp_user (id, userName, organizationId, firstName, lastName, email, phone, okayToContact, externalOrganizationName, externalOrganizationType)
		SELECT id, CONVERT(nvarchar(50), userName), organizationId, CONVERT(nvarchar(125), firstName), CONVERT(nvarchar(125), lastName), CONVERT(nvarchar(255), email), CONVERT(nvarchar(50), phone), okayToContact, CONVERT(nvarchar(50), externalOrganizationName), CONVERT(nvarchar(50), externalOrganizationType) FROM dbo.[user] WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_user OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_user_group_user')
BEGIN
	ALTER TABLE dbo.user_group
		DROP CONSTRAINT FK_user_group_user
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_implementationguide_permission_user')
BEGIN
	ALTER TABLE dbo.implementationguide_permission
		DROP CONSTRAINT FK_implementationguide_permission_user
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_user_role_user')
BEGIN
	ALTER TABLE dbo.user_role
		DROP CONSTRAINT FK_user_role_user
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_user')
BEGIN
	ALTER TABLE dbo.template
		DROP CONSTRAINT FK_template_user
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_organization_defaultpermission_user')
BEGIN
	ALTER TABLE dbo.organization_defaultpermission
		DROP CONSTRAINT FK_organization_defaultpermission_user
END

DROP TABLE dbo.[user]

EXECUTE sp_rename N'dbo.Tmp_user', N'user', 'OBJECT' 

ALTER TABLE dbo.[user] ADD CONSTRAINT
	PK_user PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.[user] ADD CONSTRAINT
	FK_user_organization FOREIGN KEY
	(
	organizationId
	) REFERENCES dbo.organization
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.organization_defaultpermission ADD CONSTRAINT
	FK_organization_defaultpermission_user FOREIGN KEY
	(
	userId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.organization_defaultpermission SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_templatesample_template')
BEGIN
	ALTER TABLE dbo.template_sample
		DROP CONSTRAINT FK_templatesample_template
END

ALTER TABLE dbo.template ADD CONSTRAINT
	FK_template_user FOREIGN KEY
	(
	authorId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template WITH NOCHECK ADD CONSTRAINT
	FK_template_templatetype FOREIGN KEY
	(
	templateTypeId
	) REFERENCES dbo.templatetype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.template SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_template_sample_lastupdated')
BEGIN
	ALTER TABLE dbo.template_sample
		DROP CONSTRAINT DF_template_sample_lastupdated
END

CREATE TABLE dbo.Tmp_template_sample
	(
	id int NOT NULL IDENTITY (1, 1),
	templateId int NOT NULL,
	lastUpdated date NOT NULL DEFAULT (getdate()),
	xmlSample text NOT NULL,
	name nvarchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_template_sample SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_template_sample ON

IF EXISTS(SELECT * FROM dbo.template_sample)
	 EXEC('INSERT INTO dbo.Tmp_template_sample (id, templateId, lastUpdated, xmlSample, name)
		SELECT id, templateId, lastUpdated, xmlSample, CONVERT(nvarchar(50), name) FROM dbo.template_sample WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_template_sample OFF

DROP TABLE dbo.template_sample

EXECUTE sp_rename N'dbo.Tmp_template_sample', N'template_sample', 'OBJECT' 

ALTER TABLE dbo.template_sample ADD CONSTRAINT
	PK_Table_1 PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.template_sample ADD CONSTRAINT
	FK_templatesample_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

ALTER TABLE dbo.user_role ADD CONSTRAINT
	FK_user_role_user FOREIGN KEY
	(
	userId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
ALTER TABLE dbo.user_role SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.implementationguide_permission ADD CONSTRAINT
	FK_implementationguide_permission_user FOREIGN KEY
	(
	userId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.implementationguide_permission SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.user_group ADD CONSTRAINT
	FK_user_group_user FOREIGN KEY
	(
	userId
	) REFERENCES dbo.[user]
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.user_group SET (LOCK_ESCALATION = TABLE)

/******* user *****************************************************/

/******* valueset *************************************************/

IF EXISTS(SELECT
    * 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='DF__valueset__isInco__2DB1C7EE')
BEGIN
	ALTER TABLE dbo.valueset
		DROP CONSTRAINT DF__valueset__isInco__2DB1C7EE
END

CREATE TABLE dbo.Tmp_valueset
	(
	id int NOT NULL IDENTITY (1, 1),
	oid nvarchar(255) NOT NULL,
	name nvarchar(255) NOT NULL,
	code nvarchar(255) NULL,
	description nvarchar(MAX) NULL,
	intensional bit NULL,
	intensionalDefinition nvarchar(MAX) NULL,
	lastUpdate datetime NULL,
	source nvarchar(1024) NULL,
	isIncomplete bit NOT NULL DEFAULT 0
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_valueset SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_valueset ON

IF EXISTS(SELECT * FROM dbo.valueset)
	 EXEC('INSERT INTO dbo.Tmp_valueset (id, oid, name, code, description, intensional, intensionalDefinition, lastUpdate, source, isIncomplete)
		SELECT id, CONVERT(nvarchar(255), oid), CONVERT(nvarchar(255), name), CONVERT(nvarchar(255), code), CONVERT(nvarchar(MAX), description), intensional, CONVERT(nvarchar(MAX), intensionalDefinition), lastUpdate, CONVERT(nvarchar(1024), source), isIncomplete FROM dbo.valueset WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_valueset OFF

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_valueset_member_valueset')
BEGIN
	ALTER TABLE dbo.valueset_member
		DROP CONSTRAINT FK_valueset_member_valueset
END

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_valueset')
BEGIN
	ALTER TABLE dbo.template_constraint
		DROP CONSTRAINT FK_template_constraint_valueset
END

DROP TABLE dbo.valueset

EXECUTE sp_rename N'dbo.Tmp_valueset', N'valueset', 'OBJECT' 

ALTER TABLE dbo.valueset ADD CONSTRAINT
	PK_valueset PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_valueset_name ON dbo.valueset
	(
	name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_valueset_oid ON dbo.valueset
	(
	oid
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.template_constraint WITH NOCHECK ADD CONSTRAINT
	FK_template_constraint_valueset FOREIGN KEY
	(
	valueSetId
	) REFERENCES dbo.valueset
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

ALTER TABLE dbo.template_constraint SET (LOCK_ESCALATION = TABLE)

ALTER TABLE dbo.valueset_member WITH NOCHECK ADD CONSTRAINT
	FK_valueset_member_valueset FOREIGN KEY
	(
	valueSetId
	) REFERENCES dbo.valueset
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.valueset_member SET (LOCK_ESCALATION = TABLE)

/******* valueset *************************************************/

/******* valueset_member ******************************************/

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_template_constraint_valueset')
BEGIN
	ALTER TABLE dbo.valueset_member
		DROP CONSTRAINT FK_valueset_member_valueset
END

ALTER TABLE dbo.valueset SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='FK_valueset_member_codesystem')
BEGIN
	ALTER TABLE dbo.valueset_member
		DROP CONSTRAINT FK_valueset_member_codesystem
END

ALTER TABLE dbo.codesystem SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * 
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
			WHERE CONSTRAINT_NAME ='DF_valueset_member_status')
BEGIN
	ALTER TABLE dbo.valueset_member
		DROP CONSTRAINT DF_valueset_member_status
END

CREATE TABLE dbo.Tmp_valueset_member
	(
	id int NOT NULL IDENTITY (1, 1),
	valueSetId int NOT NULL,
	codeSystemId int NULL,
	code nvarchar(255) NULL,
	displayName nvarchar(1024) NULL,
	[status] nvarchar(255) NULL DEFAULT ('active'),
	statusDate datetime NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_valueset_member SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_valueset_member ON

IF EXISTS(SELECT * FROM dbo.valueset_member)
	 EXEC('INSERT INTO dbo.Tmp_valueset_member (id, valueSetId, codeSystemId, code, displayName, status, statusDate)
		SELECT id, valueSetId, codeSystemId, CONVERT(nvarchar(255), code), CONVERT(nvarchar(1024), displayName), CONVERT(nvarchar(255), status), statusDate FROM dbo.valueset_member WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_valueset_member OFF

DROP TABLE dbo.valueset_member

EXECUTE sp_rename N'dbo.Tmp_valueset_member', N'valueset_member', 'OBJECT' 

ALTER TABLE dbo.valueset_member ADD CONSTRAINT
	PK_valueset_member PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


CREATE NONCLUSTERED INDEX IDX_valueset_member_ ON dbo.valueset_member
	(
	codeSystemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX IDX_valueset_member_valuesetId ON dbo.valueset_member
	(
	valueSetId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.valueset_member WITH NOCHECK ADD CONSTRAINT
	FK_valueset_member_codesystem FOREIGN KEY
	(
	codeSystemId
	) REFERENCES dbo.codesystem
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.valueset_member WITH NOCHECK ADD CONSTRAINT
	FK_valueset_member_valueset FOREIGN KEY
	(
	valueSetId
	) REFERENCES dbo.valueset
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

/******* valueset_member ******************************************/

COMMIT

PRINT 'Finished Running Main Scripts ...'

GO

create trigger ConformanceNumberTrigger on dbo.template_constraint
after insert, update
as
   SET NoCount ON
   DECLARE @constraintId INT
   DECLARE @templateId INT

   IF EXISTS (SELECT * FROM INSERTED WHERE [number] IS NULL)
   BEGIN
     SET @constraintId = (SELECT id FROM INSERTED)
	 SET @templateId = (SELECT templateId FROM INSERTED)

     UPDATE template_constraint SET [number] = dbo.GetNextConformanceNumber(@templateId)
	 WHERE id = @constraintId
   END

GO

PRINT 'Finished creating trigger ...'