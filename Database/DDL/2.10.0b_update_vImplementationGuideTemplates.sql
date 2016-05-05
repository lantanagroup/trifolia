IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'v_implementationGuideTemplates')
BEGIN
	DROP VIEW v_implementationGuideTemplates
END
GO

CREATE VIEW [dbo].[v_implementationGuideTemplates] AS
	WITH versionedTemplateIgs 
	AS (
		select t.id as templateId, nig.id as implementationGuideId
		from template t
			left join template nt on nt.previousVersionTemplateId	= t.id
			join implementationguide nig on nig.previousVersionImplementationGuideId = t.owningImplementationGuideId
		where nt.id is null and nig.id is not null

		union all

		select vig.templateId, nig.id
		from versionedTemplateIgs vig
		join implementationguide nig on nig.previousVersionImplementationGuideId = vig.implementationGuideId
	)

	select ig.id as implementationGuideId, t.id as templateId 
	from implementationguide ig
		join template t on t.owningImplementationGuideId = ig.id

	union all

	select ig.id, vig.templateId
	from implementationguide ig
		join versionedTemplateIgs	vig on vig.implementationGuideId = ig.id
GO