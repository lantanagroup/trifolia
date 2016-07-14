BEGIN TRANSACTION

-- Output the data that we are changing
SELECT template.oid as oldIdentifier, CONCAT('hl7-att:', t2.oid, ':2014-06-09') as newIdentifier
FROM template
	JOIN template t2 ON t2.id = template.previousVersionTemplateId
	JOIN implementationguide ig ON ig.id = template.owningImplementationGuideId
	JOIN publish_status ps ON ps.id = ig.publishStatusId
WHERE
	ps.[status] != 'Published'

UNION ALL

SELECT template.oid as oldIdentifier, CONCAT('oid:', oid) as newIdentifier
FROM template
	JOIN implementationguide ig ON ig.id = template.owningImplementationGuideId
	JOIN publish_status ps ON ps.id = ig.publishStatusId
WHERE
	previousVersionTemplateId IS NULL
	OR ps.[status] = 'Published'

-- Update versioned templates that are NOT published to use the hl7-att format,
-- setting the date of versioning to today
UPDATE template
SET oid = CONCAT('urn:hl7ii:', t2.oid, ':2014-06-09')
FROM template
	JOIN template t2 ON t2.id = template.previousVersionTemplateId
	JOIN implementationguide ig ON ig.id = template.owningImplementationGuideId
	JOIN publish_status ps ON ps.id = ig.publishStatusId
WHERE
	ps.[status] != 'Published'

-- Update any templates that are not versioned, or are versioned and have already
-- been published, to use the "oid:" format
UPDATE template
SET oid = CONCAT('oid:', oid)
FROM template
	JOIN implementationguide ig ON ig.id = template.owningImplementationGuideId
	JOIN publish_status ps ON ps.id = ig.publishStatusId
WHERE
	previousVersionTemplateId IS NULL
	OR ps.[status] = 'Published'

COMMIT TRANSACTION