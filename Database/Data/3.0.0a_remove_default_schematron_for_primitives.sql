begin transaction
update template_constraint set schematron = NULL where isPrimitive = 1 and schematron = 'not(.)'
commit transaction