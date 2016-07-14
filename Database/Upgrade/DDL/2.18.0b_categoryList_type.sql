IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'CategoryList')
BEGIN
	CREATE TYPE CategoryList AS TABLE(category nvarchar(255) NULL )

	DECLARE @grantUser VARCHAR(255)
	SET @grantUser = (SELECT name FROM sys.database_principals WHERE name = 'trifolia' OR name = 'tdb_app')

	IF (@grantUser IS NOT NULL)
	BEGIN
		SET @grantUser = quotename(@grantUser)
		exec ('GRANT EXEC ON TYPE::dbo.CategoryList TO ' + @grantUser)
	END
END