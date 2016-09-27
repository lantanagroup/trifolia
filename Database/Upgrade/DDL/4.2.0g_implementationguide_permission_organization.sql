IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide_permission' AND COLUMN_NAME = 'organizationId')
BEGIN
	ALTER TABLE dbo.implementationguide_permission DROP CONSTRAINT FK_implementationguide_permission_organization
	ALTER TABLE dbo.implementationguide_permission DROP COLUMN organizationId
END