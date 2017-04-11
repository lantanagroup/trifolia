ALTER VIEW [dbo].[v_containedtemplatecount] AS 
select 
  template_constraint.containedTemplateId AS containedTemplateId, 
  count(*) AS total 
from template_constraint 
group by template_constraint.containedTemplateId;