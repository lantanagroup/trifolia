IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide' AND COLUMN_NAME = 'webDisplayName')
BEGIN
	ALTER TABLE dbo.implementationguide ADD webDisplayName nvarchar(255) NULL
END