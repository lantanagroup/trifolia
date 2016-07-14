BEGIN TRANSACTION
GO
ALTER TABLE dbo.implementationguide ADD
	displayName varchar(255) NULL
GO
ALTER TABLE dbo.implementationguide SET (LOCK_ESCALATION = TABLE)
GO
COMMIT