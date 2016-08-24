USE [templatedb_auth]
GO

/****** Object:  View [dbo].[v_templateList]    Script Date: 8/9/2016 4:30:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

ALTER VIEW [dbo].[v_templateList]
--WITH SCHEMABINDING
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
  t1.[description],
  t1.primaryContextType
from dbo.template t1
  join dbo.implementationguidetype igt on igt.id = t1.implementationGuideTypeId
  join dbo.templatetype tt on tt.id = t1.templateTypeId
  left join dbo.implementationguide ig on ig.id = t1.owningImplementationGuideId
  left join dbo.organization o on o.id = ig.organizationId
  left join dbo.template it on it.id = t1.impliedTemplateId

GO


