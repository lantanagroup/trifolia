IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME = 'GetImplementationGuideTemplates')
BEGIN
	DROP PROCEDURE GetImplementationGuideTemplates
END
GO

CREATE PROCEDURE GetImplementationGuideTemplates
	@implementationGuideId INT,
	@inferred BIT,
	@parentTemplateId INT = NULL
AS
BEGIN
	DECLARE @currentImplementationGuideId INT = @implementationGuideId, @relationshipCount INT, @retiredStatusId INT
	DECLARE @False BIT = 0, @True BIT = 1

	CREATE TABLE #implementationGuides (id INT, [version] INT)
	CREATE TABLE #templates (id INT)
	CREATE TABLE #relationships (id INT)

	SEt @retiredStatusId = (SELECT id FROM publish_status WHERE [status] = 'Retired')

	WHILE (@currentImplementationGuideId IS NOT NULL)
	BEGIN
		INSERT INTO #implementationGuides (id, [version])
		SELECT @currentImplementationGuideId, [version] FROM implementationguide WHERE id = @currentImplementationGuideId

		SET @currentImplementationGuideId = (SELECT previousVersionImplementationGuideId FROM implementationguide WHERE id = @currentImplementationGuideId)
	END

	-- Loop through the IG versions from beginning to end adding/removing templates as appropriate
	IF (@parentTemplateId IS NULL)
	BEGIN
		DECLARE ig_cursor CURSOR
			FOR SELECT id FROM #implementationGuides ORDER BY [version]
		OPEN ig_cursor
		FETCH NEXT FROM ig_cursor INTO @currentImplementationGuideId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM #templates
			WHERE id in (SELECT previousVersionTemplateId FROM template WHERE owningImplementationGuideId = @currentImplementationGuideId)

			INSERT INTO #templates (id)
			SELECT id FROM template WHERE owningImplementationGuideId = @currentImplementationGuideId

			FETCH NEXT FROM ig_cursor INTO @currentImplementationGuideId;
		END

		CLOSE ig_cursor;
		DEALLOCATE ig_cursor;
	END
	ELSE
	BEGIN
		INSERT INTO #templates
		SELECT @parentTemplateId
	END

	-- Remove any retired templates that aren't part of this version of the IG
	DELETE FROM #templates WHERE id IN (SELECT id FROM template WHERE owningImplementationGuideId != @implementationGuideId AND statusId = @retiredStatusId)

	insert_relationships:

	INSERT INTO #relationships (id)
	-- Contained templates
	SELECT tc.containedTemplateId AS id
	FROM template_constraint tc
		JOIN #templates tt ON tt.id = tc.templateId
	WHERE
		tc.containedTemplateId IS NOT NULL AND
		tc.containedTemplateId NOT IN (SELECT id FROM #templates)
	UNION ALL
	-- Implied templates
	SELECT t.impliedTemplateId
	FROM template t
		JOIN #templates tt ON tt.id = t.id
	WHERE
		t.impliedTemplateId IS NOT NULL AND
		t.impliedTemplateId NOT IN (SELECT id FROM #templates)

	IF (@inferred = 1)
	BEGIN
		INSERT INTO #templates
		SELECT id
		FROM #relationships
		SET @relationshipCount = @@ROWCOUNT
	END
	ELSE
	BEGIN
		INSERT INTO #templates
		SELECT #relationships.id
		FROM #relationships
			JOIN template t ON #relationships.id = t.id
			JOIN #implementationGuides ON t.owningImplementationGuideId = #implementationGuides.id
		SET @relationshipCount = @@ROWCOUNT
	END

	DELETE FROM #relationships

	IF (@relationshipCount > 0)
	BEGIN
		GOTO insert_relationships
	END

	SELECT DISTINCT id FROM #templates

	DROP TABLE #relationships
	DROP TABLE #templates
	DROP TABLE #implementationGuides
END
GO

DECLARE @grantUser VARCHAR(255)
SET @grantUser = (SELECT name FROM sys.database_principals WHERE name = 'trifolia' OR name = 'tdb_app')

IF (@grantUser IS NOT NULL)
BEGIN
	SET @grantUser = quotename(@grantUser)
	exec ('GRANT EXECUTE ON [dbo].GetImplementationGuideTemplates TO ' + @grantUser)
END
GO