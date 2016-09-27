ALTER VIEW [dbo].[v_template] AS
select
  t.id,
  t.oid,
  t.name,
  t.isOpen,
  ig.id as owningImplementationGuideId,
  ig.name as owningImplementationGuideTitle,
  igt.name as implementationGuideTypeName,
  tt.id as templateTypeId,
  tt.name as templateTypeName,
  ISNULL(t.primaryContext, tt.rootContext) as primaryContext,
  tt.name + ' (' + igt.name + ')' as templateTypeDisplay,
  o.name as organizationName,
  ig.publishDate,
  it.oid as impliedTemplateOid,
  it.name as impliedTemplateTitle,
  cc.total as constraintCount,
  ctc.total as containedTemplateCount,
  itc.total as impliedTemplateCount
from template t
  join implementationguidetype igt on igt.id = t.implementationGuideTypeId
  join templatetype tt on tt.id = t.templateTypeId
  left join implementationguide ig on ig.id = t.owningImplementationGuideId
  left join organization o on o.id = ig.organizationId
  left join template it on it.id = t.impliedTemplateId
  left join v_constraintCount cc on cc.templateId = t.id
  left join v_containedTemplateCount ctc on ctc.containedTemplateId = t.id
  left join v_impliedTemplateCount itc on itc.impliedTemplateId = t.id;