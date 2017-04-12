CREATE VIEW v_templateRelationship
AS
	select
		pt.id as parentTemplateId,
		pt.[name] as parentTemplateName,
		pt.oid as parentTemplateIdentifier,
		pt.bookmark as parentTemplateBookmark,
		t.id as childTemplateId,
		t.[name] as childTemplateName,
		t.oid as childTemplateIdentifier,
		t.bookmark as childTemplateBookmark,
		ISNULL(tc.conformance, 'MAY') as conformance
	from template t
		join template_constraint_reference tcr on tcr.referenceIdentifier = t.oid
		join template_constraint tc on tc.id = tcr.templateConstraintId
		join template pt on pt.id = tc.templateId
	where
		tcr.referenceType = 0