IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide_section' AND COLUMN_NAME = 'order')
BEGIN
	ALTER TABLE dbo.implementationguide_section ADD [order] INT NOT NULL DEFAULT 0
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide_section' AND COLUMN_NAME = 'level')
BEGIN
	ALTER TABLE dbo.implementationguide_section ADD [level] INT NOT NULL DEFAULT 1
END