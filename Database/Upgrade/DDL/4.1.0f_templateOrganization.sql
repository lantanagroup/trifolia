IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('template') AND name = 'IDX_template_organizationId')
BEGIN
	DROP INDEX IDX_template_organizationId ON dbo.template
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID('dbo.FK_template_organization'))
BEGIN
	ALTER TABLE dbo.template DROP CONSTRAINT FK_template_organization
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template' AND COLUMN_NAME = 'organizationId')
BEGIN
	ALTER TABLE dbo.template DROP COLUMN organizationId
END