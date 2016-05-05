USE [templatedb]
GO

/****** Object:  View [dbo].[v_templateList]    Script Date: 8/15/2014 9:45:32 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

ALTER VIEW [dbo].[v_templateList]
AS
select 
  t1.id,
  t1.name,
  t1.oid,
  CASE 
    WHEN t1.isOpen = 1 THEN 'Yes'
	ELSE 'No' END AS [open],
  tt.name + ' (' + igt.name + ')' as templateType,
  tt.id as templateTypeId,
  CASE WHEN ig.[version] != 1 THEN ig.name + ' V' + CAST(ig.[version] AS VARCHAR(10)) ELSE ig.name END as implementationGuide,
  ig.id as implementationGuideId,
  o.name as organization,
  o.id as organizationId,
  ig.publishDate as publishDate,
  it.name as impliedTemplateName,
  it.oid as impliedTemplateOid,
  it.id as impliedTemplateId,
  ISNULL(t4.[count], 0) as constraintCount
  ,CASE 
    WHEN t2.constraints IS NOT NULL AND LEN(t2.constraints) > 0 THEN LEFT(t2.constraints, LEN(t2.constraints)-1)
    ELSE t2.constraints END AS constraints,
  t1.[description],
  t1.primaryContextType
from template t1
  join (
    select t3.id, (SELECT DISTINCT CAST(number AS VARCHAR) + ', ' FROM template_constraint tc WHERE tc.templateId = t3.id FOR XML PATH('')) as constraints
    from template t3) t2 on t2.id = t1.id
  left join (
    select tc.templateId, count(*) as [count] from template_constraint tc group by tc.templateId) t4 on t4.templateId = t1.id
  join implementationguidetype igt on igt.id = t1.implementationGuideTypeId
  join templatetype tt on tt.id = t1.templateTypeId
  left join implementationguide ig on ig.id = t1.owningImplementationGuideId
  left join organization o on o.id = t1.organizationId
  left join template it on it.id = t1.impliedTemplateId



GO


