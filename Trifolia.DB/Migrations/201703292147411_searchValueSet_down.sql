ALTER PROCEDURE [dbo].[SearchValueSet]
	@userId INT = NULL,
	@searchText VARCHAR(255) = '',
	@count INT = 20,
	@page INT = 1,
	@orderProperty VARCHAR(128) = 'Name',
	@orderDesc BIT = 0
AS
BEGIN
	DECLARE @searchTextAny VARCHAR(512)
	SET @searchTextAny = CONCAT('%', @searchText, '%')

	DECLARE @offset INT
	SET @offset = (@page - 1) * @count

	DECLARE @totalItems INT

	SELECT 
		ig.id as implementationGuideId,
		CASE WHEN igp.permission IS NOT NULL THEN 1 ELSE 0 END AS canEditImplementationGuide
	INTO #publishedImplementationGuides
	FROM implementationguide ig
		JOIN publish_status ps on ps.id = ig.publishStatusId
		LEFT JOIN v_implementationGuidePermissions igp on igp.implementationGuideId = ig.id AND igp.userId = @userId AND igp.permission = 'Edit'
	WHERE
		ps.[status] = 'Published'

	SELECT DISTINCT
		vs.id as valueSetId,
		pig.canEditImplementationGuide as canEdit
	INTO #publishedValueSets
	FROM valueset vs
		JOIN template_constraint tc on tc.valueSetId = vs.id
		JOIN template t on t.id = tc.templateId
		JOIN #publishedImplementationGuides pig on pig.implementationGuideId = t.owningImplementationGuideId

	CREATE TABLE #valuesets (
		id INT, 
		name VARCHAR(255), 
		oid VARCHAR(255), 
		code VARCHAR(255), 
		[description] NVARCHAR(max), 
		intensional BIT NULL, 
		intensionalDefinition NVARCHAR(max), 
		source NVARCHAR(1024), 
		isComplete BIT)

	INSERT INTO #valuesets
	SELECT DISTINCT
		vs.id,
		vs.name,
		vs.oid,
		vs.code,
		vs.[description],
		vs.intensional,
		vs.intensionalDefinition,
		vs.source,
		CAST(CASE 
			WHEN vs.isIncomplete = 0 THEN 1
			ELSE 0
		END AS BIT) AS isComplete
	FROM valueset vs
	WHERE
		vs.code LIKE @searchTextAny
		OR vs.name LIKE @searchTextAny
		OR vs.oid LIKE @searchTextAny
		OR ISNULL(vs.description, '') LIKE @searchTextAny
		OR ISNULL(vs.source, '') LIKE @searchTextAny

	SET @totalItems = (SELECT COUNT(*) FROM #valuesets)

	SELECT
		@totalItems as totalItems,
		vs.*,
		CAST(CASE WHEN p.publishedIgCount IS NULL OR p.publishedIgCount = 0 THEN 0 ELSE 1 END AS BIT) AS hasPublishedIg,
		CAST(CASE WHEN up.uneditablePublishedIgCount IS NULL OR up.uneditablePublishedIgCount = 0 THEN 1 ELSE 0 END AS BIT) AS canEditPublishedIg
	FROM #valuesets vs
		LEFT JOIN (SELECT valueSetId, COUNT(*) as publishedIgCount FROM #publishedValueSets group by valueSetId) AS p ON p.valueSetId = vs.id
		LEFT JOIN (SELECT valueSetId, COUNT(*) as uneditablePublishedIgCount FROM #publishedValueSets WHERE canEdit = 0 GROUP BY valueSetId) as up ON up.valueSetId = vs.id
	ORDER BY 
		CASE @orderDesc WHEN 0 THEN
			CASE @orderProperty
				WHEN 'Name' THEN vs.name
				WHEN 'Oid' THEN vs.oid
				WHEN 'IsComplete' THEN CAST(vs.isComplete as VARCHAR(255))
			END
		END ASC,
		CASE @orderDesc WHEN 1 THEN
			CASE @orderProperty
				WHEN 'Name' THEN vs.name
			END
		END DESC
	OFFSET @offset ROWS
	FETCH NEXT @count ROWS ONLY
END