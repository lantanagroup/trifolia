IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.VIEWS WHERE table_name = 'v_igaudittrail')
BEGIN
	DROP VIEW v_igaudittrail
END
GO

CREATE VIEW [dbo].[v_igaudittrail]
AS
SELECT
	a.username, 
	a.auditDate, 
	a.ip, 
	a.type, 
	a.note, 
	ISNULL(a.implementationGuideId, t.owningImplementationGuideId) AS implementationGuideId, 
	a.templateId, 
	a.templateConstraintId, 
	t.name AS templateName,
	tc.number AS conformanceNumber
FROM
	audit AS a 
	LEFT JOIN template AS t ON t.id = a.templateId
	LEFT JOIN template_constraint tc ON tc.id = a.templateConstraintId

GO