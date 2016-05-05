IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide' AND COLUMN_NAME = 'webDescription')
BEGIN
	ALTER TABLE dbo.[implementationguide] ADD [webDescription] nvarchar(MAX) NULL
END