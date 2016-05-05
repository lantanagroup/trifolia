IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template_constraint' AND COLUMN_NAME = 'isModifier')
BEGIN
	ALTER TABLE dbo.template_constraint ADD
		isModifier bit NOT NULL CONSTRAINT DF_template_constraint_isModifier DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template_constraint' AND COLUMN_NAME = 'mustSupport')
BEGIN
	ALTER TABLE dbo.template_constraint ADD
		mustSupport bit NOT NULL CONSTRAINT DF_template_constraint_mustSupport DEFAULT 0
END
GO