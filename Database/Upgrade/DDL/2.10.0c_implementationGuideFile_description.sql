IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide_file' AND COLUMN_NAME = 'description')
BEGIN
	ALTER TABLE dbo.implementationguide_file ADD
		description text NOT NULL CONSTRAINT DF_implementationguide_file_description DEFAULT ''
END