CREATE VIEW v_codeSystemUsage
AS
	select
	  distinct
	  t.id as templateId,
	  t.oid as templateIdentifier,
	  t.name as templateName,
	  t.bookmark as templateBookmark,
	  cs.id as codeSystemId,
	  cs.oid as codeSystemIdentifier,
	  cs.name as codeSystemName
	from template_constraint tc
	  join template t on t.id = tc.templateId
	  join codesystem cs on cs.id = tc.codeSystemId

	union all

	select
	  distinct
	  t.id as templateId,
	  t.oid as templateIdentifier,
	  t.name as templateName,
	  t.bookmark as templateBookmark,
	  cs.id as codeSystemId,
	  cs.oid as codeSystemIdentifier,
	  cs.name as codeSystemName
	from template_constraint tc
	  join template t on t.id = tc.templateId
	  join valueset_member vsm on vsm.valueSetId = tc.valueSetId
	  join codesystem cs on cs.id = vsm.codeSystemId