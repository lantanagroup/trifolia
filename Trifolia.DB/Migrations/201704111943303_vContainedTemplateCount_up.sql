ALTER VIEW [dbo].[v_containedtemplatecount] AS 
select 
  t.id AS containedTemplateId, 
  count(*) AS total 
from template_constraint_reference tcr
  join template t on t.id = tcr.referenceIdentifier
where
  tcr.referenceType = 0
group by t.id;