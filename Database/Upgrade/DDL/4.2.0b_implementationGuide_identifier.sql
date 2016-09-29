IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide' and COLUMN_NAME = 'identifier')
BEGIN
	ALTER TABLE dbo.implementationguide ADD identifier varchar(255) NULL
END