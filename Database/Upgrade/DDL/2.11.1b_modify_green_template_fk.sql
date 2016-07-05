BEGIN TRAN

IF EXISTS (SELECT *
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='FK_green_template_template')
BEGIN
	ALTER TABLE dbo.green_template
	DROP CONSTRAINT FK_green_template_template

	ALTER TABLE dbo.green_template
		ADD CONSTRAINT FK_green_template_template
		FOREIGN KEY (templateId)
		REFERENCES dbo.template(id)
		ON DELETE CASCADE
END

IF EXISTS (SELECT *
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='FK_green_constraint_green_template')
BEGIN
	ALTER TABLE dbo.green_constraint
	DROP CONSTRAINT FK_green_constraint_green_template

	ALTER TABLE dbo.green_constraint
		ADD CONSTRAINT FK_green_constraint_green_template
		FOREIGN KEY (greenTemplateId)
		REFERENCES dbo.green_template(id)
		ON DELETE CASCADE
END

IF EXISTS (SELECT *
			FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='FK_template_constraint_sample_template_constraint')
BEGIN
	ALTER TABLE dbo.template_constraint_sample
	DROP CONSTRAINT FK_template_constraint_sample_template_constraint

	ALTER TABLE dbo.template_constraint_sample
		ADD CONSTRAINT FK_template_constraint_sample_template_constraint
		FOREIGN KEY (templateConstraintId)
		REFERENCES dbo.template_constraint(id)
		ON DELETE CASCADE
END

COMMIT