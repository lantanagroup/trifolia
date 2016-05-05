IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide' AND COLUMN_NAME = 'webReadmeOverview')
BEGIN
	ALTER TABLE dbo.[implementationguide] ADD webReadmeOverview nvarchar(MAX) NULL
END