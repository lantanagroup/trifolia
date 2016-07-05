BEGIN TRAN

ALTER TABLE dbo.green_constraint
	DROP CONSTRAINT FK_green_constraint_template_constraint

ALTER TABLE dbo.green_constraint WITH NOCHECK ADD CONSTRAINT
	FK_green_constraint_template_constraint FOREIGN KEY
	(
	templateConstraintId
	) REFERENCES dbo.template_constraint
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 

COMMIT