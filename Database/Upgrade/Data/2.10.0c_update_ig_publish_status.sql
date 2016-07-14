BEGIN TRAN

DECLARE @PublishId INT
DECLARE @DraftId INT

SELECT	@PublishId = ps.id
FROM	publish_status ps
WHERE	ps.[status] = 'Published'

SELECT	@DraftId = ps.id
FROM	publish_status ps
WHERE	ps.[status] = 'Draft'

CREATE TABLE #publishedIgs(
	id INT NOT NULL
)

INSERT INTO #publishedIgs
SELECT		ig.id
FROM		implementationguide ig
WHERE		ig.publishDate IS NOT NULL
OR			ig.publishStatusId = @PublishId

UPDATE	ig
SET		ig.publishStatusId = @PublishId
FROM	implementationguide ig
WHERE	EXISTS (SELECT id FROM #publishedIgs pgs WHERE ig.id = pgs.id)
	AND (ig.publishStatusId IS NULL OR ig.publishStatusId != @PublishId)

UPDATE	ig
SET		ig.publishDate = '1/1/1900'
FROM	implementationguide ig
WHERE	EXISTS (SELECT id FROM #publishedIgs pgs WHERE ig.id = pgs.id)
	AND ig.publishDate IS NULL

UPDATE	t
SET		t.statusId = @PublishId
FROM	template t
WHERE	EXISTS(SELECT id FROM #publishedIgs pgs WHERE t.owningImplementationGuideId = pgs.id)

SELECT name, publishDate, ps.status
FROM implementationguide ig
	JOIN publish_status ps ON ps.id = ig.publishStatusId
	JOIN #publishedIgs pig ON pig.id = ig.id

-- Default all unset publish statuses on IGs to "Draft"
UPDATE implementationguide
SET publishStatusId = @DraftId
WHERE publishStatusId IS NULL

-- Require publish status for all implementation guides
ALTER TABLE implementationguide ALTER COLUMN publishStatusId INT NOT NULL

-- Default all unset publish statuses on templates to the associated IG's status
UPDATE template
SET template.statusId = implementationguide.publishStatusId
FROM implementationguide
WHERE
	template.statusId IS NULL
	AND template.owningImplementationGuideId = implementationguide.id

-- Require status for all templates
ALTER TABLE template ALTER COLUMN statusId INT NOT NULL

DROP TABLE #publishedIgs

COMMIT