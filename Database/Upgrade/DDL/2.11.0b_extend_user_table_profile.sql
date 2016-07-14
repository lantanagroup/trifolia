BEGIN TRANSACTION

ALTER TABLE dbo.[user] ADD
	okayToContact bit NULL,
	externalOrganizationName varchar(50) NULL,
	externalOrganizationType varchar(50) NULL

COMMIT