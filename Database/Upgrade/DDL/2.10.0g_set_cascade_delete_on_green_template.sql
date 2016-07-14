BEGIN TRANSACTION

ALTER TABLE dbo.green_template
	DROP CONSTRAINT FK_green_template_template

ALTER TABLE dbo.green_template WITH NOCHECK ADD CONSTRAINT
	FK_green_template_template FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.template
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
COMMIT