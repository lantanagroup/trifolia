begin transaction

-- Update all constraints for FHIR to default mustSupport to true
update template_constraint set
  mustSupport = 1
from template_constraint
  join template t on t.id = template_constraint.templateId
  join templatetype tt on tt.id = t.templateTypeId
  join implementationguidetype igt on igt.id = tt.implementationGuideTypeId
where igt.name = 'FHIR DSTU1' or igt.name = 'FHIR DSTU2'

commit transaction