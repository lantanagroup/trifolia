BEGIN TRANSACTION

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide' AND COLUMN_NAME = 'accessManagerId')
BEGIN
	ALTER TABLE dbo.implementationguide ADD
		accessManagerId int NULL

	ALTER TABLE dbo.implementationguide ADD CONSTRAINT
		FK_implementationguide_user FOREIGN KEY
		(
		accessManagerId
		) REFERENCES dbo.[user]
		(
		id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'implementationguide' AND COLUMN_NAME = 'allowAccessRequests')
BEGIN
	ALTER TABLE dbo.implementationguide ADD
		allowAccessRequests bit NOT NULL CONSTRAINT DF_implementationguide_allowAccessRequests DEFAULT 0
END

COMMIT TRANSACTION