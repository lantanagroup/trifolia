IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template' AND COLUMN_NAME = 'organizationId')
BEGIN
	DROP INDEX IDX_template_organizationId ON dbo.template
	ALTER TABLE dbo.template DROP CONSTRAINT FK_template_organization
	ALTER TABLE dbo.template DROP COLUMN organizationId
END