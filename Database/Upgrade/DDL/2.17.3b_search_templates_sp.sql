IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME = 'SearchTemplates')
BEGIN
	DROP PROCEDURE SearchTemplates
END
GO

CREATE PROCEDURE SearchTemplates
	@userId INT = NULL,
	@filterImplementationGuideId INT = NULL,
	@filterName NVARCHAR(255) = NULL,
	@filterIdentifier NVARCHAR(255) = NULL,
	@filterTemplateTypeId INT = NULL,
	@filterOrganizationId INT = NULL,
	@filterContextType NVARCHAR(255) = NULL,
	@queryText NVARCHAR(255) = NULL
AS
BEGIN
	IF (@filterName = '') SET @filterName = NULL
	IF (@filterIdentifier = '') SET @filterIdentifier = NULL
	IF (@filterContextType = '') SET @filterContextType = NULL
	IF (@queryText = '') SET @queryText = NULL
	
	IF (@queryText IS NOT NULL) SET @queryText = '%' + @queryText + '%'

	DECLARE @userIsAdmin BIT = 0

	IF (@userId IS NOT NULL)
		SET @userIsAdmin = 
			CASE WHEN (SELECT COUNT(*) FROM user_role ur JOIN [role] r ON r.id = ur.roleId WHERE ur.userId = @userId AND isAdmin = 1) > 0 THEN 1
			ELSE 0 END

	CREATE TABLE #templateIds (id INT)

	-- Filter by implementation guide
	IF (@filterImplementationGuideId IS NOT NULL)
	BEGIN
		CREATE TABLE #implementationGuideTemplates (id INT)

		INSERT INTO #implementationGuideTemplates (id)
		EXEC GetImplementationGuideTemplates @implementationGuideId = @filterImplementationGuideId, @inferred = 1
		
		INSERT INTO #templateIds
		SELECT id FROM #implementationGuideTemplates
	END
	ELSE
	BEGIN
		INSERT INTO #templateIds (id)
		SELECT id FROM template
	END

	IF (@queryText IS NOT NULL OR @queryText != '')
	BEGIN
		CREATE TABLE #queryTextTemplates (id INT)

		INSERT INTO #queryTextTemplates (id)
		SELECT t.id
		FROM template t
			JOIN templatetype tt ON tt.id = t.templateTypeId
			JOIN implementationguide ig ON ig.id = t.owningImplementationGuideId
		WHERE
			t.name LIKE @queryText OR 
			t.oid LIKE @queryText OR
			tt.name LIKE @queryText OR
			ig.name LIKE @queryText OR
			EXISTS (SELECT * FROM template_constraint WHERE CONCAT(CAST(t.owningImplementationGuideId AS NVARCHAR), '-', CAST(number AS NVARCHAR)) LIKE @queryText AND template_constraint.templateId = t.id)

		DELETE FROM #templateIds
		WHERE id NOT IN (SELECT id FROM #queryTextTemplates)
	END

	IF (@userId IS NOT NULL AND @userIsAdmin = 0)
	BEGIN
		DELETE FROM #templateIds
		WHERE id NOT IN (SELECT templateId FROM v_templatePermissions WHERE userId = @userId AND permission = 'View')
	END
	
	SELECT t.id
	FROM v_templateList t
		JOIN #templateIds tid ON tid.id = t.id
	WHERE
		CHARINDEX(CASE WHEN @filterName IS NOT NULL THEN @filterName ELSE t.name END, t.name) > 0
		AND CHARINDEX(CASE WHEN @filterIdentifier IS NOT NULL THEN @filterIdentifier ELSE t.oid END, t.oid) > 0
		AND CHARINDEX(CASE WHEN @filterContextType IS NOT NULL THEN @filterContextType ELSE t.primaryContextType END, t.primaryContextType) > 0
		AND ISNULL(t.templateTypeId, 0) = CASE WHEN @filterTemplateTypeId IS NOT NULL THEN @filterTemplateTypeId ELSE ISNULL(t.templateTypeId, 0) END
		AND ISNULL(t.organizationId, 0) = CASE WHEN @filterOrganizationId IS NOT NULL THEN @filterOrganizationId ELSE ISNULL(t.organizationId, 0) END

	DROP TABLE #templateIds
END
GO

DECLARE @grantUser VARCHAR(255)
SET @grantUser = (SELECT name FROM sys.database_principals WHERE name = 'trifolia' OR name = 'tdb_app')

IF (@grantUser IS NOT NULL)
BEGIN
	SET @grantUser = quotename(@grantUser)
	exec ('GRANT EXECUTE ON [dbo].SearchTemplates TO ' + @grantUser)
END
GO
