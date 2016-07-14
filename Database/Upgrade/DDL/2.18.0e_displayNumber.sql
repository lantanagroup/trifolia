IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template_constraint' AND COLUMN_NAME = 'displayNumber')
BEGIN
	ALTER TABLE dbo.template_constraint ADD displayNumber nvarchar(128) NULL
END