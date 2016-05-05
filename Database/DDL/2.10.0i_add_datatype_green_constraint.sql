BEGIN TRANSACTION

ALTER TABLE dbo.green_constraint ADD
	igtype_datatypeId int NULL

ALTER TABLE dbo.green_constraint ADD CONSTRAINT
	FK_green_constraint_igtype_datatype FOREIGN KEY
	(
	igtype_datatypeId
	) REFERENCES dbo.implementationguidetype_datatype
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
COMMIT