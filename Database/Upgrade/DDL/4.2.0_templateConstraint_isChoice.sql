IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template_constraint' AND COLUMN_NAME = 'isChoice')
BEGIN
	ALTER TABLE dbo.template_constraint ADD
		isChoice bit NOT NULL CONSTRAINT DF_template_constraint_isChoice DEFAULT 0
END
GO