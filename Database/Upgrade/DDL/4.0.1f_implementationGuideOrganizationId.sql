DECLARE @organizationIdIsNullable INT
SET @organizationIdIsNullable = (SELECT COLUMNPROPERTY(OBJECT_ID('implementationguide', 'U'), 'organizationId', 'AllowsNull'))

IF (@organizationIdIsNullable = 0)
BEGIN
	ALTER TABLE implementationguide ALTER COLUMN organizationId INT NULL
END