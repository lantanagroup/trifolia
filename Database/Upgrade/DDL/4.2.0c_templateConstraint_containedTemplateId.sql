-- If the template_constraint_template table exists, then move the referenced templates to the new table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'template_constraint_template')
BEGIN
	INSERT INTO template_constraint_template (templateConstraintId, templateId)
	SELECT id, containedTemplateId FROM template_constraint WHERE containedTemplateId IS NOT NULL
END

-- Remove the foreign key
IF EXISTS (SELECT * FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_template_constraint_template1')
   AND parent_object_id = OBJECT_ID(N'dbo.template_constraint'))
BEGIN
	ALTER TABLE dbo.template_constraint DROP CONSTRAINT FK_template_constraint_template1
END

-- Remove the index
IF EXISTS (SELECT * FROM sys.indexes 
	WHERE name='IDX_template_constraint_containedTemplateId' AND object_id = OBJECT_ID('template_constraint'))
BEGIN
	DROP INDEX IDX_template_constraint_containedTemplateId ON dbo.template_constraint
END

-- Remove the column
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template_constraint' AND COLUMN_NAME = 'containedTemplateId')
BEGIN
	ALTER TABLE dbo.template_constraint DROP COLUMN containedTemplateId
END