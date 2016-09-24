IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'template_constraint_template')
BEGIN
	CREATE TABLE dbo.template_constraint_template
		(
		id int NOT NULL IDENTITY (1, 1),
		templateConstraintId int NOT NULL,
		templateId int NOT NULL
		)  ON [PRIMARY]

	ALTER TABLE dbo.template_constraint_template ADD CONSTRAINT
		FK_template_constraint_templateConstraintId FOREIGN KEY
		(
		templateConstraintId
		) REFERENCES dbo.template_constraint
		(
		id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
	
	ALTER TABLE dbo.template_constraint_template ADD CONSTRAINT
		FK_template_constraint_templateId FOREIGN KEY
		(
		templateId
		) REFERENCES dbo.template
		(
		id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
END